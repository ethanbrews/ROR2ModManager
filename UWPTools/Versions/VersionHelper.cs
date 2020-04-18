using ROR2ModManager;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UWPTools.Settings;

namespace UWPTools.Versions
{

    /// <summary>
    /// Detects when the app is running on a new version for the first time.
    /// </summary>
    public abstract class VersionHelper
    {

        private static WebClient client;

        private LocalSettingsValue<string> LastLoadedVersionSetting = new LocalSettingsValue<string>("UWPTools.LastLoadedVersion", null);

        private string LatestAppVersionOnServer; //The latest app version as string which is obtained from AppInstallerUri
        private bool? IsAppUpToDate;
        public Uri AppInstallerUri { get; private set; }

        /// <summary>Has the app ever been loaded before?</summary>
        public bool HasAppBeenLoadedBefore { get; private set; }
        public bool IsApplicationUpToDateDetermined { get => IsAppUpToDate.HasValue; }
        /// <summary>Has the app ever been loaded before on this version</summary>
        public bool HasThisAppVersionLoadedBefore { get; private set;}

        public VersionHelper(Uri appInstallerUri)
        {
            var thisAppVersion = AppPackageHelper.GetAppPackageVersionAsString();
            HasThisAppVersionLoadedBefore = thisAppVersion == LastLoadedVersionSetting.Value;
            LastLoadedVersionSetting.Value = thisAppVersion;
            AppInstallerUri = appInstallerUri;
            
        }

        static VersionHelper()
        {
            client = new WebClient();
        }

        public async Task<bool> IsApplicationUpToDateAsync()
        {
            if (IsAppUpToDate.HasValue)
            {
                return IsAppUpToDate.Value;
            } else
            {
                var serializer = new XmlSerializer(typeof(Data.UWPAppInstaller.AppInstaller));
                var reader = new System.IO.StringReader(await client.DownloadStringTaskAsync(AppInstallerUri.OriginalString));
                var appInstaller = serializer.Deserialize(reader) as Data.UWPAppInstaller.AppInstaller;
                LatestAppVersionOnServer = appInstaller.Version;
                IsAppUpToDate = (LatestAppVersionOnServer == AppPackageHelper.GetAppPackageVersionAsString());
                return IsAppUpToDate.Value;
            }
        }

        
    }
}
