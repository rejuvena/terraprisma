// ReSharper disable InvalidXmlDocComment

using System;
using System.IO;

namespace Rejuvena.Terraprisma.Utilities
{
    /// <summary>
    ///     Provides various logging utilities.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        ///     Whether the logger has been initialized.
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        ///     If a file was created, this <see cref="StreamWriter"/> writes data to the log file.
        /// </summary>
        public static StreamWriter? FileWriter { get; private set; }

        private static FileStream? CreatedFile;

        internal static void Initialize()
        {
            Initialized = true;

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
            FileWriter?.Flush();
            CreatedFile?.Flush();
        }

        /// <inheritdoc cref="LogMessage(string,string)"/>
        /// <param name="severity">The severity (INFO, DEBUG, etc.).</param>
        public static void LogMessage(string owner, string severity, string message) =>
            LogMessage(owner, $"[{severity}] {message}");
    }
}