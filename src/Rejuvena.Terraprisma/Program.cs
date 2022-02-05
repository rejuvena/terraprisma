using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
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
        public static void Main(string[] args)
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

            ModResolver.Resolve();

            ModuleDefinition? tMLModule = CecilResolver.Resolve();

            if (tMLModule is null)
            {
                Logger.LogMessage("Terraprisma", "Failed to resolve a patchable module, aborting.");
                return;
            }

            Patcher tMLPatcher = new(
                tMLModule,
                ModResolver.AvailableMods.SelectMany(x => x.ResolveVisitors()).ToList()
            );
            
            PatchRuntime.RunModule(tMLPatcher.PatchModule(), args);
        }

        /// <summary>
        ///         Renders the large Terraprisma sword in the console.
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
    }
}