﻿using Discord;
using GenshinLibrary.GenshinWishes;
using GenshinLibrary.ReactionCallback.PrimogemCalculator;
using GenshinLibrary.Utility;
using System;
using System.Collections.Generic;

namespace GenshinLibrary.Calculators.PrimogemCalculator
{
    class PrimogemCalculator
    {
        public static readonly DateTime LaunchDay = new DateTime(2020, 9, 30);
        public static readonly int UpdateDuration = 42;
        public static readonly int FatePrice = 160;
        public static readonly int EventAverage = 2200;

        private int Days { get; }

        private DateTime StartDate { get; }
        private DateTime EndDate { get; }
        private PrimogemCalculatorSettings Settings { get; }

        private List<Version> Versions { get; } = new List<Version>();

        public PrimogemCalculator(DateTime start, DateTime end, PrimogemCalculatorSettings settings = null)
        {
            Settings = settings ?? new PrimogemCalculatorSettings();
            StartDate = start;
            EndDate = end;
            Days = (end - start).Days;
            Versions = GetVersions(start, end);
        }

        public Embed ConstructEmbed()
        {
            var totals = GetTotals();

            var lastVersion = Versions[^1];

            string footer = "S - Sojourner, G - Gnostic.";
            if (Settings.Events)
                footer += "\nRewards for events are average per update.";

            var embed = new EmbedBuilder();
            embed.WithColor(Globals.MainColor)
                .WithTitle($"Primogem calculator")
                .WithDescription(
                $"Period: **{StartDate:dd.MM.yyyy}** - **{EndDate:dd.MM.yyyy}** (**{Days}** days)\n" +
                $"Version: {lastVersion.Info()}\n" +
                $"Banner: {GetCurrentBanner()}"
                )
                .WithFooter(footer);

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

            int totalRolls = totalGems / FatePrice + totalAcquaint + totalIntertwined;
            int fourStars = (int)(totalRolls * 0.13);
            int starglitter = fourStars * 2;

            int extraRolls = totalGems / FatePrice;
            int extraGems = totalGems % FatePrice;

            embed.AddField($"= Grand total =",
                $"**{totalGems}**{GenshinEmotes.Primogem} " +
                $"(**{extraRolls}** {GenshinEmotes.Intertwined}/{GenshinEmotes.Acquaint} and **{extraGems}**{GenshinEmotes.Primogem})\n" +
                $"**{totalIntertwined}**{GenshinEmotes.Intertwined} (**{totalIntertwined + extraRolls}**{GenshinEmotes.Intertwined} w/ the primogems)\n" +
                $"**{totalAcquaint}**{GenshinEmotes.Acquaint} (**{totalAcquaint + extraRolls}**{GenshinEmotes.Acquaint} w/ the primogems)\n" +
                $"**{starglitter}**{GenshinEmotes.Starglitter} (avg)");

            return embed.Build();
        }

        private string GetCurrentBanner()
        {
            int bannersPerUpdate = 2;
            var version = Versions[^1];
            int banner = (int)((EndDate - version.Start) / (version.End - version.Start) * bannersPerUpdate);
            string bannerString = banner switch
            {
                0 => "1st",
                1 => "2nd",
                _ => throw new NotImplementedException()
            };

            return $"**{bannerString}** // since **{version.Start + TimeSpan.FromDays(UpdateDuration / bannersPerUpdate) * banner:dd.MM.yyyy}**";
        }

