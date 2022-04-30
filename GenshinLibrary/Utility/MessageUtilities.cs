using Discord;
using Discord.Interactions;
using GenshinLibrary.Services.Wishes;
using System.Threading.Tasks;

namespace GenshinLibrary.Utility
{
    static class MessageUtilities
    {
        public async static Task<Result<IMessage>> ParseMessageFromLinkAsync(SocketInteractionContext context, string link)
        {
            string pattern = @"(http|https?:\/\/)?(www\.)?(discord\.(gg|io|me|li|com)|discord(app)?\.com\/channels)\/(?<Guild>\w+)\/(?<Channel>\w+)\/(?<Message>\w+)";
            var match = System.Text.RegularExpressions.Regex.Match(link, pattern);

            // Check if the given link matches the pattern
            if (!match.Success)
                return new Result<IMessage>(null, false, "Couldn't match the link");

            // Check if the guild in the link is valid
            if (!ulong.TryParse(match.Groups["Guild"].Value, out var guildid))
                return new Result<IMessage>(null, false, "Invalid guild ID");

            // Check if the channel is valid
            if (!ulong.TryParse(match.Groups["Channel"].Value, out var channelid))
                return new Result<IMessage>(null, false, "Invalid channel ID");

            // Check if the guild is the same as this one
            if (context.Guild.Id != guildid)
                return new Result<IMessage>(null, false, "Specified message is from a different sever");

            // Check if the message is valid
            if (!ulong.TryParse(match.Groups["Message"].Value, out var messageid))
                return new Result<IMessage>(null, false, "Invalid message ID");

            // Check if the channel exists
            if (context.Guild.GetTextChannel(channelid) is not ITextChannel channel)
                return null;

            // Return the message
            return new Result<IMessage>(await channel.GetMessageAsync(messageid), true, null);

        }
    }
}
