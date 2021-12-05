using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using GenshinLibrary.Models;
using GenshinLibrary.ReactionCallback.Base;
using GenshinLibrary.StringTable;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.ReactionCallback
{
    class WishHistoryPaged : FragmentedPagedMessage<CompleteWishItemRecord>
    {
        private readonly string _bannerName;
        private readonly int _count;
        private readonly bool _showType;

        public WishHistoryPaged(InteractiveService interactive,
            SocketCommandContext context,
            int displayPerPage,
            IEnumerable<CompleteWishItemRecord> records,
            string bannerName,
            bool showType) : base(interactive, context, records, displayPerPage)
        {
            _bannerName = bannerName;
            _count = records.Count();
            _showType = showType;
        }

        protected override Embed ConstructEmbed(IEnumerable<CompleteWishItemRecord> currentPage)
        {
            TextTable table;

            if (_showType)
            {
                table = new TextTable("Pity", "Type", "Name", "DateTime");
                foreach (var wir in currentPage)
                {
                    var name = wir.WishItem.GetFormattedName(27);
                    table.AddRow(wir.Pity == -1 ? "-" : wir.Pity.ToString(), wir.GetShortBannerString(), name, wir.DateTime.ToString(@"dd.MM.yyyy HH:mm:ss"));
                }
            }
            else
            {
                table = new TextTable("Pity", "Name", "DateTime");
                foreach (var wir in currentPage)
                {
                    var name = wir.WishItem.GetFormattedName(27);
                    table.AddRow(wir.Pity == -1 ? "-" : wir.Pity.ToString(), name, wir.DateTime.ToString(@"dd.MM.yyyy HH:mm:ss"));
                }
            }

            var embed = new EmbedBuilder();
            embed.WithColor(Globals.MainColor)
                .WithTitle($"Wish history for {_bannerName}")
                .WithDescription($"Total items: **{_count}**\n{table.GetTable()}");

            embed = SetDefaultFooter(embed);

            return embed.Build();
        }
    }
}
