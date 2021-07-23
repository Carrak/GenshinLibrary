using Discord;
using System;

namespace GenshinLibrary
{
    static class Logger
    {
        public static void Log(string source, string message) => Console.WriteLine(new LogMessage(LogSeverity.Info, source, message));
    }
}
