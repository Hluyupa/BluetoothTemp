using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using BluetoothTemp.Views;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace BluetoothTemp.ViewModels
{
    public class MainPageVM : BaseViewModel
    {
        //Объект класса для работы с Bluetooth
        private BluetoothAPI _bluetoothAPI;

        //Команда для поиска bluetooth устройств
        //public ICommand ScanDevicesCommand { get; set; }

        //Команда вкл/выкл bluetooth
        public ICommand OnOffBluetoothCommand { get; set; }
        public ICommand StartScanNfcCommand { get; set; }
       
        public ICommand OpenAutoconnectDevicesPageCommand { get; set; }
        public ICommand OpenDiscoverDevicesPageCommand { get; set; }

        //Список найденных устройств.
        

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


        

        public MainPageVM()
        {
            _bluetoothAPI = BluetoothAPI.GetInstance();

            OnOffBluetoothText = _bluetoothAPI.bluetoothAdapter.IsEnabled ? OnOffBluetoothText = "Off Bluetooth" : OnOffBluetoothText = "On Bluetooth";
            

            OpenAutoconnectDevicesPageCommand = new Command(OpenAutoconnectDevicesPage);
            OpenDiscoverDevicesPageCommand = new Command(OpenDiscoverDevicesPage);

            

            OnOffBluetoothCommand = new Command(() => {
                _bluetoothAPI.OnOffBluetooth(() => OnOffBluetoothText = "Off Bluetooth", () => OnOffBluetoothText = "On Bluetooth");
            });
            //ScanDevicesCommand = new Command(() => );

            StartScanNfcCommand = new Command(OpenNfcReaderPage);
        }

        

        private async void OpenAutoconnectDevicesPage()
        {
            await App.Current.MainPage.Navigation.PushAsync(
                new AutoconnectDevicesPage
                {
                    BindingContext = new AutoconnectDevicesPageVM()
                }
            );
        }

        private async void OpenDiscoverDevicesPage()
        {
            await App.Current.MainPage.Navigation.PushAsync(
                new DiscoverDevicesPage
                {
                    BindingContext = new DiscoverDevicesPageVM()
                }
            );
        }

        private async void OpenNfcReaderPage()
        {
            await App.Current.MainPage.Navigation.PushAsync(
                new NfcReaderPage
                {
                    BindingContext = new NfcReaderPageVM()
                }
            );
        }
    }
}
