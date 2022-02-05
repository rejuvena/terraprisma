using System;
using System.IO;
using Rejuvena.Terraprisma.Patcher;

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
            Logger.Initiate();

            if (OperatingSystem.IsWindows())
                WindowsAnsiBootstrapper.Bootstrap();

            using Stream? img = typeof(Program).Assembly.GetManifestResourceStream(
                "Rejuvena.Terraprisma.Resources.TerraprismaImage"
            );

            if (img is not null)
            {
                using StreamReader reader = new(img);
                Console.WriteLine(reader.ReadToEnd());
            }

            Logger.LogMessage(
                "Terraprisma",
                "Debug",
                $"Launched with arguments: {string.Join(", ", args)}"
            );

            CecilResolver.Resolve(args);

            Logger.Dispose();
        }
    }
}