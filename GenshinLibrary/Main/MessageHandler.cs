using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace GenshinLibrary
{
    public class MessageHandler
    {
        private readonly DiscordSocketClient _client;

        public MessageHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public void Init()
        {
            _client.MessageReceived += HandleMessagesAsync;
        }

        private async Task HandleMessagesAsync(SocketMessage arg)
        {
            // Return if the message is from a bot
            if (arg is not SocketUserMessage message || message.Author.IsBot)
                return;

            if (message.Content.StartsWith("gl!"))
            {
                var embed = new EmbedBuilder()
                    .WithColor(Globals.MainColor)
                    .WithTitle("GenshinLibrary has moved to slash commands")
                    .WithDescription("That means all of the old text commands prefixed with `gl!` no longer exist\nTo use slash commands, make sure you have the corresponding setting enabled (shown below)\n\n" +
                    "If you've done this, but no commands show up from GenshinLibrary, you need to re-invite the bot with [this](https://discord.com/api/oauth2/authorize?client_id=830870729390030960&permissions=379968&scope=applications.commands%20bot) link")
                    .WithImageUrl("https://cdn.discordapp.com/attachments/461538521551863825/968556672085815396/unknown.png");

                await message.Channel.SendMessageAsync(embed: embed.Build());
            }
        }
    }
}
