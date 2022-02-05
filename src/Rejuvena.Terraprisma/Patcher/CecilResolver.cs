using System;
using System.Linq;
using System.Reflection;
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

        internal static void Resolve()
        {
            Logger.LogMessage("CecilResolver", "Debug", "Beginning initial commit resolution.");

            TModLoaderAssembly = AssemblyDefinition.ReadAssembly("tModLoader.dll");
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
        }
    }
}