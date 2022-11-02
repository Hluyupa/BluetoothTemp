using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using BluetoothTemp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace BluetoothTemp.ViewModels
{
    public class DiscoverDevicesPageVM : BaseViewModel
    {
        public ICommand TappedBluetoothDeviceCommand { get; set; }
        public ObservableCollection<ScannedBluetoothDeviceModel> ScannedBluetoothDevicesList { get; set; }

        private BluetoothAPI _bluetoothAPI;

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
                    OnPropertyChanged();
                }
            }
        }

        public DiscoverDevicesPageVM()
        {
            _bluetoothAPI = BluetoothAPI.GetInstance();
            ScannedBluetoothDevicesList = new ObservableCollection<ScannedBluetoothDeviceModel>();
            TappedBluetoothDeviceCommand = new Command(OpenBluetoothDevicePage);

            _bluetoothAPI.GetScanDevices(ScannedBluetoothDevicesList);
        }

        private async void OpenBluetoothDevicePage()
        {
            var context = new BluetoothDevicePageVM(SelectedBluetoothDevice.Device);

            context.DisposeEvent = () => _bluetoothAPI.GetScanDevices(ScannedBluetoothDevicesList);

            await App.Current.MainPage.Navigation.PushAsync(
                new BluetoothDevicePage
                {
                    BindingContext = context
                }
            );
        }
    }
}
