using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace ROR2ModManager.Download
{
    class FileDownloader
    {
        public async void StartDownloadsForMods()
        {
            BackgroundDownloader downloader = new BackgroundDownloader();

            /*
            List<Task> downloadCompletionTasks = new List<Task>();
            foreach (FileInfo info in infoFiles)
            {
                StorageFile file = /* ... * /;

                DownloadOperation op = downloader.CreateDownload(new Uri(info.Url), file);

                // Consider moving this line into HandleDownloadAsync so you won't have to repeat
                // it in the code that's handling the GetCurrentDownloadsAsync logic.
                activeDownloads.Add(op);

                // Starting the download, but not awaiting its completion yet
                Task downloadCompletionTask = HandleDownloadAsync(op, true);
                downloadCompletionTasks.Add(downloadCompletionTask);
            }


            // Now that we've got all the downloads started concurrently, we can wait until all of them complete
            await Task.WhenAll(downloadCompletionTasks);
            */
        }
    }
}
