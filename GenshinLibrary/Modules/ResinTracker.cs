using Discord;
using Discord.Commands;
using GenshinLibrary.Commands;
using GenshinLibrary.Preconditions;
using GenshinLibrary.Services.Resin;
using System;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Summary("Keep track of your resin outside of the game.")]
    public class ResinTracker : GLInteractiveBase
    {
        private readonly ResinTrackerService _resinTracker;

        public ResinTracker(ResinTrackerService resinTracker)
        {
            _resinTracker = resinTracker;
        }

        [Command("setresin")]
        [Summary("Update your resin.")]
        [Ratelimit(5)]
        public async Task SetResin(
            [Summary("The value to set.")] int value,
            [Summary("Current in-game resin countdown timer. Specify this in `0m0s` format.")] TimeSpan? adjustBy = null
            )
        {
            if (value < 0 || value >= ResinUpdate.MaxResin)
            {
                await ReplyAsync($"Can only update to a value from 0 to {ResinUpdate.MaxResin - 1}.");
                return;
            }

            var dt = DateTime.UtcNow;
            if (adjustBy.HasValue)
            {
                var ts = adjustBy.Value;
                if (ts > TimeSpan.FromMinutes(8))
                {
                    await ReplyAsync($"Cannot adjust by more than {ResinUpdate.RechargeRateMinutes} minutes.");
                    return;
                }

                var rechargeRate = TimeSpan.FromMinutes(ResinUpdate.RechargeRateMinutes);
                dt = dt.Add(ts - rechargeRate);
            }

            var resinUpdate = await _resinTracker.SetValueAsync(Context.User, dt, value);
            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.User)
                .WithColor(Globals.MainColor)
                .WithDescription($"Done! Your current resin: {ResinUpdate.GetResinString(resinUpdate.Value)}")
                .WithFooter($"Fully refills in {resinUpdate.TimeBeforeFullRefill():hh\\:mm\\:ss}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("resin")]
        [Summary("View your resin.")]
        [Ratelimit(5)]
        public async Task GetResin()
        {
            var resinUpdate = _resinTracker.GetValue(Context.User);
            var embed = new EmbedBuilder();

            int currentResin = ResinUpdate.MaxResin;

            if (resinUpdate != null)
            {
                currentResin = resinUpdate.GetCurrentResin();

                if (currentResin < 160)
                    embed.WithFooter($"Fully refills in {resinUpdate.TimeBeforeFullRefill():hh\\:mm\\:ss}");
            }

            embed.WithAuthor(Context.User)
                .WithColor(Globals.MainColor)
                .WithDescription($"Current resin: {ResinUpdate.GetResinString(currentResin)}");

            await ReplyAsync(embed: embed.Build());
        }
    }
}
