using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GenshinLibrary.Models;
using GenshinLibrary.Pagers;
using GenshinLibrary.Preconditions;
using GenshinLibrary.Services.GachaSim;
using GenshinLibrary.Services.Menus;
using GenshinLibrary.Services.Wishes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    public class GachaSimulator : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly GachaSimulatorService _sim;
        private readonly WishService _wishes;
        private readonly MenuService _menus;

        private const string FILE_NAME = "result.png";

        public GachaSimulator(GachaSimulatorService sim, WishService wishes, MenuService menus)
        {
            _sim = sim;
            _menus = menus;
            _wishes = wishes;
        }

        [SlashCommand("gachasimulator", "Open the menu of the gacha simulator")]
        public async Task GachaSimulatorMenu()
        {
            var menuId = _menus.CreateMenu(Context.User.Id);
            var profile = _sim.GetOrCreateProfile(Context.User);
            var embed = GetMenuEmbed(profile);
            var components = GetMenuComponents(Context.User.Id, menuId);
            await RespondAsync(components: components, embed: embed);
        }

        [ComponentInteraction("wishsim_menu:*,*")]
        [VerifyUserAndMenu]
        public async Task WishSimMenu(ulong userId, int menuId)
        {
            var profile = _sim.GetOrCreateProfile(Context.User);
            var embed = GetMenuEmbed(profile);
            var components = GetMenuComponents(Context.User.Id, menuId);

            var component = Context.Interaction as SocketMessageComponent;

            await component.Message.ModifyAsync(x =>
            {
                x.Attachments = new Optional<IEnumerable<FileAttachment>>(Enumerable.Empty<FileAttachment>());
                x.Components = components;
                x.Embed = embed;
            });
            await component.UpdateAsync(x => { });

            /*await component.UpdateAsync(x => 
            {
                x.Attachments = new Optional<IEnumerable<FileAttachment>>(Enumerable.Empty<FileAttachment>());
                x.Components = components;
                x.Embed = embed;
            });*/
        }

        [ComponentInteraction("wishsim_banners:*,*,*")]
        [VerifyUserAndMenu]
        public async Task WishSimBanners(ulong userId, int menuId, Banner banner)
        {
            IEnumerable<EventWish> banners = _wishes.BannersByBID.Values.Where(x => banner.HasFlag(x.BannerType)).Cast<EventWish>().OrderByDescending(x => x.Date);

            var row2 = new ActionRowBuilder();
            switch (banner)
            {
                case Banner.Character: row2.AddComponent(new ButtonBuilder("Weapon", $"wishsim_banners:{Context.User.Id},{menuId},{(int)Banner.Weapon}", ButtonStyle.Secondary).Build()); break;
                case Banner.Weapon: row2.AddComponent(new ButtonBuilder("Character", $"wishsim_banners:{Context.User.Id},{menuId},{(int)Banner.Character}", ButtonStyle.Secondary).Build()); break;
            }
            row2.AddComponent(new ButtonBuilder("Standard", $"wishsim_banners_selected:{Context.User.Id},{menuId},{_wishes.StandardBID}", ButtonStyle.Secondary).Build())
                .AddComponent(new ButtonBuilder("Beginner", $"wishsim_banners_selected:{Context.User.Id},{menuId},{_wishes.BeginnerBID}", ButtonStyle.Secondary).Build())
                .AddComponent(new ButtonBuilder("Menu", $"wishsim_menu:{Context.User.Id},{menuId}", ButtonStyle.Secondary).Build());

            var pager = new BannerSelectionPager(banners, "wishsim_banners_selected", menuId, row2);
            _menus.SetMenuContent(userId, pager);

            await pager.UpdateAsync(Context);
        }

        [ComponentInteraction("wishsim_banners_selected:*,*,*")]
        [VerifyUserAndMenu]
        public async Task WishSimBannersSelected(ulong userId, int menuId, int bid)
        {
            var profile = _sim.GetOrCreateProfile(Context.User);
            var wish = _wishes.BannersByBID[bid];
            profile.ChangeBanner(wish);

            await WishSimMenu(userId, menuId);
        }

        [ComponentInteraction("wishsim_inventory:*,*")]
        [VerifyUserAndMenu]
        public async Task WishSimInventory(ulong userId, int menuId)
        {
            var profile = _sim.GetOrCreateProfile(Context.User);

            if (profile.Inventory.Count == 0)
            {
                await RespondAsync($"Your inventory is empty! Do some wishes first.", ephemeral: true);
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
                .WithColor(Globals.MainColor)
                .AddField("Stats",
                    $"Wishes: **{total}**\n" +
                    $"3-star: **{threestar}** | **{threestar / (float)total:0.00%}**\n" +
                    $"4-star: **{fourstar}** | **{fourstar / (float)total:0.00%}**\n" +
                    $"5-star: **{fivestar}** | **{fivestar / (float)total:0.00%}**\n"
                    )
                .AddField("5★ items", string.IsNullOrEmpty(fivestarsString) ? "None yet!" : fivestarsString)
                .AddField("4★ items", string.IsNullOrEmpty(fourstarString) ? "None yet!" : fourstarString)
                .AddField("3★ items", string.IsNullOrEmpty(threestarString) ? "None yet!" : threestarString);

            var cb = new ComponentBuilder().WithButton("Menu", $"wishsim_menu:{Context.User.Id},{menuId}", ButtonStyle.Secondary);
            var component = Context.Interaction as SocketMessageComponent;
            await component.UpdateAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = cb.Build();
            });

            static string FormatWishItemCount(WishItemCount wic) => $"**{wic.WishItem.Name}** (x{wic.Count})";
        }

        [ComponentInteraction("wishsim_reset:*,*")]
        [VerifyUserAndMenu]
        public async Task WishSimReset(ulong userId, int menuId)
        {
            _sim.ResetProfile(Context.User);
            await WishSimMenu(userId, menuId);
        }

        [ComponentInteraction("wishsim:*,*,*")]
        [VerifyUserAndMenu]
        public async Task WishSimButton(ulong userId, int menuId, int count)
        {
            var button1 = new ButtonBuilder("Wish 10", $"wishsim:{Context.User.Id},{menuId},10", ButtonStyle.Primary, emote: GenshinEmotes.Intertwined, isDisabled: true);
            var button2 = new ButtonBuilder("Wish 1", $"wishsim:{Context.User.Id},{menuId},1", ButtonStyle.Primary, emote: GenshinEmotes.Intertwined, isDisabled: true);
            var button3 = new ButtonBuilder("Menu", $"wishsim_menu:{Context.User.Id},{menuId}", ButtonStyle.Secondary, isDisabled: true);

            var result = GetWishResult(count, Context.Interaction.User);

            var component = Context.Interaction as SocketMessageComponent;

            // Discord.NET please fix #358 in Labs so I don't have to do this SHIT
            await component.Message.ModifyAsync(x =>
            {
                x.Attachments = new Optional<IEnumerable<FileAttachment>>(Enumerable.Empty<FileAttachment>());
                x.Embed = result.Embeds[0];
                x.Components = new ComponentBuilder().WithButton(button1).WithButton(button2).WithButton(button3).Build();
            });
            await component.UpdateAsync(x => { });
            await Task.Delay(750);

            await component.ModifyOriginalResponseAsync(x => x.Embed = result.Embeds[1]);
            await Task.Delay(1500);

            button1.WithDisabled(false);
            button2.WithDisabled(false);
            button3.WithDisabled(false);

            using var stream = result.WishImage.GetImage();
            using FileAttachment fa = new FileAttachment(stream, FILE_NAME);
            await component.ModifyOriginalResponseAsync(x =>
            {
                x.Attachments = new[] { fa };
                x.Embed = result.Embeds[2];
                x.Components = new ComponentBuilder().WithButton(button1).WithButton(button2).WithButton(button3).Build();
            });
        }

        private GachaSimResult GetWishResult(int count, IUser user)
        {
            var profile = _sim.GetOrCreateProfile(user);
            var session = profile.GetCurrentSession();
            var pityBefore = session.CurrentFiveStarPity;
            var wishCountBefore = profile.Inventory.Count;

            Embed[] embeds = new Embed[3];

            var embed = new EmbedBuilder()
                .WithAuthor(user)
                .WithTitle($"Wishing {(count == 1 ? "once" : $"{count} times")}... Pity: `{pityBefore}`")
                .WithFooter($"Total wishes: {wishCountBefore} | Banner: {session.Banner.GetFullName()}")
                .WithColor(Color.DarkGrey);

            embeds[0] = embed.Build();

            GachaSimWishItemRecord[] result = result = _sim.Wish(Context.User, count);
            var rarityColor = GenshinColors.GetRarityColor(result.Max(x => x.WishItem.Rarity));
            embed.WithColor(rarityColor)
                .WithTitle($"Wishing {(count == 1 ? "once" : $"{count} times")}... Pity: `{pityBefore} -> {session.CurrentFiveStarPity}`")
                .WithFooter($"Total wishes: {profile.Inventory.Count} | Banner: {session.Banner.GetFullName()}");

            embeds[1] = embed.Build();

            embed.WithImageUrl($"attachment://{FILE_NAME}");

            embeds[2] = embed.Build();

            return new GachaSimResult(embeds, new GachaSimImage(result));
        }

        private Embed GetMenuEmbed(GachaSimulatorProfile profile)
        {
            var session = profile.GetCurrentSession();
            var embed = new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithTitle("Gacha simulator menu")
                .WithFooter($"Total wishes: {profile.Inventory.Count} | Banner: {session.Banner.GetFullName()}");

            if (session.Banner is EventWish ew)
                embed.WithDescription($"__You are wishing on {session.Banner.Name}__\n{string.Join(", ", ew.RateUpFivestars.Select(x => x.GetNameWithEmotes()))}\n" +
                    $"{string.Join("\n", ew.RateUpFourstars.Select(x => x.Name))}");
            else
                embed.WithDescription($"You are wishing on **{session.Banner.Name}**");

            var wish = session.Banner as EventWish;

            return embed.Build();
        }

        private static MessageComponent GetMenuComponents(ulong userId, int menuId) => new ComponentBuilder()
               .WithButton("Wish 10", $"wishsim:{userId},{menuId},10", ButtonStyle.Primary, emote: GenshinEmotes.Intertwined)
               .WithButton("Wish 1", $"wishsim:{userId},{menuId},1", ButtonStyle.Primary, emote: GenshinEmotes.Intertwined)
               .WithButton("Banner", $"wishsim_banners:{userId},{menuId},{(int)Banner.Character}", ButtonStyle.Secondary, row: 1)
               .WithButton("Inventory", $"wishsim_inventory:{userId},{menuId}", ButtonStyle.Secondary, row: 1)
               .WithButton("Reset", $"wishsim_reset:{userId},{menuId}", ButtonStyle.Secondary, row: 1)
               .Build();
    }
}
