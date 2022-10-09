using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using BluetoothTemp.Models;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothTemp.Abstract
{
    public class BluetoothAPI
    {
        //BluetoothManager нужен для получения объекта BluetoothAdapter,
        //а также для общего управления.
        public BluetoothManager bluetoothManager;

        //BluetoothAdapter необходим для выполнения основных
        //задач Bluetooth (получение списка сопряжённых устройств,
        //поиск других устройств).
        public BluetoothAdapter bluetoothAdapter;

        //BluetoothLeScanner необходим для включения
        //сакнирования близжайших Bluetooth устройств
        public BluetoothLeScanner scanner;

        //CustomScanCallback - в этом классе переопределяются методы,
        //которые срабатывают при обнаружении новых устройств
        public CustomScanCallback scanCallback;

        //ScanSettings - установка параметров сканирования
        private ScanSettings scanSettings;

        private BluetoothDevice _device;

        public CustomBluetoothGattCallback bluetoothGattCallback;

        private BluetoothGatt _gatt;

        //Список характеристик подключенного устрйоства. 
        //Необходимо передать извне ссылку на коллекцию.
        public ICollection<DeviceCharacteristicModel> DeviceCharacterisctics { get; set; }


        //Принимает обработчики вкл/выкл Bluetooth
        public delegate void OnOffBluetoothHandler();

        private static BluetoothAPI _instance;
        private BluetoothAPI()
        {
            bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            bluetoothAdapter = bluetoothManager.Adapter;
            scanSettings = new ScanSettings.Builder()
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                .SetCallbackType(ScanCallbackType.AllMatches)
                .SetMatchMode(BluetoothScanMatchMode.Sticky)
                .SetNumOfMatches(1)
                .SetReportDelay(1)
                .Build();
        }

        public static BluetoothAPI GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BluetoothAPI();
            }
            return _instance;
        }

        //Метод включения и выключения Bluetooth.
        //Принимает события на включение и выключение Bluetooth.
        public void OnOffBluetooth(OnOffBluetoothHandler enableHandler, OnOffBluetoothHandler disableHandler)
        {
            if (bluetoothAdapter.IsEnabled)
            {
                bluetoothAdapter.Disable();
                disableHandler();
            }
            else
            {
                bluetoothAdapter.Enable();
                enableHandler();
            }
        }

        //Метод получения списка найденных устрйств
        public void GetScanDevices(ICollection<BluetoothDevice> bluetoothDevicesList)
        {
            if (bluetoothAdapter.IsEnabled)
            {
                scanner = bluetoothAdapter.BluetoothLeScanner;
                scanCallback = new CustomScanCallback(bluetoothDevicesList);
                if (scanner != null)
                {
                    scanner.StartScan(filters: null, settings: scanSettings, callback: scanCallback);
                }
            }   
        }

        //Подключение к устройству, которое передаётся в параметрах
        public void Connect(BluetoothDevice device)
        {
            _device = device;
            bluetoothGattCallback = new CustomBluetoothGattCallback();

            bluetoothGattCallback.ConnectionStateChangeEvent += ConnectionStateChange;
            bluetoothGattCallback.CharacteristicReadEvent += CharacteristicRead;
            bluetoothGattCallback.CharacteristicChangedEvent += CharacteristicChanged;

            _gatt = _device.ConnectGatt(Application.Context, false, bluetoothGattCallback, transport: BluetoothTransports.Le);
            
        }

        private void GattCallbackCreator()
        {

        }

        //Метод, выполняющийся при изменении состояния подключения.
        //Также обрабатывает разрыв соединения и пытается выполнить переподклбчение.
        //Он передаётся в качестве события объекту BluetoothGattCallback
        private async void ConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            
            if (status == GattStatus.Success)
            {
                if (newState == ProfileState.Connected)
                {
                    gatt.DiscoverServices();
                    Read();
                }
                else if (newState == ProfileState.Disconnected)
                {
                    gatt.Close();
                }
            }
            else
            {
                _gatt.Connect();
            }
        }

        //Метод, выполняющийся при чтении характеристики.
        //Он передаётся в качестве события объекту BluetoothGattCallback
        private void CharacteristicRead(BluetoothGattCharacteristic characteristic)
        {
            var isNotify = _gatt.SetCharacteristicNotification(characteristic, true);

            byte[] buffer = new byte[characteristic.GetValue().Length];
            Array.Copy(characteristic.GetValue(), 0, buffer, 0, characteristic.GetValue().Length);

            string value = Encoding.UTF8.GetString(buffer);
            string uuidHeader = characteristic.Uuid.ToString().ToLower().Substring(0, 8);

            if (isNotify)
            {
                UUID uuid = UUID.FromString("00002902-0000-1000-8000-00805f9b34fb");
                BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(uuid);
                descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                _gatt.WriteDescriptor(descriptor);
                //ОБЯЗАТЕЛЬНО ВЫНЕСТИ В ФУНКЦИЮ
                switch (uuidHeader)
                {
                    case "00002a1f":
                        var characteristicTemp = DeviceCharacterisctics.FirstOrDefault(p=>p.UUID.Equals(uuidHeader));
                        if (characteristicTemp != null)
                        {
                            characteristicTemp.Value = value;
                        }
                        else
                            DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Temperature", Value = value + "°C", UUID = uuidHeader });
                        break;
                    case "00002a19":
                        var characteristicBattery = DeviceCharacterisctics.FirstOrDefault(p => p.UUID.Equals(uuidHeader));
                        if (characteristicBattery != null)
                        {
                            characteristicBattery.Value = value;
                        }
                        DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Battery Level", Value = value + "%", UUID = uuidHeader });
                        break;
                    default:
                        break;
                }
            }
            else
            {
                DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = characteristic.Uuid.ToString().ToLower().Substring(0, 8), Value = value, UUID = uuidHeader });
            }
        }

        //Метод, выполняющийся при получении уведомления об обновлении характеристики.
        //Он передаётся в качестве события объекту BluetoothGattCallback
        private void CharacteristicChanged(BluetoothGattCharacteristic characteristic)
        {
            byte[] buffer = new byte[characteristic.GetValue().Length];
            Array.Copy(characteristic.GetValue(), 0, buffer, 0, characteristic.GetValue().Length);
            var value = Encoding.UTF8.GetString(buffer);
            DeviceCharacterisctics.FirstOrDefault(p => p.UUID == characteristic.Uuid.ToString().ToLower().Substring(0, 8)).Value = value;
        }

        //Отключение от устройства
        public void Disconnect()
        {
            if (_gatt != null)
            {
                _gatt.Disconnect();
            }
            _gatt = null;
            _device = null;

        }

        //Поиск сервисов и чтение их характеристик
        public async void Read()
        {
            int trialsCount = 0;
            bool detectedServiceFlag = false;
            do
            {
                if (_gatt.Services.Count != 0)
                {
                    detectedServiceFlag = true;
                }
                else
                {
                    if (trialsCount > 10)
                    {
                        return;
                    }
                    await Task.Delay(1000);
                }
            } while (detectedServiceFlag == false);


            foreach (var service in _gatt.Services)
            {
                var tempCharacteristic = service.Characteristics.FirstOrDefault(p => p.Uuid.ToString().ToLower().Contains("00002a1f"));
                if (tempCharacteristic != null)
                {
                    _gatt.ReadCharacteristic(tempCharacteristic);
                }
            }
        }
    }
}
