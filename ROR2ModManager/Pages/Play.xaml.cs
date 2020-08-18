using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.Storage;
using System.Runtime.Serialization.Formatters.Binary;
using Windows.System;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using System.Threading;
using Windows.UI.Core;
using Microsoft.AppCenter.Crashes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.Pages
{

    public class PlayParameters
    {
        public ContentDialog contentDialogToShow;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Play : Page
    {

        public ObservableCollection<Profile> Profiles = new ObservableCollection<Profile>();

        public Play()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var args = e.Parameter as PlayParameters;

            if (args?.contentDialogToShow != null)
                await args.contentDialogToShow.ShowAsync();

            ReloadProfiles();
        }

        private async void ReloadProfiles()
        {
            Profiles.Clear();
            await ProfileManager.LoadProfilesFromFile();
            foreach (var profile in await ProfileManager.GetProfiles())
            {
                profile._IsVanilla = profile.Name == "Vanilla";
                if (profile._IsVanilla)
                    profile.PacksLW = new API.LWPackageData[] { new API.LWPackageData { full_name = "Prismatic Trials", version = null }, new API.LWPackageData { full_name = "Eclipse", version = null } };
                Profiles.Add(profile);
                System.Diagnostics.Debug.WriteLine($"Displaying profile: {profile.Name}");
            }
            PacksGridView.ItemsSource = Profiles;
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var p = (sender as Button).DataContext as Profile;
            await ProfileManager.DeleteProfile(p);
            _ = Task.Run(() =>
            {
                Thread.Sleep(300);
                _ = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => ReloadProfiles());
            });
            //MainStackPanel.Children.Remove(MainStackPanel.Children.Where(x => (x as FrameworkElement).Tag as string == p.Name as string).First());
        }

        private async void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            var profile = (sender as Button).DataContext as Profile;
            var result = await new ContentDialog
            {
                Title = "Share " + profile.Name,
                Content = "Share this profile by either sending them the file or\nhosting the file online.\nTo add this profile, use the 'Import Profile' button on the Play page",
                PrimaryButtonText = "View profile in explorer",
                PrimaryButtonStyle = this.Resources["AccentButtonStyle"] as Style,
                SecondaryButtonText = "Cancel"
            }.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("export", CreationCollisionOption.OpenIfExists);
                var file = await folder.CreateFileAsync(profile.Name + ".r2pm", CreationCollisionOption.ReplaceExisting);
                BinaryFormatter bf = new BinaryFormatter();
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    bf.Serialize(stream, profile);
                }
                await Launcher.LaunchFolderAsync(folder);
            }
        }

        private void CheckUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            var ctxt = ((sender as Button).DataContext as Profile);
            MainPage.Current.contentFrame.Navigate(typeof(Install.Select), new Install.SelectParameters { packagesLW = ctxt.PacksLW, DefaultName = ctxt.Name });
        }

        private async void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            try
            {
                await ProfileManager.StartGameForProfile((Profile)button.DataContext);
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await new ContentDialog
                {
                    Title = "Error launching game",
                    Content = new TextBlock { TextWrapping = TextWrapping.WrapWholeWords, Text = $"An error occurred launching the game. Ensure both Risk of Rain 2 and Steam are installed. If the game can be launched via steam, try updating the mods. (Error: {ex.GetType()})" },
                    IsSecondaryButtonEnabled = false,
                    PrimaryButtonText = "Close"
                }.ShowAsync();
            }
            button.IsEnabled = true;

        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ((sender as FrameworkElement).FindName("slide") as Storyboard).Begin();
        }

        private void MarqueeStackPanelContainer_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ApplicationSettings.UseMarqueeEffectForMods.Value)
            {
                (sender as FrameworkElement).Visibility = Visibility.Collapsed;
                return;
            }
            (sender as FrameworkElement).Visibility = Visibility.Visible;

            var width = ((sender as FrameworkElement).FindName("ModsList") as FrameworkElement).ActualWidth;
            var displayWidth = (sender as StackPanel).ActualWidth - ((sender as StackPanel).Padding.Left + (sender as StackPanel).Padding.Right);
            var storyboard = ((sender as FrameworkElement).FindName("MarqueeStoryBoard") as Storyboard);
            var control = ((sender as FrameworkElement).FindName("ModsList") as ItemsControl);
            var animation = new DoubleAnimation();
            animation.From = displayWidth;
            animation.To = -width;
            animation.Duration = new Duration(TimeSpan.FromSeconds(width / 50));
            animation.RepeatBehavior = RepeatBehavior.Forever;



            Storyboard.SetTarget(storyboard, control);
            Storyboard.SetTargetProperty(storyboard, "(Canvas.Left)");

            storyboard.Children.Add(animation);
            if (control.ActualWidth > displayWidth)
                storyboard.Begin();
        }
    }
}
