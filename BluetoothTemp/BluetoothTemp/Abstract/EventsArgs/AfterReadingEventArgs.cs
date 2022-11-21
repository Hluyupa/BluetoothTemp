using BluetoothTemp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Abstract.EventsArgs
{
    public class AfterReadingEventArgs : EventArgs
    {
        public ICollection<DeviceCharacteristicModel> DeviceCharacteristics { get; set; }
    }
}
