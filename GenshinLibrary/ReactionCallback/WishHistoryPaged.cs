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
        private readonly int count;
        private const int maxNameLength = 27;

        public WishHistoryPaged(InteractiveService interactive,
            SocketCommandContext context,
            int displayPerPage,
            IEnumerable<CompleteWishItemRecord> records,
            string bannerName) : base(interactive, context, records, displayPerPage)
        {
            _bannerName = bannerName;
            count = records.Count();
        }

        protected override Embed ConstructEmbed(IEnumerable<CompleteWishItemRecord> currentPage)
        {
            var table = new TextTable("Pity", "Name", "DateTime");

            foreach (var wir in currentPage)
            {
                var name = wir.WishItem.GetFormattedName(maxNameLength);
                table.AddRow(wir.Pity == -1 ? "-" : wir.Pity.ToString(), name, wir.DateTime.ToString(@"dd.MM.yyyy HH:mm:ss"));
            }

            var embed = new EmbedBuilder();
            embed.WithColor(Globals.MainColor)
                .WithTitle($"Wish history for {_bannerName}")
                .WithDescription($"Total items: **{count}**\n{table.GetTable()}");

            embed = SetDefaultFooter(embed);

            return embed.Build();
        }
    }
}
