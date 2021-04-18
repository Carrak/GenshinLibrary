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
    }
}
