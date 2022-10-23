using BluetoothTemp.TelephoneServices.NFC;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.ViewModels
{
    public class NfcReaderPageVM
    {
        private NfcAPI nfcAPI;
        public NfcReaderPageVM()
        {
            nfcAPI = new NfcAPI();
        }
    }
}
