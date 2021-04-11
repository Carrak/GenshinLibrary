using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Services.GachaSim.Banners;
using GenshinLibrary.Utility;
using System;

namespace GenshinLibrary.Services.GachaSim.Sessions
{
    class BeginnerWishSession : WishSession
    {
        public BeginnerWish BeginnerWish => Banner as BeginnerWish;

        private int Counter = 0;
        private bool ObtainedNoelle = false;

        public BeginnerWishSession(BeginnerWish banner) : base(banner)
        {
        }

        public override WishItem[] Wish(int count)
        {
            if (count != 10)
                throw new Exception("Can only do 10-pulls on Beginne.");

            if (Counter == 20)
                throw new Exception("Already wished 20 times.");

            Counter += 10;
            return base.Wish(count);
        }

        protected override WishItem GetWishItem()
        {
            CurrentFiveStarPity++;
            CurrentFourStarPity++;

            if (RollFivestar(Random))
                return BeginnerWish.FiveStarCharacters.RandomElement(Random);

            if (RollFourstar(Random))
            {
                if (!ObtainedNoelle)
                {
                    ObtainedNoelle = true;
                    return BeginnerWish.Noelle;
                }

                return BeginnerWish.FourStarCharacters.RandomElement(Random);
            }

            return BeginnerWish.Threestars.RandomElement(Random);
        }
    }
}
