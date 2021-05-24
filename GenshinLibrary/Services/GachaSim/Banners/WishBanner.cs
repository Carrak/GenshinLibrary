using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Services.GachaSim.Sessions;
using System.Collections.Generic;

namespace GenshinLibrary.Services.GachaSim
{
    public abstract class WishBanner
    {
        public int BID { get; }
        public string Name { get; }
        public Banner BannerType { get; }

        public float SoftPityChance = 0.33f;
        public float FiveStarChance { get; }
        public float FourStarChance { get; }

        public int FiveStarHardPity { get; }
        public int FourStarHardPity { get; } = 10;
        public int FiveStarSoftPity => FiveStarHardPity - 15;
        public int FourStarSoftPity => FourStarHardPity - 3;

        public IEnumerable<WishItem> Pool { get; }

        protected WishBanner(IEnumerable<WishItem> pool, int bid, string name, Banner bannerType, float fiveStarChance, float fourStarChance, int fiveStarHardPity)
        {
            Pool = pool;
            BID = bid;
            Name = name;
            BannerType = bannerType;
            FiveStarChance = fiveStarChance;
            FourStarChance = fourStarChance;
            FiveStarHardPity = fiveStarHardPity;
        }

        public virtual string GetFullName() => Name;

        public abstract WishSession NewSession();
    }
}
