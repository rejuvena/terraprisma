using Mono.Cecil;

namespace Rejuvena.Terraprisma.Patching.API.Impl
{
    public abstract class AssemblyNameVisitor : BaseVisitor<AssemblyNameReference>
    {
        public override bool Visitable(object toVisit) =>
            toVisit.GetType().IsSubclassOf(typeof(AssemblyNameReference)) || base.Visitable(toVisit);
    }
}