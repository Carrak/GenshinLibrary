using GenshinLibrary.Models;
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
            switch (GetObtainedRarity())
            {
                case 5:
                    if (Globals.Random.NextDouble() < 0.5f)
                        return StandardWish.FiveStarCharacters.RandomElement();
                    else
                        return StandardWish.FiveStarWeapons.RandomElement();
                case 4:
                    if (Globals.Random.NextDouble() < 0.5f)
                        return StandardWish.FourStarCharacters.RandomElement();
                    else
                        return StandardWish.FourStarWeapons.RandomElement();
                case 3:
                    return StandardWish.Threestars.RandomElement();
                default:
                    throw new Exception("Unknown rarity.");

            }
        }
    }
}
