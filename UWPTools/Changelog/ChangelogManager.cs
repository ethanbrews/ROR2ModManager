using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UWPTools.Settings;
using Windows.UI.Xaml;

namespace UWPTools.Changelog
{
    public class ChangelogManager
    {
        private Dictionary<string, Type> Changelogs;

        public ChangelogManager(Dictionary<string, Type> keyValuePairs) => Changelogs = keyValuePairs;
        public ChangelogManager() => Changelogs = new Dictionary<string, Type>();

        private static Settings.LocalSettingsValue<string> LastShownChangelog = new LocalSettingsValue<string>("UWPTools.Changelog.LastShownChangelog", null);

        public void AddChangelogForVersion(string version, Type pageType) => Changelogs.Add(version, pageType);

        public async Task ShowChangelogForCurrentVersion()
        {
            var version = AppPackageHelper.GetAppPackageVersionAsString();
            Type pageType;

            if (!Changelogs.TryGetValue(version, out pageType))
                throw new Exceptions.ChangelogNotFoundForCurrentVersionException { VersionString = version };

            await new ChangelogDialog(pageType).ShowAsync();
            LastShownChangelog.Value = version;
        }

        public bool HasChangelogBeenShownForThisVersionBefore()
        {
            return LastShownChangelog.Value == AppPackageHelper.GetAppPackageVersionAsString();
        }

        public bool VersionHasAssociatedChangelog() => Changelogs.ContainsKey(AppPackageHelper.GetAppPackageVersionAsString());

        public async Task ShowChangelogForVersion(string version) => await new ChangelogDialog(Changelogs[version]).ShowAsync();

        public async Task ShowChangelogForCurrentVersionIfNotPreviouslyShown()
        {
            if (!HasChangelogBeenShownForThisVersionBefore())
                await ShowChangelogForCurrentVersion();
        }
    }
}
