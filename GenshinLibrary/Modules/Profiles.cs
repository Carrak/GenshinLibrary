using Discord;
using Discord.Commands;
using GenshinLibrary.Analytics;
using GenshinLibrary.Commands;
using GenshinLibrary.Models;
using GenshinLibrary.Preconditions;
using GenshinLibrary.Services.Wishes;
using Npgsql;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        public Profiles(WishService wishService)
        {
            _wishes = wishService;
        }

        [Command("profile")]
        [Alias("p")]
        [Summary("View your profile.")]
        [Ratelimit(5)]
        public async Task UserProfile() => await UserProfile(Context.User);

        [Command("profile")]
        [Alias("p")]
        [Summary("View someone's profile.")]
        [Ratelimit(5)]
        public async Task UserProfile(
            [Summary("The user whose profile you want to see")][Remainder] IUser user
            )
        {
            var embed = new EmbedBuilder();
            Dictionary<Banner, BannerStats> analytics = null;

            try
            {
                analytics = await _wishes.GetAnalyticsAsync(user);
            }
            catch (PostgresException)
            {
                embed.WithFooter("Set your server using gl!setserver to view wish stats in profile.");
            }

            var pities = await _wishes.GetPities(user);
            var profile = await _wishes.GetProfileAsync(user);

            string fileName = "avatar.png";
            using var bitmap = new Bitmap(profile.ProfileCharacter?.AvatarImagePath ?? Character.DefaultAvatarPath);
            using var image = new MemoryStream();
            bitmap.Save(image, System.Drawing.Imaging.ImageFormat.Png);
            image.Position = 0;

            embed.WithColor(profile.ProfileCharacter is null ? GenshinColors.NoElement : GenshinColors.GetElementColor(profile.ProfileCharacter.Vision))
                .WithTitle(user.ToString())
                .AddField("5★ Characters", $"{(profile.Characters.Any() ? string.Join('\n', profile.Characters.Select(x => x.ToString())) : "None yet!")}")
                .AddField("5★ Weapons", $"{(profile.Weapons.Any() ? string.Join('\n', profile.Weapons.Select(x => x.ToString())) : "None yet!")}")
                .WithThumbnailUrl($"attachment://{fileName}");

            if (analytics != null)
            {
                var analyticsValues = analytics.Values;
                var total = analyticsValues.Sum(x => x.TotalWishes);
                var fivestar = analyticsValues.Sum(x => x.FiveStarWishes);
                var fourstar = analyticsValues.Sum(x => x.FourStarWishes);
                var threestar = analyticsValues.Sum(x => x.ThreeStarWishes);

                if (total != 0)
                    embed.AddField("Stats",
                        $"Wishes: **{total}**\n" +
                        $"3★: **{threestar}** | **{threestar / (float)total:0.00%}**\n" +
                        $"4★: **{fourstar}** | **{fourstar / (float)total:0.00%}**\n" +
                        $"5★: **{fivestar}** | **{fivestar / (float)total:0.00%}**\n"
                        );
            }

            if (pities != null)
                embed.AddField("Pities", pities.ToString(), true);

            if (analytics != null)
                embed.AddField("Rate Ups",
                    $"{(analytics[Banner.Character] as EventBannerStats).RateUpGuarantees()}\n" +
                    $"{(analytics[Banner.Weapon] as EventBannerStats).RateUpGuarantees()}", true);

            await Context.Channel.SendFileAsync(image, fileName, embed: embed.Build());
        }

        [Command("setavatar")]
        [Summary("Change your profile avatar.")]
        [Alias("avatar", "changeavatar")]
        [Ratelimit(5)]
        public async Task SetAvatar(
            [Summary("The name of the character to set as the avatar.")][Remainder] WishItem wi
            )
        {
            if (wi is Character c)
            {
                await _wishes.SetAvatarAsync(Context.User, c);
                await ReplyAsync("Successfully changed!");
            }
            else
                await ReplyAsync("Given wish item is not a character.");
        }

        [Command("resetavatar")]
        [Summary("Set your avatar to default (Aether).")]
        [Alias("deleteavatar")]
        [Ratelimit(5)]
        public async Task ResetAvatar()
        {
            await _wishes.RemoveAvatarAsync(Context.User);

            await ReplyAsync("Successfully reset.");
        }
    }
}
