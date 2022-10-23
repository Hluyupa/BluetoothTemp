
using Android.Content;
using Android.Nfc;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.TelephoneServices.NFC
{
    public class NfcAPI
    {
        private NfcAdapter _nfcAdapter;
        
        private CustomReaderCallback readerCallback;
        
        public NfcAPI()
        {
            readerCallback = new CustomReaderCallback();
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(Android.App.Application.Context);
            _nfcAdapter.EnableReaderMode(null, readerCallback, NfcReaderFlags.SkipNdefCheck, null);
        }

    }
}
