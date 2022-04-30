using GenshinLibrary.Models;
using GenshinLibrary.Services.GachaSim.Sessions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GenshinLibrary.Services.GachaSim.Banners
{
    public class StandardWish : WishBanner
    {
        public IReadOnlyList<WishItem> FiveStarCharacters { get; }
        public IReadOnlyList<WishItem> FiveStarWeapons { get; }
        public IReadOnlyList<WishItem> FourStarCharacters { get; }
        public IReadOnlyList<WishItem> FourStarWeapons { get; }
        public IReadOnlyList<WishItem> Threestars { get; }

        public StandardWish(bool gachaSimAvailable, int bid, string name, IEnumerable<WishItem> pool, ReadOnlyCollection<double> fivestarChances, ReadOnlyCollection<double> fourstarChances)
            : base(gachaSimAvailable, pool, bid, name, Banner.Standard, fivestarChances, fourstarChances)
        {
            List<WishItem> threestars = new List<WishItem>();
            List<WishItem> fourstarWeapons = new List<WishItem>();
            List<WishItem> fourstarCharacters = new List<WishItem>();
            List<WishItem> fivestarWeapons = new List<WishItem>();
            List<WishItem> fivestarCharacters = new List<WishItem>();

            foreach (var wi in pool)
            {
                switch (wi.Rarity)
                {
                    case 3: threestars.Add(wi); break;
                    case 4:
                        if (wi is Character)
                            fourstarCharacters.Add(wi);
                        else if (wi is Weapon)
                            fourstarWeapons.Add(wi);
                        break;
                    case 5:
                        if (wi is Character)
                            fivestarCharacters.Add(wi);
                        else if (wi is Weapon)
                            fivestarWeapons.Add(wi);
                        break;
                }
            }

            Threestars = threestars.AsReadOnly();
            FourStarCharacters = fourstarCharacters.AsReadOnly();
            FourStarWeapons = fourstarWeapons.AsReadOnly();
            FiveStarCharacters = fivestarCharacters.AsReadOnly();
            FiveStarWeapons = fivestarWeapons.AsReadOnly();
        }

        public override WishSession NewSession()
        {
            return new StandardWishSession(this);
        }
    }
}
