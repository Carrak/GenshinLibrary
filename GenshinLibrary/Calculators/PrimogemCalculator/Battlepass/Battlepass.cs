using GenshinLibrary.ReactionCallback.PrimogemCalculator;
using System;
using System.Collections.Generic;

namespace GenshinLibrary.Calculators.PrimogemCalculator
{
    public static class Battlepass
    {
        public static IEnumerable<Reward> GetFullRewardsGnostic(int quantity)
        {
            var gnostics = new List<Reward>
            {
                new Reward(Currency.Primogems, quantity, 680),
                new Reward(Currency.Acquaint, quantity, 5),
                new Reward(Currency.Intertwined, quantity, 4)
            };
            return gnostics;
        }

        public static IEnumerable<Reward> GetFullRewardsSojourner(int quantity)
        {
            var sojourners = new List<Reward>
            {
                new Reward(Currency.Acquaint, quantity, 5)
            };
            return sojourners;
        }

        public static IEnumerable<Reward> GetPartialRewardsGnostic(int levels, int currentLevel = 0)
        {
            var partialGnostic = new List<Reward>();
            int reachedLevel = Math.Min(50, currentLevel + levels);
            int lastIntertwined = Math.Min(40, reachedLevel);

            int unclaimedAcquaints = reachedLevel / 10 - currentLevel / 10;
            int unclaimedIntertwineds = lastIntertwined / 10 - currentLevel / 10;

            if (reachedLevel == 50)
                partialGnostic.Add(new Reward(Currency.Primogems, 1, 680));

            if (unclaimedAcquaints > 0)
                partialGnostic.Add(new Reward(Currency.Acquaint, 1, unclaimedAcquaints));

            if (unclaimedIntertwineds > 0)
                partialGnostic.Add(new Reward(Currency.Intertwined, 1, unclaimedIntertwineds));

            if (partialGnostic.Count == 0)
                return null;

            return partialGnostic;
        }

        public static IEnumerable<Reward> GetPartialRewardsSojourner(int levels, int currentLevel = 0)
        {
            var partialSojourner = new List<Reward>();
            int reachedLevel = Math.Min(50, currentLevel + levels);

            int unclaimedRewards = reachedLevel / 10 - currentLevel / 10;
            if (unclaimedRewards > 0)
                partialSojourner.Add(new Reward(Currency.Acquaint, 1, unclaimedRewards));

            if (partialSojourner.Count == 0)
                return null;

            return partialSojourner;
        }

        public static int CalculateLevels(int days, int currentLevel = 0) => Math.Min(50, currentLevel + (int)((float)days / 28 * 50));
    }
}
