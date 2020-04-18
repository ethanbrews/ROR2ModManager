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

        //public ObservableCollection<Profile> Profiles = new ObservableCollection<Profile>();

        public Play()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var args = e.Parameter as PlayParameters;

            if (args?.contentDialogToShow != null)
                await args.contentDialogToShow.ShowAsync();

            await ProfileManager.LoadProfilesFromFile();
            foreach (var p in await ProfileManager.GetProfiles())
            {
                var content = new StackPanel
                {
                    Height = 90,
                    Margin = new Thickness(8),
                    Tag = p.Name,
                    Padding = new Thickness(10, 5, 5, 5),
                    BorderThickness = new Thickness(3),
                    BorderBrush = this.Resources["SystemControlBackgroundListMediumRevealBorderBrush"] as Brush
                };
                var buttons = new StackPanel { Orientation = Orientation.Horizontal };
                content.Children.Add(new TextBlock { Text = p.Name, Style = this.Resources["TitleTextBlockStyle"] as Style, Margin = new Thickness(0, 0, 0, 4) });
                
                // Buttons
                
                var playbtn = new Button
                {
                    Content = "Play with this Profile",
                    Style = this.Resources["AccentButtonStyle"] as Style
                };
                playbtn.Click += this.Button_Play_Click;

                var checkUpdateBtn = new Button
                {
                    Content = "Update all mods",
                    Margin = new Thickness(0, 5, 0, 0)
                };
                checkUpdateBtn.Click += CheckUpdateBtn_Click;

                var exportBtn = new Button { Content = "Share this Profile" };
                exportBtn.Click += ExportBtn_Click;

                var deleteBtn = new Button { Content = "Delete this profile", Margin = new Thickness(0, 5, 0, 0) };
                deleteBtn.Click += DeleteBtn_Click;

                // Add buttons to parents

                var flyoutStackPanel = new StackPanel();
                var flyout = new Flyout();
                flyout.Content = flyoutStackPanel;

                var triggerFlyoutButton = new Button
                {
                    Content = "More...",
                    Margin = new Thickness(10, 0, 0, 0)
                };
                triggerFlyoutButton.Click += (object sender, RoutedEventArgs args1) =>
                {
                    content.ContextFlyout.ShowAt(content);
                };


                buttons.Children.Add(playbtn);
                if (p.Name != "Vanilla") { 
                    buttons.Children.Add(triggerFlyoutButton);
                    content.ContextFlyout = flyout;
                } else
                {
                    content.Children.Add(new TextBlock { Text = "When playing on Vanilla, BepInEx is disabled and prismatic trials are available", Margin = new Thickness(0, 0, 0, 4)});
                    content.Height += 20;
                }
                content.Children.Add(buttons);


                flyoutStackPanel.Children.Add(exportBtn);
                flyoutStackPanel.Children.Add(checkUpdateBtn);
                flyoutStackPanel.Children.Add(deleteBtn);

                content.DataContext = p;
                
                MainStackPanel.Children.Add(content);
            }

            //System.Diagnostics.Debug.WriteLine($"Loaded {this.Profiles.Count()} profiles");
        }

        private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var p = (sender as Button).DataContext as Profile;
            await ProfileManager.DeleteProfile(p);
            MainStackPanel.Children.Remove(MainStackPanel.Children.Where(x => (x as FrameworkElement).Tag as string == p.Name as string).First());
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
            var packslw = ((sender as Button).DataContext as Profile).PacksLW;
            MainPage.Current.contentFrame.Navigate(typeof(Install.Select), new Install.SelectParameters { packagesLW = packslw });
        }

        private async void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            await ProfileManager.StartGameForProfile((Profile)button.DataContext);
            button.IsEnabled = true;
        }

        private async void Button_Update_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
