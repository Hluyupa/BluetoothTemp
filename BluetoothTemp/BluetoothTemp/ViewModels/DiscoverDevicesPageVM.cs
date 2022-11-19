using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using BluetoothTemp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace BluetoothTemp.ViewModels
{
    public class DiscoverDevicesPageVM : BaseViewModel, INavigationVM
    {
        public ICommand TappedBluetoothDeviceCommand { get; set; }
        public ObservableCollection<ScannedBluetoothDeviceModel> ScannedBluetoothDevicesList { get; set; }

        

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

        private bool bluetoothState;
        public bool BluetoothState
        {
            get
            {
                return bluetoothState;
            }
            set
            {
                bluetoothState = value;
                OnPropertyChanged();
            }
        }

        private BluetoothAPI _bluetoothAPI;

        private IBluetoothStateChanged _bluetoothStateChangedReciever;

        public DiscoverDevicesPageVM()
        {
            _bluetoothAPI = BluetoothAPI.GetInstance();
            _bluetoothStateChangedReciever = DependencyService.Get<IBluetoothStateChanged>();
            ScannedBluetoothDevicesList = new ObservableCollection<ScannedBluetoothDeviceModel>();
            TappedBluetoothDeviceCommand = new Command(OpenBluetoothDevicePage);
        }

        private async void OpenBluetoothDevicePage()
        {
            await App.Current.MainPage.Navigation.PushAsync(
                new BluetoothDevicePage
                {
                    BindingContext = new BluetoothDevicePageVM(SelectedBluetoothDevice.Device)
                }
            );
        }

        public void OnAppearing()
        {
            BluetoothState = _bluetoothAPI.BluetoothAdapter.IsEnabled;

            _bluetoothStateChangedReciever.SetOnBluetoothEvent(() =>
            {
                BluetoothState = true;
                _bluetoothAPI.ScanDevices(ScannedBluetoothDevicesList);
            });
            _bluetoothStateChangedReciever.SetOffBluetoothEvent(() =>
            {
                BluetoothState = false;
                _bluetoothAPI.Disconnect();
                ScannedBluetoothDevicesList.Clear();
            });

            if (BluetoothState)
            {
                _bluetoothAPI.ScanDevices(ScannedBluetoothDevicesList);
            }
        }

        public void OnDisappearing()
        {
            _bluetoothStateChangedReciever.ClearEvents();
            ScannedBluetoothDevicesList.Clear();
            _bluetoothAPI.StopScanDevices();
        }
    }
}
