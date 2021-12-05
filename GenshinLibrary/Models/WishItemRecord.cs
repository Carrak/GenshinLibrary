using System;

namespace GenshinLibrary.Models
{
    public class WishItemRecord
    {
        public DateTime DateTime { get; }
        public WishItem WishItem { get; }
        public Banner Banner { get; }

        public WishItemRecord(DateTime dateTime, WishItem wishItem, Banner banner)
        {
            DateTime = dateTime;
            WishItem = wishItem;
            Banner = banner;
        }

        public string GetShortBannerString()
        {
            return Banner switch
            {
                Banner.Character1 => "C1",
                Banner.Character2 => "C2",
                Banner.Weapon => "W",
                Banner.Standard => "S",
                Banner.Beginner => "B",
                _ => throw new Exception("Banner type is not supported.")
            };
        }
    }
}
