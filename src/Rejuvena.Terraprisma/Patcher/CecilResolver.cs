using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Mono.Cecil;

namespace Rejuvena.Terraprisma.Patcher
{
    /// <summary>
    ///     Resolves required data and begins the loading process with <c>Mono.Cecil</c>.
    /// </summary>
    public static class CecilResolver
    {
        public static AssemblyDefinition? TModLoaderAssembly { get; private set; }

        /// <summary>
        ///     Special version data provided by the tModLoader assembly, such as the Terraria version, tModLoader version, and commit SHA.
        /// </summary>
        public static CustomAttribute? TModLoaderVersionData { get; private set; }

        internal static void Resolve(string[] args)
        {
            Logger.LogMessage("CecilResolver", "Debug", "Beginning initial commit resolution.");

            TModLoaderAssembly = AssemblyDefinition.ReadAssembly("tModLoader.dll", new ReaderParameters
            {
                AssemblyResolver = new LibraryAssemblyResolver()
            });

            TModLoaderVersionData = TModLoaderAssembly.CustomAttributes.First(x =>
                x.AttributeType.FullName == "System.Reflection.AssemblyInformationalVersionAttribute"
            );

            string[] data = ((string) TModLoaderVersionData.ConstructorArguments[0].Value).Split('-');

            Logger.LogMessage("CecilResolver", $"Resolved tModLoader assembly, commit: {data[3]}");

            if (data[3] != Commit.Latest.LongForm)
            {
                Logger.LogMessage(
                    "CecilResolver",
                    "Warning",
                    "Latest confirmed commit does not match your current commit. Press <enter> to continue."
                );

                while (Console.ReadKey().Key != ConsoleKey.Enter)
                {
                }
            }

            Logger.LogMessage("CecilResolver", "Proceeding with mod resolution.");

            ModResolver.Resolve();

            // VISITOR CODE WILL GO HERE

            using MemoryStream asmStream = new();

            TModLoaderAssembly.Write(asmStream, new WriterParameters());

            byte[] d = asmStream.ToArray();

            // Load libraries required by tModLoader into the AppDomain
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromLibraryFolder;
            // AssemblyLoadContext.Default.ResolvingUnmanagedDll += ResolveNativeLibrary;
            foreach (FileInfo nativeDll in new DirectoryInfo(Path.Combine(Program.LocalPath, "Libraries", "Native", GetNativeDir())).GetFiles())
                NativeLibrary.Load(nativeDll.FullName);

            Assembly tmlAsm = Assembly.Load(d, asmStream.ToArray());
            tmlAsm.GetType("MonoLaunch")!.GetMethod(
                "Main",
                BindingFlags.Static | BindingFlags.NonPublic
            )!.Invoke(null, new object?[] {args});
        }

        private static Assembly ResolveFromLibraryFolder(object? sender, ResolveEventArgs args)
        {
            AssemblyName name = new(args.Name);
            string asmName = name.Name ?? "";
            string asmVers = name.Version?.ToString() ?? "";

            if (asmVers == "1.0.0.0")
                asmVers = "1.0.0";

            // In the case of two libraries, be as accurate as possible.
            string path = Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers, asmName + ".dll");

            // If no correct version is found, resort to the first one located.
            if (!Directory.Exists(Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers)))
            {
                asmVers = new DirectoryInfo(Path.Combine(
                    Program.LocalPath,
                    "Libraries",
                    asmName
                )).GetDirectories()[0].Name;
                path = Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers, asmName + ".dll");
            }

            // Handle stupid "lib" folder stuff.
            string libMaybeDir = Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers);
            if (!File.Exists(path) && Directory.Exists(Path.Combine(libMaybeDir, "lib")))
            {
                string net = new DirectoryInfo(Path.Combine(libMaybeDir, "lib")).GetDirectories()[0].Name;
                path = Path.Combine(Program.LocalPath, "Libraries", asmName, asmVers, "lib", net, asmName + ".dll");
            }

            Console.WriteLine(path);
            Assembly asm = Assembly.LoadFile(path);

            return asm;
        }

        private static string GetNativeDir()
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