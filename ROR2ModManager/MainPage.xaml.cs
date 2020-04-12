using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
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

        public static MainPage Instance;
        public MainPage()
        {
            Instance = this;
            this.InitializeComponent();
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;
        }

        private async void CoreWindow_CharacterReceived(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.CharacterReceivedEventArgs args)
        {
            if (args.KeyCode == 72) //H
            {
                await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
            }
            
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
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(Pages.Settings.Root));
            }

            SwitchToPageByTag(((NavigationViewItem)args.SelectedItem).Tag as string);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await ProfileManager.LoadProfilesFromFile();
        }
    }
}
