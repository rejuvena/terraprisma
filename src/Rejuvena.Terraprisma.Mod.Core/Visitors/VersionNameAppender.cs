using System;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rejuvena.Terraprisma.Patching;
using Rejuvena.Terraprisma.Patching.API.Impl;

namespace Rejuvena.Terraprisma.Mod.Core.Visitors
{
    public class VersionNameAppender : MethodVisitor
    {
        public override void Visit(MethodDefinition visited)
        {
            // We want Terraria.Main
            if (visited.DeclaringType.FullName != "Terraria.Main")
                return;

            // We want "private static void DrawVersionNumber(Color menuColor, float upBump)"
            if (visited.Name != "DrawVersionNumber")
                return;

            ILContext context = new(visited);
            ILCursor c = new(context);

            c.GotoNext(MoveType.After, il => il.MatchLdsfld("Terraria.Main", "versionNumber"));
            c.Emit(OpCodes.Call, typeof(VersionNameAppender).GetMethod(nameof(GetVersion), BindingFlags.Static | BindingFlags.Public));
        }
        
        public static string GetVersion(string a) => a + "\nTerraprisma v" + typeof(Patcher).Assembly.GetName().Version;
    }
}