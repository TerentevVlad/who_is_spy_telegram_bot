using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace WhoIsSpy
{
    class Preferences
    {
        public static string GetString(string key, string def)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string value = localSettings.Values[key] as string;
            if (value != null)
                return value;
            else
                return def;
        }

        public static void SaveString(string key, string value)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values[key] = value;
        }
    }
}
