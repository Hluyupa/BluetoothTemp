using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using BluetoothTemp.Abstract;
using BluetoothTemp.Abstract.EventsArgs;
using BluetoothTemp.Models;
using Java.Util;
using Javax.Security.Auth;
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
        private CustomScanCallback connectScanCallback;
        
        private CustomScanCallback cacheScanCallback;

        //ScanSettings - установка параметров сканирования
        private ScanSettings _scanSettingsForCache;

        private ScanSettings _scanSettingsForDiscovery;

        //Фильтры для сканирования
        private List<ScanFilter> filters;

        //Устройство для подключения
        private Dictionary<string, ConnectDevice> _connectedDevices;

        //CustomBluetoothGattCallback - в этом классе переопределяются методы,
        //которые срабатывают при обнаружении новых сервисов,
        //чтении, записи, получении новых данных с устройства 
        private CustomBluetoothGattCallback bluetoothGattCallback;

        //Список характеристик подключенного устрйоства. 
        //Необходимо передать извне ссылку на коллекцию.
        public ICollection<DeviceCharacteristicModel> DeviceCharacterisctics { get; set; }
        
        //Список обнаруженных устройств
        public ICollection<ScannedBluetoothDeviceModel> ScannedDevices { get; set; }

        //Список устройств для автоматического подключения
        public ICollection<AutoconnectDeviceModel> AutoconnectDevices { get; set; }

        private Dictionary<string, BluetoothDeviceInfoModel> bluetoothDevicesInfo;

        private Queue<BluetoothDevice> deviceForCacheQueue;

        private QueueController<BluetoothDevice> deviceForCacheQueueController;

        //Событие, срабатывающее после прочтения всех характеристик.
        public event EventHandler<AfterReadingEventArgs> EventAfterReading;

        //Флаг автоподклбючения устройств
        private bool _isAutoConnect;

        private static BluetoothAPI _instance;
        
        //В конструкторе происходит инициализация Bluetooth адаптера,
        //колбеков, списков и настроек сканирования.
        private BluetoothAPI()
        {
            bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            BluetoothAdapter = bluetoothManager.Adapter;

            connectScanCallback = new CustomScanCallback(scanResult: ConnectScanResult);
            cacheScanCallback = new CustomScanCallback(scanResult: ScanResultForCache);

            filters = new List<ScanFilter>();
           
            _connectedDevices = new Dictionary<string, ConnectDevice>();
            bluetoothDevicesInfo = new Dictionary<string, BluetoothDeviceInfoModel>();

            deviceForCacheQueue = new Queue<BluetoothDevice>();
            deviceForCacheQueueController = new QueueController<BluetoothDevice>(deviceForCacheQueue, QueueStartEventCacheDevice, EveryStepEventCacheDevice, QueueEndedEventCacheDevice);

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

            _isAutoConnect = false;

            bluetoothGattCallback = new CustomBluetoothGattCallback(connectionStateChange: ConnectionStateChange, characteristicRead: CharacteristicRead, servicesDiscovered: ServicesDiscovered);
        }

        

        public static BluetoothAPI GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BluetoothAPI();
            }
            return _instance;
        }

        //Метод автоматического поделючения к устройствам через мак-адреса,
        //полученными в списке.
        public void AutoconnectToDevices(ICollection<AutoconnectDeviceModel> autoconnectDevices)
        {
            if (BluetoothAdapter.IsEnabled)
            {
                if (_scanner == null)
                    _scanner = BluetoothAdapter.BluetoothLeScanner;
                AutoconnectDevices = autoconnectDevices;
                _isAutoConnect = true;
                foreach (var device in AutoconnectDevices)
                {
                    filters.Add(new ScanFilter.Builder().SetDeviceAddress(device.MacAddress).Build());
                }
                _scanner.StartScan(filters: filters, settings: _scanSettingsForDiscovery, callback: connectScanCallback);
            }
        }

        //Метод получения списка найденных устройств
        public void ScanDevices(ICollection<ScannedBluetoothDeviceModel> scannedDevices)
        {
            if (BluetoothAdapter.IsEnabled)
            {
                ScannedDevices = scannedDevices;
                if (_scanner == null)
                    _scanner = BluetoothAdapter.BluetoothLeScanner;
                _scanner.StartScan(filters: null, settings: _scanSettingsForDiscovery, callback: connectScanCallback);
            }
        }

        //Результаты сканирования с фильтром при подключении
        //Метод передаётся в качестве события объекту ScanCallback
        private void ConnectScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            if (result != null) { }
            {
                if (_isAutoConnect)
                {
                    var scannedDevice = AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(result.Device.Address));
                    if (scannedDevice != null)
                    {
                        if (scannedDevice.StatusConnect == 0)
                        {
                            scannedDevice.StatusConnect = 1;
                            Task.Run(() => Connect(result.Device));
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
        }

        private void ScanResultForCache(ScanCallbackType callbackType, ScanResult result)
        {
            if (result != null)
            {
                ConnectDevice connectableDevice;
                if (_connectedDevices.TryGetValue(result.Device.Address, out connectableDevice))
                {
                    if (!connectableDevice.IsDeviceCached)
                    {
                        Connect(result.Device);
                        connectableDevice.IsDeviceCached = true;
                        deviceForCacheQueueController.NextAction();
                    }
                }
            }
            
        }

        //Подключение к устройству, которое передаётся в параметрах
        public void Connect(BluetoothDevice device)
        {
            ConnectDevice connectableDevice;
            if (!_connectedDevices.TryGetValue(device.Address, out connectableDevice))
            {
                _connectedDevices.Add(device.Address, new ConnectDevice { Device = device, IsTryReconnect = true, IsDeviceCached = true, ConnectStatus = 0, CountStepsOfReadTemperature = 0 });
            }
            
            //Проверка на то, находится ли Bluetooth устройство в кеше Bluetooth.
            //Если нет, то это устройство сканируется отдельно,
            //при этом останавливается общее сканирование.
            //Иначе начинается соединение с Bluetooth устройством.
            BluetoothDevice checkedCacheDevice = BluetoothAdapter.GetRemoteDevice(device.Address);
            if (checkedCacheDevice.Type == BluetoothDeviceType.Unknown)
            {
                _connectedDevices[device.Address].IsDeviceCached = false;
                deviceForCacheQueue.Enqueue(checkedCacheDevice);
                if (!deviceForCacheQueueController.IsQueueStarted)
                {
                    deviceForCacheQueueController.StartQueue();
                }
            }
            else
            {
                _connectedDevices[device.Address].Gatt = _connectedDevices[device.Address].Device.ConnectGatt(Application.Context, false, bluetoothGattCallback, transport: BluetoothTransports.Le);
                _connectedDevices[device.Address].ConnectStatus = 1;
            }
            
        }

        //Отключение от устройства и установка значений по умолчанию
        public void Disconnect(string macAddress = null)
        {
            if (macAddress != null && _isAutoConnect) 
            {
                ConnectDevice connectDevice;

                if (_connectedDevices.TryGetValue(macAddress, out connectDevice))
                {
                   
                    
                    if (connectDevice.ConnectStatus != 1)
                        connectDevice.Gatt.Disconnect();
                    else if (connectDevice.ConnectStatus == 1)
                    {
                        Task.Run(async () =>
                        {
                            connectDevice.Gatt.Disconnect();
                            await Task.Delay(100);
                            connectDevice.Gatt.Close();
                        });
                    }
                }
                return;
            }

            bluetoothDevicesInfo.Clear();
            filters.Clear();
            StopScanDevices();

            if (_isAutoConnect)
            {
                _isAutoConnect = false;
            }
           
            if (_connectedDevices.Count != 0)
            {
                foreach (var connectedDevice in _connectedDevices)
                {
                    if (connectedDevice.Value.ConnectStatus == 1)
                    {
                        Task.Run(async () =>
                        {
                            connectedDevice.Value.Gatt.Disconnect();
                            await Task.Delay(100);
                            connectedDevice.Value.Gatt.Close();
                        });
                        
                    }
                    else if (connectedDevice.Value.ConnectStatus == 2)
                    {
                        connectedDevice.Value.Gatt.Disconnect();
                    }
                    
                }
                _connectedDevices.Clear();
            }
        }

        public void StopScanDevices()
        {
            if (_scanner != null)
            {
                deviceForCacheQueue.Clear();
                _scanner.StopScan(connectScanCallback);
                _scanner.StopScan(cacheScanCallback);
            }
        }

        //Метод, выполняющийся при изменении состояния подключения.
        //Он передаётся в качестве события объекту BluetoothGattCallback
        private void ConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            ConnectDevice connectDevice;
            _connectedDevices.TryGetValue(gatt.Device.Address, out connectDevice);
            if (status == GattStatus.Success)
            {
                if (newState == ProfileState.Connected && connectDevice != null) 
                {
                    connectDevice.IsTryReconnect = false;
                    connectDevice.Gatt = gatt;
                    if (_isAutoConnect)
                    {
                        connectDevice.ConnectStatus = 2;
                        AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(gatt.Device.Address)).StatusConnect = 2;
                    }
                    gatt.DiscoverServices();
                }
                else if (newState == ProfileState.Disconnected)
                {
                    gatt.Close();
                }
            }
            else
            {
                if (connectDevice!=null)
                {
                    if (connectDevice.IsTryReconnect)
                    {
                        gatt.Close();
                        connectDevice.Device.ConnectGatt(Application.Context, false, bluetoothGattCallback, BluetoothTransports.Le);
                    }
                    else
                        gatt.Close();
                }
                else
                    gatt.Close();
            }
        }

        private void ServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            Queue<Action> gattReadCharacteristicQueue = new Queue<Action>();
            foreach (var service in gatt.Services)
            {
                foreach (var characteristic in service.Characteristics)
                {
                    switch (characteristic.Uuid.ToString().ToLower().Substring(0, 8))
                    {
                        case "00002a1f":
                        case "00002a19":
                        case "00002a25":
                            gattReadCharacteristicQueue.Enqueue(() => gatt.ReadCharacteristic(characteristic));
                            break;
                        default:
                            break;
                    }
                }
            }
            var connectedDevice = _connectedDevices[gatt.Device.Address];
            connectedDevice.QueueOfReadCharateristics = gattReadCharacteristicQueue;
            if (connectedDevice.QueueOfReadCharateristics.Count > 0)
            {
                bluetoothDevicesInfo.Add(gatt.Device.Address, new BluetoothDeviceInfoModel());
                connectedDevice.QueueOfReadCharateristics.Dequeue().Invoke();
            }
        }

        //Метод, выполняющийся при чтении характеристики.
        //Также сообщает очереди команд о завершении чтения характеристики.
        //Он передаётся в качестве события объекту BluetoothGattCallback.
        private void CharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            byte[] buffer = new byte[characteristic.GetValue().Length];
            Array.Copy(characteristic.GetValue(), 0, buffer, 0, characteristic.GetValue().Length);
            
            string value = Encoding.UTF8.GetString(buffer);
            string uuidHeader = characteristic.Uuid.ToString().ToLower().Substring(0, 8);

            var connectedDevice = _connectedDevices[gatt.Device.Address];

            switch (uuidHeader)
            {
                case "00002a1f":
                    if (value == "-127.00")
                    {
                        if (connectedDevice.CountStepsOfReadTemperature < 2)
                        {
                            connectedDevice.QueueOfReadCharateristics.Enqueue(() => gatt.ReadCharacteristic(characteristic));
                            connectedDevice.CountStepsOfReadTemperature++;
                        }
                        else
                        {
                            bluetoothDevicesInfo[gatt.Device.Address].Temperature = "Ошибка";
                            if (_isAutoConnect)
                                AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(gatt.Device.Address)).Temperature = "Ошибка";
                            else
                                DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Температура", Value = "Ошибка", UUID = uuidHeader });
                        }
                        
                    }
                    else
                    {
                        bluetoothDevicesInfo[gatt.Device.Address].Temperature = value;
                        if (_isAutoConnect)
                            AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(gatt.Device.Address)).Temperature = value;
                        else
                            DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Температура", Value = value + "°C", UUID = uuidHeader });
                    }
                    break;
                case "00002a19":
                    if (_isAutoConnect)
                        AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(gatt.Device.Address)).BatteryLevel = value;
                    else
                        DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Заряд батареи", Value = value + "%", UUID = uuidHeader });
                    break;
                case "00002a25":
                    bluetoothDevicesInfo[gatt.Device.Address].Id = value;
                    if(_isAutoConnect)
                        AutoconnectDevices.FirstOrDefault(p => p.MacAddress.Equals(gatt.Device.Address)).SerialNumber = value;
                    else
                        DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "Серийный номер", Value = value, UUID = uuidHeader });
                    break;
                default:
                    DeviceCharacterisctics.Add(new DeviceCharacteristicModel { Name = "N/A", Value = value, UUID = uuidHeader });
                    break;
            }

            
            
            if (connectedDevice.QueueOfReadCharateristics.Count > 0)
                connectedDevice.QueueOfReadCharateristics.Dequeue().Invoke();
            else
            {
                connectedDevice.QueueOfReadCharateristics.Clear();
                var deviceInfo = bluetoothDevicesInfo[gatt.Device.Address];
                CompositeAndSendJson(deviceInfo);
                EventAfterReading?.Invoke(this, new AfterReadingEventArgs { DeviceCharacteristics = DeviceCharacterisctics});
                bluetoothDevicesInfo.Remove(gatt.Device.Address);
            }
        }

        //Метод отправки данных на сервер.
        private void CompositeAndSendJson(BluetoothDeviceInfoModel bluetoothDeviceInfo)
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

        private void QueueStartEventCacheDevice(BluetoothDevice device)
        {
            _scanner.StopScan(connectScanCallback);
            _scanner.StartScan(
                new List<ScanFilter> { new ScanFilter.Builder().SetDeviceAddress(device.Address).Build() },
                _scanSettingsForCache,
                cacheScanCallback);
        }

        private void EveryStepEventCacheDevice(BluetoothDevice device)
        {
            _scanner.StopScan(cacheScanCallback);
            _scanner.StartScan(
                new List<ScanFilter> { new ScanFilter.Builder().SetDeviceAddress(device.Address).Build() },
                _scanSettingsForCache,
                cacheScanCallback);
        }

        private void QueueEndedEventCacheDevice()
        {
            _scanner.StopScan(cacheScanCallback);
            _scanner.StartScan(filters: filters, settings: _scanSettingsForDiscovery, callback: connectScanCallback);
        }
    }
}
