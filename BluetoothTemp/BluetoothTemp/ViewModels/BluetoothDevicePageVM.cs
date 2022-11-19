using Android.Bluetooth;
using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using BluetoothTempEntities;
using Java.IO;
using Java.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;


namespace BluetoothTemp.ViewModels
{
    public class BluetoothDevicePageVM : BaseViewModel, IDisposable
    {
        //Событие уничтожения объекта
        public Action DisposeEvent;
        
        private BluetoothAPI bluetoothAPI;
        public ObservableCollection<DeviceCharacteristicModel> deviceCharacteristicsList;
        public ObservableCollection<DeviceCharacteristicModel> DeviceCharacteristicsList 
        { 
            get 
            {
                return deviceCharacteristicsList;
            } 
            set
            {
                deviceCharacteristicsList = value;
                OnPropertyChanged();
            } 
        }

        //Информация о соединении с Bluetooth устройством
        private string connectionInfo;
        public string ConnectionInfo
        {
            get 
            { 
                return connectionInfo; 
            }
            set 
            {
                connectionInfo = value; 
                OnPropertyChanged(); 
            }
        }

        //Выбранное Bluetooth устройство для пожключения
        private BluetoothDevice bluetoothDevice;
        public BluetoothDevice BluetoothDevice
        {
            get 
            {
                return bluetoothDevice;
            }
            set 
            {
                bluetoothDevice = value;
                OnPropertyChanged();
            }
        }

        private bool isAutoconnect;
        public bool IsAutoconnect
        {
            get 
            { 
                return isAutoconnect; 
            }
            set 
            { 
                isAutoconnect = value;
                OnPropertyChanged();
                if (!isPageLoading)
                {
                    using (var context = new ApplicationContext(_dbPath)) 
                    {
                        var device = context.BluetoothDevicesWasСonnected.FirstOrDefault(p => p.MacAddress == this.BluetoothDevice.Address);
                        if (isAutoconnect)
                            device.IsAutoconnect = Convert.ToInt32(true);
                        else
                            device.IsAutoconnect = Convert.ToInt32(false);
                        context.SaveChanges();
                    }
                }
                isPageLoading = false;
            }
        }


        //Команда отправки сообщения
        public ICommand SendMessage { get; set; }

        private string _dbPath;

        private bool isPageLoading;
        public BluetoothDevicePageVM(BluetoothDevice bluetoothDevice)
        {
            isPageLoading = true;

            bluetoothAPI = BluetoothAPI.GetInstance();
            DeviceCharacteristicsList = new ObservableCollection<DeviceCharacteristicModel>();
            bluetoothAPI.DeviceCharacterisctics = DeviceCharacteristicsList;
            BluetoothDevice = bluetoothDevice;

            ConnectionInfo = "Waiting connection";
            bluetoothAPI.Connect(BluetoothDevice);
            bluetoothAPI.EventAfterReading += AfterReadingInfo;
            App.NfcAPI.WritingNfcEvent += AfterWritingNfc;

            _dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DbFilename);
            using (var context = new ApplicationContext(_dbPath))
            {
                var bluetotohDeviceWasConnected = context.BluetoothDevicesWasСonnected.FirstOrDefault(p => p.MacAddress == this.BluetoothDevice.Address);
                if (bluetotohDeviceWasConnected == null)
                {
                    var device = new BluetoothDeviceWasСonnected { Name = this.BluetoothDevice.Name, MacAddress = bluetoothDevice.Address, IsAutoconnect = 0, IsNfcWrited = 0 };
                    context.BluetoothDevicesWasСonnected.Add(device);
                    context.SaveChanges();
                    isPageLoading = false;
                }
                else
                {
                    IsAutoconnect = Convert.ToBoolean(bluetotohDeviceWasConnected.IsAutoconnect);
                }
            }
        }

        private void AfterReadingInfo()
        {
            using (var context = new ApplicationContext(_dbPath))
            {
                var device = context.BluetoothDevicesWasСonnected.FirstOrDefault(p => p.MacAddress == BluetoothDevice.Address);
            
                if (Convert.ToBoolean(device.IsNfcWrited))
                {
                    return;
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        bool request = await App.Current.MainPage.Navigation.NavigationStack.Last().DisplayAlert("NFC", "Вы хотите записать серийный номер устройства в NFC метку", "Да", "Нет");
                        if (request)
                        {
                            App.NfcAPI.StartScanning();
                            App.NfcAPI.Write(DeviceCharacteristicsList.FirstOrDefault(p => p.Name.Equals("Серийный номер")).Value);
                            bool canceled = await App.Current.MainPage.Navigation.NavigationStack.Last().DisplayAlert("NFC", "Поднесите устройство к NFC метке для записи", null, "Отмена");
                            if (canceled)
                            {
                                App.NfcAPI.StopScanning();
                            }
                            
                        }
                    });
                }
            }
        }

        private void AfterWritingNfc()
        {
            using (var context = new ApplicationContext(_dbPath))
            {
                var device = context.BluetoothDevicesWasСonnected.FirstOrDefault(p => p.MacAddress == BluetoothDevice.Address);
                device.IsNfcWrited = Convert.ToInt32(true);
                context.SaveChanges();
            }
            Device.BeginInvokeOnMainThread(async () =>
            {
                await App.Current.MainPage.Navigation.NavigationStack.Last().DisplayAlert("NFC", "Запись произошла успешна", "Ок");
            });
            App.NfcAPI.WritingNfcEvent -= AfterWritingNfc;
        }

        public void Dispose()
        {
            bluetoothAPI.EventAfterReading -= AfterReadingInfo;
            bluetoothAPI.Disconnect();
            DisposeEvent?.Invoke();
        }
    }
}
