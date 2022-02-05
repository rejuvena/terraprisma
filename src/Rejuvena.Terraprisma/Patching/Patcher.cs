using System.Collections.Generic;
using Mono.Cecil;
using Rejuvena.Terraprisma.Patching.API;

namespace Rejuvena.Terraprisma.Patching
{
    /// <summary>
    ///     A patcher capable of visiting and modifying assemblies.
    /// </summary>
    public sealed class Patcher
    {
        /// <summary>
        ///     The module being patched.
        /// </summary>
        public readonly ModuleDefinition Module;
        
        /// <summary>
        ///     The visitors being applied to the module.
        /// </summary>
        public readonly List<IVisitor> Visitors;

        public Patcher(ModuleDefinition module, List<IVisitor> visitors)
        {
            Module = module;
            Visitors = visitors;
        }

        public ModuleDefinition PatchModule()
        {
            // TODO: Patching.
            
            return Module;
        }
    }
}