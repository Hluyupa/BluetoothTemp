using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using BluetoothTemp.Abstract;
using BluetoothTemp.Abstract.EventsArgs;
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
        public BluetoothAdapter BluetoothAdapter;

        //BluetoothLeScanner необходим для включения
        //сакнирования близжайших Bluetooth устройств
        private BluetoothLeScanner _scanner;

        //CustomScanCallback - в этом классе переопределяются методы,
        //которые срабатывают при обнаружении новых устройств
        public CustomScanCallback scanCallback;

        //ScanSettings - установка параметров сканирования
        private ScanSettings _scanSettingsForCache;

        private ScanSettings _scanSettingsForDiscovery;

        //Фильтры для сканирования
        private List<ScanFilter> filters;

        //Устройство для подключения
        private BluetoothDevice _device;

        //CustomBluetoothGattCallback - в этом классе переопределяются методы,
        //которые срабатывают при обнаружении новых сервисов,
        //чтении, записи, получении новых данных с устройства 
        private CustomBluetoothGattCallback bluetoothGattCallback;

        private BluetoothGatt _gatt;

        private Dictionary<string, BluetoothGatt> _connectedGattDevices;

        //Список характеристик подключенного устрйоства. 
        //Необходимо передать извне ссылку на коллекцию.
        public ICollection<DeviceCharacteristicModel> DeviceCharacterisctics { get; set; }
        
        //Список обнаруженных устройств
        public ICollection<ScannedBluetoothDeviceModel> ScannedDevices { get; set; }

        //Список устройств для автоматического подключения
        public ICollection<AutoconnectDeviceModel> AutoconnectDevices { get; set; }

        //Очередь команд
        public Queue<Action> CommandQueue;

        //Очередь команд, нацеленных на работу с подключением и отключением
        public Queue<Action> BluetoothNetworkCommandQueue;

        //Очередь устройств на подключение
        public Queue<Action> ConnectDeviceQueue;

        public Queue<Action> DisconnectDeviceQueue;
        public BluetoothDeviceInfoModel bluetoothDeviceInfo { get; set; }

        //Событие, срабатывающее после прочтения всех характеристик.
        public event EventHandler<AfterReadingEventArgs> EventAfterReading;

        //Флаг, показывающий необходимость в попытках передподключения
        //при неудачном подключении
        private bool _isTryReconnect;

        //Флаг для определения, закешировано устройство или нет.
        private bool _isDeviceCached;

        //Флаг автоподклбючения устройств
        private bool _isAutoConnect;

        //Флаг, информирующий о старте очереди
        private bool _isQueueStarted;

        private string _readbleDeviceMacAddress;

        private static BluetoothAPI _instance;

        //В конструкторе происходит инициализация Bluetooth адаптера,
        //колбеков, списков и настроек сканирования.
        private BluetoothAPI()
        {
            bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            BluetoothAdapter = bluetoothManager.Adapter;
            

            scanCallback = new CustomScanCallback();
            filters = new List<ScanFilter>();
            _connectedGattDevices = new Dictionary<string, BluetoothGatt>();
            CommandQueue = new Queue<Action>();
            ConnectDeviceQueue = new Queue<Action>();
            DisconnectDeviceQueue = new Queue<Action>();
            

            _scanner = BluetoothAdapter.BluetoothLeScanner;

            _scanSettingsForDiscovery = new ScanSettings.Builder()
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                .SetCallbackType(ScanCallbackType.AllMatches)
                .SetMatchMode(BluetoothScanMatchMode.Aggressive)
                .SetNumOfMatches(1)
                .SetReportDelay(0)
                .Build();
            _scanSettingsForCache = new ScanSettings.Builder()
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                .SetCallbackType(ScanCallbackType.AllMatches)
                .SetMatchMode(BluetoothScanMatchMode.Sticky)
                .SetNumOfMatches(1)
                .SetReportDelay(0)
                .Build();

            _isDeviceCached = true;
            _isAutoConnect = false;
            _isQueueStarted = false;
            scanCallback.ScanResultEvent += ScanResult;

            bluetoothGattCallback = new CustomBluetoothGattCallback();
            bluetoothGattCallback.ConnectionStateChangeEvent = ConnectionStateChange;
            bluetoothGattCallback.CharacteristicReadEvent = CharacteristicRead;
            bluetoothGattCallback.CharacteristicChangedEvent = CharacteristicChanged;
            bluetoothGattCallback.ServicesDiscoveredEvent = ServicesDiscovered;
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
            if (BluetoothAdapter.IsEnabled)
            {
                BluetoothAdapter.Disable();
                disableHandler();
            }
            else
            {
                BluetoothAdapter.Enable();
                enableHandler();
            }
        }

        //Метод автоматического поделючения к устройствам через мак-адреса,
        //полученными в списке.
        public void AutoconnectToDevices(ICollection<AutoconnectDeviceModel> autoconnectDevices)
        {
            if (BluetoothAdapter.IsEnabled)
            {
                AutoconnectDevices = autoconnectDevices;
                _isAutoConnect = true;
                foreach (var device in AutoconnectDevices)
                {
                    filters.Add(new ScanFilter.Builder().SetDeviceAddress(device.MacAddress).Build());
                }
                _scanner.StartScan(filters: filters, settings: _scanSettingsForDiscovery, callback: scanCallback);
            }
        }

        //Метод получения списка найденных устройств
        public void ScanDevices(ICollection<ScannedBluetoothDeviceModel> scannedDevices)
        {
            if (BluetoothAdapter.IsEnabled)
            {
                ScannedDevices = scannedDevices;
                if (_scanner != null)
                {
                    _scanner.StartScan(filters: null, settings: _scanSettingsForDiscovery, callback: scanCallback);
                }
            }
        }

        //Результаты сканирования с филтром при подключении
        //Метод передаётся в качестве события объекту ScanCallback
        private void ScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            if (filters.Count != 0 && result != null)
            {
                if (!_isDeviceCached)
                {
                    _gatt = result.Device.ConnectGatt(Application.Context, false, bluetoothGattCallback, transport: BluetoothTransports.Le);
                    _isDeviceCached = true;
                    _scanner.StopScan(scanCallback);
                    _scanner.StartScan(filters: filters, settings: _scanSettingsForDiscovery, callback: scanCallback);
                }
                else if (_isAutoConnect)
                {
                    var scannedDevice = AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(result.Device.Address));
                    
                    if (scannedDevice != null && scannedDevice.StatusConnect == 0)
                    {
                        scannedDevice.StatusConnect = 1;
                        ConnectDeviceQueue.Enqueue(() => Connect(result.Device));
                        if (!_isQueueStarted && ConnectDeviceQueue.Count > 0)
                        {
                            ConnectDeviceQueue.Peek().Invoke();
                            _isQueueStarted = true;
                        }
                    }
                }
            }
            else
            {
                var device = ScannedDevices.FirstOrDefault(p => p.Device.Address == result.Device.Address);
                if (device == null)
                {
                    ScannedDevices.Add(new ScannedBluetoothDeviceModel { Name = result.ScanRecord.DeviceName, Device = result.Device });
                }
            }
        }


        //Подключение к устройству, которое передаётся в параметрах
        public void Connect(BluetoothDevice device)
        {
            bluetoothDeviceInfo = new BluetoothDeviceInfoModel();
            _device = device;
            _isTryReconnect = true;
            
            //Проверка на то, находится ли Bluetooth устройство в кеше Bluetooth.
            //Если нет, то это устройство сканируется отдельно,
            //при этом останавливается общее сканирование.
            //Иначе начинается соединение с Bluetooth устройством.
            BluetoothDevice checkedCacheDevice = BluetoothAdapter.GetRemoteDevice(device.Address);
            if (checkedCacheDevice.Type == BluetoothDeviceType.Unknown)
            {
                _isDeviceCached = false;
                _scanner.StopScan(scanCallback);
                _scanner.StartScan(
                    new List<ScanFilter> { new ScanFilter.Builder().SetDeviceAddress(checkedCacheDevice.Address).Build() },
                    _scanSettingsForCache,
                    scanCallback);
            }
            else
            {
                _connectedGattDevices.Add(_device.Address, _device.ConnectGatt(Application.Context, false, bluetoothGattCallback, transport: BluetoothTransports.Le));
            }
        }

        //Отключение от устройства и установка значений по умолчанию
        public void Disconnect(string macAddress = null)
        {
            if (macAddress != null && _isAutoConnect) 
            {
                if (_connectedGattDevices.Count != 0)
                {
                    DisconnectDeviceQueue.Enqueue(() => _connectedGattDevices[macAddress].Disconnect());
                    DisconnectDeviceQueue.Peek().Invoke();
                }
                else
                {
                    StopScanDevices();
                    AutoconnectDevices.Remove(AutoconnectDevices.FirstOrDefault(p => p.MacAddress == macAddress));
                    if (AutoconnectDevices.Count != 0) 
                    {
                        AutoconnectToDevices(AutoconnectDevices);
                    }
                }
                return;
            }
            _isTryReconnect = false;
            filters.Clear();
            StopScanDevices();

            if (_isAutoConnect)
            {
                ConnectDeviceQueue.Clear();
                _isQueueStarted = false;
                _isAutoConnect = false;
            }
           
            if (_connectedGattDevices.Count != 0)
            {
                foreach (var connectedDevice in _connectedGattDevices)
                {
                    DisconnectDeviceQueue.Enqueue(() => connectedDevice.Value.Disconnect());
                }
                //_connectedGattDevices.Clear();
                DisconnectDeviceQueue.Peek().Invoke();
            }
            _device = null;
        }

        public void StopScanDevices()
        {
            _scanner.StopScan(scanCallback);
        }

        //Метод, выполняющийся при изменении состояния подключения.
        //Он передаётся в качестве события объекту BluetoothGattCallback
        private void ConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            if (status == GattStatus.Success)
            {
                if (newState == ProfileState.Connected)
                {
                    _isTryReconnect = false;
                    _gatt = _connectedGattDevices[gatt.Device.Address];
                    gatt.DiscoverServices();
                    if (_isAutoConnect)
                    {
                        AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(gatt.Device.Address)).StatusConnect = 2;
                    }
                }
                else if (newState == ProfileState.Disconnected)
                {
                    if (DisconnectDeviceQueue.Count == 0)
                    {
                        _connectedGattDevices.Remove(gatt.Device.Address);
                        gatt.Close();
                    }
                    else
                    {
                        gatt.Close();
                        _connectedGattDevices.Remove(gatt.Device.Address);
                        DisconnectDeviceQueue.Dequeue();
                        if (DisconnectDeviceQueue.Count > 0)
                            DisconnectDeviceQueue.Peek().Invoke();
                    }
                }
            }
            else
            {
                if (_isTryReconnect)
                {
                    gatt.Close();
                    _device.ConnectGatt(Application.Context, false, bluetoothGattCallback, BluetoothTransports.Le);
                }
                else 
                {
                    if (DisconnectDeviceQueue.Count == 0)
                    {
                        _connectedGattDevices.Remove(gatt.Device.Address);
                        gatt.Close();
                    }
                    else
                    {
                        gatt.Close();
                        _connectedGattDevices.Remove(gatt.Device.Address);
                        DisconnectDeviceQueue.Dequeue();
                        if (DisconnectDeviceQueue.Count > 0)
                            DisconnectDeviceQueue.Peek().Invoke();
                    }
                }
            }
        }

        private void ServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            //Read(gatt.Device.Address);
            foreach (var service in gatt.Services)
            {
                foreach (var characteristic in service.Characteristics)
                {
                    switch (characteristic.Uuid.ToString().ToLower().Substring(0, 8))
                    {
                        case "00002a1f":
                        case "00002a19":
                        case "00002a25":
                            CommandQueue.Enqueue(() => gatt.ReadCharacteristic(characteristic));
                            break;
                        default:
                            break;
                    }
                }
            }
            NextCommand();
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
                    if (value == "-127.00")
                    {
                        CommandQueue.Enqueue(() => _gatt.ReadCharacteristic(characteristic));
                    }
                    else
                    {
                        bluetoothDeviceInfo.Temperature = value;
                        if (_isAutoConnect)
                            AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(_gatt.Device.Address)).Temperature = value;
                        else
                            DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Температура", Value = value + "°C", UUID = uuidHeader });
                    }
                    break;
                case "00002a19":
                    if (_isAutoConnect)
                        AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(_gatt.Device.Address)).BatteryLevel = value;
                    else
                        DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Заряд батареи", Value = value + "%", UUID = uuidHeader });
                    break;
                case "00002a25":
                    bluetoothDeviceInfo.Id = value;
                    if(_isAutoConnect)
                        AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(_gatt.Device.Address)).SerialNumber = value;
                    else
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

                if (_isAutoConnect)
                {
                    ConnectDeviceQueue.Dequeue();
                    if (ConnectDeviceQueue.Count > 0)
                        ConnectDeviceQueue.Peek().Invoke();
                    else
                        _isQueueStarted = false;
                }
                else
                {
                    EventAfterReading?.Invoke(this, new AfterReadingEventArgs { DeviceCharacteristics = this.DeviceCharacterisctics });
                }
                
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
