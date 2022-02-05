using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Rejuvena.Terraprisma.Utilities;

namespace Rejuvena.Terraprisma.Patching.Cecil
{
    public class LibraryAssemblyResolver : BaseAssemblyResolver
    {
        private DefaultAssemblyResolver DefaultResolver = new();
        private List<AssemblyDefinition> Libraries;

        public LibraryAssemblyResolver()
        {
            Libraries = new List<AssemblyDefinition>();
            
            foreach (string dll in Directory.GetFiles(Path.Combine(
                Program.LocalPath,
                "Libraries"
            ), "*.dll", SearchOption.AllDirectories))
            {
                if (dll.Contains("runtimes") || dll.Contains("resources") || dll.Contains("Native") || dll.Contains("lib"))
                    continue;
                
                try
                {
                    Libraries.Add(AssemblyDefinition.ReadAssembly(dll));
                }
                catch (Exception e)
                {
                    Logger.LogMessage("LibraryAssemblyResolver", "Error", $"Failed to resolve: {e}");
                }
            }
        }
        
        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition assembly;

            try
            {
                assembly = DefaultResolver.Resolve(name);
            }
            catch
            {
                assembly = Libraries.First(x => x.Name.Name == name.Name);
            }
            
            return assembly ?? base.Resolve(name);
        }
    }
}