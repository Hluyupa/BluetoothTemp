using BluetoothTemp.Abstract;
using BluetoothTemp.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace BluetoothTemp.ViewModels
{
    public class AutoconnectDevicesPageVM : BaseViewModel
    {
        public ObservableCollection<BluetoothDeviceWasСonnected> AutoconnectDevicesList { get; set; }

        private string _dbPath;
        public AutoconnectDevicesPageVM()
        {
            _dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DbFilename);
            AutoconnectDevicesList = new ObservableCollection<BluetoothDeviceWasСonnected>();
            using (var context = new ApplicationContext(_dbPath))
            {
                var devices = context.BluetoothDevicesWasСonnected.Where(p => p.IsAutoconnect.Equals(Convert.ToInt32(true))).ToList();
                foreach (var device in devices)
                {
                    AutoconnectDevicesList.Add(device);
                }
            }
        }

    }
}
