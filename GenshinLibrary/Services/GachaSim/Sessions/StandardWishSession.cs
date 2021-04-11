using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Services.GachaSim.Banners;
using GenshinLibrary.Utility;
using System;

namespace GenshinLibrary.Services.GachaSim.Sessions
{
    class StandardWishSession : WishSession
    {
        public StandardWish StandardWish => Banner as StandardWish;

        public StandardWishSession(StandardWish standardWish) : base(standardWish)
        {
        }

        protected override WishItem GetWishItem()
        {
            CurrentFiveStarPity++;
            CurrentFourStarPity++;

            if (RollFivestar(Random))
            {
                if (Random.NextDouble() < 0.5f)
                    return StandardWish.FiveStarCharacters.RandomElement(Random);
                else
                    return StandardWish.FiveStarWeapons.RandomElement(Random);
            }

            if (RollFourstar(Random))
            {
                if (Random.NextDouble() < 0.5f)
                    return StandardWish.FourStarCharacters.RandomElement(Random);
                else
                    return StandardWish.FourStarWeapons.RandomElement(Random);
            }

            return StandardWish.Threestars.RandomElement(Random);
        }
    }
}
