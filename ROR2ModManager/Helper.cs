using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace ROR2ModManager
{
    class Helper
    {
        public static async Task<IStorageItem> GetStorageItemForToken(string token)
        {
            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(token)) return null;
            return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
        }

        public static void RememberStorageItem(string token, StorageFolder file)
        {
            StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, file);
        }

        public static string RememberStorageItem(StorageFolder file)
        {
            string token = Guid.NewGuid().ToString();
            RememberStorageItem(token, file);
            return token;
        }
    }
}
