using GenshinLibrary.Models;
using GenshinLibrary.Utility;
using System;

namespace GenshinLibrary.Services.GachaSim.Sessions
{
    class EventWishSession : WishSession
    {
        private bool FiveStarRateUpGuarantee { get; set; } = false;
        private bool FourStarRateUpGuarantee { get; set; } = false;
        private EventWish EventWish => Banner as EventWish;

        public EventWishSession(EventWish eventWish) : base(eventWish)
        {
        }

        protected override WishItem GetWishItem()
        {
            return GetObtainedRarity() switch
            {
                5 => GetFivestar(),
                4 => GetFourstar(),
                3 => EventWish.StandardThreestars.RandomElement(),
                _ => throw new Exception("Unknown rarity.")
            };
        }

        private WishItem GetFivestar()
        {
            if (FiveStarRateUpGuarantee)
            {
                FiveStarRateUpGuarantee = false;
                return EventWish.RateUpFivestars.RandomElement();
            }

            if (Globals.Random.NextDouble() >= EventWish.RateUpChance)
            {
                FiveStarRateUpGuarantee = true;
                return EventWish.StandardFivestars.RandomElement();
            }
            else
                return EventWish.RateUpFivestars.RandomElement();
        }

        private WishItem GetFourstar()
        {
            if (FourStarRateUpGuarantee)
            {
                FourStarRateUpGuarantee = false;
                return EventWish.RateUpFourstars.RandomElement();
            }

            if (Globals.Random.NextDouble() >= EventWish.RateUpChance)
            {
                FourStarRateUpGuarantee = true;
                if (Globals.Random.NextDouble() < 0.5)
                    return EventWish.StandardFourstarCharacters.RandomElement();
                else
                    return EventWish.StandardFourstarWeapons.RandomElement();
            }
            else
                return EventWish.RateUpFourstars.RandomElement();
        }
    }
}
