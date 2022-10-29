using BluetoothTemp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BluetoothTemp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothDevicePage : ContentPage
    {
        public BluetoothDevicePage()
        {
            InitializeComponent();
        }

        //Вызывается, когда страница закрывается.
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var viewModel = (BluetoothDevicePageVM)this.BindingContext;
            viewModel.Dispose();
        }
    }
}