using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Rejuvena.Terraprisma.Utilities;

namespace Rejuvena.Terraprisma.Patching
{
    public class NativeAssemblyLoadContext : AssemblyLoadContext
    {
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

            if (match is not null && NativeLibrary.TryLoad(match, out IntPtr handle))
            {
                Logger.LogMessage("NativeAssemblyLoadContext", "Debug", "Success!");
                return handle;
            }

            Logger.LogMessage("NativeAssemblyLoadContext", "Error", "Failed to load: " + match);
            throw new FileLoadException("Failed to load: " + match);

            return base.LoadUnmanagedDll(unmanagedDllName);
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