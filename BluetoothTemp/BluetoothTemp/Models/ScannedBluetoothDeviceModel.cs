using Android.Bluetooth;
using BluetoothTemp.Abstract;
using BluetoothTempEntities;
using Microsoft.Extensions.DependencyModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BluetoothTemp.Models
{
    public class ScannedBluetoothDeviceModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public BluetoothDevice Device { get; set; }

        private bool isAutoconnect;
        public bool IsAutoconnect
        {
            get 
            { 
                return isAutoconnect;
            }
            set
            {
                isAutoconnect = value;
                using (var context = new ApplicationContext(_dbPath))
                {
                    var device = context.BluetoothDevicesWasСonnected.FirstOrDefault(p => p.MacAddress == Device.Address);
                    if (isAutoconnect && device == null)
                    {
                        context.BluetoothDevicesWasСonnected.Add(new BluetoothDeviceWasСonnected
                        {
                            Name = Device.Name,
                            MacAddress = Device.Address,
                            IsAutoconnect = Convert.ToInt32(isAutoconnect),
                            IsNfcWrited = 0,
                        });
                    }
                    else if (!isAutoconnect && device != null) 
                        context.BluetoothDevicesWasСonnected.Remove(device);
                    context.SaveChanges();
                }
                OnPropertyChanged();
            }
        }

        private string _dbPath;
        public ScannedBluetoothDeviceModel()
        {
            _dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DbFilename);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
