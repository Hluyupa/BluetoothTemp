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
        private readonly Action<BluetoothGatt, BluetoothGattCharacteristic> CharacteristicRead;
        private readonly Action<BluetoothGattCharacteristic> CharacteristicChanged;
        private readonly Action<BluetoothGatt, GattStatus, ProfileState> ConnectionStateChange;
        private readonly Action<BluetoothGatt, GattStatus> ServicesDiscovered;

        public CustomBluetoothGattCallback(Action<BluetoothGatt, BluetoothGattCharacteristic> characteristicRead, Action<BluetoothGatt, GattStatus, ProfileState> connectionStateChange, Action<BluetoothGatt, GattStatus> servicesDiscovered, Action<BluetoothGattCharacteristic> characteristicChanged = null)
        {
            CharacteristicRead = characteristicRead;
            CharacteristicChanged = characteristicChanged;
            ConnectionStateChange = connectionStateChange;
            ServicesDiscovered = servicesDiscovered;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            ConnectionStateChange.Invoke(gatt, status, newState);
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);
            if (status == GattStatus.Failure)
            {
                gatt.Disconnect();
                return;
            }
            ServicesDiscovered.Invoke(gatt, status);
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);
            
            CharacteristicRead.Invoke(gatt, characteristic);
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);
            
            CharacteristicChanged?.Invoke(characteristic);
        }
    }
}
