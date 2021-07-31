﻿using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary
{
    public class CommandSupportService
    {
        private readonly CommandService _commands;

        public CommandSupportService(CommandService commands)
        {
            _commands = commands;
        }

        public string FormatCommandForHelp(ModuleInfo module, CommandInfo command) => $"`{GetFullCommandName(command)}{(IsNameUnique(module, command) ? "*" : "")}`";

        /// <summary>
        ///     Constructs a predetermined embed which has the primary information about the bot.
        /// </summary>
        /// <returns>Info embed</returns>
        public EmbedBuilder GetInfoEmbed()
        {
            var embed = new EmbedBuilder()
                .WithTitle("GenshinLibrary / Info")
                .WithFooter("For any inquires regarding data deletion or security, please contact me directly: Carrak#8088")
                .AddField("Getting started", $"Use `{Globals.DefaultPrefix}help` for the list of command modules and more info.")
                .WithColor(Globals.MainColor)
                .WithDescription("GenshinLibrary is a tool bot made for Genshin Impact players that " +
                "allows easier access to player details and flexible control over them. Features: pity counters, wish history with filters, " +
                "personal analytics, resin tracker, profiles, various calculators and a gacha simulator.\n\n" +
                "If you're enjoying the bot, please vote for it and leave a review on [top.gg](https://top.gg/bot/830870729390030960)!\n" +
                $"{GetLinks()}");

            return embed;
        }

        public string GetLinks() => "[Community/Support Server](https://discord.gg/4P23TZFZUN) | " +
                "[Invite the bot](https://discord.com/oauth2/authorize?client_id=830870729390030960&scope=bot&permissions=298048)";

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
