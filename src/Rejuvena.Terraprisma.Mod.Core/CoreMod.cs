using System.Collections.Generic;
using Rejuvena.Terraprisma.Patcher.API;

namespace Rejuvena.Terraprisma.Mod.Core
{
    public class CoreMod : Patcher.API.Mod
    {
        public override IEnumerable<IVisitor> ResolveVisitors()
        {
            yield break;
        }
    }
}