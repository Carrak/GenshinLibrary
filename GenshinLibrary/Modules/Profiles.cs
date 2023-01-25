using Discord;
using Discord.Interactions;
using GenshinLibrary.Analytics;
using GenshinLibrary.AutocompleteHandlers;
using GenshinLibrary.Models;
using GenshinLibrary.Models.Profiles;
using GenshinLibrary.Services.Wishes;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Group("profile", "Profiles with neatly packed data ")]
    public class Profiles : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly WishService _wishes;

        public Profiles(WishService wishService)
        {
            _wishes = wishService;
        }

        [SlashCommand("view", "View your or someone's profile")]
        public async Task UserProfile(
            [Summary(description: "Whose analytics to display. Leave empty to see your own")] IUser user = null
            )
        {
            await DeferAsync();

            user ??= Context.User;

            var embed = new EmbedBuilder();
            var analyticsResult = await _wishes.GetAnalyticsAsync(user);

            var pities = await _wishes.GetPities(user);
            var profile = await _wishes.GetProfileAsync(user);

            string fileName = "avatar.png";
            using var image = SixLabors.ImageSharp.Image.Load(profile.ProfileCharacter?.AvatarImagePath ?? Character.DefaultAvatarPath);
            MemoryStream stream = new();
            image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);

            embed.WithColor(profile.ProfileCharacter is null ? GenshinColors.NoElement : GenshinColors.GetElementColor(profile.ProfileCharacter.Vision))
                .WithTitle(user.ToString())
                .AddField("5★ Characters", $"{(profile.Characters.Any() ? WishItemString(profile.Characters) : "None yet!")}", true)
                .AddField("5★ Weapons", $"{(profile.Weapons.Any() ? WishItemString(profile.Weapons) : "None yet!")}", true)
                .WithThumbnailUrl($"attachment://{fileName}");

            if (analyticsResult.IsSuccess)
            {
                var analyticsValues = analyticsResult.Value.Values;
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
            else
                embed.WithFooter("Set your server using gl!setserver to view wish stats in profile.");

            if (pities != null)
                embed.AddField("Pities", pities.ToString(), true);

            if (analyticsResult != null)
                embed.AddField("Rate Ups",
                    $"{(analyticsResult.Value[Banner.Character] as EventBannerStats).RateUpGuarantees()}\n" +
                    $"{(analyticsResult.Value[Banner.Weapon] as EventBannerStats).RateUpGuarantees()}", true);

            await FollowupWithFileAsync(stream, fileName, embed: embed.Build());

            static string WishItemString(IEnumerable<WishCount> wcs)
            {
                const int embedLimit = 1024;
                const string format = "\n+{0} more...";

                StringBuilder sb = new();
                int added = 0;
                var count = wcs.Count();

                foreach(var wc in wcs)
                {
                    var wcstr = $"\n{wc}";

                    if (sb.Length + wcstr.Length + 1 >= embedLimit - format.Length)
                    {
                        sb.Append(string.Format(format, count - added));
                        break;
                    }
                    
                    sb.Append(wcstr);
                    added++;
                }

                return sb.ToString();
            }
        }

        [SlashCommand("avatar", "Change your profile avatar")]
        public async Task SetAvatar(
            [Summary(description: "The character to set as the avatar."), Autocomplete(typeof(WishItemAutocomplete<Character>))] Character character
            )
        {
            await _wishes.SetAvatarAsync(Context.User, character);
            await RespondAsync($"Successfully changed your avatar to **{character.Name}**!");
        }

        [SlashCommand("resetavatar", "Reset your wish profile avatar to default")]
        public async Task ResetAvatar()
        {
            await _wishes.RemoveAvatarAsync(Context.User);
            await RespondAsync("Successfully reset.");
        }
    }
}
