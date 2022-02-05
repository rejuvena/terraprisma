using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Rejuvena.Terraprisma.Patcher.API;

// Loosely based on: https://github.com/ppy/osu/blob/master/osu.Game/Rulesets/RulesetStore.cs

namespace Rejuvena.Terraprisma.Patcher
{
    /// <summary>
    ///     Resolves Terraprisma mods.
    /// </summary>
    public static class ModResolver
    {
        public const string AssemblyPrefix = "Rejuvena.Terraprisma.Mod";

        public static readonly Dictionary<Assembly, Type> Assembles = new();
        public static readonly List<Mod> AvailableMods = new();

        public static bool ResolvedMods { get; private set; }

        internal static void Resolve()
        {
            if (ResolvedMods)
                return;

            ResolvedMods = true;

            Directory.CreateDirectory(Path.Combine(Program.TerrarprismaDataPath, "Mods"));

            LoadFromModsFolder();

            foreach (Mod? mod in Assembles.Values.Select(x => Activator.CreateInstance(x) as Mod))
            {
                if (mod is null)
                {
                    Logger.LogMessage(
                        "ModResolver",
                        "Error",
                        "Could not resolve a mod as it failed to instantiate."
                    );
                    continue;
                }
                
                AvailableMods.Add(mod);
            }
        }

        private static void LoadFromModsFolder()
        {
            FileInfo[] mods = new DirectoryInfo(Path.Combine(
                Program.TerrarprismaDataPath,
                "Mods")
            ).GetFiles($"{AssemblyPrefix}.*.dll");

            foreach (string mod in mods.Select(x => x.FullName))
                LoadMod(mod);
        }

        private static void LoadMod(string file)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);

            if (Assembles.Values.Any(x => Path.GetFileNameWithoutExtension(x.Assembly.Location) == fileName))
                return;

            AddMod(Assembly.LoadFrom(file));
        }

        private static void AddMod(Assembly assembly)
        {
            if (Assembles.ContainsKey(assembly))
                return;

            if (Assembles.Any(x => x.Key.FullName == assembly.FullName))
                return;

            Logger.LogMessage("ModResolver", $"Resolved mod assembly: {assembly.GetName().FullName}");

            Assembles[assembly] = assembly.GetTypes().First(
                x => x.IsPublic && !x.IsAbstract && x.IsSubclassOf(typeof(Mod))
            );
        }
    }
}