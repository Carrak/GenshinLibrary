using Discord;
using Discord.Commands;
using GenshinLibrary.Attributes;
using GenshinLibrary.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [RequireOwner]
    [HelpIgnore]
    [Group("cl")]
    public class Changelogs : GLInteractiveBase
    {
        private static readonly Emoji success = new Emoji("✅");
        private static readonly List<string> updates = new List<string>();

        private readonly ulong changelogsChannelID = 831565644272107582;

        [Command("add")]
        public async Task Add([Remainder] string toAdd)
        {
            updates.Add($"- {toAdd}");
            await Context.Message.AddReactionAsync(success);
        }

        [Command("clear")]
        public async Task Clear()
        {
            updates.Clear();
            await Context.Message.AddReactionAsync(success);
        }

        [Command("view")]
        public async Task View()
        {
            await ReplyAsync(embed: GetEmbed());
        }

        [Command("remove")]
        public async Task Remove(int index)
        {
            updates.RemoveAt(index);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("send")]
        public async Task Send()
        {
            var channel = Context.Client.GetGuild(Globals.GenshinLibraryGuildID).GetChannel(changelogsChannelID) as ITextChannel;
            await channel.SendMessageAsync(embed: GetEmbed());
        }

        private Embed GetEmbed()
        {
            var embed = new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithTitle($"Changelog {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC")
                .WithDescription(string.Join('\n', updates));

            return embed.Build();
        }
    }
}
