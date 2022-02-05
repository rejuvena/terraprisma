using System.Collections.Generic;

namespace Rejuvena.Terraprisma.Patcher.API
{
    /// <summary>
    ///     A loadable mod.
    /// </summary>
    public abstract class Mod
    {
        public abstract IEnumerable<IVisitor> ResolveVisitors();
    }
}