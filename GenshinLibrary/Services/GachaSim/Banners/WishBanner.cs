using GenshinLibrary.Models;
using GenshinLibrary.Services.GachaSim.Sessions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GenshinLibrary.Services.GachaSim
{
    public abstract class WishBanner
    {
        public bool GachaSimAvailable { get; }
        public int BID { get; }
        public string Name { get; }
        public Banner BannerType { get; }
        public ReadOnlyCollection<double> FivestarChances { get; }
        public ReadOnlyCollection<double> FourstarChances { get; }

        public IEnumerable<WishItem> Pool { get; }

        protected WishBanner(bool gachaSimAvailable, IEnumerable<WishItem> pool, int bid, string name, Banner bannerType, ReadOnlyCollection<double> fivestarChances, ReadOnlyCollection<double> fourstarChances)
        {
            GachaSimAvailable = gachaSimAvailable;
            Pool = pool;
            BID = bid;
            Name = name;
            BannerType = bannerType;
            FivestarChances = fivestarChances;
            FourstarChances = fourstarChances;
        }

        public virtual string GetFullName() => Name;

        public abstract WishSession NewSession();
    }
}
