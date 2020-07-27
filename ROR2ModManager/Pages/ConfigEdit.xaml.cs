using Salaros.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;
using Windows.UI.Xaml.Controls.Primitives;
using UWPTools.Storage;
using Microsoft.UI.Xaml.Controls;
using Windows.System;
using Microsoft.AppCenter.Crashes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.Pages
{

    public class ConfigData
    {
        public string Name { get; set; }
        public ConfigParser Parser { get; set; }
        public IEnumerable<string> SectionNames { get; set; }
        public string ConfigPath { get; set; }
    }

    public class AllConfigItemsProfile : Profile { };

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigEdit : Page
    {
        public ConfigEdit()
        {
            this.InitializeComponent();
        }

        ObservableCollection<ConfigData> ConfigItems;
        ObservableCollection<Profile> Profiles;
        List<ConfigData> AllConfigItems;

        private async Task<StorageFolder> GetConfigFolderAsync()
        {
            try
            {
                var path = ApplicationData.Current.LocalSettings.Values["ror2"] as string;
                if (path is null)
                {
                    await ProfileManager.ChooseRoRInstallFolder(true);
                    path = ApplicationData.Current.LocalSettings.Values["ror2"] as string;
                    if (path is null)
                        return null;
                }
                return await (await StorageFolder.GetFolderFromPathAsync(path)).GetFolderAsync(@"BepInEx\config");
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            var configFolder = await GetConfigFolderAsync();
            if (configFolder is null)
            {
                MainPage.Current.contentFrame.Navigate(typeof(Pages.Play));
                return;
            }

            AllConfigItems = new List<ConfigData>();
            foreach (var configFile in await configFolder.GetFilesAsync())
            {
                try
                {
                    var parser = new ConfigParser(configFile.Path);
                    AllConfigItems.Add(new ConfigData
                    {
                        Name = configFile.Name,
                        SectionNames = from sec in parser.Sections select sec.SectionName,
                        Parser = parser,
                        ConfigPath = configFile.Path
                    });
                } catch (Exception ex)
                {
                    await new ContentDialog
                    {
                        Title = "Error",
                        Content = $"An exception ({ex.GetType().FullName}) occurred when loading config.",
                        IsSecondaryButtonEnabled = false,
                        PrimaryButtonText = "Close"
                    }.ShowAsync();
                    Crashes.TrackError(ex);
                    MainPage.Current.contentFrame.Navigate(typeof(Play));
                    return;
                }
                
            }

            ConfigItems = new ObservableCollection<ConfigData>(AllConfigItems);

            CfgTree.ItemsSource = ConfigItems;

            Profiles = new ObservableCollection<Profile>((await ProfileManager.GetProfiles()).Where(x => x.Name != "Vanilla").ToList());
            Profiles.Insert(0, new AllConfigItemsProfile { Name = "All", PacksLW = new API.LWPackageData[] { } });
            ProfilesPivot.ItemsSource = Profiles;
            ProfilesPivot.SelectionChanged += ProfilesPivot_SelectionChanged;
        }

        private void FilterListForProfile(Profile profile)
        {
            List<ConfigData> TempFiltered = new List<ConfigData>();

            if (profile is null)
            {
                TempFiltered = AllConfigItems;
            } else
            {
                foreach (var p in profile.PacksLW)
                {
                    var parts = p.full_name.Split('-', '_', '.');
                    foreach (var part in parts)
                    {
                        var configs = AllConfigItems.Where(x => x.Name.Split('.').Contains(part)).ToList();
                        TempFiltered.AddRange(configs);
                    }
                }
            }


            if (profile is AllConfigItemsProfile)
                TempFiltered = AllConfigItems;

            for (int i = ConfigItems.Count - 1; i >= 0; i--)
            {
                var item = ConfigItems[i];
                if (!TempFiltered.Contains(item))
                {
                    ConfigItems.Remove(item);
                }
            }

            /* Next, add back any Person objects that are included in TempFiltered and may 
            not currently be in PeopleFiltered (in case of a backspace) */

            foreach (var item in TempFiltered)
            {
                if (!ConfigItems.Contains(item))
                {
                    ConfigItems.Add(item);
                }
            }

        }

        private bool IsBool(string v) => v == "true" || v == "false";
        private bool IsInteger(string v)
        {
            return false; // Number box causes crash right now
            try { int.Parse(v); } catch { return false; }
            return true;
        }

        private void ProfilesPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) => FilterListForProfile(e.AddedItems[0] as Profile);

        private void CfgTree_ItemInvoked(object sender, SelectionChangedEventArgs e)
        {

            var configData = (sender as ListView).SelectedItem as ConfigData;
            if (configData == null)
                return;

            EditorStackPanel.Children.Clear();

            foreach(var sec in configData.Parser.Sections)
            {
                EditorStackPanel.Children.Add(new TextBlock { Text = sec.SectionName, Style = this.Resources["TitleTextBlockStyle"] as Style, Margin = new Thickness(0, 10, 0, 0) });
                foreach (var item in sec.Keys)
                {
                    var containerStackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
                    containerStackPanel.Children.Add(new TextBlock { Text = item.Name, MinWidth = 300, VerticalAlignment = VerticalAlignment.Center });
                    FrameworkElement inputElement;

                    if (IsBool(item.Content))
                    {
                        /*
                        inputElement = new ToggleButton { Content = item.Content, IsChecked = item.Content == "true", MinWidth = 300 };
                        (inputElement as ToggleButton).Checked += async (object _1, RoutedEventArgs _2) =>
                        {
                            await WriteConfiguration((inputElement as ToggleButton).IsChecked.ToString().ToLower(), sec.SectionName, item.Name, configData.ConfigPath, configData.Parser);
                            (inputElement as ToggleButton).Content = (_1 as ToggleButton).IsChecked.ToString().ToLower();
                        };*/
                        inputElement = new ToggleSwitch { IsOn = item.Content == "true", OnContent = "true", OffContent = "false", MinWidth = 300 };
                        (inputElement as ToggleSwitch).Toggled += async (object _1, RoutedEventArgs _2) =>
                        {
                            await WriteConfiguration((inputElement as ToggleSwitch).IsOn.ToString().ToLower(), sec.SectionName, item.Name, configData.ConfigPath, configData.Parser);
                        };
                    } else if (IsInteger(item.Content))
                    {
                        inputElement = new Microsoft.UI.Xaml.Controls.NumberBox { Value = int.Parse(item.Content)};
                        (inputElement as NumberBox).ValueChanged += async (NumberBox _1, NumberBoxValueChangedEventArgs _2) =>
                        {
                            await WriteConfiguration((inputElement as NumberBox).Value.ToString(), sec.SectionName, item.Name, configData.ConfigPath, configData.Parser);
                        };
                    } else
                    {
                        inputElement = new TextBox { Width = 200, Text = item.Content, MinWidth = 300 };
                        (inputElement as TextBox).TextChanged += async (object _1, TextChangedEventArgs _2) =>
                        {
                            await WriteConfiguration((inputElement as TextBox).Text, sec.SectionName, item.Name, configData.ConfigPath, configData.Parser);
                        };
                    }

                    containerStackPanel.Children.Add(inputElement);
                    try { EditorStackPanel.Children.Add(containerStackPanel); } catch { System.Diagnostics.Debug.WriteLine(item.Name); }
                    
                }
            }
        }

        private async Task WriteConfiguration(string Value, string sectionName, string keyName, string path, ConfigParser parser)
        {
            parser.SetValue(sectionName, keyName, Value);
            var tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(Guid.NewGuid().ToString() + ".cfg", CreationCollisionOption.ReplaceExisting);
            var newFile = await StorageFile.GetFileFromPathAsync(path);
            parser.Save(tempFile.Path);
            await tempFile.CopyAndReplaceAsync(newFile);
            _ = tempFile.DeleteAsync();
            
        }

        private async void Button_OpenConfig_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(await GetConfigFolderAsync());
        }
    }
}
