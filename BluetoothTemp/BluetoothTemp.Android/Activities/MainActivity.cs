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
using BluetoothTemp.Droid.Callbacks.NfcCallback;

namespace BluetoothTemp.Droid.Activities
{
    [Activity(Label = "BluetoothTemp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
      

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
            //RequestPermissions(Permissions, 2);
            LoadApplication(new App(new NfcAPI(this)));
            
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