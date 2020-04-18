using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace UWPTools.Threads
{
    class ThreadHelper
    {
        public static async Task RunInUIThread(DispatchedHandler action) =>
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);

        public static async Task RunInBackgroundThread(Action action) => await Task.Run(action);
    }
}