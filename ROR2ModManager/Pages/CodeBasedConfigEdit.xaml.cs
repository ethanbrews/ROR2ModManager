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
using Monaco.Editor;
using Monaco.Helpers;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.AppCenter.Crashes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CodeBasedConfigEdit : Page
    {
        public bool IsCurrentFileModified = false;
        private bool IgnoreSelectionChanged = false;
        private bool DeleteIconTapped = false;

        public CodeBasedConfigEdit()
        {
            this.InitializeComponent();
        }

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

        private void Editor_KeyDown(Monaco.CodeEditor sender, WebKeyEventArgs args) => IsCurrentFileModified = true;

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            var cfgf = await GetConfigFolderAsync();
            var configFiles = from f in await cfgf.GetFilesAsync() where f.FileType == ".cfg" select f;
            FilesList.ItemsSource = configFiles;
            if (configFiles.Count() > 0)
                FilesList.SelectedIndex = 0;
            else
                OutOfFiles();
        }

        private async Task WriteChangesToFile(StorageFile file = null)
        {
            if (file == null)
                file = FilesList.SelectedItem as StorageFile;

            await FileIO.WriteTextAsync(file, Editor.Text);
            IsCurrentFileModified = false;
        }

        private async void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            await WriteChangesToFile();
        }

        private async void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreSelectionChanged)
            {
                IgnoreSelectionChanged = false;
                return;
            }

            if (DeleteIconTapped)
            {
                DeleteIconTapped = false;
                IgnoreSelectionChanged = true;
                FilesList.SelectedItem = e.RemovedItems[0];
                return;
            }
                

            if (e.RemovedItems.Count > 0)
            {
                var result = GenericDialogs.FileClosedConfirmResult.Discard;
                if (IsCurrentFileModified)
                {
                    var d = new GenericDialogs.FileClosedConfirm();
                    await d.ShowAsync();
                    result = d.Result;
                }
                if (result == GenericDialogs.FileClosedConfirmResult.Cancel)
                {
                    IgnoreSelectionChanged = true;
                    FilesList.SelectedItem = e.RemovedItems[0];
                    return;
                }
                else if (result == GenericDialogs.FileClosedConfirmResult.Save)
                {
                    await WriteChangesToFile(e.RemovedItems[0] as StorageFile);
                }
            }
            if (e.AddedItems.Count() == 0)
            {
                OutOfFiles();
                return;
            }
            ReopenEditor();
            try
            {
                var sf = e.AddedItems[0] as StorageFile;
                System.Diagnostics.Debug.WriteLine($"Reading: {sf.Path}");

                var cc = await FileIO.ReadTextAsync(sf);
                Editor.Text = cc;
                IsCurrentFileModified = false;
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await new ContentDialog
                {
                    Title = "File editor error",
                    Content = new TextBlock { TextWrapping = TextWrapping.WrapWholeWords, Text = $"The file editor has encountered an error and will be closed. ({ex.ToString()})" },
                    IsSecondaryButtonEnabled = false,
                    PrimaryButtonText = "Close"
                }.ShowAsync();
                MainPage.Current.contentFrame.Navigate(typeof(Pages.Play));
                return;
            }
        }

        private void OutOfFiles()
        {
            EditorNoFilesMessage.Visibility = Visibility.Visible;
            foreach(var c in FileCommandBar.PrimaryCommands)
            {
                if (c is AppBarButton)
                {
                    (c as AppBarButton).IsEnabled = false;
                }
            }
        }

        private void ReopenEditor()
        {
            EditorNoFilesMessage.Visibility = Visibility.Collapsed;
            foreach (var c in FileCommandBar.PrimaryCommands)
            {
                if (c is AppBarButton)
                {
                    (c as AppBarButton).IsEnabled = true;
                }
            }
        }

        private async void CfgFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(await GetConfigFolderAsync());
        }

        private async void RevertConfirmation_Click(object sender, RoutedEventArgs e)
        {
            var sf = FilesList.SelectedItem as StorageFile;
            System.Diagnostics.Debug.WriteLine($"Reading: {sf.Path}");

            var cc = await FileIO.ReadTextAsync(sf);
            Editor.Text = cc;
            IsCurrentFileModified = false;
        }

        private void FontIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (sender as FrameworkElement).ContextFlyout.ShowAt(sender as FrameworkElement, new FlyoutShowOptions { Placement = FlyoutPlacementMode.Right });
            DeleteIconTapped = true;
        }

        private async void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            var file = ((sender as FrameworkElement).DataContext as StorageFile);
            await file.DeleteAsync();
            var index = FilesList.SelectedIndex -1;
            FilesList.ItemsSource = from f in FilesList.ItemsSource as IEnumerable<StorageFile> where f != file select f;
            if (index >= 0)
                FilesList.SelectedIndex = index;
            else if ((FilesList.ItemsSource as IEnumerable<StorageFile>).Count() > 0)
                FilesList.SelectedIndex = 0;
            else
                OutOfFiles();
        }

        private void FontIcon_PointerEntered(object sender, PointerRoutedEventArgs e) => Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);

        private void FontIcon_PointerExited(object sender, PointerRoutedEventArgs e) => Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);

        private void Editor_Loaded(object sender, RoutedEventArgs e)
        {
            EditorLoadingMessage.Visibility = Visibility.Collapsed;
        }
    }
}
