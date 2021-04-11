using Discord;
using Discord.Commands;
using GenshinLibrary.Commands;
using GenshinLibrary.Preconditions;
using GenshinLibrary.Services.Resin;
using System;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Summary("Keep track of your resin.")]
    public class ResinTracker : GLInteractiveBase
    {
        private readonly ResinTrackerService _resinTracker;

        public ResinTracker(ResinTrackerService resinTracker)
        {
            _resinTracker = resinTracker;
        }

        [Command("setresin")]
        [Alias("resin")]
        [Summary("Update your resin.")]
        [Ratelimit(10)]
        public async Task SetResin(
            [Summary("The value to set.")] int value
            )
        {
            if (value < 0 || value >= ResinUpdate.MaxResin)
            {
                await ReplyAsync($"Can only update to a value from 0 to {ResinUpdate.MaxResin - 1}.");
                return;
            }

            var resinUpdate = _resinTracker.SetValue(Context.User, DateTime.UtcNow, value);
            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.User)
                .WithColor(Globals.MainColor)
                .WithDescription($"Done! Your current resin: {ResinUpdate.GetResinString(resinUpdate.Value)}")
                .WithFooter($"Fully refills in {resinUpdate.TimeBeforeFullRefill():hh\\:mm\\:ss}");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("resin")]
        [Summary("View your resin.")]
        [Ratelimit(10)]
        public async Task GetResin()
        {
            var resinUpdate = _resinTracker.GetValue(Context.User);
            var embed = new EmbedBuilder();

            int currentResin = ResinUpdate.MaxResin;

            if (resinUpdate != null)
            {
                currentResin = resinUpdate.GetCurrentResin();
                embed.WithFooter($"Fully refills in {resinUpdate.TimeBeforeFullRefill():hh\\:mm\\:ss}");
            }

            embed.WithAuthor(Context.User)
                .WithColor(Globals.MainColor)
                .WithDescription($"Current resin: {ResinUpdate.GetResinString(currentResin)}");

            await ReplyAsync(embed: embed.Build());
        }
    }
}
