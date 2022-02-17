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

            foreach (FileInfo nativeDll in new DirectoryInfo(
                    Path.Combine(Program.LocalPath, "Libraries", "Native", GetNativeDirectory())
                ).GetFiles()
            )
                NativeLibrary.Load(nativeDll.FullName);
        }

        private static Assembly LoadContextOnResolving(AssemblyLoadContext arg1, AssemblyName arg2) =>
            ResolveFromLibraryFolder(null, new ResolveEventArgs(arg2.FullName ?? "", null));

        private static Assembly ResolveFromLibraryFolder(object? sender, ResolveEventArgs args)
        {
            AssemblyName name = new(args.Name);

            if (name.Name == "tModLoader")
                return TmlAssembly!;

            string? asm = Program.Dependencies.Libraries.Keys.FirstOrDefault(
                x => x.Split('/', 2)[0] == name.Name || x == name.Name
            );

            string path = asm is not null && Program.Dependencies.Libraries[asm].ContainsKey("path") 
                ? ResolveUnprobableAssemblyPath(asm)
                : ResolveProbableAssemblyPath(name);

            Logger.LogMessage("PatchRuntime", "Debug", "Resolved library at: " + path);

            return Assembly.LoadFile(path);
        }

        private static string ResolveProbableAssemblyPath(AssemblyName name)
        {
            string asmName = name.Name ?? "";
            string asmVers = name.Version?.ToString() ?? "";
            
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

            return path;
        }

        private static string ResolveUnprobableAssemblyPath(string depName)
        {
            string dependency = depName.Split('/', 2)[0];

            string depPath = Path.Combine(Program.Dependencies.Libraries[depName]["path"].Split('/'));
            string path = Path.Combine(Program.LocalPath, "Libraries", depPath, "lib");
            
            return Path.Combine(path, new DirectoryInfo(path).GetDirectories()[0].ToString(), dependency + ".dll");
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