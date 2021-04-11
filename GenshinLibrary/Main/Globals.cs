using Discord;
using System.IO;
using System.Reflection;

namespace GenshinLibrary
{
    public static class Globals
    {
        public static string DefaultPrefix { get; } = "gl!";
        public static Color MainColor { get; } = new Color(74, 185, 169);
        public static string ProjectDirectory { get; } = Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Parent.Parent.FullName + Path.DirectorySeparatorChar;
    }
}
