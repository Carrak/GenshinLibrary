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
            "**Events** - includes an average amount of primogems from events. [true/false]\n" +
            "**Hoyolab** - your current HoyoLab check-in day. Specifying this settings also adds future HoyoLab rewards.\n\n" +
            "Settings are specified in the following format:\n" +
            "`[setting]:[value]`";

        [Command("primogems")]
        [Alias("primogemcalculator", "pc")]
        [Ratelimit(10)]
        [Summary("Calculates the amount of primogems you can get before the end of a given banner in a given version.")]
        [Example("`gl!primogems 1.5 2 primogems:1305 events:true abyss:600 currsojourner:44 welkin:34 gnostic:1`")]
        public async Task Primogems(
            [Summary("The version to calculate to.")] string version,
            [Summary("The banner to calculate to. Either `1` (first banner) or `2` (second banner).")] int banner,
            [Summary(SettingsSummary)] PrimogemCalculatorSettings settings = null
            )
        {
            settings ??= new PrimogemCalculatorSettings();

            if (!await ValidateSettingsAsync(settings))
                return;

            PrimogemCalculator calculator;
            try
            {
                calculator = new PrimogemCalculator(DateTime.UtcNow.Date, version, banner, settings);
            }
            catch (ArgumentException e)
            {
                var helpEmbed = new EmbedBuilder();

                helpEmbed.WithTitle("Invalid input.")
                    .WithDescription(e.Message)
                    .WithColor(Color.Red);

                await ReplyAsync(embed: helpEmbed.Build());
                return;
            }

            await ReplyAsync(embed: calculator.ConstructEmbed());
        }

        [Command("primogems")]
        [Alias("primogemcalculator", "pc")]
        [Ratelimit(10)]
        [Summary("Calculates the amount of primogems you can get across a given amount of days.")]
        [Example("`gl!primogems 50 primogems:1305 events:true abyss:600 currsojourner:44 welkin:34 gnostic:1`")]
        public async Task Primogems(
            [Summary("Amount of days to calculate through.")] int days,
            [Summary(SettingsSummary)] PrimogemCalculatorSettings settings = null) => await Primogems(DateTime.UtcNow.AddDays(days), settings);

        [Command("primogems")]
        [Alias("primogemcalculator", "pc")]
        [Ratelimit(10)]
        [Summary("Calculates the amount of primogems you can get before a given date.")]
        [Example("`gl!primogems 08.06.2021 primogems:1305 events:true abyss:600 currsojourner:44 welkin:34 gnostic:1`")]
        public async Task Primogems(
            [Summary("The end date to calculate to. Format: MM.DD.YYYY")] DateTime end,
            [Summary(SettingsSummary)] PrimogemCalculatorSettings settings = null
            )
        {
            settings ??= new PrimogemCalculatorSettings();

            if (end.Date - DateTime.UtcNow.Date < TimeSpan.FromDays(1))
            {
                await ReplyAsync("Cannot calculate periods shorter than 1 day.");
                return;
            }

            if (!await ValidateSettingsAsync(settings))
                return;

            var calculator = new PrimogemCalculator(DateTime.UtcNow.Date, end.Date, settings);
            await ReplyAsync(embed: calculator.ConstructEmbed());
        }

        private async Task<bool> ValidateSettingsAsync(PrimogemCalculatorSettings settings)
        {
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
                return false;
            }

            return true;
        }
    }
}
