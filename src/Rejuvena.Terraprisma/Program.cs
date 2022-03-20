using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Mono.Cecil;
using Newtonsoft.Json;
using Rejuvena.Terraprisma.Patching;
using Rejuvena.Terraprisma.Patching.API;
using Rejuvena.Terraprisma.Patching.Cecil;
using Rejuvena.Terraprisma.Utilities;

namespace Rejuvena.Terraprisma
{
    /// <summary>
    ///     Entrypoint class that will handle launch bootstrapping, applying mods, etc.
    /// </summary>
    internal static class Program
    {
        public static readonly string LocalPath = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string TerrarprismaDataPath = Path.Combine(LocalPath, "Terraprisma");

        /// <summary>
        ///     The entrypoint method.
        /// </summary>
        /// <param name="args"><see cref="Environment.GetCommandLineArgs"/></param>
        public static async Task Main(string[] args)
        {
            Directory.CreateDirectory(Path.Combine(TerrarprismaDataPath, "Mods"));
            Directory.CreateDirectory(Path.Combine(TerrarprismaDataPath, "Logs"));

            Logger.Initialize();

            if (OperatingSystem.IsWindows())
                WindowsAnsiBootstrapper.Bootstrap();

            WritePrettySword();

            if (args.Length != 0)
                Logger.LogMessage(
                    "Terraprisma",
                    "Debug",
                    $"Launched with arguments: {string.Join(", ", args)}"
                );

            PreLoadLibraries();

            ModResolver.Resolve();

            ModuleDefinition? tmlModule = CecilResolver.Resolve();

            if (tmlModule is null)
            {
                Logger.LogMessage("Terraprisma", "Failed to resolve a patchable module, aborting.");
                return;
            }

            Patcher tmlPatcher = new(
                tmlModule,
                ModResolver.AvailableMods.SelectMany(x => x.ResolveVisitors()).ToList()
            );
            
            Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
            
            await Task.Run(() =>
            {
                PatchRuntime.RunModule(tmlPatcher.PatchModule(), args);
            });
        }

        /// <summary>
        ///     Renders the large Terraprisma sword in the console.
        /// </summary>
        private static void WritePrettySword()
        {
            using Stream? img = typeof(Program).Assembly.GetManifestResourceStream(
                "Rejuvena.Terraprisma.Resources.TerraprismaImage"
            );

            if (img is null)
                return;

            using StreamReader reader = new(img);
            Console.WriteLine(reader.ReadToEnd());
        }

        private static void PreLoadLibraries()
        {
            DirectoryInfo libraryDir = new(Path.Combine(LocalPath, "Libraries"));
            FileInfo[] libraryFiles = libraryDir
                .GetDirectories("**", SearchOption.AllDirectories)
                .SelectMany(x => x.GetFiles())
                .ToArray();
            string[] blacklist =
            {
                "Native",
                "runtimes",
                "resources"
            };

            foreach (FileInfo libraryFile in libraryFiles)
            {
                if (libraryFile.Extension != ".dll" || libraryFile.FullName.Split(Path.DirectorySeparatorChar).Any(
                        x => blacklist.Any(x.Equals)
                    ) || libraryFile.Name.EndsWith("resources.dll")) continue;
                
                PatchRuntime.AssemblyMap.Add(Path.GetFileNameWithoutExtension(libraryFile.Name), libraryFile.FullName);
            }
        }
    }
}