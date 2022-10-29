using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Models.EFModels
{

    public class BluetoothDeviceWasСonnected
    {
        public int Id { get; set; }
        public string MacAddress { get; set; }
        public int IsAutoconnect{ get; set; }
        public int IsNfcWrited{ get; set; }
    }
}
