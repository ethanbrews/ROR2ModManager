using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace ROR2ModManager
{

    [Serializable]
    public class Profile
    {
        public API.LWPackageData[] PacksLW { get; set; }
        public string Name { get; set; }
    }

    class ProfileManager
    {

        private static List<Profile> Profiles = new List<Profile>();

        public static async Task LoadProfilesFromFile()
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            try
            {
                Profiles = JsonConvert.DeserializeObject<List<Profile>>(await FileIO.ReadTextAsync(await localFolder.GetFileAsync("profiles.json")));
            } catch (Exception)
            {
                Profiles = new List<Profile>();
            }
            
        }

        public static async Task CopyStorageFolderAsync(StorageFolder src, StorageFolder dest) { 
            foreach(var f in await src.GetItemsAsync())
            {
                if (f.IsOfType(StorageItemTypes.File)) {
                    await (f as StorageFile).CopyAsync(dest, f.Name, NameCollisionOption.ReplaceExisting);
                } else
                {
                    var newFolder = await dest.CreateFolderAsync(f.Name, CreationCollisionOption.OpenIfExists);
                    await CopyStorageFolderAsync((f as StorageFolder), newFolder);
                }
            }
        }

        public static async Task StartGameForProfile(Profile profile)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            StorageFolder folder;
            try
            {

                folder = (await StorageFolder.GetFolderFromPathAsync(localSettings.Values["ror2"] as string));
            } catch (System.Exception ex)
            {
                if (ex is System.IO.IOException || ex is ArgumentNullException || true)
                {
                    await new ContentDialog
                    {
                        IsPrimaryButtonEnabled = true,
                        PrimaryButtonText = "Okay",
                        Title = "Locate RoR2 Install",
                        Content = "Please locate the RoR2 install folder.\nThis gives the application permission to install mods to the RoR2 folder."
                    }.ShowAsync();
                    var folderPicker = new Windows.Storage.Pickers.FolderPicker();
                    //folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Unspecified;
                    folderPicker.FileTypeFilter.Add("*");
                    folder = await folderPicker.PickSingleFolderAsync();
                    if (folder is null)
                    {
                        await new ContentDialog
                        {
                            IsPrimaryButtonEnabled = true,
                            PrimaryButtonText = "Close",
                            Title = "Can't find RoR2 Installation",
                            Content = "No folder was selected. The game cannot be launched."
                        }.ShowAsync();
                        return;
                    }

                    if (!System.IO.File.Exists(System.IO.Path.Combine(folder.Path, "Risk of Rain 2.exe")))
                    {
                        var result = await new ContentDialog
                        {
                            IsPrimaryButtonEnabled = true,
                            IsSecondaryButtonEnabled = true,
                            PrimaryButtonText = "Change Folder",
                            SecondaryButtonText = "Use this folder",
                            Title = "Are you sure this is correct?",
                            Content = "The selected folder doesn't contain \"Risk of Rain 2.exe\"\nAre you sure this is the correct folder?"
                        }.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            await StartGameForProfile(profile);
                        }
                    }

                    localSettings.Values["ror2"] = folder.Path;
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("ror2", folder);
                } else
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    //throw;
                }
            }

            // Do game install

            //Is game vanilla?
            if (profile.Name == "Vanilla")
            {
                System.Diagnostics.Debug.WriteLine("Starting Vanilla. Removing winhttp.dll");
                try
                {
                    var f = await folder.GetFileAsync("winhttp.dll");
                    await f.DeleteAsync();
                } catch
                {
                    // Assume winhttp.dll doesn't exist
                }
                System.Diagnostics.Debug.WriteLine("Running game via steam (ID 632360)");
            } else
            {
                System.Diagnostics.Debug.WriteLine("(1/3) Starting Modded Game. Installing missing bepinex files");
                var bepinex = await (await ApplicationData.Current.LocalFolder.GetFoldersAsync()).Where(x => x.Name.Contains("BepInExPack")).FirstOrDefault().GetFolderAsync("BepInExPack");
                await (await bepinex.GetFileAsync("winhttp.dll")).CopyAsync(folder, "winhttp.dll", NameCollisionOption.ReplaceExisting);
                await (await bepinex.GetFileAsync("doorstop_config.ini")).CopyAsync(folder, "doorstop_config.ini", NameCollisionOption.ReplaceExisting);
                var bepinexf = await bepinex.GetFolderAsync("BepInEx");

                // Copy [bepinexf] anyway because it might be missing subdirectories
                await CopyStorageFolderAsync(bepinexf, await folder.CreateFolderAsync("BepInEx", CreationCollisionOption.OpenIfExists));

                var plugins = await folder.GetFolderAsync(@"BepInEx\plugins");
                System.Diagnostics.Debug.WriteLine("(2/3) Installing missing mods for modded game");
                foreach (var mod in profile.PacksLW)
                {
                    if (!Directory.Exists(Path.Combine(plugins.Path, mod.full_name)) && mod.full_name != "bbepis-BepInExPack")
                    {
                        System.Diagnostics.Debug.WriteLine($" # Installing {mod.full_name}");
                        try
                        {
                            var modFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(mod.full_name);

                            bool CopyModFolder = true;

                            if (Directory.Exists(Path.Combine(modFolder.Path, "plugins")))
                            {
                                CopyModFolder = false;
                                var modPluginsFolder = await modFolder.GetFolderAsync("plugins");
                                var rorPluginsFolder = await folder.CreateFolderAsync(@"BepInEx\plugins\" + mod.full_name);
                                foreach (var f in await modPluginsFolder.GetItemsAsync())
                                {
                                    if (f.IsOfType(StorageItemTypes.File))
                                        await (f as StorageFile).CopyAsync(rorPluginsFolder, f.Name, NameCollisionOption.ReplaceExisting);
                                    else
                                        await CopyStorageFolderAsync((f as StorageFolder), await rorPluginsFolder.CreateFolderAsync(f.Name, CreationCollisionOption.OpenIfExists));
                                }
                                    
                            }

                            if (Directory.Exists(Path.Combine(modFolder.Path, "monomod")))
                            {
                                CopyModFolder = false;
                                var modMonoFolder = await modFolder.GetFolderAsync("monomod");
                                var rorMonoFolder = await folder.CreateFolderAsync(@"BepInEx\monomod\"+mod.full_name, CreationCollisionOption.OpenIfExists);
                                foreach (var f in await modMonoFolder.GetItemsAsync())
                                {
                                    if (f.IsOfType(StorageItemTypes.File))
                                        await (f as StorageFile).CopyAsync(rorMonoFolder, f.Name, NameCollisionOption.ReplaceExisting);
                                    else
                                        await CopyStorageFolderAsync((f as StorageFolder), await rorMonoFolder.CreateFolderAsync(f.Name, CreationCollisionOption.OpenIfExists));
                                }

                            }

                            if (CopyModFolder)
                            {
                                await CopyStorageFolderAsync(modFolder, await plugins.CreateFolderAsync(mod.full_name, CreationCollisionOption.OpenIfExists));
                            }

                        } catch (FileNotFoundException ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                            await new ContentDialog
                            {
                                Title = "Corrupt Mod Installation",
                                Content = new TextBlock { Text = "Right click the profile and then select \"Repair\"" },
                                PrimaryButtonText = "Close"
                            }.ShowAsync();
                            return;
                        }
                        
                    }
                }

                foreach(var f in await plugins.GetFoldersAsync())
                {
                    if (profile.PacksLW.Where(x => x.full_name == f.Name).FirstOrDefault() == null)
                    {
                        System.Diagnostics.Debug.WriteLine($" # Removing non-selected mod {f.Name}");
                        await f.DeleteAsync();
                    }
                }

                System.Diagnostics.Debug.WriteLine("(3/3) Running game via steam (ID 632360)");
            }

            //await Launcher.LaunchUriAsync(new Uri("steam://rungameid/632360"));
        }

        public static async Task SaveProfilesToFile()
        {
            var profiles = Profiles;
            var vanilla = profiles.Where(x => x.Name == "Vanilla").FirstOrDefault();
            if (vanilla != null)
                profiles.Remove(vanilla);
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            await FileIO.WriteTextAsync(await localFolder.CreateFileAsync("profiles.json", CreationCollisionOption.ReplaceExisting), JsonConvert.SerializeObject(profiles, Formatting.Indented));
        }

        public static string[] GetProfileNames()
        {

            var strs = new List<string>();
            strs.Add("Vanilla");
            foreach(var p in Profiles)
            {
                strs.Add(p.Name);
            }
            return strs.ToArray();
        }

        public static async Task<Profile[]> GetProfiles()
        {
            await LoadProfilesFromFile();
            var profilesCopy = Profiles;
            profilesCopy.Add(new Profile
            {
                Name = "Vanilla",
                PacksLW = new API.LWPackageData[0]
            });
            return profilesCopy.ToArray();
        }

        public static Profile GetProfileByName(string name)
        {
            return Profiles.Where((x) => x.Name == name).FirstOrDefault(null);
        }

        public static async Task AddNewProfile(string name, API.Package[] packages)
        {
            var lws = new API.LWPackageData[packages.Length];
            for(int i=0;i<packages.Length;i++)
            {
                lws[i] = packages[i].ConvertToLW();
            }

            if (Profiles == null) { Profiles = new List<Profile>(); }
            var profile = Profiles.Where(x => x.Name == name).FirstOrDefault();
            if (profile == null)
            {
                Profiles.Add(new Profile
                {
                    Name = name,
                    PacksLW = lws
                });
            } else
            {
                profile.PacksLW = lws;
            }

            

            await SaveProfilesToFile();
        }
    }
}
