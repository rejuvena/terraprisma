// ReSharper disable InvalidXmlDocComment

using System;
using System.IO;

namespace Rejuvena.Terraprisma
{
    /// <summary>
    ///     Provides various logging utilities.
    /// </summary>
    public static class Logger
    {
        public static bool Initiated { get; private set; }

        public static StreamWriter? FileWriter { get; private set; }

        private static FileStream? CreatedFile;

        internal static void Initiate()
        {
            Initiated = true;

            Directory.CreateDirectory(Path.Combine(Program.TerrarprismaDataPath, "Logs"));

            try
            {
                CreatedFile = File.Create(Path.Combine(
                    Program.TerrarprismaDataPath,
                    "Logs",
                    $"{DateTime.Now:dd-MM-yyyy-hh-mm-ss}.txt"
                ));

                FileWriter = new StreamWriter(CreatedFile);
            }
            catch (Exception e)
            {
                LogMessage("Logger", "Error", $"Error thrown while trying to initiate logging: {e}");
            }

            LogMessage("Logger", "Debug", "Initiated logging.");
        }

        /// <summary>
        ///     Logs a message to the console and a log file.
        /// </summary>
        /// <param name="owner">The owner of this log message (displayed as <c>[{owner}]</c>)/</param>
        /// <param name="message">The log message to display.</param>
        public static void LogMessage(string owner, string message)
        {
            message = $"[{owner}] {message}";

            Console.WriteLine(message);
            FileWriter?.WriteLine(message);
        }

        /// <inheritdoc cref="LogMessage(string,string)"/>
        /// <param name="severity">The severity (INFO, DEBUG, etc.).</param>
        public static void LogMessage(string owner, string severity, string message) =>
            LogMessage(owner, $"[{severity}] {message}");

        internal static void Dispose()
        {
            LogMessage("Logger", "Debug", "Disposing logging instance.");
            FileWriter?.Dispose();
            CreatedFile?.Dispose();
        }
    }
}