using System.Linq;
using Mono.Cecil;
using Rejuvena.Terraprisma.Patching;
using Rejuvena.Terraprisma.Patching.API.Impl;

namespace Rejuvena.Terraprisma.Mod.Core.Visitors
{
    public class TerraprismaMarker : ModuleVisitor
    {
        public override void Visit(ModuleDefinition visited)
        {
            // Example of adding a new class. Ours has some data!
            //if (visited.Name != "tModLoader.dll")
            //    return;
            
            if (visited.Types.Any(x => x.Namespace == "Rejuvena.Terraprisma" && x.Name == "WasHere"))
                return;

            FieldDefinition versField = new(
                "RejuvenaVersion",
                FieldAttributes.Public
                | FieldAttributes.Literal,
                visited.TypeSystem.String
            )
            {
                Constant = typeof(Patcher).Assembly.GetName().Version!.ToString()
            };
            
            TypeDefinition type = new(
                "Rejuvena.Terraprisma",
                "WasHere",
                TypeAttributes.Public
                | TypeAttributes.Class
            )
            {
                BaseType = visited.TypeSystem.Object,
                Fields = { versField }
            };
            
            visited.Types.Add(type);
        }
    }
}