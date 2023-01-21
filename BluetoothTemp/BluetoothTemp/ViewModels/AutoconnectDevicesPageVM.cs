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

        private bool isAutoconnectDeviceDetected;
        public bool IsAutoconnectDeviceDetected
        {
            get
            {
                return isAutoconnectDeviceDetected;
            }
            set
            {
                isAutoconnectDeviceDetected = value;
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
                if (devices != null && devices.Count != 0)
                {
                    IsAutoconnectDeviceDetected = true;
                    foreach (var device in devices)
                    {
                        AutoconnectDevicesList.Add(new AutoconnectDeviceModel { Name = device.Name, MacAddress = device.MacAddress, StatusConnect = 0 });
                    }
                }
                else
                {
                    IsAutoconnectDeviceDetected = false;
                }
            }
            if (AutoconnectDevicesList.Count != 0)
                _bluetoothAPI.AutoconnectToDevices(AutoconnectDevicesList);

            
        }

        public async void DeleteDevice(string macAddress)
        {
            using (var context = new ApplicationContext(_dbPath))
            {
                var removedDevice = AutoconnectDevicesList.FirstOrDefault(p => p.MacAddress == macAddress);

                var responce = await App.Current.MainPage.DisplayAlert("Удаление", "Вы уверены, что хотите удалить выбранное устройство из списка?", "Да", "Нет");
                if (responce && removedDevice.StatusConnect != 1)
                {
                    _bluetoothAPI.Disconnect(macAddress);
                    AutoconnectDevicesList.Remove(AutoconnectDevicesList.FirstOrDefault(p => p.MacAddress == macAddress));

                    if (AutoconnectDevicesList.Count != 0)
                        _bluetoothAPI.AutoconnectToDevices(AutoconnectDevicesList);
                    else
                        IsAutoconnectDeviceDetected = false;

                    context.BluetoothDevicesWasСonnected.Remove(context.BluetoothDevicesWasСonnected.FirstOrDefault(p => p.MacAddress == macAddress));
                    context.SaveChanges();                    
                }
                else if (removedDevice.StatusConnect == 1) 
                {
                    await App.Current.MainPage.DisplayAlert("Ошибка", "Невозможно удалить устройство из списка во время подключения", "Ок");
                }
            }
        }

        private void OnBluetoothEventHandler(object sender, EventArgs args)
        {
            BluetoothState = true;
            ShowAutoconnectDevices();
        }

        private void OffBluetoothEventHandler(object sender, EventArgs args)
        {
            BluetoothState = false;
            _bluetoothAPI.Disconnect();
            AutoconnectDevicesList.Clear();
        }

        public void OnAppearing()
        {
            BluetoothState = _bluetoothAPI.BluetoothAdapter.IsEnabled;
            _bluetoothStateChangedReciever.SetOnBluetoothEvent(OnBluetoothEventHandler);
            _bluetoothStateChangedReciever.SetOffBluetoothEvent(OffBluetoothEventHandler);

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
