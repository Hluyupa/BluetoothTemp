using Android.Bluetooth;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Models
{
    public class ScannedBluetoothDeviceModel
    {
        public string Name { get; set; }
        public BluetoothDevice Device { get; set; }
    }
}
