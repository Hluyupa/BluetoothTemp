using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using BluetoothTemp.Views;
using Android.App;
using BluetoothTemp.Abstract;

namespace BluetoothTemp
{
    public partial class App : Xamarin.Forms.Application
    {
        public const string DbFilename = "bluetoothTempApp.db3";
        public static INfcAPI NfcAPI { get; private set; }
        public App(INfcAPI nfcAPI)
        {
           
            InitializeComponent();
            NfcAPI = nfcAPI;
            
            
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
