using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace BluetoothTemp.Abstract
{
    public class Client
    {
        private static Client Instance;
        private HttpClient httpClient;
        public RestClient restClient;
        private Client()
        {
            httpClient = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler());
            //httpClient.BaseAddress = new Uri("https://temp.skid.su");
            restClient = new RestClient(httpClient);
        }
        
        public static Client GetInstance()
        {
            if (Instance == null)
            {
                Instance = new Client();
            }
            return Instance;
        }
    }
}
