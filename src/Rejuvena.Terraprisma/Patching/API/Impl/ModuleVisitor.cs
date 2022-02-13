using Mono.Cecil;

namespace Rejuvena.Terraprisma.Patching.API.Impl
{
    public abstract class ModuleVisitor : BaseVisitor<ModuleDefinition>
    {
        public override bool Visitable(object toVisit) =>
            toVisit.GetType().IsSubclassOf(typeof(ModuleDefinition)) || base.Visitable(toVisit);
    }
}