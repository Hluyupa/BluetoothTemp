using Android.Bluetooth;

using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using Java.IO;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;


namespace BluetoothTemp.ViewModels
{
    public class BluetoothDevicePageVM : INotifyPropertyChanged, IDisposable
    {
        //Событие уничтожения объекта
        public event Action DisposeEvent;

        private BluetoothAPI bluetoothAPI;
        public ObservableCollection<DeviceCharacteristicModel> DeviceCharacteristicsList { get; set; }

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

        private string outputInfo;
        public string OutputInfo
        {
            get
            {
                return outputInfo;
            }
            set
            {
                outputInfo = value;
                OnPropertyChanged();
            }
        }

        private string inputInfo;
        public string InputInfo
        {
            get
            {
                return inputInfo;
            }
            set
            {
                inputInfo = value;
                OnPropertyChanged();
            }
        }

        //Команда отправки сообщения
        public ICommand SendMessage { get; set; }

        public BluetoothDevicePageVM(BluetoothDevice bluetoothDevice)
        {
            bluetoothAPI = BluetoothAPI.GetInstance();
            DeviceCharacteristicsList = new ObservableCollection<DeviceCharacteristicModel>();
            bluetoothAPI.DeviceCharacterisctics = DeviceCharacteristicsList;
            BluetoothDevice = bluetoothDevice;
            ConnectionInfo = "Waiting connection";
            bluetoothAPI.Connect(BluetoothDevice);
            //bluetoothAPI.Read();
            
            /*SendMessage = new Command(Write);
            ConnectBluetoothDevice();*/
            
            
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Dispose()
        {
            bluetoothAPI.Disconnect();
            DisposeEvent();
        }
    }
}
