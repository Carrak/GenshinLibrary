using GenshinLibrary.Models;
using GenshinLibrary.Services.GachaSim.Sessions;
using System.Collections.Generic;

namespace GenshinLibrary.Services.GachaSim
{
    public class GachaSimulatorProfile
    {
        public List<GachaSimWishItemRecord> Inventory { get; } = new List<GachaSimWishItemRecord>();
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

        public void Reset()
        {
            var currentSession = GetCurrentSession();
            currentSession = currentSession.Banner.NewSession();

            Inventory.Clear();
            sessions.Clear();

            sessions[SelectedBannerBID] = currentSession;
        }

        public WishSession GetCurrentSession()
        {
            return sessions[SelectedBannerBID];
        }

        public GachaSimWishItemRecord[] Wish(int count)
        {
            GachaSimWishItemRecord[] wishes = sessions[SelectedBannerBID].Wish(count);
            Inventory.AddRange(wishes);
            return wishes;
        }
    }
}
