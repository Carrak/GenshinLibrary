using Discord;
using Discord.Commands;
using GenshinLibrary.Attributes;
using GenshinLibrary.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [RequireOwner]
    [HelpIgnore]
    public class Owner : GLInteractiveBase
    {
        private readonly CommandService _commands;
        private readonly CommandSupportService _support;

        public Owner(CommandService commands, CommandSupportService support)
        {
            _commands = commands;
            _support = support;
        }

        [Command("commands")]
        public async Task Commands()
        {
            string header = "| Command | Description |\n|-|-|";
            string result = $"{header}";

            foreach (var module in _commands.Modules.Where(x => !x.Attributes.Any(attr => attr is HelpIgnoreAttribute) && x.Name != "Support").Reverse())
            {
                string moduleString = $"";
                foreach (var cmd in module.Commands)
                    moduleString += $"\n| `{_support.GetCommandHeader(cmd)}` | {cmd.Summary.Split('\n')[0]} |";

                result += moduleString;
            }

            Console.WriteLine(result);
        }

        [Command("botstats")]
        public async Task Stats()
        {
            var embed = new EmbedBuilder();

            var client = Context.Client;

            embed.WithAuthor(client.CurrentUser)
                .WithColor(Color.Blue)
                .AddField("Guilds", client.Guilds.Count, true)
                .AddField("Users", client.Guilds.Sum(x => x.Users.Count), true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("maintenance")]
        public async Task Maintenance(bool value)
        {
            Globals.Maintenance = value;
            await ReplyAsync($"Maintenance: {value}");
        }
    }
}
