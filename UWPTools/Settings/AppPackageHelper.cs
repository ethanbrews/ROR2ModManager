using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel;

namespace UWPTools.Settings
{
    class AppPackageHelper
    {
        public static string GetAppPackageVersionAsString()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
