using Discord;
using Discord.Commands;
using GenshinLibrary.Analytics;
using GenshinLibrary.Attributes;
using GenshinLibrary.Commands;
using GenshinLibrary.Models;
using GenshinLibrary.Preconditions;
using GenshinLibrary.ReactionCallback;
using GenshinLibrary.Services.GachaSim;
using GenshinLibrary.Services.Wishes;
using GenshinLibrary.Services.Wishes.Filtering;
using GenshinLibrary.StringTable;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Summary("Manage your wishes and view wish history and detailed analytics.")]
    public class Wishes : GLInteractiveBase
    {
        private const string FiltersSummary =
            "**Rarity** - filter by rarity.\n" +
            "**Pity** - filter by pity.\n" +
            "**DateTime** - filter by date received.\n" +
            "**Name** - filter by item name.\n" +
            "**SeparatePity** - display according pity only on 4- and 5- drops. Specify this filter as `sp:true`\n\n" +
            "Filters are specified in the following format:\n" +
            "`[filter]:[operator][value]`\n" +
            "Where `[operator]` is either an equality (`=`, `!=`) or an inequality (`<`, `>`, `<=`, `>=`) operator.\nYou can specify several filter values of one type by separating them with a comma.\nOperator can be unspecified if it's `=`.\n\n" +
            "You can also specify the order by adding `order:pity` or `order:rarity`. By default wishes are ordered chronologically.";

        private static readonly Emoji checkmark = new Emoji("✅");
        private readonly WishService _wishes;

        public Wishes(WishService wishes)
        {
            _wishes = wishes;
        }

        [Command("addwishbulk")]
        [Summary("Add up to 6 wishes at once by copying them from the game.")]
        [Alias("awb")]
        [GifExample("https://cdn.discordapp.com/attachments/461538521551863825/836371667001409586/awb_example.gif")]
        [Ratelimit(5)]
        public async Task AddWishBulk(
            Banner banner,
            [Summary("The data copied from the game's history.\nWhen importing, wishes must be added in chronological order, therefore, " +
            "__**copy each page starting from the last on each banner and don't modify the copied data.**__. " +
            "The bot will handle the rest for you (including reversing the copied data). For mobile-only players, sadly, there's no viable solution for now.")] [Remainder] string data
            )
        {
            // dumb check for dumb people xd
            if (data.Contains("@everyone") || data.Contains("@here"))
            {
                await ReplyAsync("That's so funny 👏 🤏 😂");
                return;
            }

            List<WishItemRecord> records = new List<WishItemRecord>();
            string errorMessage = "";
            string[] splitData = data.Split('\n');

            for (int i = 0; i < splitData.Length - 2; i += 3)
            {
                var name = splitData[i + 1];
                var dateTimeString = splitData[i + 2];

                if (!_wishes.WishItems.TryGetValue(name, out var wi))
                {
                    errorMessage += $"`{name}` is not a character/weapon. Skipped.\n";
                    continue;
                }

                if (!DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out var dateTime))
                {
                    errorMessage += $"`{dateTimeString}` is not a DateTime. Skipped.\n";
                    continue;
                }

                if (!CheckWish(wi, banner))
                {
                    await ReplyAsync($"`{wi.Name}` does not drop from the `{banner}` banner. Skipping everything.");
                    return;
                }

                var wir = new WishItemRecord(dateTime, wi);
                records.Add(wir);
            }

            if (records.Count > 6)
            {
                await ReplyAsync("Can only import one page (6 wishes) at a time.");
                return;
            }

            var embed = new EmbedBuilder();

            embed.WithColor(Globals.MainColor)
                .WithTitle($"{records.Count} wishes recorded!")
                .WithDescription(GetTable(records.ToArray()).GetTable());

            records.Reverse();

            try
            {
                await _wishes.AddWishesAsync(Context.User, banner, records);
            }
            catch (PostgresException pe)
            {
                await ReplyAsync(pe.MessageText);
                return;
            }

            await ReplyAsync(errorMessage, embed: embed.Build());
        }

        [Command("addwish", RunMode = RunMode.Async)]
        [Summary("Add a single wish to a certain banner.")]
        [Ratelimit(5)]
        public async Task AddWish(
            Banner banner,
            [Summary("Date of the wish.")] DateTime datetime,
            [Summary("The name of the received item.")][Remainder] WishItem wishItem)
        {
            try
            {
                await AddWish(wishItem, banner, datetime);
            }
            catch (PostgresException pe)
            {
                await ReplyAsync(pe.MessageText);
                return;
            }

        }

        [Command("removerecent", RunMode = RunMode.Async)]
        [Alias("rr")]
        [Summary("Remove recently added wishes on a certain banner.")]
        [Ratelimit(5)]
        public async Task RemoveRecent(
            Banner banner,
            [Summary("Amount to remove. Can only delete up to 12 wishes at once.")] int count)
        {
            if (count <= 0)
            {
                await ReplyAsync("Cannot delete 0 or less records.");
                return;
            }
            else if (count > 12)
            {
                await ReplyAsync("Cannot remove more than 12 records at once.");
                return;
            }

            var records = await _wishes.GetRecentRecordsAsync(Context.User, banner, count);

            if (records == null)
            {
                await ReplyAsync("No records to be removed.");
                return;
            }

            var table = GetTable(records.ToArray());

            var embed = new EmbedBuilder();
            embed.WithColor(Globals.MainColor)
                .WithTitle($"Wishes to be removed")
                .WithDescription(table.GetTable());

            var msg = await ReplyAsync($"Are you sure you want to remove these wishes from the `{banner}` banner?", embed: embed.Build());
            await msg.AddReactionAsync(checkmark);

            if (await NextReactionAsync(checkmark, msg, Context.User))
            {
                await _wishes.RemoveWishesAsync(records);
                await ReplyAsync("Successfully removed.");
            }
        }

        [Command("analytics")]
        [Summary("View your wish analytics.")]
        [Ratelimit(7)]
        public async Task Analytics() => await Anal(Context.User);

        [Command("analytics")]
        [Summary("View someone's wish analytics.")]
        [Ratelimit(7)]
        public async Task Anal(
            [Summary("The user whose analytics you want to see.")][Remainder] IUser user
            )
        {

            Dictionary<Banner, BannerStats> data;
            try
            {
                data = await _wishes.GetAnalyticsAsync(user);
            }
            catch (PostgresException pe)
            {
                await ReplyAsync($"{pe.MessageText}");
                return;
            }

            if (data is null)
            {
                await ReplyAsync("No records exist for this user.");
                return;
            }

            var beginner = data[Banner.Beginner];
            var standard = data[Banner.Standard];
            var character = data[Banner.Character] as EventBannerStats;
            var weapon = data[Banner.Weapon] as EventBannerStats;

            int characterTotal = character.TotalWishes + standard.TotalWishes + beginner.TotalWishes;
            int characterThreestar = character.ThreeStarWishes + standard.ThreeStarWishes + beginner.ThreeStarWishes;
            int characterFourstar = character.FourStarWishes + standard.FourStarWishes + beginner.FourStarWishes;
            int characterFivestar = character.FiveStarWishes + standard.FiveStarWishes + beginner.FiveStarWishes;

            int total = weapon.TotalWishes + characterTotal;
            int totalThreestar = weapon.ThreeStarWishes + characterThreestar;
            int totalFourstar = weapon.FourStarWishes + characterFourstar;
            int totalFivestar = weapon.FiveStarWishes + characterFivestar;

            var embed = new EmbedBuilder();
            string errorMessage = "None to compare";

            embed.WithTitle($"Stats for {user}")
                .WithColor(Globals.MainColor)
                .WithFooter("W - weapons, C - characters.\nBeginner banner is not displayed here (but included for the calculations).")
                .WithDescription(
                $"Total wishes: **{total}**\n" +
                $"Total 3★: **{totalThreestar}**\n" +
                $"Total 4★: **{totalFourstar}**\n" +
                $"Total 5★: **{totalFivestar}**\n\n" +
                $"Odds across character banners\n" +
                $"4★: {(characterFourstar == 0 ? errorMessage : GetPercentageString(characterFourstar, characterTotal))}\n" +
                $"5★: {(characterFivestar == 0 ? errorMessage : GetPercentageString(characterFivestar, characterTotal))}\n\n" +
                $"Combined odds (character banners + weapon banner)\n" +
                $"4★: {(totalFourstar == 0 ? errorMessage : GetPercentageString(totalFourstar, total))}\n" +
                $"5★: {(totalFivestar == 0 ? errorMessage : GetPercentageString(totalFivestar, total))}\n")
                .AddField("Character", character.TotalWishes == 0 ? errorMessage : character.GetGeneralInfo(true, false), true)
                .AddField("Weapon", weapon.TotalWishes == 0 ? errorMessage : weapon.GetGeneralInfo(true, false), true)
                .AddField("Standard", standard.TotalWishes == 0 ? errorMessage : standard.GetGeneralInfo(true, true), true)
                .AddField("Character/Extras", characterTotal == 0 ? errorMessage : $"Odds across **all** character banners:\n" +
                $"4★: {(characterFourstar == 0 ? errorMessage : ComparedToAverage(characterFourstar / (float)characterTotal, 0.13f))}\n" +
                $"5★: {(characterFivestar == 0 ? errorMessage : ComparedToAverage(characterFivestar / (float)characterTotal, 0.016f))}\n\n" +
                $"{character.RateUpStats()}", true)
                .AddField("Weapon/Extras", weapon.TotalWishes == 0 ? errorMessage :
                $"Odds for the weapon banner:\n" +
                $"4★: {(weapon.FourStarWishes == 0 ? errorMessage : ComparedToAverage(weapon.FourStarWishes / (float)weapon.TotalWishes, 0.145f))}\n" +
                $"5★: {(weapon.FiveStarWishes == 0 ? errorMessage : ComparedToAverage(weapon.FiveStarWishes / (float)weapon.TotalWishes, 0.0185f))}\n\n" +
                $"{weapon.RateUpStats()}", true);

            await ReplyAsync(embed: embed.Build());

            static string ComparedToAverage(float player, float average)
            {
                if (player > average)
                    return $"**{player / average:0.00}x** __higher__ than average (**{average:0.00%}**)";
                else if (average > player)
                    return $"**{average / player:0.00}x** __lower__ than average (**{average:0.00%}**)";
                else
                    return $"exactly the average";
            }

            static string GetPercentageString(float fraction, float total)
            {
                return $"**{fraction / total:0.00%}** (**1** in **{total / fraction:0.00}**)";
            }
        }

        [Command("history")]
        [Alias("h")]
        [Summary("View your wish history.")]
        [Example("This command will display all the 4 stars and 5 stars that you got before 60 pity on the standard banner ordered by the pity values.\n`gl!history standard rarity:4,5 pity:<60 order:pity`")]
        [Ratelimit(7)]
        public async Task History
            (Banner banner,
            [Summary(FiltersSummary)] WishHistoryFilterValues filters = null
            ) => await History(banner, Context.User, filters);

        [Command("history")]
        [Alias("h")]
        [Summary("View someone's wish history.")]
        [Example("This command will display all the 4 stars and 5 stars that @user got before 60 pity on the standard banner ordered by the pity values.\n`gl!history standard @user rarity:4,5 pity:<60 order:pity`")]
        [Ratelimit(7)]
        public async Task History(
            Banner banner,
            [Summary("The user whose history you wish to see.")] IUser user,
            [Summary(FiltersSummary)] WishHistoryFilterValues filters = null)
        {
            WishHistoryFilters parsedFilters = null;
            if (filters != null)
            {
                try
                {
                    parsedFilters = new WishHistoryFilters(filters);
                }
                catch (ArgumentException e)
                {
                    await ReplyAsync(embed: GetInvalidFiltersEmbed(e.Message));
                    return;
                }

                var result = _wishes.ValidateFilters(parsedFilters, banner);
                if (!result.IsSuccess)
                {
                    await ReplyAsync(embed: GetInvalidFiltersEmbed(result.ErrorMessage));
                    return;
                }
            }

            var records = await _wishes.GetRecordsAsync(user, banner, parsedFilters);
            if (records == null)
            {
                await ReplyAsync("No wishes have been found.");
                return;
            }

            var history = new WishHistoryPaged(Interactive, Context, 18, records, $"{banner} banner");
            await history.DisplayAsync();
        }

        [Command("bannerhistory", RunMode = RunMode.Async)]
        [Alias("bhistory", "bh")]
        [Summary("View your wishes on a certain character/weapon banner.")]
        [Example("`gl!banner xiao rarity:4 pity:<60`\nFor more information regarding filters, please refer to `gl!help history`")]
        [Ratelimit(7)]
        public async Task BannerHistory(
            [Remainder, Summary("The name of the rate-up item. E.g. `zhongli`, `xiao`, etc. For names that contain spaces, use quotes: `\"staff of homa\"`")] WishItem wishItem
            ) => await BannerHistory(wishItem, Context.User, null);

        [Command("bannerhistory", RunMode = RunMode.Async)]
        [Alias("bhistory", "bh")]
        [Summary("View your wishes on a certain character/weapon banner.")]
        [Example("`gl!banner xiao rarity:4 pity:<60`\nFor more information regarding filters, please refer to `gl!help history`")]
        [Ratelimit(7)]
        public async Task BannerHistory(
            [Summary("The name of the rate-up item. E.g. `zhongli`, `xiao`, etc. For names that contain spaces, use quotes: `\"staff of homa\"`")] WishItem wishItem,
            [Summary(FiltersSummary)] WishHistoryFilterValues filters = null
            ) => await BannerHistory(wishItem, Context.User, filters);

        [Command("bannerhistory", RunMode = RunMode.Async)]
        [Alias("bhistory", "bh")]
        [Summary("View someone's wishes wishes on a certain character/weapon banner.")]
        [Example("`gl!banner xiao @user rarity:4 pity:<60`\nFor more information regarding filters, please refer to `gl!help history`")]
        [Ratelimit(7)]
        public async Task BannerHistory(
            [Summary("The name of the rate-up item. E.g. `zhongli`, `xiao`, etc. For names that contain spaces, use quotes: `\"staff of homa\"`")] WishItem wishItem,
            [Summary("The user whose history you wish to see.")] IUser user,
            [Summary(FiltersSummary)] WishHistoryFilterValues filters = null
            )
        {
            if (wishItem.Rarity != 5)
            {
                await ReplyAsync("Please search using 5-star characters/weapon names.");
                return;
            }

            var selection = _wishes.Banners.Values.Where(x => x is EventWish ew && ew.RateUpFivestars.Contains(wishItem));
            if (!selection.Any())
            {
                await ReplyAsync("This item is not rate-up on any event wishes.");
                return;
            }

            WishBanner selectedBanner = await BannerSelectionAsync(selection);
            if (selectedBanner == null)
                return;

            WishHistoryFilters parsedFilters = null;
            if (filters != null)
            {
                try
                {
                    parsedFilters = new WishHistoryFilters(filters);
                }
                catch (ArgumentException e)
                {
                    await ReplyAsync(embed: GetInvalidFiltersEmbed(e.Message));
                    return;
                }

                var result = _wishes.ValidateFilters(parsedFilters, selectedBanner.BannerType);
                if (!result.IsSuccess)
                {
                    await ReplyAsync(embed: GetInvalidFiltersEmbed(result.ErrorMessage));
                    return;
                }
            }

            IEnumerable<CompleteWishItemRecord> records = null;
            try
            {
                records = await _wishes.GetBannerWishesAsync(user, selectedBanner as EventWish, parsedFilters);
            }
            catch (PostgresException pe)
            {
                await ReplyAsync($"{pe.MessageText}");
                return;
            }

            if (records == null)
            {
                await ReplyAsync("No wishes have been found.");
                return;
            }

            var history = new WishHistoryPaged(Interactive, Context, 18, records, selectedBanner.GetFullName());
            await history.DisplayAsync();
        }

        [Command("setserver", RunMode = RunMode.Async)]
        [Summary("Set your in-game server to get access to wishes by banners and more analytics.\nThis is necessary to separate wishes, so to adjust the timezone correctly and match them by banners." +
            "That, on the other hand, allows to tell what is rate-up and what isn't, hence providing more data to work with.")]
        [Ratelimit(7)]
        public async Task SetServer()
        {
            var servers = _wishes.Servers.Values.ToArray();

            var embed = new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithTitle("Available servers")
                .WithFooter($"Reply with just the index of the selected server.")
                .WithDescription(string.Join('\n', servers.Select((x, index) => $"**{index + 1}. {x.ServerName} ({x.ServerTimezone})**")));

            await ReplyAsync(embed: embed.Build());

            int index = -1;
            if (await NextMessageWithConditionAsync(Context, x =>
            {
                if (int.TryParse(x.Content, out var num))
                {
                    index = num;
                    return true;
                }
                return false;
            }) != null)
            {
                index--;
                if (index < 0 || index > servers.Length)
                {
                    await ReplyAsync("Number invalid.");
                    return;
                }

                await _wishes.SetServerAsync(Context.User, servers[index].ServerID);
                await ReplyAsync($"Successfully changed your server to **{servers[index].ServerName}**");
            }
        }

        [Command("summary")]
        [Alias("s")]
        [Ratelimit(3)]
        [Summary("Provides a user's wish summary of a certain character or weapon.")]
        public async Task Summary(
            [Summary("User to look up.")] IUser user,
            [Summary("Character or weapon.")][Remainder] WishItem wishItem
            )
        {
            var summary = await _wishes.GetSummaryAsync(user, wishItem);

            if (summary is null)
            {
                await ReplyAsync("The user has not obtained this character/weapon yet!");
                return;
            }

            Color color;
            string imagePath;

            if (wishItem is Character c)
            {
                color = GenshinColors.GetElementColor(c.Vision);
                imagePath = c.AvatarImagePath;
            }
            else if (wishItem is Weapon w)
            {
                color = GenshinColors.GetRarityColor(w.Rarity);
                imagePath = w.WishArtPath;
            }
            else
                throw new Exception("Invalid type.");

            string fileName = "avatar.png";
            using var bitmap = new System.Drawing.Bitmap(imagePath);
            using var image = new MemoryStream();
            bitmap.Save(image, System.Drawing.Imaging.ImageFormat.Png);
            image.Position = 0;

            var embed = new EmbedBuilder()
                .WithTitle(wishItem.Name)
                .WithColor(color)
                .WithThumbnailUrl($"attachment://{fileName}")
                .WithDescription($"Total count: **{summary.Count}**")
                .AddField("Banners", string.Join('\n', summary.GroupedCounts.OrderByDescending(x => x.Count).Select(x => $"{x.Banner}: **{x.Count}**")));

            await Context.Channel.SendFileAsync(image, fileName, embed: embed.Build());
        }

        [Command("summary")]
        [Alias("s")]
        [Ratelimit(3)]
        [Summary("Provides your wish summary of a certain character or weapon.")]
        public async Task Summary(
            [Summary("Character or weapon.")][Remainder] WishItem wishItem
            ) => await Summary(Context.User, wishItem);

        private async Task AddWish(WishItem wi, Banner banner, DateTime datetime)
        {
            if (!CheckWish(wi, banner))
            {
                await ReplyAsync($"`{wi.Name}` does not drop from the `{banner}` banner!");
                return;
            }

            await _wishes.AddWishAsync(Context.User, wi, banner, datetime);

            var embed = new EmbedBuilder();
            var table = GetTable(new WishItemRecord(datetime, wi));

            embed.WithColor(Globals.MainColor)
                .WithTitle("Wish recorded!")
                .WithDescription(table.GetTable());

            await ReplyAsync(embed: embed.Build());
        }

        private bool CheckWish(WishItem wishItem, Banner banner) => wishItem.Banners.HasFlag(banner);

        private TextTable GetTable(params WishItemRecord[] wishRecords)
        {
            var table = new TextTable("Type", "Name", "DateTime");
            foreach (var wir in wishRecords)
                table.AddRow(wir.WishItem.GetType().Name, wir.WishItem.GetFormattedName(29), wir.DateTime.ToString(@"dd.MM.yyyy HH:mm:ss"));

            return table;
        }

        private Embed GetInvalidFiltersEmbed(string message)
        {
            return new EmbedBuilder()
                .WithTitle("Invalid filters.")
                .WithDescription(message)
                .WithColor(Color.Red)
                .Build();
        }
    }
}
