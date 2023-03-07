using BluetoothTemp.Abstract;
using BluetoothTemp.Models;
using BluetoothTemp.TelephoneServices.Bluetooth;
using BluetoothTemp.Views;
using BluetoothTempEntities;
using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using static Android.Bluetooth.BluetoothClass;

namespace BluetoothTemp.ViewModels
{
    public class DiscoverDevicesPageVM : BaseViewModel, INavigationVM
    {
        public ICommand TappedBluetoothDeviceCommand { get; set; }

        private ObservableCollection<ScannedBluetoothDeviceModel> searchResult;
        public ObservableCollection<ScannedBluetoothDeviceModel> SearchResult
        {
            get 
            {
                return searchResult; 
            }
            set 
            { 
                searchResult = value;
                OnPropertyChanged();
            }
        }

        
        public ObservableCollection<ScannedBluetoothDeviceModel> ScannedBluetoothDevicesList { get; set; }
        

        //Выбранное устрйоство для подключения
        private ScannedBluetoothDeviceModel selectedBluetoothDevice;
        public ScannedBluetoothDeviceModel SelectedBluetoothDevice
        {
            get
            {
                return selectedBluetoothDevice;
            }
            set
            {
                if (selectedBluetoothDevice != value)
                {
                    selectedBluetoothDevice = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool bluetoothState;
        public bool BluetoothState
        {
            get
            {
                return bluetoothState;
            }
            set
            {
                bluetoothState = value;
                OnPropertyChanged();
            }
        }

        private string searchDeviceNameText;
        public string SearchDeviceNameText
        {
            get 
            {
                return searchDeviceNameText; 
            }
            set 
            { 
                searchDeviceNameText = value.ToLower();
                if (searchDeviceNameText != "") 
                {
                    Task.Run(() =>
                    {
                        SearchResult = new ObservableCollection<ScannedBluetoothDeviceModel>(ScannedBluetoothDevicesList.Where(p =>
                        {
                            if (p.Name != null)
                                return p.Name.ToLower().Contains(searchDeviceNameText);
                            else
                                return false;

                        }).ToList());
                    });
                }
                else
                {
                    Task.Run(() => SearchResult = ScannedBluetoothDevicesList);
                }
                
                OnPropertyChanged();
            }
        }

        private string _dbPath;

        private List<BluetoothDeviceWasСonnected> _autoconnectDevices;

        private BluetoothAPI _bluetoothAPI;

        private IBluetoothStateChanged _bluetoothStateChangedReciever;

        public DiscoverDevicesPageVM()
        {
            _bluetoothAPI = BluetoothAPI.GetInstance();
            _dbPath = DependencyService.Get<IPath>().GetDatabasePath(App.DbFilename);

            _bluetoothStateChangedReciever = DependencyService.Get<IBluetoothStateChanged>();
            
            ScannedBluetoothDevicesList = new ObservableCollection<ScannedBluetoothDeviceModel>();
            SearchResult = new ObservableCollection<ScannedBluetoothDeviceModel>();
            SearchResult = ScannedBluetoothDevicesList;
            ScannedBluetoothDevicesList.CollectionChanged += NewBluetoothDeviceHandler;

            TappedBluetoothDeviceCommand = new Command(OpenBluetoothDevicePage);
        }

        private void NewBluetoothDeviceHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add) 
            {
                ScannedBluetoothDeviceModel device;
                foreach (var item in e.NewItems)
                {
                    device = item as ScannedBluetoothDeviceModel;

                    if (_autoconnectDevices.FirstOrDefault(p => p.MacAddress == device.Device.Address) != null)
                        device.IsAutoconnect = true;
                    else
                        device.IsAutoconnect = false;

                    if (SearchDeviceNameText != "" && SearchDeviceNameText != String.Empty && SearchDeviceNameText != null)
                        if (device.Name != null)
                            if (device.Name.ToLower().Contains(SearchDeviceNameText))
                                SearchResult.Add(device);
                }
            }
        }

        private async void OpenBluetoothDevicePage(object args)
        {
            var scannedDevice = (args as ItemTappedEventArgs).Item as ScannedBluetoothDeviceModel;
            await App.Current.MainPage.Navigation.PushAsync(
                new BluetoothDevicePage
                {
                    BindingContext = new BluetoothDevicePageVM(scannedDevice.Device)
                }
            );
        }

        private void OnBluetoothEventHandler(object sender, EventArgs args)
        {
            BluetoothState = true;
            _bluetoothAPI?.ScanDevices(ScannedBluetoothDevicesList);
        }

        private void OffBluetoothEventHandler(object sender, EventArgs args)
        {
            
            BluetoothState = false;
            _bluetoothAPI.StopScanDevices();
            ScannedBluetoothDevicesList.Clear();
            SearchResult.Clear();
        }

        public void OnAppearing()
        {
            BluetoothState = _bluetoothAPI.BluetoothAdapter.IsEnabled;

            using (var context = new ApplicationContext(_dbPath))
            {
                _autoconnectDevices = context.BluetoothDevicesWasСonnected.Where(p => p.IsAutoconnect == Convert.ToInt32(true)).ToList();
            }
            
            _bluetoothStateChangedReciever.SetOnBluetoothEvent(OnBluetoothEventHandler);
            _bluetoothStateChangedReciever.SetOffBluetoothEvent(OffBluetoothEventHandler);

            if (BluetoothState)
                _bluetoothAPI.ScanDevices(ScannedBluetoothDevicesList);
        }

        public void OnDisappearing()
        {
            _bluetoothAPI.StopScanDevices();
            _bluetoothStateChangedReciever.ClearEvents();
            ScannedBluetoothDevicesList.Clear();
            SearchResult.Clear();
            SearchDeviceNameText = String.Empty;
        }
    }
}
