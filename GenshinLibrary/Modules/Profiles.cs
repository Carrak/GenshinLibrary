using Discord;
using Discord.Commands;
using GenshinLibrary.Commands;
using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Preconditions;
using GenshinLibrary.Services;
using GenshinLibrary.Services.Wishes;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Summary("View and manage your profile.\nProfiles display your 5 stars and current pity counters across all banners. " +
        "They use the same data as analytics and wishes, so if you're wondering how to add anything to your profile, you might want to import your wishes first. " +
        "For that, refer to the `Wishes` module.")]
    public class Profiles : GLInteractiveBase
    {
        private readonly WishService _wishes;
        private readonly PatreonService _patreon;

        public Profiles(WishService wishService, PatreonService patreon)
        {
            _wishes = wishService;
            _patreon = patreon;
        }

        [Command("profile")]
        [Summary("View your profile.")]
        [Ratelimit(5)]
        public async Task UserProfile() => await UserProfile(Context.User);

        [Command("profile")]
        [Summary("View someone's profile.")]
        [Ratelimit(5)]
        public async Task UserProfile(
            [Summary("The user whose profile you want to see")][Remainder] IUser user
            )
        {
            var analytics = await _wishes.GetAnalyticsAsync(user);
            var pities = await _wishes.GetPities(user);
            var profile = await _wishes.GetProfileAsync(user);

            string fileName = "avatar.png";

            var color = profile.Character is null ? GenshinColors.NoElement : GenshinColors.GetElementColor(profile.Character.Vision);
            var image = profile.Character.GetAvatar();

            var embed = new EmbedBuilder();

            string name = user.ToString();
            if (_patreon.IsPatron(user))
                name = $"{name} | {_patreon.PatreonLogo} Supporter";

            embed.WithColor(color)
                .WithTitle(name)
                .AddField("5★ Characters", $"{(profile.Characters.Any() ? string.Join('\n', profile.Characters.Select(x => x.ToString())) : "None yet!")}")
                .AddField("5★ Weapons", $"{(profile.Weapons.Any() ? string.Join('\n', profile.Weapons.Select(x => x.ToString())) : "None yet!")}")
                .WithThumbnailUrl($"attachment://{fileName}");

            if (analytics != null)
            {
                var total = analytics.Sum(x => x.TotalWishes);
                var fivestar = analytics.Sum(x => x.FiveStarWishes);
                var fourstar = analytics.Sum(x => x.FourStarWishes);
                var threestar = analytics.Sum(x => x.ThreeStarWishes);

                if (total != 0)
                    embed.AddField("Stats",
                        $"Wishes: **{total}**\n" +
                        $"3-star: **{threestar}** // **{threestar / (float)total:0.00%}**\n" +
                        $"4-star: **{fourstar}** // **{fourstar / (float)total:0.00%}**\n" +
                        $"5-star: **{fivestar}** // **{fivestar / (float)total:0.00%}**\n"
                        );
            }

            if (pities != null)
                embed.AddField("Pities", pities.ToString());

            await Context.Channel.SendFileAsync(image, fileName, embed: embed.Build());
        }

        [Command("setavatar")]
        [Summary("Change your profile avatar.")]
        [Alias("avatar", "changeavatar")]
        [Ratelimit(5)]
        public async Task SetAvatar(
            [Summary("The name of the character to set as the avatar.")][Remainder] string character
            )
        {
            if (_wishes.WishItems.TryGetValue(character, out WishItem wi) && wi is Character foundCharacter)
            {
                await _wishes.SetAvatarAsync(Context.User, foundCharacter);
                await ReplyAsync("Successfully changed!");
            }
            else
                await ReplyAsync("No such character exists.");
        }
    }
}
