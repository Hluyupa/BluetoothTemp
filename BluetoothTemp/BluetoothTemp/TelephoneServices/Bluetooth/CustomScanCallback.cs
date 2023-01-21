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
        private readonly Action<IList<ScanResult>> BatchScanResults;
        private readonly Action<ScanCallbackType, ScanResult> ScanResult;

        public CustomScanCallback(Action<ScanCallbackType, ScanResult> scanResult, Action<IList<ScanResult>> batchScanResults = null)
        {
            ScanResult = scanResult;
            BatchScanResults = batchScanResults;
        }
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);
            ScanResult.Invoke(callbackType, result);
        }
        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            base.OnBatchScanResults(results);
            BatchScanResults.Invoke(results);
            
        }
    }
}
