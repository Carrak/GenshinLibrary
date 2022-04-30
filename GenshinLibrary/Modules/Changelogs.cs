using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [RequireOwner]
    [Group("changelog", "changelog")]
    [DontAutoRegister]
    public class Changelogs : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly List<string> updates = new();

        [SlashCommand("add", "add")]
        public async Task Add(string toAdd)
        {
            updates.Add($"- {toAdd}");
            await RespondAsync("Added", ephemeral: true);
        }

        [SlashCommand("clear", "clear")]
        public async Task Clear()
        {
            updates.Clear();
            await RespondAsync("Cleared", ephemeral: true);
        }

        [SlashCommand("view", "view")]
        public async Task View()
        {
            await RespondAsync(embed: GetEmbed(), ephemeral: true);
        }

        [SlashCommand("remove", "remove")]
        public async Task Remove(int index)
        {
            updates.RemoveAt(index);
            await RespondAsync("Removed", ephemeral: true);
        }

        [SlashCommand("send", "remove")]
        public async Task Send()
        {
            var channel = Context.Client.GetChannel(Globals.GetConfig().ChangelogChannelId) as ITextChannel;
            await channel.SendMessageAsync(embed: GetEmbed());
        }

        private static Embed GetEmbed()
        {
            var embed = new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithTitle($"Changelog {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC")
                .WithDescription(string.Join('\n', updates));

            return embed.Build();
        }
    }
}
