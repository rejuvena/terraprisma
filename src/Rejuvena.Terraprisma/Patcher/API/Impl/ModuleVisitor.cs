using Mono.Cecil;

namespace Rejuvena.Terraprisma.Patcher.API.Impl
{
    public abstract class ModuleVisitor : BaseVisitor<ModuleReference>
    {
        public override bool Visitable(object toVisit) =>
            toVisit.GetType().IsSubclassOf(typeof(ModuleReference)) || base.Visitable(toVisit);
    }
}