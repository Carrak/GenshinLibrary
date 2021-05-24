using GenshinLibrary.GenshinWishes;
using System;

namespace GenshinLibrary.Services.GachaSim.Sessions
{
    public abstract class WishSession
    {
        public WishBanner Banner;
        protected static Random Random = new Random();

        protected WishSession(WishBanner banner)
        {
            Banner = banner;
        }

        public int CurrentFourStarPity { get; protected set; } = 0;
        public int CurrentFiveStarPity { get; protected set; } = 0;

        protected abstract WishItem GetWishItem();

        public virtual WishItem[] Wish(int count)
        {
            if (count > 100)
                throw new Exception("Cannot wish more than 100 times per once.");
            if (count < 1)
                throw new Exception("Cannot wish 0 or less times.");

            WishItem[] items = new WishItem[count];
            for (int i = 0; i < count; i++)
                items[i] = GetWishItem();
            return items;
        }

        protected bool RollFivestar(Random random)
        {
            var roll = random.NextDouble();

            var result = CurrentFiveStarPity >= Banner.FiveStarHardPity ||
                   CurrentFiveStarPity > Banner.FiveStarSoftPity && roll < Banner.SoftPityChance ||
                   roll < Banner.FiveStarChance;

            if (result)
                CurrentFiveStarPity = 0;

            return result;
        }

        protected bool RollFourstar(Random random)
        {
            var roll = random.NextDouble();

            var result = CurrentFourStarPity >= Banner.FourStarHardPity ||
                   CurrentFourStarPity > Banner.FourStarSoftPity && roll < Banner.SoftPityChance ||
                   roll < Banner.FourStarChance;

            if (result)
                CurrentFourStarPity = 0;

            return result;
        }
    }
}
