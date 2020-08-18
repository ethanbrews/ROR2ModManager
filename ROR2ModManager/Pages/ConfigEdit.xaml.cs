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
using ROR2ModManager;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.Pages
{

    public class ConfigData
    {
       
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigEdit : Page
    {
        public ConfigEdit()
        {
            this.InitializeComponent();
        }

        class AllConfigItemsProfile : Profile { };

        public Dictionary<string, IniParser.Model.IniData> AllFiles;
        public Dictionary<string, IniParser.Model.IniData> Files;
        public List<Profile> Profiles;
        public IniParser.Model.IniData SelectedData;

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
            var newFiles = new Dictionary<string, IniParser.Model.IniData>();
            var folder = await GetConfigFolderAsync();
            foreach(var file in await folder.GetFilesAsync())
            {
                try
                {
                    var text = await FileIO.ReadTextAsync(file);
                    var cfg = Configuration.LoadConfig(text);
                    newFiles[file.DisplayName] = cfg;
                } catch (Exception ex)
                {
                    Crashes.TrackError(ex, new Dictionary<string, string> { { "name", file.Name } });
                }
                
            }

            var AllProfiles = (await ProfileManager.GetProfiles()).Where(x => x.Name != "Vanilla").ToList();
            AllProfiles.Insert(0, new AllConfigItemsProfile { Name = "All", PacksLW = new API.LWPackageData[] { } });
            Profiles = AllProfiles;

            AllFiles = newFiles;
            Files = newFiles;
            System.Diagnostics.Debug.WriteLine($"Loaded {Files.Count()} configuration files.");
            ModsList.ItemsSource = Files;
            ConfigItems.ItemsSource = SelectedData;
            ModsList.SelectedIndex = 0;
            OptionsPivot.ItemsSource = Profiles;
        }

        private void ModsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedData = ((KeyValuePair<string, IniParser.Model.IniData>)(sender as ListView).SelectedItem).Value;
            ConfigItems.ItemsSource = SelectedData.Sections;
        }

        private void FilterListForProfile(Profile profile)
        {
            var TempFiltered = new Dictionary<string, IniParser.Model.IniData>();

            if (profile == null)
            {
                TempFiltered = AllFiles;
            }
            else
            {
                foreach (var p in profile.PacksLW)
                {
                    var parts = p.full_name.Split('-', '_', '.');
                    foreach (var part in parts)
                    {
                        var configs = AllFiles.Where(x => x.Key.Split('.').Contains(part)).ToList();
                        foreach(var c in configs)
                        {
                            if (!TempFiltered.ContainsKey(c.Key))
                                TempFiltered.Add(c.Key, c.Value);
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Filtering config list from {Files.Count()} to {TempFiltered.Count()} items out of a possible {AllFiles.Count()}");


            if (profile is AllConfigItemsProfile)
                TempFiltered = AllFiles;

            for (int i = Files.Count - 1; i >= 0; i--)
            {
                var item = Files.Keys.ElementAt(i);
                if (!TempFiltered.ContainsKey(item))
                {
                    Files.Remove(item);
                }
            }

            /* Next, add back any Person objects that are included in TempFiltered and may 
            not currently be in PeopleFiltered (in case of a backspace) */

            foreach (var item in TempFiltered)
            {
                if (!Files.ContainsKey(item.Key))
                {
                    Files.Add(item.Key, item.Value);
                }
            }

        }

        private void OptionsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e) => FilterListForProfile(e.AddedItems[0] as Profile);


        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            var cfgDir = await GetConfigFolderAsync();
            foreach(var f in AllFiles)
            {
                string cfgs = Configuration.WriteConfig(f.Value);
                await (await cfgDir.GetOrCreateStorageFileAsync($"{f.Key}.cfg")).WriteTextAsync(cfgs);
            }
        }

        private async void ConfigFolderButton_Click(object sender, RoutedEventArgs e) => await Launcher.LaunchFolderAsync(await GetConfigFolderAsync());
    }


    public class ConfigEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate IntTemplate { get; set; }
        public DataTemplate BoolTemplate { get; set; }

        public new DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Null value can be passed by IDE designer
            if (item == null)
            {
                System.Diagnostics.Debug.WriteLine("Parameter 'item' for ConfigEditorTemplateSelector#SelectTemplate is null!");
                return null;
            } else if (!(item is IniParser.Model.KeyData))
            {
                System.Diagnostics.Debug.WriteLine("Parameter 'item' for ConfigEditorTemplateSelector#SelectTemplate is not an instance of IniParser.Model.KeyData !");
                return null;
            }

            var data = item as IniParser.Model.KeyData;
            System.Diagnostics.Debug.WriteLine($"Selecting DataTemplate for {data.KeyName} = {data.Value};");

            try
            {
                Convert.ToInt32(data.Value);
                return IntTemplate;
            }
            catch { }
            try
            {
                Convert.ToBoolean(data.Value);
                return BoolTemplate;
            }
            catch { }
            return StringTemplate;
        }
    }

    enum DataTypes
    {
        STRING,
        INT,
        BOOL,
        INVALID
    }

    class TypeChecker
    {
        public static DataTypes GetDataTypeForValue(object item)
        {
            try
            {
                Convert.ToInt32(item);
                return DataTypes.INT;
            }
            catch { }
            try
            {
                Convert.ToBoolean(item);
                return DataTypes.BOOL;
            }
            catch { }
            return DataTypes.STRING;
        }
    }

    class StringValueVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => TypeChecker.GetDataTypeForValue(value) == DataTypes.STRING ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }

    class BoolValueVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => TypeChecker.GetDataTypeForValue(value) == DataTypes.BOOL ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }

    class IntValueVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => TypeChecker.GetDataTypeForValue(value) == DataTypes.INT ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}
