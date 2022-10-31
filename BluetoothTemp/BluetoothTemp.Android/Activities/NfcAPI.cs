using Android.App;
using Android.Content;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BluetoothTemp.Abstract;
using BluetoothTemp.Droid.Callbacks.NfcCallback;
using Java.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluetoothTemp.Droid.Activities
{
    [Activity(Label = "NfcActivity")]
    public class NfcAPI : INfcAPI
    {
        private delegate void TagDiscoveredDelegate();
        private TagDiscoveredDelegate _tagDiscoveredDelegate;
        private Ndef _ndef;
        private NfcAdapter _nfcAdapter;
        private Activity _activity;

        public event Action<string> ReadingNfcEvent;
        public event Action WritingNfcEvent;

        public NfcAPI(Activity activity)
        {
            _activity = activity;
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(_activity);

        }

        public void StartScanning()
        {
            if (_nfcAdapter != null)
            {
                Bundle options = new Bundle();
                options.PutInt(NfcAdapter.ExtraReaderPresenceCheckDelay, 250);
                CustomReaderCallback customReaderCallback = new CustomReaderCallback();
                customReaderCallback.TagDiscoveredEvent += TagDiscovered;
                _nfcAdapter.EnableReaderMode(
                    _activity,
                    customReaderCallback,
                    NfcReaderFlags.NfcA |
                    NfcReaderFlags.NfcB |
                    NfcReaderFlags.NfcF |
                    NfcReaderFlags.NfcV |
                    NfcReaderFlags.NfcBarcode,
                    options);

            }
        }

        public void StopScanning()
        {
            if (_nfcAdapter != null)
            {
                _nfcAdapter.DisableReaderMode(_activity);
            }
        }

        public void Read()
        {
            _tagDiscoveredDelegate = () =>
            {
                
                if (_ndef.NdefMessage != null)
                {
                    NdefRecord[] ndefRecords = _ndef.NdefMessage.GetRecords();
                    byte[] ndefValue = ndefRecords[0].GetPayload();
                    byte[] mainValue = new byte[ndefValue.Length - 3];
                    Array.Copy(ndefValue, 3, mainValue, 0, mainValue.Length);
                    string text = Encoding.UTF8.GetString(mainValue);
                    ReadingNfcEvent?.Invoke(text);
                }
            };
        }

        public void Write(string message)
        {
            _tagDiscoveredDelegate = () =>
            {
                NdefMessage ndefMessage;
                NdefRecord[] records = new NdefRecord[1];
                records[0] = NdefRecord.CreateTextRecord("ru", message);
                ndefMessage = new NdefMessage(records);
                _ndef.WriteNdefMessage(ndefMessage);
                WritingNfcEvent?.Invoke();
            };
        }
        private void TagDiscovered(Tag tag)
        {
            /*_mifare = MifareClassic.Get(tag);
            if (_mifare != null) 
            {
                _mifare.Connect();
                
                var authKeyA = _mifare.AuthenticateSectorWithKeyA(0, MifareClassic.KeyDefault.ToArray());
                if (authKeyA==true)
                {
                    _tagDiscoveredDelegate?.Invoke();
                }
            }*/

            List<byte> values = new List<byte>();
            MifareClassic mifare = MifareClassic.Get(tag);
            if (mifare != null)
            {
                mifare.Connect();
                for (int i = 0; i < mifare.SectorCount; i++)
                {
                    var authKeyA = mifare.AuthenticateSectorWithKeyA(i, MifareClassic.KeyDefault.ToArray());
                    if (authKeyA == true)
                    {
                        var blockCount = mifare.GetBlockCountInSector(i);
                        for (int j = 0; j < blockCount; j++)
                        {
                            if ((i == 0 && j == 0))
                            {
                                continue;
                            }
                            /*if (i == 0 && j == 1)
                            {
                                List<byte> writeMessgae = new List<byte>(16);
                                var bytes = Encoding.ASCII.GetBytes("3209613569");
                                writeMessgae.AddRange(bytes);
                                for (int l = 0 + bytes.Length; l < 16; l++)
                                {
                                    writeMessgae.Add(00);
                                }
                                mifare.WriteBlock(j, writeMessgae.ToArray());
                            }*/
                            var message = mifare.ReadBlock(j);
                            foreach (var item in message)
                            {
                                values.Add(item);
                            }
                        }
                    }
                }
                var b = Convert.ToBase64String(values.ToArray());
                var a = Encoding.ASCII.GetString(values.ToArray());
                mifare.Close();
            }

            _ndef = Ndef.Get(tag);
            if (_ndef != null)
            {
                _ndef.Connect();
                if (_ndef.IsConnected)
                {
                    _tagDiscoveredDelegate?.Invoke();
                    
                }
            }
        }

        
    }
}