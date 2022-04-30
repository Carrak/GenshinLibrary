using GenshinLibrary.Models;
using GenshinLibrary.Services.GachaSim.Banners;
using GenshinLibrary.Utility;
using System;

namespace GenshinLibrary.Services.GachaSim.Sessions
{
    class BeginnerWishSession : WishSession
    {
        public BeginnerWish BeginnerWish => Banner as BeginnerWish;

        private bool ObtainedNoelle = false;

        public BeginnerWishSession(BeginnerWish banner) : base(banner)
        {
        }

        protected override WishItem GetWishItem()
        {
            switch (GetObtainedRarity())
            {
                case 5: return BeginnerWish.FiveStarCharacters.RandomElement();
                case 4:
                    if (!ObtainedNoelle)
                    {
                        ObtainedNoelle = true;
                        return BeginnerWish.Noelle;
                    }
                    return BeginnerWish.FourStarCharacters.RandomElement();
                case 3: return BeginnerWish.Threestars.RandomElement();
                default: throw new Exception("Unknown rarity.");
            }
        }
    }
}
