using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GenshinLibrary.Services.GachaSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Pagers
{
    public class BannerSelectionPager : Pager
    {
        public const int DISPLAY_PER_PAGE = 3;

        public IEnumerable<ActionRowBuilder> Rows { get; set; }

        private readonly IReadOnlyList<EventWish> _banners;
        private readonly string _selectionCustomId;
        private readonly int _menuId;

        public BannerSelectionPager(IEnumerable<EventWish> banners, string selectionCustomId, int menuId, params ActionRowBuilder[] rows) : base((int)Math.Ceiling(banners.Count() / (float)DISPLAY_PER_PAGE))
        {
            _banners = banners.ToList().AsReadOnly();
            _selectionCustomId = selectionCustomId;
            _menuId = menuId;
            Rows = rows;
        }

        public async Task UpdateAsync(SocketInteractionContext ctx)
        {
            var embed = new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithFooter($"Page {Page + 1} / {TotalPages}")
                .WithTitle($"Select one of the following banners");

            int low = Page * DISPLAY_PER_PAGE;
            int high = Math.Min(_banners.Count, (Page + 1) * DISPLAY_PER_PAGE);
            var row1 = new ActionRowBuilder();
            for (int i = low; i < high; i++)
            {
                embed.AddField($"{i + 1}. {_banners[i].GetFullName()}",
                    $"{string.Join(", ", _banners[i].RateUpFivestars.Select(x => x.GetNameWithEmotes()))}\n" +
                    $"{string.Join("\n", _banners[i].RateUpFourstars.Select(x => x.Name))}"
                    );

                row1.AddComponent(new ButtonBuilder($"{i + 1}", $"{_selectionCustomId}:{ctx.User.Id},{_menuId},{_banners[i].BID}", ButtonStyle.Primary).Build());
            }
            row1.Components.Insert(0, new ButtonBuilder("<<", $"banner_pager:{ctx.User.Id},{_menuId},0", ButtonStyle.Primary, isDisabled: TotalPages == 1).Build());
            row1.Components.Add(new ButtonBuilder(">>", $"banner_pager:{ctx.User.Id},{_menuId},1", ButtonStyle.Primary, isDisabled: TotalPages == 1).Build());
            var mc = new ComponentBuilder().AddRow(row1);
            foreach (var row in Rows)
                mc.AddRow(row);

            if (ctx.Interaction is SocketMessageComponent smc)
                await smc.UpdateAsync(x =>
                {
                    x.Embed = embed.Build();
                    x.Components = mc.Build();
                });
            else
                await ctx.Interaction.RespondAsync(embed: embed.Build(), components: mc.Build());
        }


    }
}
