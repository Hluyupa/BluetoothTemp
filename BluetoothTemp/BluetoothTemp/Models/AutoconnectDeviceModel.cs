using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace BluetoothTemp.Models
{
    public class AutoconnectDeviceModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string MacAddress { get; set; }
        

        private int statusConnect;
        public int StatusConnect
        {
            get 
            {
                return statusConnect; 
            }
            set 
            {
                statusConnect = value; 
                OnPropertyChanged();
            }
        }

        private string temperature;
        public string Temperature
        {
            get
            {
                return temperature;
            }
            set
            {
                temperature = value;
                OnPropertyChanged();
            }
        }

        private string batteryLevel;
        public string BatteryLevel
        {
            get
            {
                return batteryLevel;
            }
            set
            {
                batteryLevel = value;
                OnPropertyChanged();
            }
        }

        private string serialNumber;
        public string SerialNumber
        {
            get
            {
                return serialNumber;
            }
            set
            {
                serialNumber = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
