using Discord;
using GenshinLibrary.Models;
using GenshinLibrary.ReactionCallback.PrimogemCalculator;
using GenshinLibrary.Utility;
using System;
using System.Collections.Generic;

namespace GenshinLibrary.Calculators.PrimogemCalculator
{
    class PrimogemCalculator
    {
        public const int VERSION_DURATION = 42;
        public const int FATE_PRICE = 160;
        public const int EVENTS_AVERAGE = 2200;
        public const int BANNERS_PER_VERSION = 2;

        private DateTime StartDate { get; }
        private DateTime EndDate { get; }
        private PrimogemCalculatorSettings Settings { get; }
        private List<GameVersion> Versions { get; } = new List<GameVersion>();

        private int Days => (EndDate - StartDate).Days;
        private GameVersion CurrentVersion => Versions[0];
        private GameVersion ReachedVersion => Versions[^1];

        public PrimogemCalculator(DateTime start, List<GameVersion> versions, int banner, PrimogemCalculatorSettings settings)
        {
            Versions = versions;
            TimeSpan bannerDuration = (ReachedVersion.End - ReachedVersion.Start) / BANNERS_PER_VERSION;
            Settings = settings;
            StartDate = start;
            EndDate = ReachedVersion.Start + bannerDuration * banner - TimeSpan.FromDays(1);
        }

        public Embed ConstructEmbed()
        {
            var embed = new EmbedBuilder();
            var totals = GetTotals();

            int totalGems = 0;
            int totalAcquaint = 0;
            int totalIntertwined = 0;

            foreach (var total in totals)
            {
                embed.AddField(total.ToField());
                int[] totalCurrencies = total.GetTotalCurrencies();
                totalGems += totalCurrencies[0];
                totalAcquaint += totalCurrencies[1];
                totalIntertwined += totalCurrencies[2];
            }

            int totalRolls = totalGems / FATE_PRICE + totalAcquaint + totalIntertwined;
            int fourStars = (int)(totalRolls * 0.13);
            int starglitter = fourStars * 3;

            int extraRolls = totalGems / FATE_PRICE;
            int extraGems = totalGems % FATE_PRICE;

            embed.Fields.Insert(0, new EmbedFieldBuilder()
            {
                Name = $"{GenshinEmotes.Intertwined} GRAND TOTAL",
                Value =
                $"**{totalIntertwined + extraRolls}**{GenshinEmotes.Intertwined} or **{totalAcquaint + extraRolls}**{GenshinEmotes.Acquaint} + **{extraGems}**{GenshinEmotes.Primogem}\n" +
                $"**{totalGems}**{GenshinEmotes.Primogem}, **{totalIntertwined}**{GenshinEmotes.Intertwined}, **{totalAcquaint}**{GenshinEmotes.Acquaint}, **{starglitter}**{GenshinEmotes.Starglitter}"
            });

            embed.WithColor(Globals.MainColor)
                .WithTitle($"Primogem calculator")
                .WithFooter($"Period: {StartDate:dd.MM.yyyy} - {EndDate:dd.MM.yyyy} ({Days} days)\n" +
                $"Version: {ReachedVersion.VersionName} | {ReachedVersion.Start:dd.MM.yyyy} - {ReachedVersion.End:dd.MM.yyyy}\n" +
                $"Banner: {GetCurrentBannerString()}\n" +
                $"Event rewards and starglitter are averaged\n" +
                $"S - Sojourner, G - Gnostic");

            return embed.Build();
        }

        private string GetCurrentBannerString()
        {
            int banner = (int)((EndDate - ReachedVersion.Start) / TimeSpan.FromDays(VERSION_DURATION) * BANNERS_PER_VERSION);
            string bannerString = banner switch
            {
                0 => "1st",
                1 => "2nd",
                _ => throw new NotImplementedException()
            };

            var daysPerBanner = VERSION_DURATION / BANNERS_PER_VERSION;
            var bannerStart = ReachedVersion.Start + TimeSpan.FromDays(VERSION_DURATION / BANNERS_PER_VERSION) * banner;
            return $"{bannerString} | {bannerStart:dd.MM.yyyy} - {bannerStart.AddDays(daysPerBanner):dd.MM.yyyy}";
        }

