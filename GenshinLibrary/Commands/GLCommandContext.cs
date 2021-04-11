using Discord.Commands;
using Discord.WebSocket;

namespace GenshinLibrary.Commands
{
    public class GLCommandContext : SocketCommandContext
    {
        public GLCommandContext(DiscordSocketClient client, SocketUserMessage message) : base(client, message)
        {
        }
    }
}
