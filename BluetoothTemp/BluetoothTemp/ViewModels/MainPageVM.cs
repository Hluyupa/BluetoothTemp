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

        private BluetoothLeScanner _scanner;

        private CustomScanCallback _scanCallback;

        //Команда подключения
        public ICommand Connect{ get; set; }
        public ICommand ScanDevicesCommand { get; set; }
        public ICommand OnOffBluetoothCommand { get; set; }

        //Список найденных устройств.
        public ObservableCollection<BluetoothDevice> BluetoothDevicesList { get; set; }



        
        private string onOffBluetoothText;
        public string OnOffBluetoothText
        {
            get 
            {
                return onOffBluetoothText; 
            }
            set 
            { 
                onOffBluetoothText = value;
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

            OnOffBluetoothText = _bluetoothAdapter.IsEnabled ? OnOffBluetoothText = "Off Bluetooth" : OnOffBluetoothText = "On Bluetooth";
            OnOffBluetoothCommand = new Command(OnOffBluetooth);

            ScanDevicesCommand = new Command(ScanDevices);

            //_bluetoothAdapter.Enable();

            BluetoothDevicesList = new ObservableCollection<BluetoothDevice>();
            
            
            //ConnectFlag = "asd";
        }

        private void OnOffBluetooth()
        {
            if (_bluetoothAdapter.IsEnabled)
            {
                _bluetoothAdapter.Disable();
                OnOffBluetoothText = "On Bluetooth";
            }
            else
            {
                _bluetoothAdapter.Enable();
                OnOffBluetoothText = "Off Bluetooth";
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

        private void ScanDevices()
        {
            if (_bluetoothAdapter.IsEnabled)
            {
                _scanner = _bluetoothAdapter.BluetoothLeScanner;
                _scanCallback = new CustomScanCallback(BluetoothDevicesList);
                if (_scanner != null)
                {
                    ScanSettings scanSettings = new ScanSettings.Builder()
                    .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                    .SetCallbackType(ScanCallbackType.AllMatches)
                    .SetMatchMode(BluetoothScanMatchMode.Sticky)
                    .SetNumOfMatches(1)
                    .SetReportDelay(1)
                    .Build();
                    _scanner.StartScan(filters: null, settings: scanSettings, callback: _scanCallback);
                }
            }
        }

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
