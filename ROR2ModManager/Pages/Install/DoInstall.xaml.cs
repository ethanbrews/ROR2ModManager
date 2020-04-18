using ROR2ModManager.API;
using System;
using System.Collections.Generic;
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
using Windows.Networking.BackgroundTransfer;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Compression;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.Pages.Install
{

    public class DoInstallParameters
    {
        public List<Package> Packages;
        public string ProfileName;
        private List<DownloadOperation> downloads = new List<DownloadOperation>();
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DoInstall : Page
    {

        private DoInstallParameters parameters;
        private int downloadsDone;
        private WebClient client;

        public DoInstall()
        {
            this.InitializeComponent();
            downloadsDone = 0;
        }

        

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            parameters = e.Parameter as DoInstallParameters;
            client = new WebClient();
            var tempdir = Windows.Storage.ApplicationData.Current.TemporaryFolder;

            //For each file, download zip to a file in temp dir named using pkg.full_name, each invalid char is shifted into the ASCII range [A-z];
            foreach (var pkg in parameters.Packages)
            {
                var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, pkg.full_name);
                if (!System.IO.Directory.Exists(path))
                {
                    await ApplicationData.Current.LocalFolder.CreateFolderAsync(pkg.full_name);
                    try { 
                        await Task.Run(async () => await InstallPackage(pkg.versions.Where((x) => x.version_number == pkg._selected_version).First().download_url, path));
                    } catch
                    {
                        await new ContentDialog {
                            Title = "HTTP error installing mods",
                            Content = "The profile was not installed"
                        }.ShowAsync();
                        return;
                    }
                } else
                    AddToDownloadsCount();
            }
                

        }

        private async Task InstallPackage(string uri, string path)
        {
            var tempPath = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, Path.GetFileName(path))+".zip";
            System.Diagnostics.Debug.WriteLine("Downloading " + uri);
            client.DownloadFile(uri, tempPath);
            ZipFile.ExtractToDirectory(
                tempPath,
                path
            );
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                AddToDownloadsCount();
            });
            
        }

        

        private void SetProgressBarValue()
        {
            ProgressBar.Value = (100*(downloadsDone/parameters.Packages.Count()));
        }

        private void AddToDownloadsCount() {
            DownloadAmount.Text = $"Downloaded {++downloadsDone}/{parameters.Packages.Count()}";
            SetProgressBarValue();
            if (downloadsDone == parameters.Packages.Count())
            {
                Task.Run(async () =>
                {
                    await ProfileManager.AddNewProfile(parameters.ProfileName, parameters.Packages.ToArray());
                    await Task.Delay(600);
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                        MainPage.Current.SwitchToPageByTag("Play");
                    });
                    
                });
                
            }
        }
    }
}
