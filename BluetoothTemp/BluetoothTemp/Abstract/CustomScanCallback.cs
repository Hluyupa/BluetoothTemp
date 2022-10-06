using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.Internals;

namespace BluetoothTemp.Abstract
{
    public class CustomScanCallback : ScanCallback
    {
        private ICollection<BluetoothDevice> _bluetoothDevices;
        public CustomScanCallback(ICollection<BluetoothDevice> bluetoothDevices)
        {
            _bluetoothDevices = bluetoothDevices;
        }
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            base.OnScanResult(callbackType, result);
        }
        public override void OnBatchScanResults(IList<ScanResult> results)
        {
            base.OnBatchScanResults(results);
            foreach (var result in results)
            {
                var device = _bluetoothDevices.FirstOrDefault(p => p.Equals(result.Device));
                if (device == null)
                {
                    _bluetoothDevices.Add(result.Device);
                }
            }
        }

        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            base.OnScanFailed(errorCode);
        }
    }
}
