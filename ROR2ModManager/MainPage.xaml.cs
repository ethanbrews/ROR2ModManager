using Dwrandaz.AutoUpdateComponent;
using Microsoft.AppCenter.Analytics;
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
        [Obsolete("Use MainPage.Current.ApplicationUpdateInformation instead")]
        public UWPTools.Versions.VersionHelper VersionHelper = new UWPTools.Versions.VersionHelper(new Uri("http://ror2modman.ethanbrews.me/Forecast.appinstaller"));
        
        public UpdateInfo ApplicationUpdateInformation;

        public ContentDialog ChangelogDialog { private set; get; }
        public MainPage()
        {
            Current = this;
            this.InitializeComponent();
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;

            // Set title bar
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            Package package = Package.Current;
            AppTitle.Text = string.Format("{0} {1}.{2}.{3}.{4}", "Forecast", package.Id.Version.Major, package.Id.Version.Minor, package.Id.Version.Build, package.Id.Version.Revision);
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.CoreWindow.SizeChanged += (s, e) => UpdateAppTitle();
            coreTitleBar.LayoutMetricsChanged += (s, e) => UpdateAppTitle();

            nav.BackRequested += Nav_BackRequested;
            Window.Current.Activated += Current_Activated;

            ChangelogManager.AddChangelogForVersion("1.0.5.0", typeof(Changelogs._1_0_5_0));
            ChangelogManager.AddChangelogForVersion("1.0.6.0", typeof(Changelogs._1_0_6_0));
            ChangelogManager.AddChangelogForVersion("1.0.7.0", typeof(Changelogs._1_0_7_0));
            ChangelogManager.AddChangelogForVersion("1.0.8.0", typeof(Changelogs._1_0_8_0));

            try { _ = ChangelogManager.ShowChangelogForCurrentVersionIfNotPreviouslyShown(); }
            catch (UWPTools.Exceptions.ChangelogNotFoundForCurrentVersionException) { /* Ignore this - there's no changelog for this version */ }

            // Run auto-update and show the manual update button if an update can be installed.
            // The update section will return from the call early if an error occurs or for control flow.
            _ = Task.Run(async () =>
            {
                try
                {
                    var path = "http://ror2modman.ethanbrews.me/Forecast.appinstaller";
                    ApplicationUpdateInformation = await AutoUpdateManager.CheckForUpdatesAsync(path);
                    if (!ApplicationUpdateInformation.Succeeded)
                    {
                        System.Diagnostics.Debug.WriteLine("AutoUpdateManager Failed");
                        Analytics.TrackEvent(AnalyticsEventNames.DwrandazUpdateManager, new Dictionary<string, string> { { "event", "CheckFailed" }, { "message", ApplicationUpdateInformation.ErrorMessage.ToString() } });
                        return;
                    }

                    var shouldShowTip = ApplicationSettings.LastNavigationUpdateButtonTeachingTipShown.Value == 0;
                    ApplicationSettings.Increment_LastNavigationUpdateButtonTeachingTipShown();
                    System.Diagnostics.Debug.WriteLine($"ApplicationSettings.LastNavigationUpdateButtonTeachingTipShown.Value = {ApplicationSettings.LastNavigationUpdateButtonTeachingTipShown.Value}");
                    await UWPTools.Threads.ThreadHelper.RunInUIThread(() =>
                    {
                        if (ApplicationUpdateInformation.ShouldUpdate)
                            UpdateLauncherButton.Visibility = Visibility.Visible;
                        if (shouldShowTip && ApplicationUpdateInformation.ShouldUpdate)
                            UpdateLauncherButtonTeachingTip.IsOpen = true;
                    });
                } catch (Exception ex)
                {
                    Analytics.TrackEvent(AnalyticsEventNames.DwrandazUpdateManager, new Dictionary<string, string> { { "event", "UncaughtError" }, { "type", ex.GetType().ToString() }, { "message", ex.Message.ToString() } });
                }
            });
        }

        

        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                if (ApplicationSettings.UpdateAppAutomatically.Value && new Type[] { typeof(Pages.Settings.Root), typeof(Pages.Play) }.Contains(contentFrame.CurrentSourcePageType))
                {
                    // We can update now
                    _ = DoApplicationUpdateIfRequired();
                }
            }
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
            if (args.Handled)
                return;
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

        private async Task<bool> DoApplicationUpdateIfRequired()
        {
            if (!ApplicationUpdateInformation.ShouldUpdate)
            {
                System.Diagnostics.Debug.WriteLine("DoApplicationUpdateIfRequired() was called, but there is no update for the app!");
                return true;
            }
                
            var result = await AutoUpdateManager.TryToUpdateAsync(ApplicationUpdateInformation);
            if (!result.Succeeded)
            {
                UpdateLauncherButton.IsEnabled = true;
                await new ROR2ModManager.GenericDialogs.UpdateFailedDialog("Error Message: " + result.ErrorMessage).ShowAsync();
                Analytics.TrackEvent(AnalyticsEventNames.DwrandazUpdateManager, new Dictionary<string, string> { { "event", "UpdateFailed" }, { "message", result.ErrorMessage } });
                System.Diagnostics.Debug.WriteLine("AutoUpdateManager The update failed");
                return false;
            }
            return true;
        }

        private async void UpdateLauncherButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Analytics.TrackEvent(AnalyticsEventNames.AppUpdateTriggeredByUser, new Dictionary<string, string> { { "AppUpToDate", (!ApplicationUpdateInformation.ShouldUpdate).ToString() } });
            UpdateLauncherButton.IsEnabled = false;
            if (!await DoApplicationUpdateIfRequired())
                UpdateLauncherButton.IsEnabled = true;
                
        }
    }
}
