using Discord;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace GenshinLibrary
{
    public static class Globals
    {
        public static Color MainColor { get; } = new(74, 185, 169);
        public static string ProjectDirectory { get; } = Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Parent.Parent.FullName + Path.DirectorySeparatorChar;
        public static Random Random { get; } = new();

        public static Embed HelpEmbed { get; } = new EmbedBuilder()
            .WithTitle("GenshinLibrary")
            .WithFooter("For any inquires regarding data deletion or security, please contact me directly: Carrak#8088")
            .WithColor(MainColor)
            .WithDescription("GenshinLibrary is a tool bot made for Genshin Impact players that " +
            "allows easier access to player details and flexible control over them. Features: pity counters, wish history with filters, " +
            "personal analytics, resin tracker, profiles, various calculators and a gacha simulator.\n" +
            "For help on commands, please refer to descriptions on provided slash commands!\n\n" +
            "If you're enjoying the bot, please vote for it and leave a review on [top.gg](https://top.gg/bot/830870729390030960)!\n" +
            "[Community/Support Server](https://discord.gg/4P23TZFZUN) | " +
            "[Invite the bot](https://discord.com/api/oauth2/authorize?client_id=830870729390030960&permissions=379968&scope=applications.commands%20bot)")
            .Build();

        public static Config GetConfig()
        {
            string path = File.ReadAllText($"{ProjectDirectory}genlibconfig.json");
            return JsonConvert.DeserializeObject<Config>(path);
        }
    }
}
