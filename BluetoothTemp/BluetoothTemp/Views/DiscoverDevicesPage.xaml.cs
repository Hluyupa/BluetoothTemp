using BluetoothTemp.Abstract;
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
    public partial class DiscoverDevicesPage : ContentPage
    {
        public DiscoverDevicesPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var viewModel = (DiscoverDevicesPageVM)this.BindingContext;
            (viewModel as INavigationVM)?.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            base.OnAppearing();
            var viewModel = (DiscoverDevicesPageVM)this.BindingContext;
            (viewModel as INavigationVM)?.OnDisappearing();
        }
    }
}