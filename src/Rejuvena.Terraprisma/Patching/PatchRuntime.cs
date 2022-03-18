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

        public static Assembly? TmlAssembly;

        internal static readonly Dictionary<string, string> AssemblyMap = new();

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
            AssemblyLoadContext.Default.ResolvingUnmanagedDll += (assembly, s) => LoadContext.LoadUnmanaged(s);

            DirectoryInfo nativeDir = new(Path.Combine(Program.LocalPath, "Libraries", "Native", GetNativeDirectory()));
            FileInfo[] nativeFiles = nativeDir.GetFiles();

            foreach (FileInfo nativeDll in nativeFiles)
            {
                if (NativeLibrary.Load(nativeDll.FullName) != IntPtr.Zero)
                    Logger.LogMessage("PatchRuntime", "Debug", "Loaded native DLL: " + nativeDll.Name);
                else
                    Logger.LogMessage("PatchRuntime", "Debug", "Failed to load native DLL: " + nativeDll.Name);
            }
        }

        private static Assembly LoadContextOnResolving(AssemblyLoadContext arg1, AssemblyName arg2) =>
            ResolveFromLibraryFolder(null, new ResolveEventArgs(arg2.FullName, null));

        private static Assembly ResolveFromLibraryFolder(object? sender, ResolveEventArgs args)
        {
            AssemblyName name = new(args.Name);

            if (name.Name == "tModLoader")
                return TmlAssembly!;
            
            if (AssemblyMap.ContainsKey(name.Name ??= ""))
                return Assembly.LoadFile(AssemblyMap[name.Name]);

            throw new FileNotFoundException(name.Name);
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