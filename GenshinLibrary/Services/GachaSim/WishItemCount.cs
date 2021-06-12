using GenshinLibrary.Models;

namespace GenshinLibrary.Services.GachaSim
{
    class WishItemCount
    {
        public WishItemCount(WishItem wishItem, int count)
        {
            WishItem = wishItem;
            Count = count;
        }

        public WishItem WishItem { get; }
        public int Count { get; set; }
    }
}
