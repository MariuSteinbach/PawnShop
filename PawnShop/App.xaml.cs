using PawnShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PawnShop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public static List<Scan> Scans { get; set; }

        // Excel Interop
        public static BackgroundTaskDeferral AppServiceDeferral;
        public static AppServiceConnection Connection;
        public static event EventHandler AppServiceConnected;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            Scans = new List<Scan>();
            LoadScans();

            AppServiceDeferral = null;
            Connection = null;

            this.Suspending += OnSuspending;
        }

        private async void LoadScans()
        {
            IReadOnlyList<StorageFile> storageFiles = null;
            try
            {
                storageFiles = await ApplicationData.Current.LocalFolder.GetFilesAsync();
                foreach (StorageFile Scan in storageFiles.Where(f => f.Name.Split('.').Last() == "scan"))
                {
                    FileStream fs = new FileStream(Scan.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    MemoryStream ms = new MemoryStream();
                    BinaryFormatter bf = new BinaryFormatter();
                    Byte[] encrypted = null;
                    try
                    {
                        encrypted = new Byte[fs.Length];
                        await fs.ReadAsync(encrypted, 0, Convert.ToInt32(fs.Length));
                    }
                    catch (SerializationException e)
                    {
                        var dialog = new MessageDialog("Failed to load scans. Error: " + e.Message);

                        await dialog.ShowAsync();
                        throw;
                    }
                    finally
                    {
                        fs.Close();
                    }
                    Byte[] decrypted = Crypter.Decrypt(encrypted);
                    ms.Write(decrypted, 0, decrypted.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    Scan LoadedScan = (Scan)bf.Deserialize(ms);
                    if (Scans.Find(s => s.Date == LoadedScan.Date) == null)
                    {
                        Scans.Add(LoadedScan);
                    }

                }
            }
            catch (Exception e)
            {
                var dialog = new MessageDialog("Failed to load scans. Error: " + e.Message);

                await dialog.ShowAsync();
            }
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            if(args.TaskInstance.TriggerDetails is AppServiceTriggerDetails)
            {
                AppServiceDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += OnTaskCanceled;

                if(args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
                {
                    Connection = details.AppServiceConnection;
                    AppServiceConnected?.Invoke(this, null);
                }
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if(AppServiceDeferral != null)
            {
                AppServiceDeferral.Complete();
            }
        }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
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
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
