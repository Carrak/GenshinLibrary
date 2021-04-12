using Discord;
using Discord.WebSocket;
using System.Linq;

namespace GenshinLibrary.Services
{
    public class PatreonService
    {
        public Emote PatreonLogo { get; } = Emote.Parse("<:Patreon:831167238308495441>");

        private readonly DiscordSocketClient _client;

        public PatreonService(DiscordSocketClient client)
        {
            _client = client;
        }

        public bool IsPatron(IUser user)
        {
            var guild = _client.GetGuild(Globals.GenshinLibraryGuildID);
            var guildUser = guild.GetUser(user.Id);

            if (guildUser is null)
                return false;

            return guildUser.Roles.Any(x => x.Id == Globals.TierOneRoleID || x.Id == Globals.TierTwoRoleID);
        }
    }
}