        private IEnumerable<GemTotal> GetTotals()
        {
            List<GemTotal> totals = new List<GemTotal>();

            // Current
            var current = new GemTotal(GenshinEmotes.Primogem, "Current");
            if (Settings.Primogems != 0)
                current.Rewards.Add(new Reward(Currency.Primogems, 1, Settings.Primogems));
            if (Settings.Acquaint != 0)
                current.Rewards.Add(new Reward(Currency.Acquaint, 1, Settings.Acquaint));
            if (Settings.Intertwined != 0)
                current.Rewards.Add(new Reward(Currency.Intertwined, 1, Settings.Intertwined));

            if (current.Rewards.Count != 0)
                totals.Add(current);

            // Dailies
            totals.Add(new GemTotal(GenshinEmotes.Daily, "Daily quests", new Reward(Currency.Primogems, Days, 60)));

            // Welkin
            if (Settings.Welkin != 0)
                totals.Add(new GemTotal(GenshinEmotes.Welkin, "Welkin", new Reward(Currency.Primogems, Math.Min(Days, Settings.Welkin), 90)));

            // Abyss
            if (Settings.Abyss != 0)
            {
                int abyssPeriods = DateCalculator(StartDate.DateTruncate(TimePartition.Month), i => i + (i.AddMonths(1) - i) / 2);
                if (abyssPeriods != 0)
                    totals.Add(new GemTotal(GenshinEmotes.Abyss, "Abyss", new Reward(Currency.Primogems, abyssPeriods, Settings.Abyss)));
            }

            // Stardust shop
            int shop = DateCalculator(StartDate.DateTruncate(TimePartition.Month), i => i.AddMonths(1));
            if (shop != 0)
                totals.Add(new GemTotal(GenshinEmotes.Stardust, "Stardust shop", new Reward(Currency.Acquaint, shop, 5), new Reward(Currency.Intertwined, shop, 5)));

            int codes = DateCalculator(LaunchDay - TimeSpan.FromDays(10), i => i.AddDays(UpdateDuration));
            int reachedUpdateDays = Math.Min((EndDate - Versions[^1].Start).Days + 1, Days);
            int currentUpdateDays = Math.Min((Versions[0].End - StartDate).Days, Days);

            // Battlepass
            if (GetBattlepassTotal(reachedUpdateDays, currentUpdateDays) is GemTotal battlepass && battlepass.Rewards.Count != 0)
                totals.Add(battlepass);

            // Updates
            if (GetUpdatesTotal(Versions.Count - 1, codes) is GemTotal updates && updates.Rewards.Count != 0)
                totals.Add(updates);

            // Events
            if (Settings.Events && GetEvents(currentUpdateDays, reachedUpdateDays) is GemTotal events)
                totals.Add(events);

            // Test runs
            int testRuns = DateCalculator(LaunchDay, i => i.AddDays(UpdateDuration / 2));
            if (testRuns != 0)
                totals.Add(new GemTotal(GenshinEmotes.Primogem, "Test Runs", new Reward(Currency.Primogems, testRuns, 20)));

            return totals;
        }

        private GemTotal GetEvents(int currentUpdateDays, int reachedUpdateDays)
        {
            List<string> updateNames = new List<string>();
            var events = new GemTotal(GenshinEmotes.Events, "Events");

            if (currentUpdateDays > 0)
            {
                events.Rewards.Add(new Reward(Currency.Primogems, 1, EventAverage * currentUpdateDays / UpdateDuration));
                updateNames.Add($"{Versions[0]}");
            }

            if (Versions.Count > 2)
            {
                events.Rewards.Add(new Reward(Currency.Primogems, Versions.Count - 2, EventAverage));
                if (Versions.Count > 3)
                    updateNames.Add($"{Versions[1]}-{Versions[^2]}");
                else
                    updateNames.Add($"{Versions[1]}");
            }

            if (Versions.Count > 1)
            {
                events.Rewards.Add(new Reward(Currency.Primogems, 1, EventAverage * reachedUpdateDays / UpdateDuration));
                updateNames.Add($"{Versions[^1]}");
            }

            events.Name += $" ({string.Join(", ", updateNames)})";
            return events;
        }

        private GemTotal GetUpdatesTotal(int updates, int codes)
        {
            List<string> names = new List<string>();
            var totals = new GemTotal(GenshinEmotes.Primogem, "Updates");

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

        private GemTotal GetBattlepassTotal(int daysFinal, int daysCurrent)
        {
            List<string> rewardNames = new List<string>();
            var battlepass = new GemTotal(GenshinEmotes.Sojourner, "Battlepass");
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

        private List<Version> GetVersions(DateTime start, DateTime end)
        {
            List<Version> versions = new List<Version>();
            int count = 0;
            for (DateTime i = LaunchDay; i <= end; i = Increment(i))
            {
                var next = Increment(i);
                if (next >= start)
                    versions.Add(new Version(i, next, $"{1 + count / 10}.{count % 10}"));
                count++;
            }

            return versions;
            static DateTime Increment(DateTime dt) => dt.AddDays(UpdateDuration);
        }

        private int DateCalculator(DateTime start, Func<DateTime, DateTime> incrementFunc)
        {
            int count = 0;
            for (DateTime i = start; i <= EndDate; i = incrementFunc(i))
                if (i > StartDate && i <= EndDate)
                    count++;

            return count;
        }
    }
}
