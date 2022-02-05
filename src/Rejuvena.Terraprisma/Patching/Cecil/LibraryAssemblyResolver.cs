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
        private readonly DefaultAssemblyResolver DefaultResolver = new();
        private readonly List<AssemblyDefinition> Libraries = new();

        public LibraryAssemblyResolver()
        {
            foreach (string dll in Directory.GetFiles(
                Path.Combine(Program.LocalPath, "Libraries"), 
                "*.dll", SearchOption.AllDirectories)
            )
            {
                // Ignore misc. DLLs that we don't care about.
                if (dll.Contains("runtimes") || dll.Contains("resources") || dll.Contains("Native"))
                    continue;
                
                // Add all usable libraries to a collection that we use to resolve from later.
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
                // Try to resolve normally.
                assembly = DefaultResolver.Resolve(name);
            }
            catch
            {
                // Resolve from our collection now. Throw if nothing is found (First instead of FirstOrDefault).
                assembly = Libraries.First(x => x.Name.Name == name.Name);
            }
            
            return assembly ?? base.Resolve(name);
        }
    }
}