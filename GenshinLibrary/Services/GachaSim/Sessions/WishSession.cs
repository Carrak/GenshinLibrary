using GenshinLibrary.Models;
using System;

namespace GenshinLibrary.Services.GachaSim.Sessions
{
    public abstract class WishSession
    {
        public int CurrentFourStarPity { get; set; }
        public int CurrentFiveStarPity { get; set; }
        public WishBanner Banner { get; }

        protected WishSession(WishBanner banner)
        {
            Banner = banner;
        }

        protected abstract WishItem GetWishItem();

        public GachaSimWishItemRecord[] Wish(int count)
        {
            GachaSimWishItemRecord[] items = new GachaSimWishItemRecord[count];
            for (int i = 0; i < count; i++)
            {
                var item = GetWishItem();
                items[i] = new GachaSimWishItemRecord(CurrentFiveStarPity, CurrentFourStarPity, item);

                CurrentFiveStarPity = item.Rarity == 5 ? 0 : CurrentFiveStarPity + 1;
                CurrentFourStarPity = item.Rarity == 4 ? 0 : CurrentFourStarPity + 1;
            }

            return items;
        }

        protected int GetObtainedRarity()
        {
            ChanceSelectionItem<int> fivestar = new ChanceSelectionItem<int>(Banner.FivestarChances[CurrentFiveStarPity], 5);
            ChanceSelectionItem<int> fourstar = new ChanceSelectionItem<int>(Banner.FourstarChances[Math.Min(CurrentFourStarPity, Banner.FourstarChances.Count - 1)], 4);
            ChanceSelectionItem<int> threestar = new ChanceSelectionItem<int>(1, 3);
            ChanceSelection<int> selection = new ChanceSelection<int>(fivestar, fourstar, threestar);
            var value = selection.GetValue(Globals.Random.NextDouble());

            return value;
        }
    }
}
