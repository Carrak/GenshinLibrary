using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GenshinLibrary.Analytics;
using GenshinLibrary.AutocompleteHandlers;
using GenshinLibrary.Models;
using GenshinLibrary.Pagers;
using GenshinLibrary.Preconditions;
using GenshinLibrary.Services.GachaSim;
using GenshinLibrary.Services.Menus;
using GenshinLibrary.Services.Wishes;
using GenshinLibrary.Services.Wishes.Filtering;
using GenshinLibrary.Services.Wishes.Images;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace GenshinLibrary.Modules
{
    [Group("wishes", "Manage your wishes")]
    public class Wishes : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly WishService _wishes;
        private readonly MenuService _menus;

        public Wishes(WishService wishes, MenuService menus)
        {
            _wishes = wishes;
            _menus = menus;
        }

        [SlashCommand("addwishbulk", "Import your wishes from the game by copying your history")]
        public async Task AddWishBulk(
            string data
            )
        {
            List<WishItemRecord> records = new List<WishItemRecord>();
            string errorMessage = "";

            string[] splitData = data.Split('\n');
            for (int i = 0; i < splitData.Length - 3; i += 4)
            {
                var name = splitData[i + 1];
                var bannerRaw = splitData[i + 2];
                var dateTimeString = splitData[i + 3];

                if (!_wishes.WishItemsByName.TryGetValue(name, out var wi))
                {
                    await ErrorMessage($"`{name}` is not a character/weapon. Skipped.");
                    return;
                }

                if (!TryParseBannerRaw(bannerRaw, out Banner banner))
                {
                    await ErrorMessage($"`{bannerRaw}` is not a banner. Skipped.");
                    return;
                }

                if (!DateTime.TryParseExact(dateTimeString, "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out var dateTime))
                {
                    await ErrorMessage($"`{dateTimeString}` is not a DateTime. Skipped.");
                    return;
                }

                if (!wi.Banners.HasFlag(banner))
                {
                    await ErrorMessage($"`{wi.Name}` does not drop from the `{banner}` banner. Skipping everything.");
                    return;
                }

                var wir = new WishItemRecord(dateTime, wi, banner);
                records.Add(wir);
            }

            if (records.Count > 6)
            {
                await ReplyAsync("Can only import one page (6 wishes) at a time.");
                return;
            }

            var embed = new EmbedBuilder();

            using var wishImage = WishImage.GetRecordsWishImage(records).GetStream();
            embed.WithColor(Globals.MainColor)
                .WithImageUrl("attachment://image.png")
                .WithTitle($"{records.Count} wishes recorded!");

            records.Reverse();

            try
            {
                await _wishes.AddWishesAsync(Context.User, records);
            }
            catch (PostgresException pe)
            {
                await ReplyAsync(pe.MessageText);
                return;
            }

            await RespondWithFileAsync(wishImage, "image.png", errorMessage, embed: embed.Build());

            static bool TryParseBannerRaw(string input, out Banner output)
            {
                switch (input)
                {
                    case "Character Event Wish":
                        output = Banner.Character1;
                        break;
                    case "Character Event Wish-2":
                        output = Banner.Character2;
                        break;
                    case "Weapon Event Wish":
                        output = Banner.Weapon;
                        break;
                    case "Permanent Wish":
                        output = Banner.Standard;
                        break;
                    case "Novice Wishes":
                        output = Banner.Beginner;
                        break;
                    default:
                        output = 0;
                        return false;
                }
                return true;
            }

            async Task ErrorMessage(string message)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Discord.Color.Red)
                    .WithTitle("Invalid input")
                    .WithDescription(message);

                await RespondAsync(embed: embed.Build(), ephemeral: true);
            }
        }

        [SlashCommand("removerecent", "Remove recently added wishes from wish history", runMode: RunMode.Async)]
        public async Task RemoveRecent(
            Banner banner,
            [Summary(description: "Amount to remove. Can only delete up to 6 wishes at once."), MinValue(1), MaxValue(6)] int count)
        {
            var records = await _wishes.GetRecentRecordsAsync(Context.User, banner, count);

            if (records == null)
            {
                await ReplyAsync("No records to be removed.");
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithTitle($"Wishes to be removed")
                .WithImageUrl("attachment://image.png");

            int hash = records.GetHashCode();
            using var wishImage = WishImage.GetRecordsWishImage(records);
            var component = new ComponentBuilder()
                .WithButton("Confirm", $"confirmation:{Context.User.Id},{hash}", ButtonStyle.Primary);

            await RespondWithFileAsync(wishImage.GetStream(), "image.png", "Are you sure want to remove these wishes?", embed: embed.Build(), components: component.Build());

            var msg = await GetOriginalResponseAsync() as SocketUserMessage;
            ButtonBuilder removedButton = new ButtonBuilder().WithCustomId("placeholder").WithDisabled(true).WithStyle(ButtonStyle.Success);
            ButtonBuilder timedOutbutton = new ButtonBuilder().WithCustomId("placeholder").WithDisabled(true).WithLabel("Timed out").WithStyle(ButtonStyle.Secondary);
            ButtonBuilder alreadyRemovedButton = new ButtonBuilder().WithCustomId("placeholder").WithDisabled(true).WithLabel("Wishes already removed").WithStyle(ButtonStyle.Danger);

            var trigger = new TaskCompletionSource<object>();
            Context.Client.ButtonExecuted += ButtonPressed;
            var task = await Task.WhenAny(trigger.Task, Task.Delay(15000));
            if (task != trigger.Task)
            {
                var timedOutComponent = new ComponentBuilder().WithButton(timedOutbutton);
                await ModifyOriginalResponseAsync(x => x.Components = timedOutComponent.Build());
            }

            async Task ButtonPressed(SocketMessageComponent comp)
            {
                var splitId = comp.Data.CustomId.Split(':');
                if (splitId.Length != 2)
                    return;

                var splitParams = splitId[1].Split(',');

                if (splitParams.Length == 2 && splitId[0] == "confirmation" && ulong.Parse(splitParams[0]) == comp.User.Id && int.Parse(splitParams[1]) == hash)
                {
                    var rowsRemoved = await _wishes.RemoveWishesAsync(records);
                    trigger.SetResult(null);

                    var component = new ComponentBuilder().WithButton(rowsRemoved == 0 ? alreadyRemovedButton : removedButton.WithLabel($"Removed {rowsRemoved} wishes!"));
                    await comp.UpdateAsync(x => x.Components = component.Build());
                }
            };
        }

        [SlashCommand("history", "View your or someone's wishes")]
        public async Task WishHistoryUser(
            Banner banner,
            [Summary(description: "Filter wishes by time. Example: \">01.03.2022,<01.04.2022\" will display wishes from March to April")] string timeReceivedFilter = null,
            [Summary(description: "Filter wishes by pity. Example: \">50,<=60\" will display wishes with pity ranging from 51 to 60")] string pityFilter = null,
            [Summary(description: "Filter wishes by name. Example: \"Rosaria,\" will display wishes containing Rosaria")] string nameFilter = null,
            [Summary(description: "Filter wishes by rarity. Example: \"5\" will display 5-stars. Equality sign can be omitted")] string rarityFilter = null,
            [Summary(description: "Whose wishes to display. Leave empty to see your own")] IUser user = null)
        {
            await DeferAsync();

            user ??= Context.User;

            var filtersResult = WishHistoryFilters.Parse(rarityFilter, timeReceivedFilter, nameFilter, pityFilter);
            if (!filtersResult.IsSuccess)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Invalid filters.")
                    .WithDescription(filtersResult.ErrorMessage)
                    .WithColor(Discord.Color.Red)
                    .Build();

                await FollowupAsync(embed: embed);
                return;
            }

            var records = await _wishes.GetRecordsAsync(user, banner, filtersResult.Value);

            if (!records.Any())
            {
                await RespondAsync("No wishes have been found matching the conditions.");
                return;
            }

            var pager = new WishHistoryPager(records, $"{banner} banner");
            var policy = new MemoryCacheEntryOptions();
            policy.SlidingExpiration = TimeSpan.FromMinutes(5);
            policy.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration() { EvictionCallback = EvictedFromCacheCallback });

            var menuId = _menus.CreateMenu(Context.User.Id, pager, policy);

            using var image = new MemoryStream();
            pager.GetPageImage().CopyTo(image);

            await FollowupWithFileAsync(image, "image.png",
                embed: GetPagerEmbed(pager, user.ToString()),
                components: GetComponent(Context.User.Id, menuId, 0, user.Id));
        }

        public class BannerHistorySelectionPager : BannerSelectionPager
        {
            public IUser User { get; }
            public WishHistoryFilters Filters { get; }

            public BannerHistorySelectionPager(IEnumerable<EventWish> banners, string selectionCustomId, int menuId, IUser user, WishHistoryFilters filters) : base(banners, selectionCustomId, menuId, Array.Empty<ActionRowBuilder>())
            {
                User = user;
                Filters = filters;
            }
        }

        [SlashCommand("bannerhistory", "View your or someone's wishes that were made during a specific banner")]
        public async Task BannerHistory(
            [Summary(description: "Name of a 5* weapon/character to search the banner"), Autocomplete(typeof(WishItemAutocomplete))] WishItem wishItem,
            [Summary(description: "Filter wishes by time. Example: \">01.03.2022,<01.04.2022\" will display wishes from March to April")] string timeReceivedFilter = null,
            [Summary(description: "Filter wishes by pity. Example: \">50,<=60\" will display wishes with pity ranging from 51 to 60")] string pityFilter = null,
            [Summary(description: "Filter wishes by name. Example: \"Rosaria,\" will display wishes containing Rosaria")] string nameFilter = null,
            [Summary(description: "Filter wishes by rarity. Example: \"5\" will display 5-stars. Equality sign can be omitted")] string rarityFilter = null,
            [Summary(description: "Whose wishes to display. Leave empty to see your own")] IUser user = null)
        {
            user ??= Context.User;

            var filtersResult = WishHistoryFilters.Parse(rarityFilter, timeReceivedFilter, nameFilter, pityFilter);
            if (!filtersResult.IsSuccess)
            {
                var embed = new EmbedBuilder()
                    .WithTitle("Invalid filters.")
                    .WithDescription(filtersResult.ErrorMessage)
                    .WithColor(Discord.Color.Red)
                    .Build();

                await RespondAsync(embed: embed, ephemeral: true);
                return;
            }

            if (wishItem.Rarity != 5)
            {
                await RespondAsync("Please search using 5-star characters/weapon names.", ephemeral: true);
                return;
            }

            var selection = _wishes.BannersByBID.Values.Where(x => x is EventWish ew && ew.RateUpFivestars.Contains(wishItem)).Cast<EventWish>();
            if (!selection.Any())
            {
                await RespondAsync("This item is not rate-up on any event wishes.");
                return;
            }

            int menuId = _menus.CreateMenu(Context.User.Id);
            var customPager = new BannerHistorySelectionPager(selection, "bh_selected", menuId, user, filtersResult.Value);
            _menus.SetMenuContent(Context.User.Id, customPager);
            await customPager.UpdateAsync(Context);
        }

        [SlashCommand("analytics", "View your wish analytics")]
        public async Task Anal(
            [Summary(description: "Whose analytics to display. Leave empty to see your own")] IUser user = null
            )
        {
            user ??= Context.User;

            var result = await _wishes.GetAnalyticsAsync(user);
            if (!result.IsSuccess)
            {
                await RespondAsync(result.ErrorMessage, ephemeral: true);
                return;
            }

            var data = result.Value;
            if (data is null)
            {
                await RespondAsync("No records exist for this user.", ephemeral: true);
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

            await RespondAsync(embed: embed.Build());

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

        [SlashCommand("summary", "Provides a user's wish summary of a certain character or weapon")]
        public async Task Summary(
            [Summary(description: "Name of the desired character/weapon"), Autocomplete(typeof(WishItemAutocomplete))] WishItem wishItem,
            [Summary(description: "Whose summary to display. Leave empty to see your own")] IUser user = null)
        {
            user ??= Context.User;

            var summary = await _wishes.GetSummaryAsync(user, wishItem);

            if (summary is null)
            {
                await RespondAsync("The user has not obtained this character/weapon yet!");
                return;
            }

            Discord.Color color;
            string imagePath;

            switch (wishItem)
            {
                case Character character:
                    color = GenshinColors.GetElementColor(character.Vision);
                    imagePath = character.AvatarImagePath;
                    break;
                case Weapon weapon:
                    color = GenshinColors.GetRarityColor(weapon.Rarity);
                    imagePath = weapon.WeaponIconPath;
                    break;
                default:
                    throw new Exception("Invalid type.");
            }

            string fileName = "avatar.png";
            using var image = SixLabors.ImageSharp.Image.Load(imagePath);
            MemoryStream stream = new();
            image.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);

            var embed = new EmbedBuilder()
                .WithTitle(wishItem.Name)
                .WithColor(color)
                .WithThumbnailUrl($"attachment://{fileName}")
                .WithDescription($"Total count: **{summary.Count}**")
                .AddField("Banners", string.Join('\n', summary.GroupedCounts.OrderByDescending(x => x.Count).Select(x => $"{x.Banner}: **{x.Count}**")));

            await RespondWithFileAsync(stream, fileName, embed: embed.Build());
        }

        [SlashCommand("server", "Select your in-game server to get access to analytics and more options")]
        public async Task Server()
        {
            var servers = _wishes.ServersBySID.Values.ToArray();

            var embed = new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithTitle("Select a server with the buttons below")
                .WithDescription("Selecting a server gives access to `/wishes bannerhistory` and `/wishes analytics`, alongside a few additions in `/profile`.");

            var mc = new ComponentBuilder();
            for (int i = 0; i < servers.Length; i++)
                mc.WithButton($"{servers[i].ServerName} ({servers[i].ServerTimezone})", $"server_selected:{servers[i].ServerID}", ButtonStyle.Success, row: i / 2);

            await RespondAsync(embed: embed.Build(), components: mc.Build(), ephemeral: true);
        }

        [ComponentInteraction("server_selected:*", true)]
        public async Task ServerSelected(int serverId)
        {
            await _wishes.SetServerAsync(Context.User, serverId);
            var server = _wishes.ServersBySID[serverId];
            await RespondAsync($"Changed your server to **{server.ServerName}**\nServer timezone: **{server.ServerTimezone}**", ephemeral: true);
        }

        [ComponentInteraction("wish_history_page:*,*,*,*", true)]
        [VerifyUserAndMenu]
        public async Task WishHistoryPage(ulong userId, int menuId, PagerDirection pd, IUser viewedUser)
        {
            var pager = _menus.GetMenuContent<WishHistoryPager>(Context.User.Id);
            pager.FlipPage(pd);

            using var image = new MemoryStream();
            pager.GetPageImage().CopyTo(image);

            var component = Context.Interaction as SocketMessageComponent;
            var attachment = new FileAttachment(image, "image.png");

            await component.UpdateAsync(x =>
            {
                //x.Attachments = new[] { attachment };
                x.Embed = GetPagerEmbed(pager, viewedUser.ToString());
                x.Components = GetComponent(userId, menuId, pager.Page, viewedUser.Id);
            });

            await component.Message.ModifyAsync(x => x.Attachments = new[] { attachment });
        }

        [ComponentInteraction("bh_selected:*,*,*", true)]
        [VerifyUserAndMenu]
        public async Task BannerHistorySelected(ulong userId, int menuId, int bid)
        {
            var component = Context.Interaction as SocketMessageComponent;

            var data = _menus.GetMenuContent<BannerHistorySelectionPager>(userId);
            var wish = _wishes.BannersByBID[bid];
            var records = await _wishes.GetBannerWishesAsync(data.User, bid, data.Filters);
            var pager = new WishHistoryPager(records, $"{wish.GetFullName()}");

            using var image = new MemoryStream();
            pager.GetPageImage().CopyTo(image);
            
            await component.UpdateAsync(x => 
            {
                x.Embed = GetPagerEmbed(pager, data.User.ToString());
                x.Components = GetComponent(Context.User.Id, menuId, 0, data.User.Id);
            });

            await component.Message.ModifyAsync(x => x.Attachments = new[] { new FileAttachment(image, "image.png") });

            _menus.SetMenuContent(userId, pager);
        }

        private static void EvictedFromCacheCallback(object key, object value, EvictionReason reason, object state)
        {
            (value as WishHistoryPager).Dispose();
        }

        private static Embed GetPagerEmbed(WishHistoryPager pager, string userName) =>
            new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithImageUrl("attachment://image.png")
                .WithTitle($"Wish history for {pager.BannerName}")
                .WithFooter($"Page {pager.Page + 1} / {pager.TotalPages} | Viewing {userName}'s wishes")
                .Build();

        private static MessageComponent GetComponent(ulong userId, int menuId, int currentPage, ulong viewedUserId) =>
            new ComponentBuilder()
                .WithButton("<", $"wish_history_page:{userId},{menuId},0,{viewedUserId}", ButtonStyle.Primary)
                .WithButton($"{currentPage + 1}", $"placeholder", ButtonStyle.Secondary, disabled: true)
                .WithButton(">", $"wish_history_page:{userId},{menuId},1,{viewedUserId}", ButtonStyle.Primary)
                .Build();
    }
}