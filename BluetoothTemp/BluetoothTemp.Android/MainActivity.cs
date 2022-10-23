using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android;
using Android.Widget;
using Android.Nfc;
using Android.Content;
using System.Text;
using Android.Nfc.Tech;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Internals;

namespace BluetoothTemp.Droid
{
    [Activity(Label = "BluetoothTemp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private NfcAdapter _nfcAdapter;

        private readonly string[] Permissions =
        {
            Manifest.Permission.BluetoothScan,
            Manifest.Permission.BluetoothConnect,
            Manifest.Permission.BluetoothAdvertise,
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.Nfc
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            global::Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            //RequestPermissions(Permissions, 2);
            /*RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation } , 2);*/
           
            CheckPermissions();
            LoadApplication(new App(this));
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (_nfcAdapter != null) 
            {
                Bundle options = new Bundle();
                options.PutInt(NfcAdapter.ExtraReaderPresenceCheckDelay, 250);
                CustomReaderCallback customReaderCallback = new CustomReaderCallback();
                customReaderCallback.TagDiscoveredEvent += TagDiscovered;
                _nfcAdapter.EnableReaderMode(
                    this,
                    customReaderCallback,
                    NfcReaderFlags.NfcA |
                    NfcReaderFlags.NfcB |
                    NfcReaderFlags.NfcF |
                    NfcReaderFlags.NfcV |
                    NfcReaderFlags.NfcBarcode,
                    options);
            }
        }

        private void TagDiscovered(Tag tag)
        {
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
                            if (i == 0  && j == 1)   
                            {
                                List<byte> writeMessgae = new List<byte>(16);
                                var bytes = Encoding.ASCII.GetBytes("3209613569");
                                writeMessgae.AddRange(bytes);
                                for (int l = 0 + bytes.Length; l < 16; l++) 
                                {
                                    writeMessgae.Add(00);
                                }
                                mifare.WriteBlock(j, writeMessgae.ToArray());
                            }
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
        }        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void CheckPermissions()
        {
            bool minimumPermissionsGranted = true;

            foreach (string permission in Permissions)
            {
                if (CheckSelfPermission(permission) != Permission.Granted)
                {
                    minimumPermissionsGranted = false;
                }
            }

            if (!minimumPermissionsGranted)
            {
                RequestPermissions(Permissions, 2);
            }
        }
    }
}