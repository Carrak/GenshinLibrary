using Discord;
using System;
using System.Threading.Tasks;

namespace GenshinLibrary.Utility
{
    static class MessageUtilities
    {
        public async static Task<IMessage> ParseMessageFromLinkAsync(Discord.Commands.SocketCommandContext context, string link)
        {

            string pattern = @"(http|https?:\/\/)?(www\.)?(discord\.(gg|io|me|li|com)|discord(app)?\.com\/channels)\/(?<Guild>\w+)\/(?<Channel>\w+)\/(?<Message>\w+)";
            var match = System.Text.RegularExpressions.Regex.Match(link, pattern);

            // Check if the given link matches the pattern
            if (!match.Success)
                throw new FormatException("Данная ссылка не соответствует паттерну ссылки на сообщение - `https://discordapp.com/channels/{guild}/{channel}/{id}`");

            // Check if the guild in the link is valid
            if (!ulong.TryParse(match.Groups["Guild"].Value, out var guildid))
                throw new FormatException("Некорректный ID сервера.");

            // Check if the channel is valid
            if (!ulong.TryParse(match.Groups["Channel"].Value, out var channelid))
                throw new FormatException("Некорректный ID канала.");

            // Check if the guild is the same as this one
            if (context.Guild.Id != guildid)
                throw new FormatException("Указанное сообщение из другого сервера.");

            // Check if the message is valid
            if (!ulong.TryParse(match.Groups["Message"].Value, out var messageid))
                throw new FormatException("Некорректное ID сообщения.");

            // Check if the channel exists
            if (!(context.Guild.GetTextChannel(channelid) is ITextChannel channel))
                return null;

            // Return the message
            return await channel.GetMessageAsync(messageid);

        }
    }
}
