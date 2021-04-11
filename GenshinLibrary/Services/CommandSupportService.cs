using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary
{
    public class CommandSupportService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandSupportService(DiscordSocketClient client, CommandService commands)
        {
            _client = client;
            _commands = commands;
        }

        public string FormatCommandForHelp(ModuleInfo module, CommandInfo command) => $"`{GetFullCommandName(command)}{(IsNameUnique(module, command) ? "*" : "")}`";

        /// <summary>
        ///     Constructs a predetermined embed which has the primary information about the bot.
        /// </summary>
        /// <param name="guild">Guild for determining prefix</param>
        /// <returns>Info embed</returns>
        public Embed GetInfoEmbed()
        {
            var embed = new EmbedBuilder();

            /*embed.AddField("Getting started", $"Use `{_database.GetPrefix(guild.Id)}help` for the list of command modules and more info.")
                .AddField("Invite the bot!", "[Click me to invite!](https://discordapp.com/oauth2/authorize?client_id=689177733464457275&scope=bot&permissions=8)")
                .WithTitle("I am TeaBot!")
                .WithDescription("TeaBot is a bot created for various handy features, fun commands, math, anime art search and detailed server statistics.")
                .WithCurrentTimestamp()
                .WithColor(Globals.MainColor)
                .WithFooter(infoFooter);*/

            return embed.Build();
        }

        public bool IsNameUnique(ModuleInfo module, CommandInfo command) => module.Commands.Count(x => x.Name == command.Name) > 1;

        public string GetFullCommandName(CommandInfo command)
        {
            string groups = "";

            ModuleInfo currentModule = command.Module;

            while (currentModule != null)
            {
                if (currentModule.Group != null)
                    groups += $"{currentModule.Group} ";
                currentModule = currentModule.Parent;
            }

            return $"{groups}{command.Name}";
        }

        public string GetCommandHeader(CommandInfo command)
        {
            string commandName = GetFullCommandName(command);
            string parameters = GetParametersString(command);

            return $"{commandName}{(string.IsNullOrEmpty(parameters) ? "" : $" {parameters}")}";
        }

        public string GetParametersString(CommandInfo command)
        {
            List<string> parameters = new List<string>();
            foreach (var parameter in command.Parameters)
            {
                string paramBody = parameter.Type.IsEnum ? string.Join("/", parameter.Type.GetEnumNames().Select(x => x.ToLower())) : parameter.Name;
                string fullParam = parameter.IsOptional ? $"<{paramBody}>" : $"[{paramBody}]";
                parameters.Add(fullParam);
            }

            return string.Join(' ', parameters);
        }

        public string GetCommandHeader(string commandName)
        {
            var searchResult = _commands.Search(commandName);

            if (!searchResult.IsSuccess)
                return null;

            return GetCommandHeader(searchResult.Commands[0].Command);
        }

        public IEnumerable<ModuleInfo> GetModuleTree(ModuleInfo module)
        {
            List<ModuleInfo> modules = new List<ModuleInfo>()
            {
                module
            };

            foreach (var submodule in module.Submodules)
                modules.AddRange(GetModuleTree(submodule));

            return modules;
        }
    }
}
