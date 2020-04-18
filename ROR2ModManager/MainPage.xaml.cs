using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using UWPTools.Changelog;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Store;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ROR2ModManager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public static MainPage Current;
        public UWPTools.Changelog.ChangelogManager ChangelogManager = new UWPTools.Changelog.ChangelogManager();
        public UWPTools.Versions.VersionHelper VersionHelper = new UWPTools.Versions.VersionHelper(new Uri("http://ror2modman.ethanbrews.me/RoR2ModMan.appinstaller"));

        public ContentDialog ChangelogDialog { private set; get; }
        public MainPage()
        {
            Current = this;
            this.InitializeComponent();
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;

            // Set title bar
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            Package package = Package.Current;
            AppTitle.Text = string.Format("{0} {1}.{2}.{3}.{4}", package.DisplayName, package.Id.Version.Major, package.Id.Version.Minor, package.Id.Version.Build, package.Id.Version.Revision);
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.CoreWindow.SizeChanged += (s, e) => UpdateAppTitle();
            coreTitleBar.LayoutMetricsChanged += (s, e) => UpdateAppTitle();

            nav.BackRequested += Nav_BackRequested;

            ChangelogManager.AddChangelogForVersion("1.0.5.0", typeof(Changelogs._1_0_5_0));

            try { _ = ChangelogManager.ShowChangelogForCurrentVersionIfNotPreviouslyShown(); }
            catch (UWPTools.Exceptions.ChangelogNotFoundForCurrentVersionException) { /* Ignore this - there's no changelog for this version */ }
                

            // This adds an 'Update Now' button to the nav bar and updates [IsApplicationUpToDate] so that the app can know if it's running the latest version
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!await VersionHelper.IsApplicationUpToDateAsync())
                        await UWPTools.Threads.ThreadHelper.RunInUIThread(() => UpdateLauncherButton.Visibility = Visibility.Visible);
                } catch
                {}
            });
        }

        private void UpdateAppTitle()
        {
            var full = (ApplicationView.GetForCurrentView().IsFullScreenMode);
            var left = 12 + (full ? 0 : CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset);
            AppTitle.Margin = new Thickness(left, 8, 0, 0);
        }

        private void Nav_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (contentFrame.CanGoBack)
                contentFrame.GoBack();

            nav.IsBackEnabled = contentFrame.CanGoBack;
        }

        private void CoreWindow_CharacterReceived(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.CharacterReceivedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("KeyDown: " + args.KeyCode.ToString());
            if (args.KeyCode == 48)
                contentFrame.Navigate(typeof(Pages.Settings.Root));
            else if (args.KeyCode == 49)
                SwitchToPageByTag("Play");
            else if (args.KeyCode == 50)
                SwitchToPageByTag("Install");
        }

        public void SwitchToPageByTag(string tag)
        {
            switch (tag)
            {
                case "Play":
                    contentFrame.Navigate(typeof(Pages.Play));
                    
                    break;
                case "Install":
                    contentFrame.Navigate(typeof(Pages.Install.Select));
                    break;
                case "Config":
                    contentFrame.Navigate(typeof(Pages.ConfigEdit));
                    break;
            }
            nav.SelectedItem = nav.MenuItems.Where(x => (x as FrameworkElement).Tag as string == tag).FirstOrDefault();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(Pages.Settings.Root));
            }

            SwitchToPageByTag(((NavigationViewItem)args.SelectedItem).Tag as string);
            nav.IsBackEnabled = contentFrame.CanGoBack;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ProfileManager.LoadProfilesFromFile();
            SwitchToPageByTag("Play");
        }

        private async void UpdateLauncherButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-appinstaller:?source=http://ror2modman.ethanbrews.me/RoR2ModMan.appinstaller"));
        }
    }
}
