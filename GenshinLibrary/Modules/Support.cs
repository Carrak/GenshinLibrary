using Discord;
using Discord.Commands;
using GenshinLibrary.Attributes;
using GenshinLibrary.Commands;
using GenshinLibrary.Preconditions;
using GenshinLibrary.ReactionCallback;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Summary("Support/help on the bot and its commands.")]
    public class Support : GLInteractiveBase
    {
        private readonly CommandService _commands;
        private readonly CommandSupportService _support;

        public Support(CommandService commands, CommandSupportService support)
        {
            _commands = commands;
            _support = support;
        }

        [Command("ping")]
        [Summary("The bot's and gateway's latencies.")]
        [Ratelimit(5)]
        public async Task Ping()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var message = await ReplyAsync("Pong!");
            await message.ModifyAsync(x => x.Content = $"Pong! | `Ping: {sw.ElapsedMilliseconds}ms` | `WebSocket: {Context.Client.Latency}ms`");
        }

        [Command("info")]
        [Summary("Basic info about the bot.")]
        [Alias("invite", "patreon", "community")]
        [Ratelimit(5)]
        public async Task Info() => await ReplyAsync(embed: _support.GetInfoEmbed().Build());

        [Command("help")]
        [Summary("Help on a specific command or module.")]
        [Ratelimit(3)]
        public async Task HelpCommandModule(
            [Summary("The name of the module or the command to get help on.")][Remainder] string name
            )
        {
            var result = _commands.Search(name);

            if (result.IsSuccess &&
                (result.Commands.Where(x => !x.Command.Module.Attributes.Any(attr => attr is HelpIgnoreAttribute)) is IEnumerable<CommandMatch> commands) &&
                commands.Any())
            {
                var commandHelp = new CommandHelp(Interactive, _support, Context, commands.Select(cm => cm.Command));
                await commandHelp.DisplayAsync();
            }
            else
            {
                var module = _commands.Modules.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() && !x.Attributes.Any(attr => attr is HelpIgnoreAttribute));

                if (module is null)
                {
                    await ReplyAsync($"Such command/module does not exist! `{name}`");
                    return;
                }

                var embed = new EmbedBuilder();

                embed.WithTitle($"{module.Name}")
                    .WithDescription(
                    $"{module.Summary}\n\n" +
                    $"Use `{Globals.DefaultPrefix}help [command name]` to get help on a specific command and see what each parameter means.\n" +
                    $"Parameters in `[]` denote necessary parameteres, whereas parameters in `<>` can be left unspecified.\n" +
                    $"If you have any questions, you can join our [community server](https://discord.gg/4P23TZFZUN)!")
                    .WithColor(Globals.MainColor)
                    .AddField("Commands", FormatCommands(module));

                await ReplyAsync(embed: embed.Build());
            }
        }

        [Command("help")]
        [Summary("General help on the bot.")]
        [Ratelimit(5)]
        public async Task HelpCommand()
        {
            string prefix = Globals.DefaultPrefix;
            var modules = _commands.Modules.Where(x => !string.IsNullOrEmpty(x.Summary)).OrderByDescending(x => x.Name.Length);

            var embed = new EmbedBuilder()
              .WithColor(Globals.MainColor)
              .WithTitle("GenshinLibrary / Help")
              .WithDescription($"`{prefix}help [command]` - information about a command.\n" +
              $"`{prefix}help [module]` - information about a module and its commands.\n" +
              $"If you have any questions, you can join our [community server](https://discord.gg/4P23TZFZUN)!")
              .AddField("Modules", string.Join('\n', modules.Select(x => $"`{x.Name}` - {x.Summary.Split('\n')[0]}")));

            await ReplyAsync(embed: embed.Build());
        }

        private string FormatCommands(ModuleInfo module) => string.Join('\n', module.Commands.Select(x => $"`{_support.GetCommandHeader(x)}`\n - {x.Summary.Split('\n')[0]}"));
    }
}
