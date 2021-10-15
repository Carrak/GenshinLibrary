﻿using Discord;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace GenshinLibrary
{
    public static class Globals
    {
        public static string DefaultPrefix { get; } = "gl!";
        public static Color MainColor { get; } = new Color(74, 185, 169);
        public static string ProjectDirectory { get; } = Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Parent.Parent.FullName + Path.DirectorySeparatorChar;
        public static bool Maintenance { get; set; }

        public static ulong GenshinLibraryGuildID = 830707093131624457;

        public static Config GetConfig()
        {
            string text = File.ReadAllText($"{ProjectDirectory}genlibconfig.json");
            return JsonConvert.DeserializeObject<Config>(text);
        }
    }
}
