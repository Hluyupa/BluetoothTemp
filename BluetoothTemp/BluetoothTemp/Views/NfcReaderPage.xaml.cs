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
    public partial class NfcReaderPage : ContentPage
    {
        public NfcReaderPage()
        {
            InitializeComponent();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var viewModel = (NfcReaderPageVM)this.BindingContext;
            viewModel.Dispose();
        }
    }
}