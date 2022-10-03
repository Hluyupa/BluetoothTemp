using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using BluetoothTemp.Views;
using Java.Util;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BluetoothTemp.ViewModels
{
    public class MainPageVM : INotifyPropertyChanged
    {
        //BluetoothManager нужен для получения объекта BluetoothAdapter,
        //а также для общего управления.
        private BluetoothManager _bluetoothManager;
        
        //BluetoothAdapter необходим для выполнения основных
        //задач Bluetooth (получение списка сопряжённых устройств,
        //поиск других устройств).
        private BluetoothAdapter _bluetoothAdapter;

        //Команда подключения
        public ICommand Connect { get; set; }

        //Список найденных устройств.
        public ObservableCollection<BluetoothDevice> BluetoothDevicesList { get; set; }

        

        //Флаг для проверки подключения (временно)
        private string connectFlag;
        public string ConnectFlag
        {
            get 
            {
                return connectFlag; 
            }
            set 
            { 
                connectFlag = value;
                OnPropertyChanged();
            }
        }


        //Выбранное устрйоство для подключения
        private BluetoothDevice selectedBluetoothDevice;
        public BluetoothDevice SelectedBluetoothDevice
        {
            get 
            {
                return selectedBluetoothDevice; 
            }
            set 
            {
                if (selectedBluetoothDevice != value)
                {
                    selectedBluetoothDevice = value;
                    OpenBluetoothDevicePage();
                    OnPropertyChanged();
                    
                }
            }
        }

        public MainPageVM()
        {
            _bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            _bluetoothAdapter = _bluetoothManager.Adapter;
            _bluetoothAdapter.Enable();
            BluetoothDevicesList = new ObservableCollection<BluetoothDevice>();
            BluetoothLeScanner scanner = _bluetoothAdapter.BluetoothLeScanner;
            CustomScanCallback scanCallback = new CustomScanCallback(BluetoothDevicesList);
            if (scanner != null) 
            {
                ScanSettings scanSettings = new ScanSettings.Builder()
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                .SetCallbackType(ScanCallbackType.AllMatches)
                .SetMatchMode(BluetoothScanMatchMode.Aggressive)
                .SetNumOfMatches(1)
                .SetReportDelay(1)
                .Build();
                scanner.StartScan(filters: null, settings: scanSettings, callback: scanCallback);
            }
            
        }
        
        /*private async void EnableBluetooth()
        {
            BluetoothDevicesList.Clear();
            IBluetoothLE ble = CrossBluetoothLE.Current;
           
            ConnectFlag = ble.State.ToString();
            IAdapter adapter = CrossBluetoothLE.Current.Adapter;
            
            adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.LowLatency;
            ble.StateChanged += (s, e) =>
            {
                ConnectFlag = $"{e.NewState}";
            };
            adapter.DeviceDiscovered += (s, a) =>
            {
                BluetoothDevicesList.Add(a.Device);
            };
            await adapter.StartScanningForDevicesAsync();
        }*/
        
        private async void OpenBluetoothDevicePage()
        {
            await App.Current.MainPage.Navigation.PushAsync(
                new BluetoothDevicePage
                {
                    BindingContext = new BluetoothDevicePageVM(SelectedBluetoothDevice)
                }
            );
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
