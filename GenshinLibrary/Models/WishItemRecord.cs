using System;

namespace GenshinLibrary.GenshinWishes
{
    public class WishItemRecord
    {
        public DateTime DateTime { get; }
        public WishItem WishItem { get; }

        public WishItemRecord(DateTime dateTime, WishItem wishItem)
        {
            DateTime = dateTime;
            WishItem = wishItem;
        }
    }
}
