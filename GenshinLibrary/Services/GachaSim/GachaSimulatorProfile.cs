using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Services.GachaSim.Sessions;
using System;
using System.Collections.Generic;

namespace GenshinLibrary.Services.GachaSim
{
    public class GachaSimulatorProfile
    {
        public List<WishItem> Inventory { get; } = new List<WishItem>();
        private Dictionary<int, WishSession> sessions { get; } = new Dictionary<int, WishSession>();
        private int SelectedBannerBID { get; set; }

        public GachaSimulatorProfile(WishBanner selectedBanner)
        {
            SelectedBannerBID = selectedBanner.BID;
            sessions[selectedBanner.BID] = selectedBanner.NewSession();
        }

        public void ChangeBanner(WishBanner banner)
        {
            SelectedBannerBID = banner.BID;
            sessions[banner.BID] = banner.NewSession();
        }

        public WishSession GetCurrentSession()
        {
            return sessions[SelectedBannerBID];
        }

        public WishItem[] Wish(int count)
        {
            WishItem[] wishes = sessions[SelectedBannerBID].Wish(count);
            Inventory.AddRange(wishes);
            return wishes;
        }
    }
}
