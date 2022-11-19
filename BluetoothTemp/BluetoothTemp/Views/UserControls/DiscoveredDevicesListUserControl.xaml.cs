using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BluetoothTemp.Views.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DiscoveredDevicesListUserControl : Grid
    {
        public DiscoveredDevicesListUserControl()
        {
            InitializeComponent(); 
        }
    }
}
