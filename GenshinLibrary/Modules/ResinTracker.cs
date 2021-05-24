using Discord;
using Discord.Commands;
using GenshinLibrary.Commands;
using GenshinLibrary.GenshinWishes;
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
        [Alias("sr")]
        [Ratelimit(5)]
        public async Task SetResin(
            [Summary("The value to set.")] int value,
            [Summary("Current in-game resin countdown timer. Specify this in `0m0s` format.")] TimeSpan? nextIn = null
            )
        {
            if (value < 0 || value >= ResinUpdate.MaxResin)
            {
                await ReplyAsync($"Can only update to a value from 0 to {ResinUpdate.MaxResin - 1}.");
                return;
            }

            var dt = DateTime.UtcNow;
            if (nextIn.HasValue)
            {
                var ts = nextIn.Value;
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
                .WithDescription($"{GenshinEmotes.Resin} {ResinUpdate.GetResinString(resinUpdate.Value)}")
                .WithFooter($"Next in {resinUpdate.UntilNext():hh\\:mm\\:ss}\nFully refills in {resinUpdate.UntilFullRefill():hh\\:mm\\:ss}");

            await ReplyAsync("Done!", embed: embed.Build());
        }

        [Command("resin")]
        [Summary("View your resin.")]
        [Alias("r")]
        [Ratelimit(5)]
        public async Task GetResin()
        {
            var resinUpdate = _resinTracker.GetResinUpdate(Context.User);
            var embed = new EmbedBuilder();

            int currentResin = ResinUpdate.MaxResin;

            if (resinUpdate != null && !resinUpdate.IsFull)
            {
                currentResin = resinUpdate.GetCurrentResin();
                embed.WithFooter($"Next in {resinUpdate.UntilNext():hh\\:mm\\:ss}\nFully refills in {resinUpdate.UntilFullRefill():hh\\:mm\\:ss}");
            }

            embed.WithAuthor(Context.User)
                .WithColor(Globals.MainColor)
                .WithDescription($"{GenshinEmotes.Resin} {ResinUpdate.GetResinString(currentResin)}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("subtract")]
        [Summary("Subtracts a certain value from your current resin.")]
        [Alias("subtractresin", "subr")]
        [Ratelimit(5)]
        public async Task SubtractResin(
            [Summary("The value to subtract. Must be between 10 and 150.")] int subtract
            )
        {
            if (subtract % 10 != 0 || subtract < 10 || subtract > ResinUpdate.MaxResin - 10)
            {
                await ReplyAsync($"Can only subtract a number that divides by 10 and that is between 10 and {ResinUpdate.MaxResin - 10}.");
                return;
            }

            var update = _resinTracker.GetResinUpdate(Context.User);

            if (update is null)
            {
                await SetResin(ResinUpdate.MaxResin - subtract);
                return;
            }

            int currentResin = update.GetCurrentResin();
            if (currentResin < subtract)
            {
                await ReplyAsync($"Current resin ({currentResin}) is less than value to subtract.");
                return;
            }

            await SetResin(currentResin - subtract, update.UntilNext());
        }
    }
}
