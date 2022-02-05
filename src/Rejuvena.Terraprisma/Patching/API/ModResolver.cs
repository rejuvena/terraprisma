using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Rejuvena.Terraprisma.Utilities;

// Loosely based on: https://github.com/ppy/osu/blob/master/osu.Game/Rulesets/RulesetStore.cs

namespace Rejuvena.Terraprisma.Patching.API
{
    /// <summary>
    ///     Resolves Terraprisma mods.
    /// </summary>
    public static class ModResolver
    {
        /// <summary>
        ///     The prefix all mod assemblies should start with.
        /// </summary>
        public const string AssemblyPrefix = "Rejuvena.Terraprisma.Mod";

        /// <summary>
        ///     A list of loaded mod assemblies, regardless of if those assemblies' mods were constructed.
        /// </summary>
        public static readonly Dictionary<Assembly, Type> Assembles = new();
        
        /// <summary>
        ///     A collection of loaded mods.
        /// </summary>
        public static readonly List<PatchMod> AvailableMods = new();

        /// <summary>
        ///     Whether the mod load process as concluded.
        /// </summary>
        public static bool ResolvedMods { get; private set; }

        internal static void Resolve()
        {
            if (ResolvedMods)
                return;

            ResolvedMods = true;

            Logger.LogMessage("ModResolver", "Proceeding with mod resolution.");

            LoadFromModsFolder();

            foreach (PatchMod? mod in Assembles.Values.Select(x => Activator.CreateInstance(x) as PatchMod))
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
            {
                string modFilename = Path.GetFileNameWithoutExtension(mod);

                if (Assembles.Values.Any(x => Path.GetFileNameWithoutExtension(x.Assembly.Location) == modFilename))
                    return;

                Assembly assembly = Assembly.LoadFrom(mod);

                if (Assembles.Any(x => x.Key.FullName == assembly.FullName) || Assembles.ContainsKey(assembly))
                    return;

                Logger.LogMessage("ModResolver", $"Resolved mod assembly: {assembly.GetName().FullName}");

                Assembles[assembly] = assembly.GetTypes().First(
                    x => x.IsPublic && !x.IsAbstract && x.IsSubclassOf(typeof(PatchMod))
                );
            }
        }
    }
}