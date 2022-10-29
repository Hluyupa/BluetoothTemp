
using BluetoothTemp.Abstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.ViewModels
{
    public class NfcReaderPageVM : BaseViewModel, IDisposable
    {
        
        private string nfcData;
        public string NfcData
        {
            get 
            {
                return nfcData; 
            }
            set 
            { 
                nfcData = value;
                OnPropertyChanged();
            }
        }

        public NfcReaderPageVM()
        {
            App.NfcAPI.StartScanning();
            App.NfcAPI.Read();
            App.NfcAPI.ReadingNfcEvent += GetNfcReadText;
        }

        private void GetNfcReadText(string reasultText)
        {
            NfcData = reasultText;
        }

        public void Dispose()
        {
            App.NfcAPI.ReadingNfcEvent -= GetNfcReadText;
            App.NfcAPI.StopScanning();
        }
    }
}
