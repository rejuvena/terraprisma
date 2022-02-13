using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Mono.Cecil;
using Rejuvena.Terraprisma.Utilities;

namespace Rejuvena.Terraprisma.Patching
{
    /// <summary>
    ///     Handles starting the patched tModLoader instance.
    /// </summary>
    public static class PatchRuntime
    {
        public static readonly NativeAssemblyLoadContext LoadContext = new();

        public static Assembly? TmlAssembly = null;

        /// <summary>
        ///     A dictionary containing hardcoded path redirects.
        /// </summary>
        public static readonly Dictionary<string, string> HardcodedAssemblyNames = new()
        {
            {"ReLogic", Path.Combine(Program.LocalPath, "Libraries", "ReLogic", "1.0.0", "ReLogic.dll")},
            {
                "Microsoft.CodeAnalysis", Path.Combine(
                    Program.LocalPath,
                    "Libraries",
                    "microsoft.codeanalysis.common",
                    "4.0.0-6.final",
                    "lib",
                    "netcoreapp3.1",
                    "Microsoft.CodeAnalysis.dll"
                )
            },
        };

        /// <summary>
        ///     Runs tModLoader, assuming the provided <paramref name="module"/> is tModLoader's.
        /// </summary>
        internal static void RunModule(ModuleDefinition module, IEnumerable<string> args)
        {
            ResolveLibraries();

            using MemoryStream moduleStream = new();

            module.Write(moduleStream, new WriterParameters());

            string tempFile = Path.Join(Program.TerrarprismaDataPath, "Temp", "tModLoader.dll");

            Directory.CreateDirectory(Path.Join(Program.TerrarprismaDataPath, "Temp"));
            
            if (File.Exists(tempFile))
                File.Delete(tempFile);
            
            File.WriteAllBytes(tempFile, moduleStream.ToArray());
            
            /*
            byte[] data = moduleStream.ToArray();
            Assembly tmlAsm = Assembly.Load(data, moduleStream.ToArray());
             */
            TmlAssembly = LoadContext.LoadFromAssemblyPath(tempFile);

            InvokeMonoLaunch(TmlAssembly, args);
        }

        private static void InvokeMonoLaunch(Assembly assembly, IEnumerable args)
        {
            Type? monoLaunch = assembly.GetType("MonoLaunch");

            if (monoLaunch is null)
            {
                Logger.LogMessage("PatchRuntime", "Error", "Couldn't resolve MonoLaunch!");
                return;
            }

            MethodInfo? main = monoLaunch.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);

            if (main is null)
            {
                Logger.LogMessage("PatchRuntime", "Error", "Couldn't resolve MonoLaunch.Main!");
                return;
            }

            try
            {
                main.Invoke(null, new object?[] {args});
            }
            catch (Exception e)
            {
                Logger.LogMessage("PatchRuntime", "Error", "Error thrown: " + e);

                // throw e;
            }
        }

        /// <summary>
        ///     Initiates our custom library resolving.
        /// </summary>
        private static void ResolveLibraries()
        {
            // Load libraries required by tModLoader into the AppDomain manually.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromLibraryFolder;
            LoadContext.Resolving += LoadContextOnResolving;

            // TODO: This seems useless?
            foreach (FileInfo nativeDll in new DirectoryInfo(Path.Combine(Program.LocalPath, "Libraries", "Native",
                GetNativeDirectory())).GetFiles())
                NativeLibrary.Load(nativeDll.FullName);
        }

        private static Assembly LoadContextOnResolving(AssemblyLoadContext arg1, AssemblyName arg2) =>
            ResolveFromLibraryFolder(null, new ResolveEventArgs(arg2.FullName ?? "", null));

        private static Assembly ResolveFromLibraryFolder(object? sender, ResolveEventArgs args)
        {
            AssemblyName name = new(args.Name);
            string asmName = name.Name ?? "";
            string asmVers = name.Version?.ToString() ?? "";

            if (asmName == "tModLoader")
                return TmlAssembly!;
            
            if (HardcodedAssemblyNames.ContainsKey(asmName))
                return Assembly.LoadFile(HardcodedAssemblyNames[asmName]);

            // 1.0.0.0 is shortened to 1.0.0 in the Libraries folder.
            if (asmVers is "1.0.0.0" or "")
                asmVers = "1.0.0";

            string path = Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers, asmName + ".dll");

            // If no correct version is found, resort to the first one located.
            if (!Directory.Exists(Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers)))
            {
                // Set the version to the first one found.
                asmVers = new DirectoryInfo(Path.Combine(
                    Program.LocalPath,
                    "Libraries",
                    asmName
                )).GetDirectories()[0].Name;

                path = Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers, asmName + ".dll");
            }

            // Handle stupid "lib" folder stuff.
            // Sometimes, DLLs are further embedded below this folder.
            string versionFolder = Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers);

            if (!File.Exists(path) && Directory.Exists(Path.Combine(versionFolder, "lib")))
            {
                // Resolve the first folder beneath "lib", such as net6.0, etc.
                string netVer = new DirectoryInfo(Path.Combine(versionFolder, "lib")).GetDirectories()[0].Name;

                path = Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers, "lib", netVer, asmName + ".dll");
            }

            Logger.LogMessage("PatchRuntime", "Debug", "Resolved library at: " + path);

            return Assembly.LoadFile(path);
        }

        private static string GetNativeDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "OSX";

            throw new InvalidOperationException("Unknown OS.");
        }
    }
}