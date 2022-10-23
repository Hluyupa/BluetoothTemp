using System;
using System.Collections.Generic;
using System.Text;
using Android.Nfc;
using Java.Interop;
using static Android.Nfc.NfcAdapter;

namespace BluetoothTemp.TelephoneServices.NFC
{
    public class CustomReaderCallback : Java.Lang.Object, IReaderCallback
    {
        public void OnTagDiscovered(Tag tag)
        {
            
        }
    }


}
