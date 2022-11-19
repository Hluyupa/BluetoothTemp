using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using BluetoothTemp.Views;
using BluetoothTempEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace BluetoothTemp.ViewModels
{
    public class AutoconnectDevicesPageVM : BaseViewModel, INavigationVM
    {

        public ObservableCollection<AutoconnectDeviceModel> AutoconnectDevicesList { get; set; }

        public ICommand DeleteDeviceCommand { get; set; }

        private AutoconnectDeviceModel selectedAutoconnectDevice;
        public AutoconnectDeviceModel SelectedAutoconnectDevice
        {
            get 
            { 
                return selectedAutoconnectDevice;
            }
            set 
            { 
                selectedAutoconnectDevice = value; 
                OnPropertyChanged();
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


        private bool isBluetoothEnabled;
        public bool IsBluetoothEnabled
        {
            get 
            {
                return isBluetoothEnabled;
            }
            set 
            {
                isBluetoothEnabled = value;
                OnPropertyChanged();
            }
        }

        private IBluetoothStateChanged _bluetoothStateChangedReciever;

        private BluetoothAPI _bluetoothAPI;

        private string _dbPath;
        public AutoconnectDevicesPageVM()
        {
            DeleteDeviceCommand = new Command<string>(DeleteDevice);
            _bluetoothAPI = BluetoothAPI.GetInstance();
            _dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DbFilename);
            _bluetoothStateChangedReciever = DependencyService.Get<IBluetoothStateChanged>();
            AutoconnectDevicesList = new ObservableCollection<AutoconnectDeviceModel>();
           
        }



        public void ShowAutoconnectDevices()
        {
            using (var context = new ApplicationContext(_dbPath))
            {
                var devices = context.BluetoothDevicesWasСonnected.Where(p => p.IsAutoconnect == Convert.ToInt32(true)).ToList();
                if (devices != null)
                {
                    foreach (var device in devices)
                    {
                        AutoconnectDevicesList.Add(new AutoconnectDeviceModel { Name = device.Name, MacAddress = device.MacAddress, StatusConnect = 0 });
                    }
                }
            }
            if (AutoconnectDevicesList.Count != 0)
                _bluetoothAPI.AutoconnectToDevices(AutoconnectDevicesList);

            
        }

        public void DeleteDevice(string macAddress)
        {
            using (var context = new ApplicationContext(_dbPath))
            {
                _bluetoothAPI.Disconnect(macAddress);
                AutoconnectDevicesList.Remove(AutoconnectDevicesList.FirstOrDefault(p => p.MacAddress == macAddress));
                context.BluetoothDevicesWasСonnected.Remove(context.BluetoothDevicesWasСonnected.FirstOrDefault(p => p.MacAddress == macAddress));
                context.SaveChanges();
            }
            
            
        }

        public void OnAppearing()
        {
            BluetoothState = _bluetoothAPI.BluetoothAdapter.IsEnabled;
            _bluetoothStateChangedReciever.SetOnBluetoothEvent(() =>
            {
                BluetoothState = true;
                ShowAutoconnectDevices();
            });
            _bluetoothStateChangedReciever.SetOffBluetoothEvent(() =>
            {
                BluetoothState = false;
                _bluetoothAPI.Disconnect();
                AutoconnectDevicesList.Clear();
            });

            if (BluetoothState)
            {
                ShowAutoconnectDevices();
            }
        }

        public void OnDisappearing()
        {
            _bluetoothStateChangedReciever.ClearEvents();
            AutoconnectDevicesList.Clear();
            _bluetoothAPI.Disconnect();
        }
    }
}
