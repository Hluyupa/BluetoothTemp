using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace BluetoothTemp.Models
{
    public class DeviceCharacteristicModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        private string value;
        public string Value
        {
            get 
            { 
                return value;
            }
            set
            {
                this.value = value; 
                OnPropertyChanged(); 
            }
        }

      
        public string UUID { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
