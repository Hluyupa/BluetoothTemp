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
        private readonly Action<BluetoothGatt, BluetoothGattCharacteristic> _characteristicRead;
        private readonly Action<BluetoothGattCharacteristic> _characteristicChanged;
        private readonly Action<BluetoothGatt, GattStatus, ProfileState> _connectionStateChange;
        private readonly Action<BluetoothGatt, GattStatus> _servicesDiscovered;

        public CustomBluetoothGattCallback(Action<BluetoothGatt, BluetoothGattCharacteristic> characteristicRead, Action<BluetoothGatt, GattStatus, ProfileState> connectionStateChange, Action<BluetoothGatt, GattStatus> servicesDiscovered, Action<BluetoothGattCharacteristic> characteristicChanged = null)
        {
            _characteristicRead = characteristicRead;
            _characteristicChanged = characteristicChanged;
            _connectionStateChange = connectionStateChange;
            _servicesDiscovered = servicesDiscovered;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);

            _connectionStateChange.Invoke(gatt, status, newState);
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);
            if (status == GattStatus.Failure)
            {
                gatt.Disconnect();
                return;
            }
            _servicesDiscovered.Invoke(gatt, status);
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);
            
            _characteristicRead.Invoke(gatt, characteristic);
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);
            
            _characteristicChanged?.Invoke(characteristic);
        }
    }
}
