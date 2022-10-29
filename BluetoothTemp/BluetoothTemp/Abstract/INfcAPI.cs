using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Abstract
{
    public interface INfcAPI
    {
        event Action<string> ReadingNfcEvent;

        event Action WritingNfcEvent;
        void StartScanning();
        void StopScanning();
        void Read();
        void Write(string message);
    }
}
