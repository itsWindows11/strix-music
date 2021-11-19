﻿using StrixMusic.Sdk.Uno.Services.ShellManagement;
using Windows.Foundation;

namespace StrixMusic.Shells.ZuneDesktop
{
    /// <summary>
    /// Handles registration for this shell.
    /// </summary>
    public static class Registration
    {
        /// <summary>
        /// Executes shell registration.
        /// </summary>
        public static void Execute()
        {
            var metadata = new ShellMetadata(id: "Zune.Desktop.4.8",
                                             displayName: "Zune Desktop",
                                             description: "A faithful recreation of the iconic Zune client for Windows",
                                             inputMethods: InputMethods.Mouse,
                                             minWindowSize: new Size(width: 700, height: 600));

            ShellRegistry.Register(() => new ZuneShell(), metadata);
        }
    }
}