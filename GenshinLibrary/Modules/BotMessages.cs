using Discord;
using Discord.Commands;
using Discord.Net;
using GenshinLibrary.Attributes;
using GenshinLibrary.Commands;
using GenshinLibrary.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Main
{
    [RequireOwner]
    [HelpIgnore]
    [Group("ce")]
    public class BotMessages : GLInteractiveBase
    {
        private static CustomMessage customMessage = new CustomMessage();
        private static readonly Emoji success = new Emoji("✅");

        private static readonly string noEmbedMessage = $"Add an embed first - `{Globals.DefaultPrefix}refreshembed`";

        [Command("cachemessage")]
        public async Task CacheEmbed(string messageLink)
        {
            IUserMessage message;
            try
            {
                message = await MessageUtilities.ParseMessageFromLinkAsync(Context, messageLink) as IUserMessage;
            }
            catch (HttpException)
            {
                await ReplyAsync("No access.");
                return;
            }
            catch (FormatException fe)
            {
                await ReplyAsync(fe.Message);
                return;
            }

            if (message is null)
            {
                await ReplyAsync($"The message does not exist.");
                return;
            }

            customMessage = new CustomMessage()
            {
                Text = message.Content,
                Message = message
            };

            if (message.Embeds.FirstOrDefault(x => x.Type == EmbedType.Rich) is Embed embed)
                customMessage.Embed = embed.ToEmbedBuilder();

            await Context.Message.AddReactionAsync(success);
        }

        [Command("refreshall")]
        public async Task RefreshAll()
        {
            customMessage = new CustomMessage();
            await Context.Message.AddReactionAsync(success);
        }

        [Command("preview")]
        public async Task Preview()
        {
            if (customMessage.Text is null && customMessage.Embed is null)
            {
                await ReplyAsync("The custom message is empty.");
                return;
            }

            await ReplyAsync(customMessage?.Text, embed: customMessage.Embed?.Build());
        }

        [Command("send")]
        public async Task Send(ITextChannel channel)
        {
            if (customMessage.Text is null && customMessage.Embed is null)
            {
                await ReplyAsync("The message is empty and cannot be sent.");
                return;
            }

            try
            {
                await channel.SendMessageAsync(customMessage?.Text, embed: customMessage?.Embed?.Build());
                customMessage = new CustomMessage();
                await Context.Message.AddReactionAsync(success);
            }
            catch (HttpException)
            {
                await ReplyAsync($"Cannot send the message to {channel.Mention}");
            }
        }

        [Command("save")]
        public async Task Save()
        {
            if (customMessage.Message is null)
            {
                await ReplyAsync($"This custom message is not bound to a message. Use `{Globals.DefaultPrefix}send [channel]`");
                return;
            }

            var component = new ComponentBuilder()
                .WithButton("Server invite", "server_invite", ButtonStyle.Link, url: "https://discord.gg/4P23TZFZUN")
                .WithButton("Bot invite", "bot_invite", ButtonStyle.Link, url: "https://discord.com/oauth2/authorize?client_id=830870729390030960&scope=bot&permissions=298048")
                .WithButton("Patreon", "patreon", ButtonStyle.Link, url: "https://www.patreon.com/genshinlibrary")
                .Build();

            await customMessage.Message.ModifyAsync(x => { x.Content = customMessage.Text; x.Embed = customMessage.Embed?.Build(); x.Components = component; });
            await Context.Message.AddReactionAsync(success);
            customMessage = new CustomMessage();
        }

        [Command("removeembed")]
        public async Task RemoveEmbed()
        {
            customMessage.Embed = null;
            await Context.Message.AddReactionAsync(success);
        }

        [Command("refreshembed")]
        public async Task CreateEmbed()
        {
            customMessage.Embed = new EmbedBuilder();
            await Context.Message.AddReactionAsync(success);
        }

        [Command("withtext")]
        public async Task WithText([Remainder] string text)
        {
            if (text.Length > 2000)
            {
                await ReplyAsync("The message cannot exceed 2000 symbols in length.");
                return;
            }

            customMessage.Text = text;
            await Context.Message.AddReactionAsync(success);
        }

        [Command("withauthor")]
        public async Task WithAuthor(IUser user)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.WithAuthor(user);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("withdescription")]
        public async Task WithDescription([Remainder] string text)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.WithDescription(text);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("withimage")]
        public async Task WithImage(string url)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.WithImageUrl(url);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("withfooter")]
        public async Task WithFooter([Remainder] string text)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.WithFooter(text);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("addfield")]
        public async Task AddField(string name, [Remainder] string text)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            if (name.Length > 256)
            {
                await ReplyAsync("Field name must be less than 256 symbols.");
                return;
            }

            if (text.Length > 1024)
            {
                await ReplyAsync("Field value must be less than 1024 symbols.");
                return;
            }

            customMessage.Embed.AddField(name, text);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("addfieldinline")]
        public async Task AddFieldInline(string name, [Remainder] string text)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            if (name.Length > 256)
            {
                await ReplyAsync("Field name must be less than 256 symbols.");
                return;
            }

            if (text.Length > 1024)
            {
                await ReplyAsync("Field value must be less than 1024 symbols.");
                return;
            }

            customMessage.Embed.AddField(name, text, true);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("editfield")]
        public async Task EditField(string name, [Remainder] string newText)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            int index = customMessage.Embed.Fields.FindIndex(x => x.Name == name);

            if (index == -1)
            {
                await ReplyAsync("No field with such name exists.");
                return;
            }

            var field = customMessage.Embed.Fields[index];
            customMessage.Embed.Fields[index] = new EmbedFieldBuilder() { Name = field.Name, Value = newText, IsInline = field.IsInline };
            await Context.Message.AddReactionAsync(success);
        }

        [Command("reorderfields")]
        public async Task ReorderFields(int index1, int index2)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            index1--;
            index2--;

            if (Math.Max(index2, index1) > customMessage.Embed.Fields.Count || index1 < 0 || index2 < 0)
            {
                await ReplyAsync("Index out of bounds.");
                return;
            }

            var temp = customMessage.Embed.Fields[index1];
            customMessage.Embed.Fields[index1] = customMessage.Embed.Fields[index2];
            customMessage.Embed.Fields[index2] = temp;
            await Context.Message.AddReactionAsync(success);
        }

        [Command("removefield")]
        public async Task RemoveField(string name)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            int index = customMessage.Embed.Fields.FindIndex(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (index == -1)
            {
                await ReplyAsync("No field exists with such name.");
                return;
            }

            customMessage.Embed.Fields.RemoveAt(index);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("withtitle")]
        public async Task WithTitle([Remainder] string text)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            if (text.Length > 256)
            {
                await ReplyAsync("Title must be less than 256 symbols.");
                return;
            }

            customMessage.Embed.WithTitle(text);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("withcolor")]
        public async Task WithColor(Color color)
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed = customMessage.Embed.WithColor(color);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("removedescription")]
        public async Task RemoveDescription()
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.WithDescription(null);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("removetitle")]
        public async Task RemoveTitle()
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.WithTitle(null);
            await Context.Message.AddReactionAsync(success);
        }

        [Command("removefooter")]
        public async Task RemoveFooter()
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.Footer = null;
            await Context.Message.AddReactionAsync(success);
        }

        [Command("removeimage")]
        public async Task RemoveImage()
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.ImageUrl = null;
            await Context.Message.AddReactionAsync(success);
        }

        [Command("removeauthor")]
        public async Task RemoveAuthor()
        {
            if (customMessage.Embed is null)
            {
                await ReplyAsync(noEmbedMessage);
                return;
            }

            customMessage.Embed.Author = null;
            await Context.Message.AddReactionAsync(success);
        }

        [Command("removetext")]
        public async Task RemoveText()
        {
            customMessage.Text = null;
            await Context.Message.AddReactionAsync(success);
        }
    }

    public class CustomMessage
    {
        public EmbedBuilder Embed { get; set; }
        public string Text { get; set; }
        public IUserMessage Message { get; set; }
    }
}
