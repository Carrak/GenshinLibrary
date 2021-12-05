using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using GenshinLibrary.Attributes;
using GenshinLibrary.ReactionCallback.Base;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.ReactionCallback
{
    class CommandHelp : SingleItemPagedMessage<CommandInfo>
    {
        private readonly CommandSupportService support;

        public CommandHelp(InteractiveService interactive,
            CommandSupportService tea,
            SocketCommandContext context,
            IEnumerable<CommandInfo> commands) : base(interactive, context, commands)
        {
            support = tea;
        }

        protected override Embed ConstructEmbed(CommandInfo cmd)
        {
            var embed = new EmbedBuilder();

            if (cmd.Aliases.Count > 1)
                embed.AddField("Aliases", string.Join(", ", cmd.Aliases.Where(name => name != cmd.Name)));

            embed.WithTitle($"{Globals.DefaultPrefix}{support.GetCommandHeader(cmd)}")
                .WithColor(Globals.MainColor)
                .WithDescription(GetSummary(cmd.Summary, "command"))
                .WithFooter($"{Page + 1} / {TotalPages}");

            var parameters = cmd.Parameters.Where(x => !x.Type.IsEnum);
            if (parameters.Any())
                embed.AddField("Parameters", string.Join("\n\n",
                    parameters.Select((param, index) => $"**{index + 1}.** `{param.Name}` {(param.IsOptional ? " [Optional]" : "")}\n{GetSummary(param.Summary, "parameter")}")));

            if (cmd.GetAttribute<ExampleAttribute>() is ExampleAttribute ea)
                embed.AddField("Example", ea.Value);

            if (cmd.GetAttribute<GifExampleAttribute>() is GifExampleAttribute gea)
                embed.WithImageUrl(gea.Link);

            return embed.Build();
        }

        private string GetSummary(string summary, string type) => string.IsNullOrEmpty(summary) ? $"No description for this {type} yet." : summary;
    }
}
