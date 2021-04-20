﻿using Discord;
using Discord.Commands;
using GenshinLibrary.Commands;
using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Preconditions;
using GenshinLibrary.ReactionCallback;
using GenshinLibrary.Services.GachaSim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Group("sim")]
    [Name("GachaSimulator")]
    [Summary("Gacha simulator for you to play around with :)\nIf not used for 24 hours, your wish inventory resets.")]
    public class GachaSimulator : GLInteractiveBase
    {
        private readonly GachaSimulatorService _sim;

        private readonly string fileName = "result.png";

        public GachaSimulator(GachaSimulatorService sim)
        {
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

            WishItem[] result;
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

            var rarityColor = result.Max(x => x.Rarity) switch
            {
                3 => Color.Blue,
                4 => Color.Purple,
                5 => Color.Gold,
                _ => throw new NotImplementedException()
            };

            var embed = new EmbedBuilder()
                .WithTitle($"Wishing {(count == 1 ? "once" : $"{count} times")}... Pity: `{pityBefore}`")
                .WithColor(Color.DarkGrey);

            var message = await ReplyAsync(embed: embed.Build());
            await Task.Delay(2500);

            embed.WithColor(rarityColor)
                .WithTitle($"Wishing {(count == 1 ? "once" : $"{count} times")}... Pity: `{pityBefore} -> {pityAfter}`");

            await message.ModifyAsync(x => x.Embed = embed.Build());
            await Task.Delay(1500);

            var resultEmbed = new EmbedBuilder()
                .WithFooter($"You are wishing on {bannerName}")
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
            }
            else
            {
                var wishImage = new WishImage(result);

                var bitmap = wishImage.GetImage();
                Stream stream = new MemoryStream();
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                resultEmbed.WithImageUrl($"attachment://{fileName}");
                await Context.Channel.SendFileAsync(stream, fileName, embed: resultEmbed.Build());

                stream.Dispose();
                stream = null;
                bitmap.Dispose();
                bitmap = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }

        [Command("banner", RunMode = RunMode.Async)]
        [Summary("Select a banner to wish on.")]
        [Alias("wishon", "selectbanner", "select")]
        [Ratelimit(5)]
        public async Task SelectBanner(Banner banner)
        {
            WishBanner selectedBanner = null;
            List<WishBanner> selection = _sim.Banners.Values.Where(x => x.BannerType == banner).ToList();

            if (selection.Count > 1)
            {
                var selectionPaged = new EventWishesPaged(Interactive, Context, selection.Cast<EventWish>(), 3);
                await selectionPaged.DisplayAsync();

                int number = -1;
                if (await NextMessageWithConditionAsync(Context, x =>
                {
                    if (int.TryParse(x.Content, out var num))
                    {
                        number = num;
                        return true;
                    }
                    return false;
                }) != null)
                {
                    number--;
                    if (number < 0 || number > selection.Count)
                    {
                        await ReplyAsync("Number invalid.");
                        return;
                    }

                    selectedBanner = selection[number];
                }
            }
            else
                selectedBanner = selection[0];

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
