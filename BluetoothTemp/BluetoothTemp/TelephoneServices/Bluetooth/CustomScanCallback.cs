using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.Internals;

namespace BluetoothTemp.TelephoneServices.Bluetooth
{
    public class CustomScanCallback : ScanCallback
    {
        public Action<IList<ScanResult>> BatchScanResultsEvent { get; set; }
        public Action<ScanCallbackType, ScanResult> ScanResultEvent { get; set; }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);
            ScanResultEvent.Invoke(callbackType, result);
        }
        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            base.OnBatchScanResults(results);
            BatchScanResultsEvent.Invoke(results);
            
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);
        }
    }
}
