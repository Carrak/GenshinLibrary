using GenshinLibrary.Models;
using GenshinLibrary.Services.GachaSim.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.Services.GachaSim
{
    public class EventWish : WishBanner
    {
        public IReadOnlyList<WishItem> RateUpFivestars { get; }
        public IReadOnlyList<WishItem> RateUpFourstars { get; }
        public IReadOnlyList<WishItem> StandardFourstarWeapons { get; }
        public IReadOnlyList<WishItem> StandardFourstarCharacters { get; }
        public IReadOnlyList<WishItem> StandardFivestars { get; }
        public IReadOnlyList<WishItem> StandardThreestars { get; }

        public DateTime Date { get; }
        public float RateUpChance { get; }

        public EventWish(int bid, string name, DateTime date, float rateUpChance, Banner bannerType, IEnumerable<WishItem> rateUpPool, IEnumerable<WishItem> standardPool, float fiveStarChance, float fourStarChance, int pity)
            : base(rateUpPool.Concat(standardPool), bid, name, bannerType, fiveStarChance, fourStarChance, pity)
        {
            Date = date;
            RateUpChance = rateUpChance;

            List<WishItem> rateUpFivestars = new List<WishItem>();
            List<WishItem> rateUpFourstars = new List<WishItem>();
            List<WishItem> standardFivestars = new List<WishItem>();
            List<WishItem> standardFourstarWeapons = new List<WishItem>();
            List<WishItem> standardFourstarCharacters = new List<WishItem>();
            List<WishItem> standardThreestars = new List<WishItem>();

            foreach (var wi in standardPool)
                switch (wi.Rarity)
                {
                    case 3: standardThreestars.Add(wi); break;
                    case 4 when wi is Weapon: standardFourstarWeapons.Add(wi); break;
                    case 4 when wi is Character: standardFourstarCharacters.Add(wi); break;
                    case 5: standardFivestars.Add(wi); break;
                }

            foreach (var wi in rateUpPool)
                switch (wi.Rarity)
                {
                    case 4: rateUpFourstars.Add(wi); standardFourstarWeapons.Remove(wi); standardFourstarCharacters.Remove(wi); break;
                    case 5: rateUpFivestars.Add(wi); standardFivestars.Remove(wi); break;
                }

            RateUpFivestars = rateUpFivestars.AsReadOnly();
            RateUpFourstars = rateUpFourstars.AsReadOnly();
            StandardFivestars = standardFivestars.AsReadOnly();
            StandardFourstarCharacters = standardFourstarCharacters.AsReadOnly();
            StandardFourstarWeapons = standardFourstarWeapons.AsReadOnly();
            StandardThreestars = standardThreestars.AsReadOnly();
        }

        public override WishSession NewSession()
        {
            return new EventWishSession(this);
        }

        public override string GetFullName() => $"{Name}/{Date:dd.MM.yyyy}";
    }
}
