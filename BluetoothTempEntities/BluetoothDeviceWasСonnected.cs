using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTempEntities
{

    public class BluetoothDeviceWasСonnected
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MacAddress { get; set; }
        public int IsAutoconnect{ get; set; }
        public int IsNfcWrited{ get; set; }
        public int? SerialNumber{ get; set; }
    }
}
