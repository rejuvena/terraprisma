using System.Collections.Generic;
using System.Linq;
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
            // Module - Done
            //  Assembly - Done
            //    Type - Done
            //      Method - Done
            //        Parameter - Done
            //      Field - Done
            //      Event - Done
            //      Property - Done
            
            ApplyVisitors(Module);
            ApplyVisitors(Module.Assembly);

            ICollection<TypeDefinition> types = new List<TypeDefinition>();
            
            foreach (TypeDefinition type in Module.Types)
                RecursivelyGetTypes(type, types);

            foreach (TypeDefinition type in types)
            {
                ApplyVisitors(type);

                foreach (MethodDefinition method in type.Methods)
                {
                    ApplyVisitors(method);

                    foreach (ParameterDefinition parameter in method.Parameters) 
                        ApplyVisitors(parameter);
                }
                
                foreach (FieldDefinition field in type.Fields)
                    ApplyVisitors(field);
                
                foreach (EventDefinition @event in type.Events)
                    ApplyVisitors(@event);
                
                foreach (PropertyDefinition property in type.Properties)
                    ApplyVisitors(property);
            }

            return Module;
        }

        private void ApplyVisitors(object obj)
        {
            foreach (IVisitor v in Visitors.Where(x => x.Visitable(obj)))
                v.Visit(obj);
        }

        private void RecursivelyGetTypes(TypeDefinition type, ICollection<TypeDefinition> types)
        {
            types.Add(type);

            if (!type.HasNestedTypes) 
                return;
            
            foreach (TypeDefinition nested in type.NestedTypes)
                RecursivelyGetTypes(nested, types);
        }
    }
}