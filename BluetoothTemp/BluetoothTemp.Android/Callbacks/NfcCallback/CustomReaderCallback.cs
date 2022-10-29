using System;
using System.Collections.Generic;
using System.Text;
using Android.Nfc;
using Java.Interop;
using static Android.Nfc.NfcAdapter;

namespace BluetoothTemp.Droid.Callbacks.NfcCallback
{
    public class CustomReaderCallback : Java.Lang.Object, IReaderCallback
    {
        public event Action<Tag> TagDiscoveredEvent;
        public void OnTagDiscovered(Tag tag)
        {
            TagDiscoveredEvent.Invoke(tag);
        }
    }
}
