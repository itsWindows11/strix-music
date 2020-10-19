﻿using Microsoft.Extensions.Logging;
using StrixMusic.Shared;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace StrixMusic
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            ConfigureFilters(Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory);

            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            FrameworkElement? rootElement = Window.Current.Content as FrameworkElement;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootElement == null)
            {
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = new AppFrame();
            }

            if (e.PrelaunchActivated == false)
            {
                TryEnablePrelaunch();

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Configures global logging.
        /// </summary>
        /// <param name="factory">The factory logger.</param>
        private static void ConfigureFilters(ILoggerFactory factory) => factory
            .WithFilter(new FilterLoggerSettings
            {
                { "Uno", LogLevel.Warning },
                { "Windows", LogLevel.Warning },

                // Debug JS interop
                // { "Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug },

                // Generic Xaml events
                // { "Windows.UI.Xaml", LogLevel.Debug },
                // { "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
                // { "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
                // { "Windows.UI.Xaml.UIElement", LogLevel.Debug },

                // Layouter specific messages
                // { "Windows.UI.Xaml.Controls", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
                // { "Windows.Storage", LogLevel.Debug },

                // Binding related messages
                // { "Windows.UI.Xaml.Data", LogLevel.Debug },

                // DependencyObject memory references tracking
                // { "ReferenceHolder", LogLevel.Debug },

                // ListView-related messages
                // { "Windows.UI.Xaml.Controls.ListViewBase", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.ListView", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.GridView", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.VirtualizingPanelLayout", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.NativeListViewBase", LogLevel.Debug },
                // { "Windows.UI.Xaml.Controls.ListViewBaseSource", LogLevel.Debug }, //iOS
                // { "Windows.UI.Xaml.Controls.ListViewBaseInternalContainer", LogLevel.Debug }, //iOS
                // { "Windows.UI.Xaml.Controls.NativeListViewBaseAdapter", LogLevel.Debug }, //Android
                // { "Windows.UI.Xaml.Controls.BufferViewCache", LogLevel.Debug }, //Android
                // { "Windows.UI.Xaml.Controls.VirtualizingPanelGenerator", LogLevel.Debug }, //WASM
            })
#if DEBUG
                .AddConsole(LogLevel.Debug);
#else
                .AddConsole(LogLevel.Information);
#endif

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

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Encapsulates the call to CoreApplication.EnablePrelaunch() so that the JIT
        /// won't encounter that call (and prevent the app from running when it doesn't
        /// find it), unless this method gets called. This method should only
        /// be called when the caller determines that we are running on a system that
        /// supports CoreApplication.EnablePrelaunch().
        /// </summary>
        private void TryEnablePrelaunch()
        {
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);
        }
    }
}