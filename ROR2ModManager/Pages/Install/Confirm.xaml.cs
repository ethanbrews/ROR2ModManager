using ROR2ModManager.API;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.Pages.Install
{

    public class ConfirmParameters
    {
        public List<Package> Packages;
        public string DefaultName = null;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Confirm : Page
    {

        ObservableCollection<String> PackagesNames = new ObservableCollection<string>();
        ObservableCollection<String> ProfileNames = new ObservableCollection<string>();
        private ConfirmParameters parameters;
        public Confirm()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var args = e.Parameter as ConfirmParameters;
            parameters = args;

            foreach (var p in args.Packages)
                PackagesNames.Add($"{p.name} {p._selected_version} by {p.owner}");

            foreach (var p in ProfileManager.GetProfileNames())
                if (!ProfileNames.Contains(p) && p != "Vanilla")
                    ProfileNames.Add(p);

            if (args.DefaultName != null)
                ProfileNameBox.Text = args.DefaultName;
               
            ValidateInput();
        }


        private bool IsInputValid()
        {
            return ProfileNameBox.Text.ToUpper().All(c => "ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890 -_".Contains(c)); //The profile can have spaces
        }
        private void ValidateInput()
        {
            System.Diagnostics.Debug.WriteLine($"The ComboBox text = {ProfileNameBox.Text}");
            if (ProfileNameBox.Text == "Vanilla")
            {
                ProfileNameMessage.Text = "A profile cannot be named \"Vanilla\"";
                ProfileNameMessage.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                InstallNowButton.IsEnabled = false;
                return;
            }

            if (ProfileNameBox.Text == "")
            {
                ProfileNameMessage.Text = "Enter a name for the profile";
                ProfileNameMessage.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                InstallNowButton.IsEnabled = false;
            }
            
            if (IsInputValid())
            {
                ProfileNameMessage.Text = (ProfileManager.GetProfileNames().Contains(ProfileNameBox.Text) ? "This profile will be overwritten" : "A new profile will be created");
                ProfileNameMessage.Foreground = new SolidColorBrush((Windows.UI.Color)Application.Current.Resources["SystemBaseHighColor"]);
                InstallNowButton.IsEnabled = true;

            } else
            {
                ProfileNameMessage.Text = "The profile Name cannot contain special characters";
                ProfileNameMessage.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                InstallNowButton.IsEnabled = false;
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInputValid())
            {
                MainPage.Current.contentFrame.Navigate(typeof(Pages.Install.DoInstall), new Install.DoInstallParameters { Packages = parameters.Packages, ProfileName= ProfileNameBox.Text });
            } else
            {
                await new ContentDialog
                {
                    PrimaryButtonText = "Close",
                    Title = "[Debug]",
                    Content = "IsInputValid(); returned false"
                }.ShowAsync();
            }
            
        }

        private void ProfileNameComboBox_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            ValidateInput();
        }
    }
}
