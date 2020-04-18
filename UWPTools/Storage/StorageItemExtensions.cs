using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace UWPTools.Storage
{
    public static class StorageItemExtensions
    {
        public static async Task<StorageFile> GetOrCreateStorageFileAsync(this StorageFolder storageFolder, string relativePath)
        {
            // Path.GetRelativePath converts / into \
            return await storageFolder.CreateFileAsync(Path.GetRelativePath(storageFolder.Path, relativePath), CreationCollisionOption.OpenIfExists);
        }

        public static async Task<StorageFolder> GetOrCreateStorageFolderAsync(this StorageFolder storageFolder, string relativePath)
        {
            // Path.GetRelativePath converts / into \
            return await storageFolder.CreateFolderAsync(Path.GetRelativePath(storageFolder.Path, relativePath), CreationCollisionOption.OpenIfExists);
        }

        public static bool StorageItemExists(this StorageFolder storageFolder, string relativePath)
        {
            var path = Path.Combine(storageFolder.Path, relativePath);
            return File.Exists(path) || Directory.Exists(path);
        }

        public static bool FileExists(this StorageFolder storageFolder, string relativePath)
        {
            return File.Exists(Path.Combine(storageFolder.Path, relativePath));
        }

        public static bool FolderExists(this StorageFolder storageFolder, string relativePath)
        {
            return Directory.Exists(Path.Combine(storageFolder.Path, relativePath));
        }

        public static async Task WriteTextAsync(this StorageFile sf, string text)
        {
            await FileIO.WriteTextAsync(sf, text);
        }

        public static async Task<string> GetMD5String(this StorageFile sf)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = await sf.OpenStreamForReadAsync())
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
