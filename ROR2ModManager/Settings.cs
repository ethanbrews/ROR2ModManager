using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ROR2ModManager
{

    public class SettingsValues
    {
        //  {readable-Name: {path, fa-token}
        public Dictionary<string, KeyValuePair<string, string>> folder_fa_token { get; set; }
    }

    class Settings
    {

        public static SettingsValues values;

        public static async Task LoadSettings()
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                values = JsonConvert.DeserializeObject<SettingsValues>(await FileIO.ReadTextAsync(await localFolder.GetFileAsync("settings.json")));
            }
            catch (Exception)
            {
                values = new SettingsValues
                {
                    folder_fa_token = new Dictionary<string, KeyValuePair<string, string>>()
                };
            }

        }

        public static async Task SaveSettings()
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            await FileIO.WriteTextAsync(await localFolder.GetFileAsync("settings.json"), JsonConvert.SerializeObject(values));
        }
    }
}
