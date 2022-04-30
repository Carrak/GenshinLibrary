using Discord.Interactions;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    public class Support : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "You know what this command does")]
        public async Task Ping()
        {
            Stopwatch sw = Stopwatch.StartNew();
            await RespondAsync("Pong!");
            await ModifyOriginalResponseAsync(x => x.Content = $"Pong! | `Ping: {sw.ElapsedMilliseconds}ms` | `WebSocket: {Context.Client.Latency}ms`");
        }

        [SlashCommand("help", "Basic information about the bot")]
        public async Task Help()
        {
            await RespondAsync(embed: Globals.HelpEmbed);
        }
    }
}
