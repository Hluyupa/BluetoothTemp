using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Abstract
{
    public interface IBluetoothStateChanged
    {
        void SetOnBluetoothEvent(Action action);
        void SetOffBluetoothEvent(Action action);
        void ClearEvents();
    }
}
