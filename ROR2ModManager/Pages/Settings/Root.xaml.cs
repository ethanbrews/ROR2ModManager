using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            ShowUpdateInNavBarCheckBox.IsChecked = ApplicationData.Current.LocalSettings.Values["showUpdateInNavBar"] as bool? ?? true;

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            ApplicationPackageIdFamily = packageId.FamilyName;
            ApplicationVersion = string.Format("{0}.{1}.{2}.{3} {4}", version.Major, version.Minor, version.Build, version.Revision, packageId.Architecture.ToString().ToLower());

            //CurrentApplicationVersionHasAssociatedChangelog = (MainPage.Current.ChangelogManager.VersionHasAssociatedChangelog() ? Visibility.Visible : Visibility.Collapsed);
            CurrentApplicationVersionHasAssociatedChangelog = MainPage.Current.ChangelogManager.VersionHasAssociatedChangelog();

            if (await MainPage.Current.VersionHelper.IsApplicationUpToDateAsync())
                UpdateAppButton_Reinstall.Visibility = Visibility.Visible;
            else
                UpdateAppButton_Update.Visibility = Visibility.Visible;


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

            /*
            WindowsLightThemeButton.Background = new SolidColorBrush(Colors.White);
            (WindowsLightThemeButton.Children[0] as TextBlock).Foreground = new SolidColorBrush(Colors.Black);
            (WindowsLightThemeButton.Children[1] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x898989) };
            (WindowsLightThemeButton.Children[2] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x0073CF) };
            (WindowsLightThemeButton.Children[3] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x898989) };
            (WindowsLightThemeButton.Children[4] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x898989) };

            WindowsDarkThemeButton.Background = new SolidColorBrush(Colors.Black);
            (WindowsDarkThemeButton.Children[0] as TextBlock).Foreground = new SolidColorBrush(Colors.White);
            (WindowsDarkThemeButton.Children[1] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x9A9A9A) };
            (WindowsDarkThemeButton.Children[2] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x0073CF) };
            (WindowsDarkThemeButton.Children[3] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x9A9A9A) };
            (WindowsDarkThemeButton.Children[4] as Rectangle).Fill = new AcrylicBrush { TintColor = FromHex(0x9A9A9A) };
            */

            /*
            var winRes = App.ThemeList["windows"].ThemeDictionaries;

            ResourceDictionary GetValueOrNull(IDictionary<object, object> d, string key)
            {
                object result;
                if (d.TryGetValue(key, out result))
                    return result as ResourceDictionary;
                return null;
            }

            var winResLight = GetValueOrNull(winRes, "Light");
            var winResDark = GetValueOrNull(winRes, "Dark");
            var winResDefault = GetValueOrNull(winRes, "Default");
            if (winResDark == null)
                winResDark = winResDefault;
            else
                winResLight = winResDefault;

            Themesp.Children.Add(new UWPTools.Themes.ThemeIcon(winResLight, winResDark));
            */

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
            }/* else if (sender == WindowsLightThemeButton)
            {
                settingsValues["theme-name"] = "windows";
                settingsValues["theme-shade"] = "Light";
            } else if (sender == WindowsDarkThemeButton)
            {
                settingsValues["theme-name"] = "windows";
                settingsValues["theme-shade"] = "Dark";
            }*/

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
            ApplicationData.Current.LocalSettings.Values["showUpdateInNavBar"] = ShowUpdateInNavBarCheckBox.IsChecked;
        }
    }
}
