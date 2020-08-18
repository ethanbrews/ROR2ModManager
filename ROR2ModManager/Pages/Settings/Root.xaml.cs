using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Root : Page
    {

        public string RiskOfRainInstallationLocation;
        public string ApplicationInstallLocation;
        public string ApplicationVersion;
        public string ApplicationPackageIdFamily;
        public bool CurrentApplicationVersionHasAssociatedChangelog;

        public Root()
        {
            this.InitializeComponent();
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            ApplicationInstallLocation = ApplicationData.Current.LocalFolder.Path;
            RiskOfRainInstallationLocation = ApplicationData.Current.LocalSettings.Values["ror2"] as string;
            if (RiskOfRainInstallationLocation == null)
                RiskOfRainInstallationLocation = "Not Set";

            //ShowUpdateInNavBarCheckBox.IsChecked = ApplicationSettings.ShowUpdateButtonInNavBar.Value;

            AnalyticsToggleSwitch.IsOn = await Analytics.IsEnabledAsync();
            CrashalyticsToggleSwitch.IsOn = await Crashes.IsEnabledAsync();

            //AutoUpdateEnabled.IsOn = ApplicationSettings.UpdateAppAutomatically.Value;
            ModsMarqueeSwitch.IsOn = ApplicationSettings.UseMarqueeEffectForMods.Value;
            ExperimentalFeaturesSwitch.IsOn = ApplicationSettings.ShowExperimentalPages.Value;
            LaunchDirectlySwitch.IsOn = ApplicationSettings.LaunchGameDirectly.Value;

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            ApplicationPackageIdFamily = packageId.FamilyName;
            ApplicationVersion = string.Format("{0}.{1}.{2}.{3} {4}", version.Major, version.Minor, version.Build, version.Revision, packageId.Architecture.ToString().ToLower());

            //CurrentApplicationVersionHasAssociatedChangelog = (MainPage.Current.ChangelogManager.VersionHasAssociatedChangelog() ? Visibility.Visible : Visibility.Collapsed);
            CurrentApplicationVersionHasAssociatedChangelog = MainPage.Current.ChangelogManager.VersionHasAssociatedChangelog();

            try
            {
                if (await MainPage.Current.VersionHelper.IsApplicationUpToDateAsync())
                    UpdateAppButton_Reinstall.Visibility = Visibility.Visible;
                else
                    UpdateAppButton_Update.Visibility = Visibility.Visible;
            } catch (WebException)
            {
                UpdateAppButton_Update.Visibility = Visibility.Visible;
            }

            Color FromHex(int hex)
            {
                return Color.FromArgb(0xFF, (byte)((hex >> 16) & 0xFF), (byte)((hex >> 8) & 0xFF), (byte)(hex & 0xFF));
            }
        

            DefaultLightThemeButton.Background = new SolidColorBrush(FromHex(0xD8D9DB));
            (DefaultLightThemeButton.Children[0] as TextBlock).Foreground = new SolidColorBrush(Colors.Black);
            (DefaultLightThemeButton.Children[1] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x717071) };
            (DefaultLightThemeButton.Children[2] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0xCF333F) };
            (DefaultLightThemeButton.Children[3] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x717071) };
            (DefaultLightThemeButton.Children[4] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x717071) };
            

            DefaultDarkThemeButton.Background = new SolidColorBrush(FromHex(0x1A1423));
            (DefaultDarkThemeButton.Children[0] as TextBlock).Foreground = new SolidColorBrush(Colors.White);
            (DefaultDarkThemeButton.Children[1] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0xD7B9C7) };
            //(DefaultDarkThemeButton.Children[2] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0xCC5460) };
            (DefaultDarkThemeButton.Children[3] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0xD7B9C7) };
            (DefaultDarkThemeButton.Children[4] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0xD7B9C7) };

            (DefaultSystemThemeCanvas.Children[0] as Polygon).Fill = new SolidColorBrush(FromHex(0xD8D9DB));
            (DefaultSystemThemeCanvas.Children[1] as Polygon).Fill = new SolidColorBrush(FromHex(0x1A1423));
            (DefaultSystemThemeCanvas.Children[2] as TextBlock).Foreground = new SolidColorBrush(Colors.Black);

            (DefaultSystemThemeCanvas.Children[3] as Polygon).Fill = new AcrylicBrush { TintColor = FromHex(0x717071) };
            (DefaultSystemThemeCanvas.Children[4] as Polygon).Fill = new AcrylicBrush { TintColor = FromHex(0xD7B9C7) };

            (DefaultSystemThemeCanvas.Children[5] as Polygon).Fill = new AcrylicBrush { TintColor = FromHex(0xCF333F) };
            (DefaultSystemThemeCanvas.Children[6] as Polygon).Fill = new AcrylicBrush { TintColor = FromHex(0xCC5460) };

            (DefaultSystemThemeCanvas.Children[7] as Polygon).Fill = new AcrylicBrush { TintColor = FromHex(0x717071) };
            (DefaultSystemThemeCanvas.Children[8] as Polygon).Fill = new AcrylicBrush { TintColor = FromHex(0xD7B9C7) };
            
            (DefaultSystemThemeCanvas.Children[9] as Polygon).Fill = new AcrylicBrush { TintColor = FromHex(0xD7B9C7) };
            (DefaultSystemThemeCanvas.Children[10] as Polygon).Fill = new AcrylicBrush { TintColor = FromHex(0x717071) };

        }

        private async void Open_App_Button_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchFolderAsync(Windows.Storage.ApplicationData.Current.LocalFolder);
        }

        private async void Open_RoR_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Windows.System.Launcher.LaunchFolderAsync(await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalSettings.Values["ror2"] as string));
            } catch
            {
                await new ContentDialog
                {
                    Title = "Cannot Open Folder",
                    Content = new TextBlock { Text = "The folder may be set incorrectly" },
                    PrimaryButtonText = "Close"
                }.ShowAsync();
            }
            
        }

        private async void Change_RoR_Button_Click(object sender, RoutedEventArgs e)
        {
            await ProfileManager.ChooseRoRInstallFolder();
        }

        private async void UpdateAppButton_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent(AnalyticsEventNames.AppUpdateTriggeredByUser, new Dictionary<string, string> { { "AppUpToDate", MainPage.Current.VersionHelper.IsApplicationUpToDateAsync().ToString() } }) ;
            await Launcher.LaunchUriAsync(new Uri("ms-appinstaller:?source=http://ror2modman.ethanbrews.me/RoR2ModMan.appinstaller"));
        }

        private void ThemeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //theme-name theme-shade
            var settingsValues = ApplicationData.Current.LocalSettings.Values;
            if (sender == DefaultLightThemeButton)
            {
                settingsValues["theme-name"] = "default";
                settingsValues["theme-shade"] = "Light";
            } else if (sender == DefaultDarkThemeButton)
            {
                settingsValues["theme-name"] = "default";
                settingsValues["theme-shade"] = "Dark";
            } else if (sender == DefaultSystemThemeButton)
            {
                settingsValues["theme-name"] = "default";
                settingsValues["theme-shade"] = "System";
            }

            App.SetApplicationThemeBySettingsValue();
        }

        private void ThemeButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void ThemeButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }

        private async void ChangelogButton_Click(object sender, RoutedEventArgs e)
        {
            await MainPage.Current.ChangelogManager.ShowChangelogForCurrentVersion();
        }

        private void ShowUpdateInNavBarCheckBox_Click(object sender, RoutedEventArgs e)
        {
            //ApplicationSettings.ShowUpdateButtonInNavBar.Value = ShowUpdateInNavBarCheckBox.IsChecked ?? true;
        }

        private void CrashalyticsToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Crashes.SetEnabledAsync(CrashalyticsToggleSwitch.IsOn);
        }

        private async void AnalyticsToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            await Analytics.SetEnabledAsync(true);
            await Analytics.SetEnabledAsync(AnalyticsToggleSwitch.IsOn);
        }

        private async void Button_IssueTracker_Click(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri("https://github.com/ethanbrews/RiskOfRain2ModManagerIssueTracker/issues"));
        private async void Button_Website_Click(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri("https://ethanbrews.me/pages/ror2modman.html"));
        private async void Button_PrivacyPolicy_Click(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri("https://ethanbrews.me/privacy/forecast-policy.html"));
        private async void Button_TermsOfService_Click(object sender, RoutedEventArgs e) => await Launcher.LaunchUriAsync(new Uri("https://ethanbrews.me/terms-of-service.html"));

        private void AutoUpdateEnabled_Toggled(object sender, RoutedEventArgs e) => ApplicationSettings.UpdateAppAutomatically.Value = AutoUpdateEnabled.IsOn;

        private void ModsMarqueeSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.UseMarqueeEffectForMods.Value = ModsMarqueeSwitch.IsOn;
            Analytics.TrackEvent(AnalyticsEventNames.ToggledModsMarquee, new Dictionary<string, string> { { "isOn", ModsMarqueeSwitch.IsOn.ToString() } });
        }

        private void ExperimentalFeaturesSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.ShowExperimentalPages.Value = ExperimentalFeaturesSwitch.IsOn;
            Analytics.TrackEvent(AnalyticsEventNames.ToggledExperimentalFeatures, new Dictionary<string, string> { { "isOn", ModsMarqueeSwitch.IsOn.ToString() } });
        }

        private void LaunchDirectlySwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationSettings.LaunchGameDirectly.Value = LaunchDirectlySwitch.IsOn;
            Analytics.TrackEvent(AnalyticsEventNames.LaunchGameWithoutSteam, new Dictionary<string, string> { { "isOn", ModsMarqueeSwitch.IsOn.ToString() } });
        }
    }
}
