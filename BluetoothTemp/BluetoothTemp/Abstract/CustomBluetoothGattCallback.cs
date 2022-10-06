using Android.Bluetooth;
using Android.Runtime;
using BluetoothTemp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluetoothTemp.Abstract
{
    public class CustomBluetoothGattCallback : BluetoothGattCallback
    {
        //private Queue<>
        private ICollection<BluetoothGattService> _services;
      
        public CustomBluetoothGattCallback(ICollection<BluetoothGattService> services)
        {
            _services = services;
        }
        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);
            if (status == GattStatus.Success)
            {
                if (newState == ProfileState.Connected)
                {
                    gatt.DiscoverServices();
                }
                else if (newState == ProfileState.Disconnected)
                {
                    gatt.Close();
                }
            }
            else
            {
                gatt.Close();
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);
            if (status == GattStatus.Failure)
            {
                gatt.Disconnect();
                return;
            }
            foreach (var service in gatt.Services)
            {
                if (!_services.Contains(service))
                {
                    _services.Add(service);
                }
            }
            //_services = gatt.Services;
            
            //gatt.ReadCharacteristic(services[0].Characteristics[0]);
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);
            byte[] buffer = new byte[characteristic.GetValue().Length];
            Array.Copy(characteristic.GetValue(), 0, buffer, 0, characteristic.GetValue().Length);
            var a = Encoding.UTF8.GetString(buffer);
        }
        
    }
}
