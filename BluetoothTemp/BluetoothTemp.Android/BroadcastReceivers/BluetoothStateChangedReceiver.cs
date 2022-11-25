using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BluetoothTemp.Droid;
using BluetoothTemp.Droid.BroadcastReceivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using BluetoothTemp.Abstract;

[assembly: Dependency(typeof(BluetoothStateChangedReceiver))] 
namespace BluetoothTemp.Droid.BroadcastReceivers
{

    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { BluetoothAdapter.ActionStateChanged })]
    public class BluetoothStateChangedReceiver : BroadcastReceiver, IBluetoothStateChanged
    {
        public static event EventHandler OnBluetoothEvent;
        public static event EventHandler OffBluetoothEvent;

        private static int _status;

        public void SetOnBluetoothEvent(EventHandler eventHandler)
        {
            OnBluetoothEvent += eventHandler;
        }
        public void SetOffBluetoothEvent(EventHandler eventHandler)
        {
            OffBluetoothEvent = eventHandler;
        }
        public void ClearEvents()
        {
            OnBluetoothEvent = null;
            OffBluetoothEvent = null;
        }
        public override void OnReceive(Context context, Intent intent)
        {
            var state = intent.GetIntExtra(BluetoothAdapter.ExtraState, -1);
            if (state != 0 && state != _status) 
            {
                switch (state)
                {
                    case (int)State.On:
                        OnBluetoothEvent?.Invoke(this, EventArgs.Empty);
                        _status = state;
                        
                        break;
                    case (int)State.Off:
                        OffBluetoothEvent?.Invoke(this, EventArgs.Empty);
                        _status = state;
                        break;
                    default:
                        break;
                }
            }
        }

        
    }
}