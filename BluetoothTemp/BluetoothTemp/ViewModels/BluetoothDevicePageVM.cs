using Android.Bluetooth;
using Java.IO;
using Java.Util;
using System;
using System.Collections.Generic;
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
       
        //UUID Bluetooth устройства
        private const string _UUID = "00001101-0000-1000-8000-00805F9B34FB";

        //Сокет по которому будет происходить "общение" с
        //Bluetooth устройством
        private BluetoothSocket _socket;

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

        //Команда отправки сообщения
        public ICommand SendMessage { get; set; }

        public BluetoothDevicePageVM(BluetoothDevice bluetoothDevice)
        {
            BluetoothDevice = bluetoothDevice;
            ConnectionInfo = "Waiting connection";
            SendMessage = new Command(Write);
            ConnectBluetoothDevice();
        }

        //
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



        //Метод подключения к Bluetooth устройству
        private async void ConnectBluetoothDevice()
        {
            _socket = BluetoothDevice.CreateInsecureRfcommSocketToServiceRecord(UUID.FromString(_UUID));
            try
            {
                await _socket.ConnectAsync();
                ConnectionInfo = "Connection success";
                Read();
            }
            catch (Exception ex)
            {
                _socket.Close();
                ConnectionInfo = "Error connection";
            }
        }

        private async void Write()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(InputInfo);
            await _socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        private async void Read()
        {
            byte[] buffer;   
            while (_socket.IsConnected)
            {
                try
                {
                    buffer = new byte[8196];
                    await _socket.InputStream.ReadAsync(buffer, 0, buffer.Length);
                    OutputInfo += Encoding.UTF8.GetString(buffer);
                }
                catch
                {
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public void Dispose()
        {
            _socket.Close();
        }
    }
}
