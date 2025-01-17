﻿using StrixMusic.Shared;
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
            if (e.PrelaunchActivated == false)
            {
                TryEnablePrelaunch();

                // Ensure the current window is active
                Window.Current.Activate();
            }

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is FrameworkElement rootElement))
            {
                // Place the frame in the current Window
                Window.Current.Content = new AppFrame();
            }

            // Bi-directional language support
#if NETFX_CORE
            // https://github.com/unoplatform/uno/issues/21
            var flowDirectionSetting = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues["LayoutDirection"];
            if (flowDirectionSetting == "LTR")
            {
                Window.Current.GetAppFrame().FlowDirection = FlowDirection.LeftToRight;
            }
            else
            {
                Window.Current.GetAppFrame().FlowDirection = FlowDirection.RightToLeft;
            }
#endif
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
#if NETFXCORE
            Windows.ApplicationModel.Core.CoreApplication.EnablePrelaunch(true);
#endif
        }
    }
}
