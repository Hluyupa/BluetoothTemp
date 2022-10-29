using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothTemp.Abstract
{
    public interface IPath
    {
        string GetDatabasePath(string filename);
    }
}
