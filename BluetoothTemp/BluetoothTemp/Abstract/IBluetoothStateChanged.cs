using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Abstract
{
    public interface IBluetoothStateChanged
    {
        void SetOnBluetoothEvent(EventHandler action);
        void SetOffBluetoothEvent(EventHandler action);
        void ClearEvents();
    }
}
