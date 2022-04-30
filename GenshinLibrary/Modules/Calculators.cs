using Discord;
using Discord.Interactions;
using GenshinLibrary.Calculators.PrimogemCalculator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    public class Calculators : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("primogems", "Calculates the amount of primogems you can get before the end of a given banner in a given version.")]
        public async Task Primogems(
            [Summary(description: "The version to calculate to (e.g. 2.5)")] string version,
            [Summary(description: "The banner to calculate to"), Choice("First", 1), Choice("Second", 2)] int banner,
            [Summary(description: "Current amount of primogems"), MinValue(1)] int primogems = 0,
            [Summary(description: "Current amount of acquaint fates"), MinValue(1)] int acquaint = 0,
            [Summary(description: "Current amount of intertwined fates"), MinValue(1)] int intertwined = 0,
            [Summary(description: "Amount of primogems you gain every abyss period"), MinValue(50), MaxValue(600)] int abyss = 0,
            [Summary(description: "Current Sojourner BP level"), MinValue(1), MaxValue(49)] int currsojourner = 0,
            [Summary(description: "Current Gnostic BP level"), MinValue(1), MaxValue(49)] int currgnostic = 0,
            [Summary(description: "Your current HoyoLab check-in day. Also adds future HoyoLab rewards."), MinValue(1), MaxValue(31)] int hoyolab = 0,
            [Summary(description: "Include an average amount of primogems from events.")] bool events = false,
            [Summary(description: "Remaining days of the Welkin blessing"), MinValue(1), MaxValue(180)] int welkin = 0,
            [Summary(description: "Amount of Gnostic passes you wish to purchase during the period"), MinValue(1)] int gnostic = 0
            )
        {
            var versions = GetVersions(Globals.GetConfig().Version, 5);
            int verIndex = 0;
            while (verIndex < versions.Count && versions[verIndex].VersionName != version)
                verIndex++;

            if (verIndex < versions.Count - 1)
                versions.RemoveRange(verIndex + 1, versions.Count - verIndex - 1);

            string errorMessage = null;
            if (verIndex == versions.Count)
                errorMessage = "No such version exists or version out of boundaries.\nCannot specify versions that have already ended or versions that are more than 5 versions ahead of the current one.";
            else if (hoyolab > DateTime.UtcNow.Day)
                errorMessage = "`Hoyolab` can't be later than the current day of month.";
            else if (versions.Count == 1 && DateTime.UtcNow
                > versions[0].Start + banner * TimeSpan.FromDays(PrimogemCalculator.VERSION_DURATION / PrimogemCalculator.BANNERS_PER_VERSION))
                errorMessage = "Banner has already ended.";

            if (errorMessage != null)
            {
                var helpEmbed = new EmbedBuilder()
                    .WithTitle("Invalid input.")
                    .WithDescription(errorMessage)
                    .WithColor(Color.Red);

                await RespondAsync(embed: helpEmbed.Build(), ephemeral: true);
                return;
            }

            var settings = new PrimogemCalculatorSettings(primogems, acquaint, intertwined, abyss, currsojourner, currgnostic, hoyolab, events, welkin, gnostic);
            PrimogemCalculator calculator = new(DateTime.UtcNow.Date, versions, banner, settings);
            await RespondAsync(embed: calculator.ConstructEmbed());
        }

        private static List<GameVersion> GetVersions(GameVersion current, int count)
        {
            List<GameVersion> versions = new();
            int version = 0;

            for (DateTime i = current.Start; version < count; i = Increment(i))
            {
                var next = Increment(i);
                versions.Add(new GameVersion(i, next, $"{current.Major + (current.Minor + version) / 10}.{(current.Minor + version) % 10}"));
                version++;
            }

            return versions;
            static DateTime Increment(DateTime dt) => dt.AddDays(PrimogemCalculator.VERSION_DURATION);
        }
    }
}
