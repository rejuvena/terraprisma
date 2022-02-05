using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Rejuvena.Terraprisma.Utilities
{
    // As of Windows 10 version 1511, ANSI support is available.
    // We need to manually enable it, however.
    // https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/
    internal static class WindowsAnsiBootstrapper
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [SupportedOSPlatform("windows")]
        internal static void Bootstrap()
        {
            IntPtr iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);

            if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                Console.WriteLine("Failed to get output console mode.");
                return;
            }

            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;

            if (!SetConsoleMode(iStdOut, outConsoleMode))
                Console.WriteLine($"Failed to set output console mode, error code: {GetLastError()}");
        }
    }
}