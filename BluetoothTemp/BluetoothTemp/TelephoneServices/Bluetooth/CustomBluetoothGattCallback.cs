using Android.Bluetooth;
using Android.Runtime;
using BluetoothTemp.Models;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluetoothTemp.TelephoneServices.Bluetooth
{
    public class CustomBluetoothGattCallback : BluetoothGattCallback
    {
        public event Action<BluetoothGattCharacteristic> CharacteristicReadEvent;
        public event Action<BluetoothGattCharacteristic> CharacteristicChangedEvent;
        public event Action<BluetoothGatt, GattStatus, ProfileState> ConnectionStateChangeEvent;
        public event Action<BluetoothGatt, GattStatus> ServicesDiscoveredEvent;

        

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            ConnectionStateChangeEvent?.Invoke(gatt, status, newState);
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);
            if (status == GattStatus.Failure)
            {
                gatt.Disconnect();
                return;
            }
            ServicesDiscoveredEvent?.Invoke(gatt, status);
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);
            
            CharacteristicReadEvent?.Invoke(characteristic);
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);
            
            CharacteristicChangedEvent?.Invoke(characteristic);
        }
    }
}
