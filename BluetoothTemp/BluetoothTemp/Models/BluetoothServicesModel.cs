using Android.Bluetooth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BluetoothTemp.Models
{
    public class BluetoothServicesModel
    {
        BluetoothGattService Service { get; set; }
        ObservableCollection<BluetoothGattCharacteristic> Characteristics { get; set; }
    }
}
