using Android.Bluetooth;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Models
{
    public class ConnectDevice
    {
        public BluetoothDevice Device { get; set; }
        public bool IsTryReconnect { get; set; }
        public bool IsDeviceCached { get; set; }
        public Queue<Action> QueueOfReadCharateristics { get; set; }
        public BluetoothGatt Gatt { get; set; }
        public int ConnectStatus { get; set; }
    }
}
