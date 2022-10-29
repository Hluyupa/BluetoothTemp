using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using Java.Util;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace BluetoothTemp.TelephoneServices.Bluetooth
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

        //Фильтры для сканирования
        private List<ScanFilter> filters;

        //Устройство для подключения
        private BluetoothDevice _device;

        //CustomBluetoothGattCallback - в этом классе переопределяются методы,
        //которые срабатывают при обнаружении новых сервисов,
        //чтении, записи, получении новых данных с устройства 
        public CustomBluetoothGattCallback bluetoothGattCallback;

        private BluetoothGatt _gatt;

        //Список характеристик подключенного устрйоства. 
        //Необходимо передать извне ссылку на коллекцию.
        public ICollection<DeviceCharacteristicModel> DeviceCharacterisctics { get; set; }
        
        //Список обнаруженных устройств
        public ICollection<ScannedBluetoothDeviceModel> ScannedDevices { get; set; }

        //Очередь команд
        public Queue<Action> CommandQueue;
        public BluetoothDeviceInfoModel bluetoothDeviceInfo { get; set; }

        //Событие, срабатывающее после прочтения всех характеристик.
        public event Action EventAfterReading;

        private static BluetoothAPI _instance;

        //В конструкторе происходит инициализация Bluetooth адаптера,
        //колбеков, списков и настроек сканирования.
        private BluetoothAPI()
        {
            bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            bluetoothAdapter = bluetoothManager.Adapter;

            scanCallback = new CustomScanCallback();
            filters = new List<ScanFilter>();
            CommandQueue = new Queue<Action>();

            scanCallback.BatchScanResultsEvent += BatchScanResults;
            scanCallback.ScanResultEvent += ScanResult;

            bluetoothGattCallback = new CustomBluetoothGattCallback();
            bluetoothGattCallback.ConnectionStateChangeEvent += ConnectionStateChange;
            bluetoothGattCallback.CharacteristicReadEvent += CharacteristicRead;
            bluetoothGattCallback.CharacteristicChangedEvent += CharacteristicChanged;

            scanSettings = new ScanSettings.Builder()
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                .SetCallbackType(ScanCallbackType.AllMatches)
                .SetMatchMode(BluetoothScanMatchMode.Aggressive)
                .SetNumOfMatches(1)
                .SetReportDelay(1000)
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
        public void OnOffBluetooth(Action enableHandler, Action disableHandler)
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

        //Метод получения списка найденных устройств
        public void GetScanDevices(ICollection<ScannedBluetoothDeviceModel> scannedDevices)
        {
            if (bluetoothAdapter.IsEnabled)
            {
                ScannedDevices = scannedDevices;
                scanner = bluetoothAdapter.BluetoothLeScanner;

                if (scanner != null)
                {
                    scanner.StartScan(filters: null, settings: scanSettings, callback: scanCallback);
                }
            }   
        }

        //Результаты сканирования приходят в этот метод
        //Он передаётся в качестве события объекту ScanCallback
        private void BatchScanResults(IList<ScanResult> results)
        {
            foreach (var result in results)
            {
                var device = ScannedDevices.FirstOrDefault(p => p.Device.Address == result.Device.Address);
                if (device == null)
                {
                    ScannedDevices.Add(new ScannedBluetoothDeviceModel { Name = result.ScanRecord.DeviceName, Device = result.Device });
                }
            }
        }

        //Результаты сканирования с филтром при подключении
        //Метод передаётся в качестве события объекту ScanCallback
        public void ScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            if (filters.Count != 0 && result != null)
            {
                _gatt = result.Device.ConnectGatt(Application.Context, false, bluetoothGattCallback, transport: BluetoothTransports.Le);
                scanner.StopScan(scanCallback);
                return;
            }
        }


        //Подключение к устройству, которое передаётся в параметрах
        public async void Connect(BluetoothDevice device)
        {
            bluetoothDeviceInfo = new BluetoothDeviceInfoModel();
            _device = device;
            
            //Проверка на то, находится ли Bluetooth устройство в кеше Bluetooth.
            //Если нет, то это устройство сканируется отдельно,
            //при этом останавливается общее сканирование.
            //Иначе начинается соединение с Bluetooth устройством.
            BluetoothDevice checkedCacheDevice = bluetoothAdapter.GetRemoteDevice(device.Address);
            if (checkedCacheDevice.Type == BluetoothDeviceType.Unknown)
            {
                filters.Add(new ScanFilter.Builder().SetDeviceAddress(checkedCacheDevice.Address).Build());
                scanner.StopScan(scanCallback);
                scanner.StartScan(
                    filters,
                    new ScanSettings.Builder()
                        .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                        .SetCallbackType(ScanCallbackType.AllMatches)
                        .SetMatchMode(BluetoothScanMatchMode.Sticky)
                        .SetNumOfMatches(1)
                        .SetReportDelay(0)
                        .Build(),
                    scanCallback);
            }
            else
            {
                await Task.Run(() => _gatt = _device.ConnectGatt(Application.Context, false, bluetoothGattCallback, transport: BluetoothTransports.Le));
            }
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

        //Метод, выполняющийся при изменении состояния подключения.
        //Также обрабатывает разрыв соединения и пытается выполнить переподклбчение.
        //Он передаётся в качестве события объекту BluetoothGattCallback
        private void ConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            
            if (status == GattStatus.Success)
            {
                if (newState == ProfileState.Connected)
                {
                    //trialConnectedCounter = 0;
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
                gatt.Close();
            }
        }

        //Метод, выполняющийся при чтении характеристики.
        //Также сообщает очереди команд о завершении чтения характеристики.
        //Он передаётся в качестве события объекту BluetoothGattCallback.
        private void CharacteristicRead(BluetoothGattCharacteristic characteristic)
        {
            byte[] buffer = new byte[characteristic.GetValue().Length];
            Array.Copy(characteristic.GetValue(), 0, buffer, 0, characteristic.GetValue().Length);
           
            string value = Encoding.UTF8.GetString(buffer);
            string uuidHeader = characteristic.Uuid.ToString().ToLower().Substring(0, 8);
            
            switch (uuidHeader)
            {
                case "00002a1f":    
                    bluetoothDeviceInfo.Temperature = value;
                    DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Температура", Value = value + "°C", UUID = uuidHeader });
                    break;
                case "00002a19":
                    DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Заряд батареи", Value = value + "%", UUID = uuidHeader });
                    break;
                case "00002a25":
                    bluetoothDeviceInfo.Id = value;
                    DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Серийный номер", Value = value, UUID = uuidHeader });
                    break;
                default:
                    DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "N/A", Value = value, UUID = uuidHeader });
                    break;
            }
            CompletedCommand();
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

      
        //Поиск сервисов, добавление команд чтения характеристик
        //в очередь на выполение и её запуск.
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
                foreach (var characteristic in service.Characteristics)
                {
                    switch (characteristic.Uuid.ToString().ToLower().Substring(0, 8))
                    {
                        case "00002a1f":
                        case "00002a19":
                        case "00002a25":
                            CommandQueue.Enqueue(() => _gatt.ReadCharacteristic(characteristic));
                            break;
                        default:
                            break;
                    }
                }
            }
            NextCommand();
        }

        //Запуск следующей команды
        private void NextCommand()
        {
            if (CommandQueue.Count > 0)
            {
                CommandQueue.Peek().Invoke();
            }
            else
            {
                CompositeAndSendJson();
                EventAfterReading?.Invoke();
            }
        }

        //Удаление выполненной команды
        private void CompletedCommand()
        {
            CommandQueue.Dequeue();
            NextCommand();
        }

        //Метод отправки данных на сервер.
        private void CompositeAndSendJson()
        {
            var dateTime = DateTime.UtcNow;
            var timeStamp = (int)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            bluetoothDeviceInfo.Time = timeStamp.ToString();

            string[,] arr = new string[1, 3]
            {
                { bluetoothDeviceInfo.Id, bluetoothDeviceInfo.Time, bluetoothDeviceInfo.Temperature }
            };
            var request = new RestRequest("https://temp.skid.su/api/v1/data", Method.Post).AddJsonBody(JsonConvert.SerializeObject(arr));
            var responce = Client.GetInstance().restClient.Execute(request);
            if (responce.StatusCode == System.Net.HttpStatusCode.OK)
            {

            }
            else
            {
                var a = responce.ErrorException;
            }
        }
    }
}