        private IEnumerable<RewardTotal> GetTotals()
        {
            List<RewardTotal> totals = new List<RewardTotal>();

            // Current
            var current = new RewardTotal(GenshinEmotes.Primogem, "Current");
            if (Settings.Primogems != 0)
                current.Rewards.Add(new Reward(Currency.Primogems, 1, Settings.Primogems));
            if (Settings.Acquaint != 0)
                current.Rewards.Add(new Reward(Currency.Acquaint, 1, Settings.Acquaint));
            if (Settings.Intertwined != 0)
                current.Rewards.Add(new Reward(Currency.Intertwined, 1, Settings.Intertwined));

            if (current.Rewards.Count != 0)
                totals.Add(current);

            // Dailies
            totals.Add(new RewardTotal(GenshinEmotes.Daily, "Daily quests", new Reward(Currency.Primogems, Days, 60)));

            // Welkin
            if (Settings.Welkin != 0)
                totals.Add(new RewardTotal(GenshinEmotes.Welkin, "Welkin", new Reward(Currency.Primogems, Math.Min(Days, Settings.Welkin), 90)));

            // Abyss
            if (Settings.Abyss != 0)
            {
                int abyssPeriods = DateCalculator(StartDate.DateTruncate(TimePartition.Month), i => i + (i.AddMonths(1) - i) / 2);
                if (abyssPeriods != 0)
                    totals.Add(new RewardTotal(GenshinEmotes.Abyss, "Abyss", new Reward(Currency.Primogems, abyssPeriods, Settings.Abyss)));
            }

            // Stardust shop
            int monthlyResets = DateCalculator(StartDate.DateTruncate(TimePartition.Month), i => i.AddMonths(1));
            if (monthlyResets != 0)
                totals.Add(new RewardTotal(GenshinEmotes.Stardust, "Stardust shop", new Reward(Currency.Acquaint, monthlyResets, 5), new Reward(Currency.Intertwined, monthlyResets, 5)));

            int codes = DateCalculator(CurrentVersion.Start - TimeSpan.FromDays(12), i => i.AddDays(VERSION_DURATION));
            int reachedUpdateDays = Math.Min((EndDate - ReachedVersion.Start).Days + 1, Days);
            int currentUpdateDays = Math.Min((CurrentVersion.End - StartDate).Days, Days);

            // Battlepass
            if (GetBattlepassTotal(reachedUpdateDays, currentUpdateDays) is RewardTotal battlepass && battlepass.Rewards.Count != 0)
                totals.Add(battlepass);

            // Updates
            if (GetUpdatesTotal(Versions.Count - 1, codes) is RewardTotal updates && updates.Rewards.Count != 0)
                totals.Add(updates);

            // Events
            if (Settings.Events && GetEvents(currentUpdateDays, reachedUpdateDays) is RewardTotal events)
                totals.Add(events);

            // Test runs
            int testRuns = DateCalculator(CurrentVersion.Start, i => i.AddDays(VERSION_DURATION / 2));
            if (testRuns != 0)
                totals.Add(new RewardTotal(GenshinEmotes.Primogem, "Test Runs", new Reward(Currency.Primogems, testRuns, 20)));

            // Hoyolab
            if (Settings.Hoyolab != 0)
            {
                var hoyolab = GetHoyolab(monthlyResets);
                if (hoyolab.Rewards.Count > 0)
                    totals.Add(hoyolab);
            }

            return totals;
        }

        private RewardTotal GetEvents(int currentUpdateDays, int reachedUpdateDays)
        {
            List<string> updateNames = new List<string>();
            var events = new RewardTotal(GenshinEmotes.Events, "Events");

            if (currentUpdateDays > 0)
            {
                events.Rewards.Add(new Reward(Currency.Primogems, 1, EVENTS_AVERAGE * currentUpdateDays / VERSION_DURATION));
                updateNames.Add($"{Versions[0]}");
            }

            if (Versions.Count > 2)
            {
                events.Rewards.Add(new Reward(Currency.Primogems, Versions.Count - 2, EVENTS_AVERAGE));
                if (Versions.Count > 3)
                    updateNames.Add($"{Versions[1]}-{Versions[^2]}");
                else
                    updateNames.Add($"{Versions[1]}");
            }

            if (Versions.Count > 1)
            {
                events.Rewards.Add(new Reward(Currency.Primogems, 1, EVENTS_AVERAGE * reachedUpdateDays / VERSION_DURATION));
                updateNames.Add($"{ReachedVersion}");
            }

            events.Name += $" ({string.Join(", ", updateNames)})";
            return events;
        }

        private RewardTotal GetUpdatesTotal(int updates, int codes)
        {
            List<string> names = new List<string>();
            var totals = new RewardTotal(GenshinEmotes.Primogem, "Updates");

            if (codes > 0)
            {
                totals.Rewards.Add(new Reward(Currency.Primogems, 3 * codes, 100));
                names.Add("promocodes");
            }

            if (updates > 0)
            {
                totals.Rewards.Add(new Reward(Currency.Primogems, updates, 300));
                names.Add("maintenances");
                totals.Rewards.Add(new Reward(Currency.Primogems, updates, 300));
                names.Add("fixes");
            }

            totals.Name += $" ({string.Join('/', names)})";

            return totals;
        }

