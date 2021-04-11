using System;

namespace GenshinLibrary.GenshinWishes
{
    public class CompleteWishItemRecord : WishItemRecord
    {
        public int Pity { get; }
        public int WishID { get; }

        public CompleteWishItemRecord(DateTime dateTime, WishItem wishItem, int pity, int wishid) : base(dateTime, wishItem)
        {
            Pity = pity;
            WishID = wishid;
        }
    }
}
