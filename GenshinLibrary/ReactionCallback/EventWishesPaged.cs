using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using GenshinLibrary.ReactionCallback.Base;
using GenshinLibrary.Services.GachaSim;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.ReactionCallback
{
    class EventWishesPaged : FragmentedPagedMessage<EventWish>
    {
        public EventWishesPaged(InteractiveService interactive, SocketCommandContext context, IEnumerable<EventWish> collection, int displayPerPage) : base(interactive, context, collection, displayPerPage)
        {
        }

        protected override Embed ConstructEmbed(IEnumerable<EventWish> currentPage)
        {
            var embed = new EmbedBuilder()
                .WithColor(Globals.MainColor)
                .WithFooter($"Reply with just the number of the selected banner.\nPage {Page + 1} / {TotalPages}")
                .WithTitle($"Select one of the following banners");

            int count = 0;
            foreach (var wish in currentPage)
            {
                count++;
                embed.AddField($"{count + Page * _displayPerPage}. {wish.GetFullName()}",
                    $"{string.Join(", ", wish.RateUpFivestars.Select(x => x.GetNameWithEmotes()))}\n" +
                    $"{string.Join("\n", wish.RateUpFourstars.Select(x => x.Name))}"
                    );
            }

            return embed.Build();
        }
    }
}