        private RewardTotal GetBattlepassTotal(int daysFinal, int daysCurrent)
        {
            List<string> rewardNames = new List<string>();
            var battlepass = new RewardTotal(GenshinEmotes.Sojourner, "Battlepass");
            int updates = Versions.Count - 1;

            // Current sojourner/gnostic
            if (Settings.CurrSojourner != 0 || Settings.CurrGnostic != 0)
            {
                int level = Battlepass.CalculateLevels(daysCurrent);
                IEnumerable<Reward> rewards;

                if (Settings.CurrGnostic != 0)
                {
                    rewards = Battlepass.GetPartialRewardsGnostic(level, Settings.CurrGnostic);
                    level = Math.Min(50, level + Settings.CurrGnostic);
                    rewardNames.Add($"LVL{Settings.CurrGnostic}-LVL{level}G");
                }
                else
                {
                    rewards = Battlepass.GetPartialRewardsSojourner(level, Settings.CurrSojourner);
                    level = Math.Min(50, level + Settings.CurrSojourner);
                    rewardNames.Add($"LVL{Settings.CurrSojourner}-LVL{level}S");
                }

                if (rewards != null)
                    battlepass.Rewards.AddRange(rewards);
            }

            if (updates == 0)
                return battlepass;

            int uncoveredUpdates = updates;
            int finalLevels = Battlepass.CalculateLevels(daysFinal);
            IEnumerable<Reward> finalRewards = null;
            string finalRewardName = null;

            // Gnostic 
            if (Settings.Gnostic != 0)
            {
                int totalGnostics = Math.Min(updates, Settings.Gnostic);
                uncoveredUpdates -= totalGnostics;

                if (updates <= Settings.Gnostic && finalLevels < 50)
                {
                    totalGnostics--;
                    if (finalLevels >= 10)
                    {
                        finalRewards = Battlepass.GetPartialRewardsGnostic(finalLevels);
                        finalRewardName = $"LVL{finalLevels}G";
                    }
                }

                if (totalGnostics > 0)
                {
                    rewardNames.Add($"{totalGnostics}G");
                    battlepass.Rewards.AddRange(Battlepass.GetFullRewardsGnostic(totalGnostics));
                }
            }

            // Sojourner
            if (uncoveredUpdates > 0)
            {
                var totalSojourners = uncoveredUpdates;

                if (finalLevels < 50)
                {
                    totalSojourners--;
                    if (finalLevels >= 10)
                    {
                        finalRewards = Battlepass.GetPartialRewardsSojourner(finalLevels);
                        finalRewardName = $"LVL{finalLevels}S";
                    }
                }

                if (totalSojourners > 0)
                {
                    battlepass.Rewards.AddRange(Battlepass.GetFullRewardsSojourner(totalSojourners));
                    rewardNames.Add($"{totalSojourners}S");
                }
            }

            // Partial leftover
            if (finalRewards != null)
            {
                battlepass.Rewards.AddRange(finalRewards);
                rewardNames.Add(finalRewardName);
            }

            battlepass.Name += $" ({string.Join(", ", rewardNames)})";

            return battlepass;
        }

        private RewardTotal GetHoyolab(int monthlyResets)
        {
            var hoyolab = new RewardTotal(GenshinEmotes.Primogem, "Hoyolab Monthly Check-in");
            var days = DateTime.DaysInMonth(StartDate.Year, StartDate.Month);

            if (Settings.Hoyolab < 21)
            {
                var daysLeft = Math.Min(Days, days - StartDate.Day);
                var canClaim = (Settings.Hoyolab + daysLeft) / 7 - Settings.Hoyolab / 7;
                if (canClaim > 0)
                    hoyolab.Rewards.Add(new Reward(Currency.Primogems, 1, canClaim * 20));
            }

            if (monthlyResets > 1)
            {
                hoyolab.Rewards.Add(new Reward(Currency.Primogems, monthlyResets - 1, 60));

                int lastMonthReward = EndDate.Day / 7 * 20;
                if (lastMonthReward >= 60)
                    hoyolab.Rewards[^1].Quantity++;
                else
                    hoyolab.Rewards.Add(new Reward(Currency.Primogems, 1, lastMonthReward));
            }
            else if (StartDate.Month != EndDate.Month)
            {
                var lastMonthReward = Math.Min(3, EndDate.Day / 7) * 20;
                if (lastMonthReward > 0)
                    hoyolab.Rewards.Add(new Reward(Currency.Primogems, 1, lastMonthReward));
            }

            return hoyolab;
        }

        private List<GameVersion> GetVersions(GameVersion current, int count)
        {
            List<GameVersion> versions = new List<GameVersion>();
            int version = 0;

            for (DateTime i = current.Start; version < count; i = Increment(i))
            {
                var next = Increment(i);
                versions.Add(new GameVersion(i, next, $"{current.Major + (current.Minor + version) / 10}.{(current.Minor + version) % 10}"));
                version++;
            }

            return versions;
            static DateTime Increment(DateTime dt) => dt.AddDays(VERSION_DURATION);
        }

        private List<GameVersion> GetVersions(GameVersion current, DateTime end)
        {
            List<GameVersion> versions = new List<GameVersion>();
            int version = 0;

            for (DateTime i = current.Start; i <= end; i = Increment(i))
            {
                var next = Increment(i);
                versions.Add(new GameVersion(i, next, $"{current.Major + (current.Minor + version) / 10}.{(current.Minor + version) % 10}"));
                version++;
            }

            return versions;
            static DateTime Increment(DateTime dt) => dt.AddDays(VERSION_DURATION);
        }

        private int DateCalculator(DateTime start, Func<DateTime, DateTime> incrementFunc)
        {
            int count = 0;
            for (DateTime i = start; i <= EndDate; i = incrementFunc(i))
                if (i > StartDate)
                    count++;

            return count;
        }
    }
}
