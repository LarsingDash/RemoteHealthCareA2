using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Shared.Log
{
    public class Logger
    {
        public static LogLevel PrintLevel = LogLevel.All;
        
        #region default code for it to work (internet)
        private const int StdOutputHandle = -11;
        private const uint EnableVirtualTerminalProcessing = 0x0004;
        private const uint DisableNewlineAutoReturn = 0x0008;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
        #endregion

        /// <summary>
        /// It takes a message and an optional exception, and prints it to the console
        /// </summary>
        /// <param name="LogImportance">This is an enum that contains the importance of the log message.</param>
        /// <param name="message">The message to be printed</param>
        /// <param name="exception">The exception to log.</param>
        /// <returns>
        /// The return value is a string.
        /// </returns>
        public static void LogMessage(LogImportance logImportance, string message, Exception? exception = null)
        {
            if (logImportance.Level > (int) PrintLevel)
                return;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var iStdOut = GetStdHandle(StdOutputHandle);
                _ = GetConsoleMode(iStdOut, out var outConsoleMode) && SetConsoleMode(iStdOut, outConsoleMode | EnableVirtualTerminalProcessing);
            
            }
            var builder = new StringBuilder();
        
            //Buildup first line
            CreatePrefix(logImportance, builder);
            builder.Append(logImportance.ColorCode + message);

            if (exception != null)
            {
                var ex = exception;
                while (ex != null)
                {
                    builder.AppendLine();
                    builder.Append("   " + LogColor.Yellow.Color);
                    builder.Append(exception.GetType().Name.Replace("Exception", string.Empty));
                    builder.Append(LogColor.Gray.Color + ": " + LogColor.Blue + exception.Message);
                    ex = ex.InnerException;
                }

                if (exception.StackTrace != null)
                {
                    var stackTraceElements = exception.StackTrace.Split('\n').Select(x => x.Trim()).ToList();
                    foreach (var stackTraceElement in stackTraceElements)
                    {
                        builder.AppendLine();
                        builder.Append(LogColor.Gray.Color + "     ");
                        builder.Append(stackTraceElement);
                    }
                }
            }

            builder.Append(LogColor.White.Color);
            Console.WriteLine(builder);

        }

        /// <summary>
        /// It takes a LogImportance and a StringBuilder, and appends a prefix to the StringBuilder
        /// </summary>
        /// <param name="LogImportance">The importance of the log.</param>
        /// <param name="StringBuilder">This is the string that will be logged.</param>
        private static void CreatePrefix(LogImportance logImportance, StringBuilder builder)
        {
            builder.Append(LogColor.Gray.Color + "[" + logImportance.ColorCode + logImportance.Name +  LogColor.Gray.Color + "] ");
        }
    
    }

    /* An enum that is used to determine the importance of the log message. */
    public enum LogLevel : ushort
    {
        All = 6,
        Debug = 4,
        Information = 5,
        Warning = 3,
        Error = 2, 
        Fatal = 1,
        Off = 0,
    
    }
}