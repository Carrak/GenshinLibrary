using GenshinLibrary.Models;

namespace GenshinLibrary.Services.GachaSim
{
    public class GachaSimWishItemRecord
    {
        public WishItem WishItem { get; }
        public int FiveStarPity { get; }
        public int FourStarPity { get; }

        public GachaSimWishItemRecord(int fivestarPity, int fourstarPity, WishItem wishItem)
        {
            FiveStarPity = fivestarPity;
            FourStarPity = fourstarPity;
            WishItem = wishItem;
        }
    }
}
