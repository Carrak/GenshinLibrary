using Discord;
using Discord.Commands;
using GenshinLibrary.Commands;
using GenshinLibrary.Models;
using GenshinLibrary.Preconditions;
using GenshinLibrary.Services.GachaSim;
using GenshinLibrary.Services.Wishes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Group("sim")]
    [Name("GachaSimulator")]
    [Summary("Gacha simulator for you to play around with :)\nIf not used for 24 hours, your wish inventory resets.")]
    public class GachaSimulator : GLInteractiveBase
    {
        private readonly WishService _wishes;
        private readonly GachaSimulatorService _sim;

        private readonly string fileName = "result.png";

        public GachaSimulator(WishService wishes, GachaSimulatorService sim)
        {
            _wishes = wishes;
            _sim = sim;
        }

        [Command("wish", RunMode = RunMode.Async)]
        [Ratelimit(7, WarnOnExceeded = false)]
        [Summary("Wish on the gacha simulator.")]
        public async Task Wish(
            [Summary("The amount of wishes to make, from 1 to 100. Defaulted to 10 if not specified.")] int count = 10
            )
        {
            var profile = _sim.GetOrCreateProfile(Context.User);
            var bannerName = profile.GetCurrentSession().Banner.GetFullName();
            var pityBefore = profile.GetCurrentSession().CurrentFiveStarPity;
            var wishCountBefore = profile.Inventory.Count;

            GachaSimWishItemRecord[] result;
            try
            {
                result = _sim.Wish(Context.User, count);
            }
            catch (Exception e)
            {
                await ReplyAsync(e.Message);
                return;
            }

            var pityAfter = profile.GetCurrentSession().CurrentFiveStarPity;
            var wishCountAfter = profile.Inventory.Count;

            var rarityColor = result.Max(x => x.WishItem.Rarity) switch
            {
                3 => Color.Blue,
                4 => Color.Purple,
                5 => Color.Gold,
                _ => throw new NotImplementedException()
            };

            var embed = new EmbedBuilder()
                .WithTitle($"Wishing {(count == 1 ? "once" : $"{count} times")}... Pity: `{pityBefore}`")
                .WithFooter($"Total wishes: {wishCountBefore} | Banner: {bannerName}")
                .WithColor(Color.DarkGrey);

            var message = await ReplyAsync(embed: embed.Build());
            await Task.Delay(2500);

            embed.WithColor(rarityColor)
                .WithFooter($"Total wishes: {wishCountAfter} | Banner: {bannerName}")
                .WithTitle($"Wishing {(count == 1 ? "once" : $"{count} times")}... Pity: `{pityBefore} -> {pityAfter}`");

            await message.ModifyAsync(x => x.Embed = embed.Build());
            await Task.Delay(1500);

            var resultEmbed = new EmbedBuilder()
                .WithColor(rarityColor);

            if (count > 10)
            {
                var counts = new WishCounts(result);

                resultEmbed.WithTitle($"{count} wishes results");

                if (counts.Fivestars.Count > 0)
                    resultEmbed.AddField("5★ items", string.Join('\n', counts.Fivestars.Select(x => FormatWishItemCount(x))));
                if (counts.Fourstars.Count > 0)
                    resultEmbed.AddField("4★ items", string.Join('\n', counts.Fourstars.Select(x => FormatWishItemCount(x))));
                if (counts.Threestars.Count > 0)
                    resultEmbed.AddField("3★ items", string.Join('\n', counts.Threestars.Select(x => FormatWishItemCount(x))));

                await ReplyAsync(embed: resultEmbed.Build());
                return;
            }

            var wishImage = new WishImage(result);
            using var stream = wishImage.GetImage();

            resultEmbed.WithImageUrl($"attachment://{fileName}");
            await Context.Channel.SendFileAsync(stream, fileName, embed: resultEmbed.Build());
        }

        [Command("banner", RunMode = RunMode.Async)]
        [Summary("Select a banner to wish on.")]
        [Alias("wishon", "selectbanner", "select")]
        [Ratelimit(5)]
        public async Task SelectBanner(Banner banner)
        {
            List<WishBanner> selection = _wishes.Banners.Values.Where(x => x.BannerType == banner).ToList();
            WishBanner selectedBanner = await BannerSelectionAsync(selection);

            if (selectedBanner != null)
            {
                _sim.ChangeBanner(Context.User, selectedBanner);
                await ReplyAsync("Sucessfully changed banner.");
            }
        }

        [Command("inventory")]
        [Alias("items", "inv")]
        [Summary("View your current items.")]
        [Ratelimit(5)]
        public async Task History()
        {
            var profile = _sim.GetOrCreateProfile(Context.User);

            if (profile.Inventory.Count == 0)
            {
                await ReplyAsync($"Your inventory is empty! Do some wishes using `{Globals.DefaultPrefix}sim wish`");
                return;
            }

            var counts = new WishCounts(profile.Inventory);

            string fivestarsString = string.Join('\n', counts.Fivestars.Select(x => FormatWishItemCount(x)));
            string fourstarString = string.Join('\n', counts.Fourstars.Select(x => FormatWishItemCount(x)));
            string threestarString = string.Join('\n', counts.Threestars.Select(x => FormatWishItemCount(x)));

            var fivestar = counts.Fivestars.Sum(x => x.Count);
            var fourstar = counts.Fourstars.Sum(x => x.Count);
            var threestar = counts.Threestars.Sum(x => x.Count);
            var total = fivestar + fourstar + threestar;

            var embed = new EmbedBuilder()
                .WithTitle($"{Context.User}")
                .WithColor(Globals.MainColor)
                .WithFooter($"{Globals.DefaultPrefix}sim reset - resets your inventory.")
                .AddField("Stats",
                    $"Wishes: **{total}**\n" +
                    $"3-star: **{threestar}** // **{threestar / (float)total:0.00%}**\n" +
                    $"4-star: **{fourstar}** // **{fourstar / (float)total:0.00%}**\n" +
                    $"5-star: **{fivestar}** // **{fivestar / (float)total:0.00%}**\n"
                    )
                .AddField("5★ items", string.IsNullOrEmpty(fivestarsString) ? "None yet!" : fivestarsString)
                .AddField("4★ items", string.IsNullOrEmpty(fourstarString) ? "None yet!" : fourstarString)
                .AddField("3★ items", string.IsNullOrEmpty(threestarString) ? "None yet!" : threestarString);

            await ReplyAsync(embed: embed.Build());

        }

        [Command("reset")]
        [Summary("Reset your inventory.")]
        [Ratelimit(5)]
        public async Task ResetHistory()
        {
            _sim.ResetProfile(Context.User);
            await ReplyAsync("Sucessfully reset.");
        }

        private string FormatWishItemCount(WishItemCount wic) => $"**{wic.WishItem.Name}** (x{wic.Count})";
    }
}
