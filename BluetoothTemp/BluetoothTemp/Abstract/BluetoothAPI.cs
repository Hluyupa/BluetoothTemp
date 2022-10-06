using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using BluetoothTemp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothTemp.Abstract
{
    public class BluetoothAPI
    {
        //BluetoothManager нужен для получения объекта BluetoothAdapter,
        //а также для общего управления.
        public BluetoothManager bluetoothManager;

        //BluetoothAdapter необходим для выполнения основных
        //задач Bluetooth (получение списка сопряжённых устройств,
        //поиск других устройств).
        public BluetoothAdapter bluetoothAdapter;

        //BluetoothLeScanner необходим для включения
        //сакнирования близжайших Bluetooth устройств
        public BluetoothLeScanner scanner;

        //CustomScanCallback - в этом классе переопределяются методы,
        //которые срабатывают при обнаружении новых устройств
        public CustomScanCallback scanCallback;

        //ScanSettings - установка параметров сканирования
        private ScanSettings scanSettings;

        public CustomBluetoothGattCallback bluetoothGattCallback;

        private BluetoothGatt _gatt;



        //Принимает обработчики вкл/выкл Bluetooth
        public delegate void OnOffBluetoothHandler();

        private static BluetoothAPI _instance;
        private BluetoothAPI()
        {
            bluetoothManager = (BluetoothManager)Android.App.Application.Context.GetSystemService("bluetooth");
            bluetoothAdapter = bluetoothManager.Adapter;
            scanSettings = new ScanSettings.Builder()
                .SetScanMode(Android.Bluetooth.LE.ScanMode.LowLatency)
                .SetCallbackType(ScanCallbackType.AllMatches)
                .SetMatchMode(BluetoothScanMatchMode.Sticky)
                .SetNumOfMatches(1)
                .SetReportDelay(1)
                .Build();
        }

        public static BluetoothAPI GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BluetoothAPI();
            }
            return _instance;
        }

        public void OnOffBluetooth(OnOffBluetoothHandler enableHandler, OnOffBluetoothHandler disableHandler)
        {
            if (bluetoothAdapter.IsEnabled)
            {
                bluetoothAdapter.Disable();
                disableHandler();
            }
            else
            {
                bluetoothAdapter.Enable();
                enableHandler();
            }
        }

        public void GetScanDevices(ICollection<BluetoothDevice> bluetoothDevicesList)
        {
            if (bluetoothAdapter.IsEnabled)
            {
                scanner = bluetoothAdapter.BluetoothLeScanner;
                scanCallback = new CustomScanCallback(bluetoothDevicesList);
                if (scanner != null)
                {
                    scanner.StartScan(filters: null, settings: scanSettings, callback: scanCallback);
                }
            }   
        }

        public void ConnectAndGetServices(BluetoothDevice device, ICollection<BluetoothGattService> availableServices)
        {
            bluetoothGattCallback = new CustomBluetoothGattCallback(availableServices);
            _gatt = device.ConnectGatt(Application.Context, false, bluetoothGattCallback, transport: BluetoothTransports.Le);
        }

        public void Disconnect()
        {
            if (_gatt != null)
            {
                _gatt.Disconnect();
            }
        }

        public void Read()
        {
            
        }

    }
}
