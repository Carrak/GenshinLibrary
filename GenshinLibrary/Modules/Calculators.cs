using Discord;
using Discord.Commands;
using GenshinLibrary.Attributes;
using GenshinLibrary.Calculators.PrimogemCalculator;
using GenshinLibrary.Commands;
using GenshinLibrary.Preconditions;
using System;
using System.Threading.Tasks;

namespace GenshinLibrary.Modules
{
    [Summary("Various calculators for the game.")]
    public class Calculators : GLInteractiveBase
    {
        private const string SettingsSummary =
            "**Primogems** - current amount of primogems.\n" +
            "**Intertwined** - current amount of intertwined fates.\n" +
            "**Acquaint** - current amount of acquaint fates.\n" +
            "**Welkin** - remaining days of the Welkin blessing.\n" +
            "**Abyss** - amount of primogems you gain every abyss period.\n" +
            "**CurrSojourner** - current Sojourner BP level.\n" +
            "**CurrGnostic** - current Gnostic BP level.\n" +
            "**Gnostic** - amount of Gnostic passes you wish to purchase during the period.\n" +
            "**Events** - includes an average amount of primogems from events. [true/false]";

        [Command("primogems")]
        [Ratelimit(10)]
        [Summary("Calculates the amount of primogems you can get across a given amount of days.")]
        [Example("`gl!primogems 50 primogems:1305 events:true abyss:600 currsojourner:44 welkin:34 gnostic:1`")]
        public async Task Primogems(
            [Summary("Amount of days to calculate through.")] int days, 
            [Summary(SettingsSummary)] PrimogemCalculatorSettings settings = null) => await Primogems(DateTime.UtcNow.Date.AddDays(days), settings);

        [Command("primogems")]
        [Ratelimit(10)]
        [Summary("Calculates the amount of primogems you can get before a given date.")]
        [Example("`gl!primogems 08.06.2021 primogems:1305 events:true abyss:600 currsojourner:44 welkin:34 gnostic:1`")]
        public async Task Primogems(
            [Summary("The end date to calculate to.")] DateTime end, 
            [Summary(SettingsSummary)] PrimogemCalculatorSettings settings = null
            )
        {
            if (end - DateTime.UtcNow < TimeSpan.FromDays(1))
            {
                await ReplyAsync("Cannot calculate periods shorter than 1 day.");
                return;
            }

            try
            {
                settings.Validate();
            }
            catch (Exception e)
            {
                var helpEmbed = new EmbedBuilder();

                helpEmbed.WithTitle("Invalid settings.")
                    .WithDescription(e.Message)
                    .WithColor(Color.Red);

                await ReplyAsync(embed: helpEmbed.Build());
                return;
            }

            var calculator = new PrimogemCalculator(DateTime.UtcNow.Date, end, settings);
            await ReplyAsync(embed: calculator.ConstructEmbed());
        }
    }
}
