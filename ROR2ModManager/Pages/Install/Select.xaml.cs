using MetroLog;
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
        public string DefaultName = null;
        public string ProtocolName = null;
        public string ProtocolUri = null;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Select : Page
    {

        private Package[] Packages;
        private ObservableCollection<Package> PackagesFiltered;
        private SelectParameters NavigatedToParameters;

        public Select()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigatedToParameters = (e.Parameter as SelectParameters);
            InstallButton.IsEnabled = false;
            _ = Task.Run(async () =>
            {
                var PackagesLocal = await ApiAccess.GetPackages();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => {
                    Packages = PackagesLocal;
                    PackagesFiltered = new ObservableCollection<Package>(Packages);
                    ModListView.ItemsSource = PackagesFiltered;

                    if ((e.Parameter as SelectParameters)?.packagesLW != null)
                    {
                        foreach (var packlw in (e.Parameter as SelectParameters).packagesLW)
                        {
                            var pack = Packages.Where(x => x.full_name == packlw.full_name).FirstOrDefault();
                            if (pack is null)
                            {
                                await new ContentDialog { Title = "Pack not found", Content = "The pack {packlw.full_name}\ncannot be found on thunderstore.io and may be deleted or moved.", PrimaryButtonText = "Close", IsSecondaryButtonEnabled = false }.ShowAsync();
                                MainPage.Current.contentFrame.Navigate(typeof(Pages.Play));
                                return;
                            }
                            pack._is_selected = true;
                            pack._selected_version = packlw.version;
                            pack.markDirty();
                        }
                        InstallButton.IsEnabled = true;
                    }
                });
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterList();
        }

        private void FilterList()
        {
            List<Package> TempFiltered;

            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            try
            {
                TempFiltered = Packages.Where(pkg => pkg.full_name.Contains(FilterTextBox.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            } catch (ArgumentNullException)
            {
                TempFiltered = Packages.ToList();
            }
            
            

            /* Go through TempFiltered and compare it with the current PeopleFiltered collection,
            adding and subtracting items as necessary: */

            // First, remove any Person objects in PeopleFiltered that are not in TempFiltered
            for (int i = PackagesFiltered.Count - 1; i >= 0; i--)
            {
                var item = PackagesFiltered[i];
                if (!TempFiltered.Contains(item))
                {
                    PackagesFiltered.Remove(item);
                }
            }

            /* Next, add back any Person objects that are included in TempFiltered and may 
            not currently be in PeopleFiltered (in case of a backspace) */

            foreach (var item in TempFiltered)
            {
                if (!PackagesFiltered.Contains(item))
                {
                    PackagesFiltered.Add(item);
                }
            }
        }

        private API.Version GetSelectedVersionForPackage(Package package)
        {
            return package.versions.Where(x => x.version_number == package._selected_version).First();
        }

        private List<Package> GetDependenciesForPackage(Package package)
        {
            var result = new List<Package>();
            var dependencies = GetSelectedVersionForPackage(package).dependencies;
            foreach(var dep in dependencies)
            {
                foreach(var p in Packages)
                {
                    if (p.versions.Where(x => x.full_name == dep as string).FirstOrDefault() != null || p.full_name == dep as string)
                    {
                        result.Add(p);
                    }
                }
            }
            return result;
        }

        private void CheckDependencies()
        {
            var FoundDependencies = new List<Package>();
            foreach(var p in Packages)
            {
                if (p._is_selected_by_user)
                    FoundDependencies.AddRange(GetDependenciesForPackage(p));
            }

            foreach(var p in Packages)
            {
                if (FoundDependencies.Contains(p))
                {
                    p._is_selected = true;
                    p._is_dependency = true;
                } else
                {
                    p._is_dependency = false;
                    if (!p._is_selected_by_user)
                        p._is_selected = false;
                }
                p.markDirty();
            }

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var cb = (sender as CheckBox);
            (cb.DataContext as Package)._is_selected_by_user = cb.IsChecked.GetValueOrDefault(false);
            CheckDependencies();
            InstallButton.IsEnabled = (Packages.Where(x => x._is_selected).FirstOrDefault() != null);
        }

        private void VersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var parameters = new ConfirmParameters { Packages = Packages.Where(x => x._is_selected).ToList(), DefaultName = NavigatedToParameters?.DefaultName };
            MainPage.Current.contentFrame.Navigate(typeof(Pages.Install.Confirm), parameters);
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterList();
        }
    }
}
