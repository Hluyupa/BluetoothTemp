using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace BluetoothTemp.Views.ValueConverters
{
    public class ConnectStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = Color.Transparent;
            switch (System.Convert.ToInt32(value))
            {
                case 0:
                    color = Color.FromHex("d6d6d6");
                    break;
                case 1:
                    color = Color.FromHex("ffff5c");
                    break;
                case 2:
                    color = Color.FromHex("6cc841");
                    break;
            }
            return color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Color.Transparent;
        }
    }
}
