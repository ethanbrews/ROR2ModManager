using Microsoft.Toolkit.Uwp.Helpers;
using ROR2ModManager.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
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

    //[ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, targetType, parameter, language);
        }

        #endregion
    }


    public class SelectParameters
    {
        public LWPackageData[] packagesLW;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Select : Page
    {
        //Packages shown in ListView
        ObservableCollection<Package> Packages = new ObservableCollection<Package>();
        //All Avaliable packages. This is cached in ApplicationData.Current.LocalFolder:ThunderstorePackagesCache.dat
        Package[] AllPackages = new Package[0];
        //Packages ticked by the user are stored here to be passed to next Page.
        List<Package> SelectedPackages = new List<Package>();

        private string lastSearchQuery; // Quick fix so that holding shift doesn't refresh the list. Should maybe use a custom ObservableCollection that does this check?

        public Select()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Loads the packages cached in ApplicationData.Current.LocalFolder:ThunderstorePackagesCache.dat
        /// to AllPackages using BinaryFormatter
        /// </summary>
        /// <returns></returns>
        public async Task LoadLocalPackageIndex()
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            if (!await localFolder.FileExistsAsync("ThunderstorePackagesCache.dat"))
                return;
            BinaryFormatter bf = new BinaryFormatter();

            try
            {
                using (var stream = await (await localFolder.GetFileAsync("ThunderstorePackagesCache.dat")).OpenStreamForReadAsync())
                {
                    AllPackages = (Package[])bf.Deserialize(stream);
                }
            } catch (SerializationException)
            {
                return;
            }



        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //await LoadLocalPackageIndex();
            //FilterPackages("");
            try
            {
                await PopulatePackagesFromWeb();
            }
            catch (System.Net.WebException)
            {
                System.Diagnostics.Debug.WriteLine("Error loading packages from web");
            }

            FilterPackages("");
            //await CacheAllPackages();
            
            if ((e.Parameter as SelectParameters)?.packagesLW != null)
            {
                foreach(var packlw in (e.Parameter as SelectParameters).packagesLW)
                {
                    var pack = Packages.Where(x => x.full_name == packlw.full_name).FirstOrDefault();
                    if (pack is null)
                    {
                        await new ContentDialog { Title = "Pack not found", Content = "The pack {packlw.full_name}\ncannot be found on thunderstore.io and may be deleted or moved.", PrimaryButtonText = "Close", IsSecondaryButtonEnabled = false }.ShowAsync();
                        MainPage.Instance.contentFrame.Navigate(typeof(Pages.Play));
                        return;
                    }
                    pack._is_selected = true;
                    pack._selected_version = packlw.version;
                    pack.markDirty();
                } 
            }
        }

        /// <summary>
        /// Loads packages from thunderstore.io and merges with AllPackages.
        /// </summary>
        /// <returns></returns>
        public async Task PopulatePackagesFromWeb()
        {
            Package[] WebPackages = await ApiAccess.GetPackages();
            AllPackages = AllPackages.Union(WebPackages).ToArray();
        }

        public async Task CacheAllPackages()
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            BinaryFormatter bf = new BinaryFormatter();
            using (var stream = await (await localFolder.CreateFileAsync("ThunderstorePackagesCache.dat", Windows.Storage.CreationCollisionOption.ReplaceExisting)).OpenStreamForWriteAsync())
            {
                bf.Serialize(stream, AllPackages);
            }
        }

        public async void FilterPackages(string input="")
        {
            input = input.ToLower();
            var hits = AllPackages.Where((x) => x.full_name.ToLower().Contains(input)).ToList();
            Packages.Clear();
            hits.ForEach((x) => Packages.Add(x));
        }

        

        private void Page_Loading(FrameworkElement sender, object args)
        {
            
            
        }

        private void TextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var text = ((TextBox)sender).Text;
            if (text != lastSearchQuery)
            {
                FilterPackages(text);
                lastSearchQuery = text;
            }
                
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            await new PackInfo().ShowAsync();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.Instance.contentFrame.Navigate(typeof(Pages.Install.Confirm), new Install.ConfirmParameters { Packages = SelectedPackages });
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            
            Package item = (Package)(sender as CheckBox).DataContext;
            if (SelectedPackages.Contains(item) && !((CheckBox)sender).IsChecked.Value)
            {
                SelectedPackages.Remove(item);
                var selectedVersion = item.versions.Where(x => (x.version_number == item._selected_version)).First();
                foreach(var dependencyItemName in selectedVersion.dependencies)
                {
                    var isStillRequired = false;
                    foreach(var selectedPackage in SelectedPackages)
                    {
                        var version = selectedPackage.versions.Where(x => x.version_number == selectedPackage._selected_version).First();
                        var modsList = new List<Package>();
                        foreach (var v in version.dependencies)
                        {
                            foreach (var p in SelectedPackages)
                            {
                                //if (p.versions.Where(x => x.full_name == v as string).FirstOrDefault() != null)
                                if (p.versions.Where(x => x.full_name == v as string).FirstOrDefault() != null)
                                    isStillRequired = true;
                                //System.Diagnostics.Debug.WriteLine($"[{string.Join(", ", p.versions)}] doesn't contain \"{v}\"");
                            }
                        }

                    }

                    if (!isStillRequired)
                    {
                        foreach(var p in Packages)
                        {
                            var version = p.versions.Where(x => x.full_name == dependencyItemName as string).FirstOrDefault();
                            if (version != null)
                            {
                                SelectedPackages.Remove(p);
                                p._is_dependency = false;
                                p._is_selected = false;
                                p.markDirty();
                            }
                                
                        }
                    }
                }
                
            } else if (!SelectedPackages.Contains(item) && ((CheckBox)sender).IsChecked.Value)
            {
                SelectedPackages.Add(item);
                var version = item.versions.Where(x => (x.version_number == item._selected_version)).First();
                foreach(var dependency in version.dependencies)
                {
                    Package dep = null;
                    foreach (var p in Packages)
                    {
                        var depv = p.versions.Where(x => x.full_name == dependency as string).FirstOrDefault();
                        if (depv != null)
                        {
                            dep = p;
                            p._selected_version = depv.version_number;
                            continue;
                        }
                    }

                    if (dep is null)
                        continue;
                    
                    SelectedPackages.Add(dep);
                    dep._is_selected = true;
                    dep._is_dependency = true;
                    dep.markDirty();
                }
                    
            }
                
        }

        private void VersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = ((string)((ComboBox)sender).SelectedValue);
            var pkg = (Package)(sender as ComboBox).DataContext;

            pkg._selected_version = selected;
        }
    }
}
