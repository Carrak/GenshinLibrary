using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Utility;
using System;

namespace GenshinLibrary.Services.GachaSim.Sessions
{
    class EventWishSession : WishSession
    {
        public EventWish EventWish => Banner as EventWish;

        private bool FiveStarRateUpGuarantee { get; set; } = false;
        private bool FourStarRateUpGuarantee { get; set; } = false;

        public EventWishSession(EventWish eventWish) : base(eventWish)
        {
        }

        protected override WishItem GetWishItem()
        {
            CurrentFourStarPity++;
            CurrentFiveStarPity++;

            if (RollFivestar(Random))
            {
                if (FiveStarRateUpGuarantee)
                {
                    FiveStarRateUpGuarantee = false;
                    return EventWish.RateUpFivestars.RandomElement();
                }

                if (Random.NextDouble() < EventWish.RateUpChance)
                {
                    FiveStarRateUpGuarantee = true;
                    return EventWish.StandardFivestars.RandomElement();
                }
                else
                    return EventWish.RateUpFivestars.RandomElement();
            }

            if (RollFourstar(Random))
            {
                if (FourStarRateUpGuarantee)
                {
                    FourStarRateUpGuarantee = false;
                    return EventWish.RateUpFourstars.RandomElement();
                }

                if (Random.NextDouble() < EventWish.RateUpChance)
                {
                    FourStarRateUpGuarantee = true;
                    if (Random.NextDouble() < 0.5)
                        return EventWish.StandardFourstarCharacters.RandomElement();
                    else
                        return EventWish.StandardFourstarWeapons.RandomElement();
                }
                else
                    return EventWish.RateUpFourstars.RandomElement();
            }

            return EventWish.StandardThreestars.RandomElement();
        }
    }
}
