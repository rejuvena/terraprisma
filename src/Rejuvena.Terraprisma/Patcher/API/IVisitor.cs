namespace Rejuvena.Terraprisma.Patcher.API
{
    /// <summary>
    ///     A basic interface containing core properties and methods of a visitor.
    /// </summary>
    public interface IVisitor
    {
        /// <summary>
        ///     Verifies if an object can be visited.
        /// </summary>
        /// <param name="toVisit">The object to test.</param>
        bool Visitable(object toVisit);
        
        /// <summary>
        ///     Called when an object is visited.
        /// </summary>
        /// <param name="visited">The object being visited.</param>
        void Visit(object visited);
    }

    /// <inheritdoc cref="IVisitor"/>
    /// <typeparam name="TVisited">The type being visited.</typeparam>
    public interface IVisitor<in TVisited> : IVisitor
    {
        /// <inheritdoc cref="IVisitor.Visit"/>
        void Visit(TVisited visited);
    }
}