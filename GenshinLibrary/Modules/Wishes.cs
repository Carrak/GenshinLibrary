using Discord;
using Discord.Commands;
using GenshinLibrary.Attributes;
using GenshinLibrary.Commands;
using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Preconditions;
using GenshinLibrary.ReactionCallback;
using GenshinLibrary.Services.Wishes;
using GenshinLibrary.Services.Wishes.Filtering;
using GenshinLibrary.StringTable;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            "**Name** - filter by item name.\n\n" +
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
        [Summary("Add up to 6 wishes at once by copying them from the game.\n" +
            "If you're wondering where to find your wish history, navigate to the wish screen in-game and press 'History', then select the appropriate banner and follow the instructions from the gif below!")]
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
            [Summary("The name of the received item.")][Remainder] string name)
        {
            if (_wishes.WishItems.TryGetValue(name, out var wishItem))
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
            else
            {
                var suggestion = _wishes.GetBestSuggestion(name, banner);
                var message = await ReplyAsync($"No such item has been found. Did you mean `{suggestion.Name}`?");
                await message.AddReactionAsync(checkmark);

                if (await NextReactionAsync(checkmark, message, Context.User))
                    await AddWish(suggestion, banner, datetime);
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
        [Ratelimit(10)]
        public async Task Analytics() => await Anal(Context.User);

        [Command("analytics")]
        [Summary("View someone's wish analytics.")]
        [Ratelimit(10)]
        public async Task Anal(
            [Summary("The user whose analytics you want to see.")][Remainder] IUser user
            )
        {
            var data = await _wishes.GetAnalyticsAsync(user);

            if (data is null)
            {
                await ReplyAsync("No records exist for this user.");
                return;
            }

            var character = data.First(x => x.Banner == Banner.Character);
            var weapon = data.First(x => x.Banner == Banner.Weapon);
            var standard = data.First(x => x.Banner == Banner.Standard);
            var beginner = data.First(x => x.Banner == Banner.Beginner);

            int characterTotal = character.TotalWishes + standard.TotalWishes + beginner.TotalWishes;
            int characterThreestar = character.ThreeStarWishes + standard.ThreeStarWishes + beginner.ThreeStarWishes;
            int characterFourstar = character.FourStarWishes + standard.FourStarWishes + beginner.FourStarWishes;
            int characterFivestar = character.FiveStarWishes + standard.FiveStarWishes + beginner.FiveStarWishes;

            int total = weapon.TotalWishes + characterTotal;
            int totalThreestar = weapon.ThreeStarWishes + characterThreestar;
            int totalFourstar = weapon.FourStarWishes + characterFourstar;
            int totalFivestar = weapon.FiveStarWishes + characterFivestar;

            var embed = new EmbedBuilder();

            embed.WithTitle($"Stats for {user}")
                .WithColor(Globals.MainColor)
                .WithFooter("W - weapons, C - characters.\nBeginner banner is not displayed here (but included for the calculations).")
                .WithDescription(
                $"Total wishes: **{total}**\n" +
                $"Total 3-star: **{totalThreestar}**\n" +
                $"Total 4-star: **{totalFourstar}**\n" +
                $"Total 5-star: **{totalFivestar}**\n\n" +
                $"Odds across character banners\n" +
                $"4-star: {ZeroTernary(characterFourstar, GetPercentageString(characterFourstar, characterTotal))}\n" +
                $"5-star: {ZeroTernary(characterFivestar, GetPercentageString(characterFivestar, characterTotal))}\n\n" +
                $"Combined odds (character banners + weapon banner)\n" +
                $"4-star: {ZeroTernary(totalFourstar, GetPercentageString(totalFourstar, total))}\n" +
                $"5-star: {ZeroTernary(totalFivestar, GetPercentageString(totalFivestar, total))}\n")
                .AddField("Character", ZeroTernary(character.TotalWishes, character.GetGeneralInfo(true, false), "No data."), true)
                .AddField("Weapon", ZeroTernary(weapon.TotalWishes, weapon.GetGeneralInfo(true, false), "No data."), true)
                .AddField("Standard", ZeroTernary(standard.TotalWishes, standard.GetGeneralInfo(true, true), "No data."), true)
                .AddField("Character/Average", ZeroTernary(characterTotal, $"Across character banners:\n" +
                $"4-star odds: {ZeroTernary(characterFourstar, ComparedToAverage(characterFourstar / (float)characterTotal, 0.13f))}\n" +
                $"5-star odds: {ZeroTernary(characterFivestar, ComparedToAverage(characterFivestar / (float)characterTotal, 0.016f))}\n\n" +
                $"4-star average: **13%**\n" +
                $"5-star average: **1.6%**", "No data across character banners."), true)
                .AddField("Weapon/Average", ZeroTernary(weapon.TotalWishes,
                $"For the weapon banner:\n" +
                $"4-star odds: {ZeroTernary(weapon.FourStarWishes, ComparedToAverage(weapon.FourStarWishes / (float)weapon.TotalWishes, 0.145f))}\n" +
                $"5-star odds: {ZeroTernary(weapon.FiveStarWishes, ComparedToAverage(weapon.FiveStarWishes / (float)weapon.TotalWishes, 0.0185f))}\n\n" +
                $"4-star average: **14.5%**\n" +
                $"5-star average: **1.85%**", "No weapon banner data."), true);

            await ReplyAsync(embed: embed.Build());

            static string ComparedToAverage(float player, float average)
            {
                if (player > average)
                    return $"**{player / average:0.00}x** __higher__ than average";
                else if (average > player)
                    return $"**{average / player:0.00}x** __lower__ than average";
                else
                    return $"exactly the average";
            }

            static string GetPercentageString(float fraction, float total)
            {
                return $"**{fraction / total:0.00%}** (**1** in **{total / fraction:0.00}**)";
            }

            static string ZeroTernary(int number, string ifNotZero, string errorMessage = "None to compare")
            {
                return number == 0 ? errorMessage : ifNotZero;
            }
        }

        [Command("history")]
        [Summary("View your wish history.")]
        [Example("This command will display all the 4 stars and 5 stars that you got before 60 pity on the standard banner ordered by the pity values.\n`gl!history standard rarity:4,5 pity:<60 order:pity`")]
        [Ratelimit(10)]
        public async Task History
            (Banner banner,
            [Summary(FiltersSummary)] WishHistoryFilterValues filters = null
            ) => await History(banner, Context.User, filters);

        [Command("history")]
        [Summary("View someone's wish history.")]
        [Ratelimit(10)]
        public async Task History(
            Banner banner,
            [Summary("The user whose history you wish to see.")] IUser user,
            [Summary(FiltersSummary)] WishHistoryFilterValues filters = null)
        {
            QueryCondition condition = null;

            if (filters != null)
                try
                {
                    var parsedFilters = new WishHistoryFilters(filters);
                    _wishes.ValidateFilters(parsedFilters, banner);
                    condition = parsedFilters.GetCondition();
                }
                catch (ArgumentException e)
                {
                    var helpEmbed = new EmbedBuilder();

                    helpEmbed.WithTitle("Invalid filters.")
                        .WithDescription(e.Message)
                        .WithColor(Color.Red);

                    await ReplyAsync(embed: helpEmbed.Build());
                    return;
                }

            var records = await _wishes.GetRecordsAsync(user, banner, condition);

            if (records == null)
            {
                await ReplyAsync("No wishes have been found.");
                return;
            }

            var history = new WishHistoryPaged(Interactive, Context, 18, records, banner);
            await history.DisplayAsync();
        }

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

        private bool CheckWish(WishItem wishItem, Banner banner)
        {
            return wishItem.Banners.HasFlag(banner);
        }

        private TextTable GetTable(params WishItemRecord[] wishRecords)
        {
            var table = new TextTable("Rarity", "Name", "DateTime");
            foreach (var wir in wishRecords)
                table.AddRow($"{wir.WishItem.Rarity}*", wir.WishItem.GetFormattedName(32), wir.DateTime.ToString(@"dd.MM.yyyy HH:mm:ss"));

            return table;
        }
    }
}
