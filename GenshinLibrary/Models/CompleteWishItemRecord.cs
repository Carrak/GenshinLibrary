using System;

namespace GenshinLibrary.Models
{
    public class CompleteWishItemRecord : WishItemRecord
    {
        public int Pity { get; }
        public int WishID { get; }

        public CompleteWishItemRecord(DateTime dateTime, WishItem wishItem, int pity, int wishid, Banner banner) : base(dateTime, wishItem, banner)
        {
            Pity = pity;
            WishID = wishid;
        }
    }
}
