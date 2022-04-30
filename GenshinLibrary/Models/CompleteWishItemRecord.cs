using System;

namespace GenshinLibrary.Models
{
    public class CompleteWishItemRecord : WishItemRecord
    {
        public int PityFive { get; }
        public int PityFour { get; }
        public int WishID { get; }

        public CompleteWishItemRecord(DateTime dateTime, WishItem wishItem, int pityFive, int pityFour, int wishid, Banner banner) : base(dateTime, wishItem, banner)
        {
            PityFive = pityFive;
            PityFour = pityFour;
            WishID = wishid;
        }
    }
}
