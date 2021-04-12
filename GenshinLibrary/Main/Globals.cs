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

        public static ulong TierTwoRoleID = 831133658576322610;
        public static ulong TierOneRoleID = 831133541134762045;
        public static ulong GenshinLibraryGuildID = 830707093131624457;
    }
}
