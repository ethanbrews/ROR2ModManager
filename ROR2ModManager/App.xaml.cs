﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Windows.Storage;
using MetroLog;
using MetroLog.Targets;

namespace ROR2ModManager
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            var file = args.Files[0];
            if (!file.IsOfType(StorageItemTypes.File))
                throw new Exception("Can only import .r2pm formatted files.");
            try
            {
                using (var fs = await (file as StorageFile).OpenStreamForReadAsync())
                {
                    var profile = formatter.Deserialize(fs) as Profile;
                    MainPage.Instance.contentFrame.Navigate(typeof(Pages.Install.Select), new Pages.Install.SelectParameters { packagesLW = profile.PacksLW });
                }
            }
            catch
            {
                throw new Exception("Invalid Formatting");
            }
            
        }

        private static void RegisterLogger()
        {
            LogManagerFactory.DefaultConfiguration.AddTarget(MetroLog.LogLevel.Trace, MetroLog.LogLevel.Fatal, new StreamingFileTarget());
            /*
            // Create IOC container and add logging feature to it.
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging();

            // Build provider to access the logging service.
            IServiceProvider provider = services.BuildServiceProvider();

            // UWP is very restrictive of where you can save files on the disk.
            // The preferred place to do that is app's local folder.
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            string fullPath = $"{folder.Path}\\Logs\\App.log";

            // Tell the logging service to use Serilog.File extension.
            provider.GetService<ILoggerFactory>().AddFile(fullPath);
            */
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param Name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            RegisterLogger();
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param Name="sender">The Frame which failed navigation</param>
        /// <param Name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param Name="sender">The source of the suspend request.</param>
        /// <param Name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
