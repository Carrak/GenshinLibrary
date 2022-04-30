using GenshinLibrary.Models;
using GenshinLibrary.Services.GachaSim.Sessions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GenshinLibrary.Services.GachaSim.Banners
{
    class BeginnerWish : WishBanner
    {
        public IReadOnlyList<WishItem> FiveStarCharacters { get; }
        public IReadOnlyList<WishItem> FourStarCharacters { get; }
        public IReadOnlyList<WishItem> Threestars { get; }

        public WishItem Noelle { get; }

        public BeginnerWish(bool gachaSimAvailable, int bid, string name, IEnumerable<WishItem> pool, WishItem noelle, ReadOnlyCollection<double> fivestarChances, ReadOnlyCollection<double> fourstarChances)
            : base(gachaSimAvailable, pool, bid, name, Banner.Beginner, fivestarChances, fourstarChances)
        {
            Noelle = noelle;

            List<WishItem> fivestars = new List<WishItem>();
            List<WishItem> fourstars = new List<WishItem>();
            List<WishItem> threestars = new List<WishItem>();

            foreach (var wi in pool)
            {
                switch (wi.Rarity)
                {
                    case 3: threestars.Add(wi); break;
                    case 4: fourstars.Add(wi); break;
                    case 5: fivestars.Add(wi); break;
                }
            }

            Threestars = threestars.AsReadOnly();
            FourStarCharacters = fourstars.AsReadOnly();
            FiveStarCharacters = fivestars.AsReadOnly();
        }

        public override WishSession NewSession()
        {
            return new BeginnerWishSession(this);
        }
    }
}
