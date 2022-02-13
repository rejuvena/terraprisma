using System.Collections.Generic;
using Rejuvena.Terraprisma.Mod.Core.Visitors;
using Rejuvena.Terraprisma.Patching.API;

namespace Rejuvena.Terraprisma.Mod.Core
{
    public class CoreMod : PatchMod
    {
        public override IEnumerable<IVisitor> ResolveVisitors()
        {
            yield return new VersionNameAppender();
        }
    }
}