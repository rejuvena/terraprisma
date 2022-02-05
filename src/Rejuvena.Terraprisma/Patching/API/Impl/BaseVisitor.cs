namespace Rejuvena.Terraprisma.Patching.API.Impl
{
    public abstract class BaseVisitor<TVisited> : IVisitor<TVisited>
    {
        public virtual bool Visitable(object toVisit) => toVisit.GetType() == typeof(TVisited);

        void IVisitor.Visit(object visited) => Visit((TVisited) visited);
        
        public abstract void Visit(TVisited visited);
    }
}