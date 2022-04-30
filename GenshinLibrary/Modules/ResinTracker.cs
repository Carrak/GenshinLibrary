using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GenshinLibrary.Models;
using GenshinLibrary.Services.Resin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    public class ResinTracker : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ResinTrackerService _resinTracker;

        private static readonly Dictionary<ulong, int> _menus = new Dictionary<ulong, int>();

        public ResinTracker(ResinTrackerService resinTracker)
        {
            _resinTracker = resinTracker;
        }

        [SlashCommand("resin", "Open the resin menu")]
        public async Task GetResin()
        {
            if (_menus.TryGetValue(Context.User.Id, out int menuId))
                menuId++;
            else
                menuId = 0;

            _menus[Context.User.Id] = menuId;

            var resinUpdate = _resinTracker.GetResinUpdate(Context.User.Id);
            if (resinUpdate is null)
                resinUpdate = await _resinTracker.SetValueAsync(Context.User.Id, DateTime.UtcNow, ResinUpdate.MAX_RESIN);

            await RespondAsync(embed: GetResinEmbed(resinUpdate), components: GetResinMenuComponent(menuId));
        }

        [ComponentInteraction("subtract_resin:*,*,*")]
        public async Task SubstractResin(ulong userId, int menuId, int toSubtract)
        {
            if (!VerifyMenu(userId, menuId))
                return;

            var resinUpdate = _resinTracker.GetResinUpdate(Context.User.Id);
            if (resinUpdate.Value < toSubtract)
            {
                await RespondAsync($"Your resin is too low to subtract that amount", ephemeral: true);
                return;
            }

            resinUpdate = await _resinTracker.SetValueAsync(Context.User.Id,
                DateTime.UtcNow.Add(resinUpdate.UntilNext() - TimeSpan.FromMinutes(ResinUpdate.RESIN_RATE_MINUTES)),
                resinUpdate.Value - toSubtract);

            var component = Context.Interaction as SocketMessageComponent;
            await component.UpdateAsync(x => x.Embed = GetResinEmbed(resinUpdate));
        }

        [ComponentInteraction("set_resin:*,*")]
        public async Task SetResin(ulong userId, int menuId)
        {
            if (!VerifyMenu(userId, menuId))
                return;

            var component = Context.Interaction as SocketMessageComponent;
            await RespondWithModalAsync<ResinModal>($"resin_modal:{component.Message.Id}");
        }

        public class ResinModal : IModal
        {
            public string Title => "Set your current resin";

            [InputLabel("Current resin")]
            [ModalTextInput("resin_value", TextInputStyle.Short, "Input your current in-game resin here", 1, 3)]
            public string Resin { get; set; }

            [InputLabel("Next resin in")]
            [ModalTextInput("next_in", TextInputStyle.Short, "Resin countdown timer. Specify in 0m0s format.", 1, 4, "8m0s")]
            public string NextIn { get; set; }
        }

        [ModalInteraction("resin_modal:*")]
        public async Task ResinModalResponse(ulong messageId, ResinModal rm)
        {
            string[] formats =
            {
                "%m'm'%s's'",
                "%m'm'",
                "%s's'"
            };

            if (!TimeSpan.TryParseExact(rm.NextIn.ToLowerInvariant(), formats, CultureInfo.InvariantCulture, out TimeSpan ts))
            {
                await RespondAsync($"\"Next in\" is not specified in `0m0s` format.", ephemeral: true);
                return;
            }

            if (!int.TryParse(rm.Resin, out int resin))
            {
                await RespondAsync($"\"Resin\" is not a number.", ephemeral: true);
                return;
            }

            var rechargeRate = TimeSpan.FromMinutes(ResinUpdate.RESIN_RATE_MINUTES);
            if (ts > rechargeRate)
            {
                await RespondAsync($"Cannot adjust by more than {ResinUpdate.RESIN_RATE_MINUTES} minutes.", ephemeral: true);
                return;
            }

            if (resin < 0 || resin >= ResinUpdate.MAX_RESIN)
            {
                await RespondAsync($"Resin must be above 0 and below {ResinUpdate.MAX_RESIN}.", ephemeral: true);
                return;
            }

            var resinUpdate = await _resinTracker.SetValueAsync(Context.User.Id, DateTime.UtcNow.Add(ts - rechargeRate), resin);
            var msg = await Context.Interaction.Channel.GetMessageAsync(messageId) as IUserMessage;

            await msg.ModifyAsync(x => x.Embed = GetResinEmbed(resinUpdate));
            await RespondAsync("Succesffuly set your resin!", ephemeral: true);
        }

        private Embed GetResinEmbed(ResinUpdate ru)
        {
            var embed = new EmbedBuilder();
            int currentResin = ResinUpdate.MAX_RESIN;

            if (ru != null && !ru.IsFull)
            {
                currentResin = ru.GetCurrentResin();
                embed.WithFooter($"Next in {ru.UntilNext():hh\\:mm\\:ss}\nFully refills in {ru.UntilFullRefill():hh\\:mm\\:ss}");
            }

            embed.WithAuthor(Context.User)
                .WithColor(Globals.MainColor)
                .WithDescription($"{GenshinEmotes.Resin} {ResinUpdate.GetResinString(currentResin)}");

            return embed.Build();
        }

        private MessageComponent GetResinMenuComponent(int menuId) => new ComponentBuilder()
            .WithButton("Set resin", $"set_resin:{Context.User.Id},{menuId}", ButtonStyle.Primary, GenshinEmotes.Resin)
            .WithButton("Subtract 20", $"subtract_resin:{Context.User.Id},{menuId},20", ButtonStyle.Secondary)
            .Build();

        private bool VerifyMenu(ulong userId, int menuId) => Context.User.Id == userId && _menus.TryGetValue(userId, out var oldMenuId) && oldMenuId == menuId;
    }
}
