﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using StrixMusic.Sdk.Uno.Assembly;
using StrixMusic.Sdk.Uno.Models;

namespace StrixMusic.Helpers
{
    /// <summary>
    /// Constants and Assembly data accessed throughout the library.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The prefix of the resources path.
        /// </summary>
        public const string ResourcesPrefix = "ms-appx:///";

        /// <summary>
        /// An accessing class for constant information about Shells.
        /// </summary>
        public static class Shells
        {
            private static Dictionary<string, ShellModel>? _loadedShells = null;

            /// <summary>
            /// The <see cref="Regex"/> to determine if an assembly is a shell by the fullname.
            /// </summary>
            public const string ShellAssemblyRegex = @"^(?:StrixMusic\.Shells\.)(\w{3,})[^.]";

            /// <summary>
            /// The <see cref="Regex"/> to determine if an assembly is a shell by the fullname.
            /// </summary>
            public const string ShellResourceDictionaryRegex = @"^(?:ms-appx:\/\/\/StrixMusic\.Shells\.)(\w{3,})(?:\/Resources\.xaml)$";

            /// <summary>
            /// The name of the DefaultShell.
            /// </summary>
            public const string DefaultShellAssemblyName = "Default";

            /// <summary>
            /// The name of the DefaultShell.
            /// </summary>
            public const string DefaultShellDisplayName = "Default Shell";

            /// <summary>
            /// The namespace before a shell.
            /// </summary>
            public const string ShellNamespacePrefix = "StrixMusic.Shells";

            /// <summary>
            /// The name of the resources fill for a shell.
            /// </summary>
            public const string ShellResourcesSuffix = "Resources.xaml";

            /// <summary>
            /// The <see cref="ShellModel"/> of the DefaultShell.
            /// </summary>
            public static ShellModel DefaultShellModel => LoadedShells[DefaultShellAssemblyName];

            /// <summary>
            /// Gets the loaded shells in the <see cref="Assembly"/> AssemblyName and DisplayName.
            /// </summary>
            public static Dictionary<string, ShellModel> LoadedShells
            {
                get
                {
                    if (_loadedShells == null)
                    {
                        _loadedShells = new Dictionary<string, ShellModel>();
                        List<ShellModel> loadedShells = new List<ShellModel>();
                        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (Assembly assembly in assemblies)
                        {
                            // Check if the assembly name is shell.
                            Match match = Regex.Match(assembly.FullName, ShellAssemblyRegex);
                            if (match.Success)
                            {
                                // Gets the AssemblyName of the shell from the Regex.
                                string assemblyName = match.Groups[1].Value;

                                // Find the ShellName attribute
                                ShellAttribute shellAttribute = assembly.GetCustomAttribute<ShellAttribute>();
                                _loadedShells.Add(assemblyName, new ShellModel(match.Groups[1].Value, shellAttribute));
                            }
                        }
                    }

                    return _loadedShells!;
                }
            }
        }
    }
}