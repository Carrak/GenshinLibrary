using GenshinLibrary.Models;
using System;

namespace GenshinLibrary.Services.GachaSim
{
    public class GachaSimWishItemRecord : WishItemRecord
    {
        public int FiveStarPity { get; }
        public int FourStarPity { get; }

        public GachaSimWishItemRecord(int fivestarPity, int fourstarPity, DateTime dateTime, WishItem wishItem) : base(dateTime, wishItem)
        {
            FiveStarPity = fivestarPity;
            FourStarPity = fourstarPity;
        }
    }
}
