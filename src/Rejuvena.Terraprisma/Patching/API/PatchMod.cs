using System.Collections.Generic;

namespace Rejuvena.Terraprisma.Patching.API
{
    /// <summary>
    ///     A loadable mod.
    /// </summary>
    public abstract class PatchMod
    {
        public abstract IEnumerable<IVisitor> ResolveVisitors();
    }
}