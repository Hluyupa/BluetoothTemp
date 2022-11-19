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
    public partial class AutoconnectDevicesPage : ContentPage
    {
        public AutoconnectDevicesPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var viewModel = (AutoconnectDevicesPageVM)this.BindingContext;
            (viewModel as INavigationVM)?.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var viewModel = (AutoconnectDevicesPageVM)this.BindingContext;
            (viewModel as INavigationVM)?.OnDisappearing();
        }
    }
}