using System.IO;
using Mono.Cecil;
using Rejuvena.Terraprisma.Utilities;

namespace Rejuvena.Terraprisma.Patching.Cecil
{
    /// <summary>
    ///     Resolves data required to initiate the patching process.
    /// </summary>
    public static class CecilResolver
    {
        /// <summary>
        ///     Resolves the tModLoader <see cref="ModuleDefinition"/>.
        /// </summary>
        internal static ModuleDefinition? Resolve()
        {
            if (File.Exists("tModLoader.dll"))
                return ModuleDefinition.ReadModule("tModLoader.dll", new ReaderParameters
                {
                    AssemblyResolver = new LibraryAssemblyResolver()
                });

            Logger.LogMessage(
                "CecilResolver",
                "Fatal",
                "Could not locate tModLoader.dll! Did you install this in the wrong folder?"
            );
            
            return null;
        }
    }
}