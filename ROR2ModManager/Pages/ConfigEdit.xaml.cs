using Salaros.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ROR2ModManager.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigEdit : Page
    {
        public ConfigEdit()
        {
            this.InitializeComponent();
        }

        private async Task<StorageFolder> GetConfigFolderAsync()
        {
            try
            {
                return await (await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalSettings.Values["ror2"] as string)).GetFolderAsync(@"BepInEx\config");
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {

            var ConfigFolder = await GetConfigFolderAsync();
            if (ConfigFolder == null)
                return;

            foreach(var file in await ConfigFolder.GetFilesAsync())
            {
                var root = new TreeViewNode { Content = file.DisplayName };
                var config = new ConfigParser(file.Path);
                foreach (var sec in config.Sections)
                {
                    var secItem = new TreeViewNode { Content = sec.SectionName };
                    foreach (var item in sec.Keys )
                    {
                        var contentSp = new StackPanel();
                        var name = new TextBlock { Text = item.Name };
                        var value = new TextBlock { Text = item.ValueRaw.ToString(), Style = this.Resources["CaptionTextBlockStyle"] as Style };
                        contentSp.Children.Add(name);
                        contentSp.Children.Add(value);

                        var treeItem = new TreeViewNode
                        {
                            Content = $"{item.Name} = {item.ValueRaw.ToString()}"
                        };
                        secItem.Children.Add(treeItem);

                        
                       
                    }
                    root.Children.Add(secItem);
                }

                CfgTree.RootNodes.Add(root);
                
                
            }

        }

        private async void SetStringEditor(string file, string section, string name, string initialValue, Action<string> updateTreeAction)
        {
            ValueSelector.Children.Clear();
            ValueSelector.Children.Add(new TextBlock { Text = name });

            var textBox = new TextBox { Text = initialValue, Width = 200 };
            var ConfigFolder = await GetConfigFolderAsync();
            if (ConfigFolder == null)
            {
                ValueSelector.Children.Clear();
                ValueSelector.Children.Add(new TextBlock { Text = "Config folder missing." });
                return;
            }
            var configPath = (await ConfigFolder.GetFileAsync(file)).Path;

            textBox.KeyUp += async (object sender, KeyRoutedEventArgs e) =>
            {
                var config = new ConfigParser(configPath);
                config.Sections.Where(x => x.SectionName == section).First().Keys.Where(x => x.Name == name).First().ValueRaw = textBox.Text;
                config.Save(configPath);
                updateTreeAction($"{name} = {textBox.Text}");
            };

            ValueSelector.Children.Add(textBox);
        }

        private async void SetBoolEditor(string file, string section, string name, bool initialValue, Action<string> updateTreeAction)
        {
            ValueSelector.Children.Clear();
            ValueSelector.Children.Add(new TextBlock { Text = name });

            var toggleButton = new ToggleButton { Content = initialValue.ToString(), IsEnabled=initialValue };
            var ConfigFolder = await GetConfigFolderAsync();
            if (ConfigFolder == null)
            {
                ValueSelector.Children.Clear();
                ValueSelector.Children.Add(new TextBlock { Text = "Config folder missing." });
                return;
            }
            var configPath = (await ConfigFolder.GetFileAsync(file)).Path;
            toggleButton.Tapped += async (object sender, TappedRoutedEventArgs e) =>
            {
                toggleButton.Content = toggleButton.IsChecked.ToString();
                var config = new ConfigParser(configPath);

                var sect = config.Sections.Where(x => x.SectionName == section).First();
                var item = sect.Keys.Where(x => x.Name == name).First();
                item.ValueRaw = toggleButton.IsChecked;
                var tempfile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);
                config.Save(tempfile.Path);
                await tempfile.CopyAndReplaceAsync(await StorageFile.GetFileFromPathAsync(configPath));
                updateTreeAction($"{name} = {toggleButton.IsChecked.ToString().ToLower()}");
            };

            ValueSelector.Children.Add(toggleButton);
        }

        private void CfgTree_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            var item = (args.InvokedItem as TreeViewNode);

            if (item.HasChildren)
                return;

            var name = item.Content.ToString().Split(" = ")[0];
            var value = item.Content.ToString().Split(" = ")[1];
            var section = item.Parent.Content.ToString();
            var file = item.Parent.Parent.Content.ToString();

            if (value == "true" || value == "false")
                SetBoolEditor(file + ".cfg", section, name, (value == "true"), (string v) => item.Content = v);
            else
                SetStringEditor(file + ".cfg", section, name, value, (string v) => item.Content = v);

        }
    }
}
