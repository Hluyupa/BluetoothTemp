using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using BluetoothTemp.Views;
using BluetoothTempEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace BluetoothTemp.ViewModels
{
    public class AutoconnectDevicesPageVM : BaseViewModel
    {
        public ObservableCollection<AutoconnectDeviceModel> AutoconnectDevicesList { get; set; }

       

        private BluetoothAPI _bluetoothAPI;

        private string _dbPath;
        public AutoconnectDevicesPageVM()
        {
            _bluetoothAPI = BluetoothAPI.GetInstance();
            _dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DbFilename);
            AutoconnectDevicesList = new ObservableCollection<AutoconnectDeviceModel>();

            using (var context = new ApplicationContext(_dbPath))
            {
                var devices = context.BluetoothDevicesWasСonnected.Where(p => p.IsAutoconnect.Equals(Convert.ToInt32(true))).ToList();
                foreach (var device in devices)
                {
                    AutoconnectDevicesList.Add(new AutoconnectDeviceModel { Name = device.Name, MacAddress = device.MacAddress, StatusConnect = 0 });
                }
            }
            _bluetoothAPI.AutoconnectToDevices(AutoconnectDevicesList);

        }
       
    }
}
