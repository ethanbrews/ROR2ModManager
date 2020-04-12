using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ROR2ModManager.API
{
    class ApiAccess
    {
        public async static Task<Package[]> GetPackages()
        {
            var uri = new Uri("https://thunderstore.io/api/v1/package/");
            string s;
            using (WebClient client = new WebClient())
            {
                s = await client.DownloadStringTaskAsync(uri);
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<Package[]>(s);
        }
    }
}
