using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BluetoothTemp.Abstract;
using BluetoothTemp.Droid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidDbPath))]
namespace BluetoothTemp.Droid
{
    public class AndroidDbPath : IPath
    {
        public string GetDatabasePath(string filename)
        {
          
            return Path.Combine(FileSystem.AppDataDirectory, filename);
        }
    }
}