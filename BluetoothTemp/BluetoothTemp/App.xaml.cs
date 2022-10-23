using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BluetoothTemp.Views;
using Android.App;

namespace BluetoothTemp
{
    public partial class App : Xamarin.Forms.Application
    {
        public App(Activity activity)
        {
            InitializeComponent();
            
            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {

        }

        protected override void OnResume()
        {

        }
    }
}
