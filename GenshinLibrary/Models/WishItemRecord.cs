using System;

namespace GenshinLibrary.Models
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
