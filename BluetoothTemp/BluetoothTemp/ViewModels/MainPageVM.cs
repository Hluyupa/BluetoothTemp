using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using BluetoothTemp.Views;
using Java.Util;
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
        //Объект класса для работы с Bluetooth
        private BluetoothAPI _bluetoothAPI;

        //Команда для поиска bluetooth устройств
        public ICommand ScanDevicesCommand { get; set; }

        //Команда вкл/выкл bluetooth
        public ICommand OnOffBluetoothCommand { get; set; }

        //Список найденных устройств.
        public ObservableCollection<ScannedBluetoothDeviceModel> ScannedBluetoothDevicesList { get; set; }

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
        private ScannedBluetoothDeviceModel selectedBluetoothDevice;
        public ScannedBluetoothDeviceModel SelectedBluetoothDevice
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
            _bluetoothAPI = BluetoothAPI.GetInstance();

            OnOffBluetoothText = _bluetoothAPI.bluetoothAdapter.IsEnabled ? OnOffBluetoothText = "Off Bluetooth" : OnOffBluetoothText = "On Bluetooth";
            ScannedBluetoothDevicesList = new ObservableCollection<ScannedBluetoothDeviceModel>();

            OnOffBluetoothCommand = new Command(() => {
                _bluetoothAPI.OnOffBluetooth(() => OnOffBluetoothText = "Off Bluetooth", () => OnOffBluetoothText = "On Bluetooth");
            });
            ScanDevicesCommand = new Command(() => _bluetoothAPI.GetScanDevices(ScannedBluetoothDevicesList));
        }
        private async void OpenBluetoothDevicePage()
        {
            var context = new BluetoothDevicePageVM(SelectedBluetoothDevice.Device);
            context.DisposeEvent += () => _bluetoothAPI.GetScanDevices(ScannedBluetoothDevicesList); 
            await App.Current.MainPage.Navigation.PushAsync(
                new BluetoothDevicePage
                {
                    BindingContext = context
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
