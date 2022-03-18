using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Rejuvena.Terraprisma.Utilities;

namespace Rejuvena.Terraprisma.Patching
{
    public class NativeAssemblyLoadContext : AssemblyLoadContext
    {
        public NativeAssemblyLoadContext()
        {
            ResolvingUnmanagedDll += (assembly, s) => LoadUnmanagedDll(s);
        }

        public IntPtr LoadUnmanaged(string s) => LoadUnmanagedDll(s);

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            Logger.LogMessage("NativeAssemblyLoadContext", "Debug", "Native resolve: " + unmanagedDllName);

            string dir = Path.Combine(Program.LocalPath, "Libraries", "Native", GetNativeDir());
            string[] files = Directory.GetFiles(dir, $"*{unmanagedDllName}*", SearchOption.AllDirectories);
            string? match = files.FirstOrDefault();

            Logger.LogMessage(
                "NativeAssemblyLoadContext",
                "Debug",
                match is null ? "Not found." : "Attempting load: " + match
            );

            if (match is not null)
            {
                Logger.LogMessage("NativeAssemblyLoadContext", "Debug", "Success!");
                return LoadUnmanagedDllFromPath(match);
            }

            Logger.LogMessage("NativeAssemblyLoadContext", "Error", "Failed to load: " + match);
            throw new FileLoadException("Failed to load: " + match);
        }

        private static string GetNativeDir()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "OSX";

            throw new InvalidOperationException("Unknown OS.");
        }
    }
}