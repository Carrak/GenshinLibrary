using System.Drawing;
using System.IO;

namespace GenshinLibrary.GenshinWishes
{
    public abstract class WishItem
    {
        public int WID { get; }
        public string Name { get; }
        public int Rarity { get; }
        public Banner Banners { get; }

        public Bitmap Icon { get; }
        public Bitmap WishArt { get; }
        public Bitmap RarityImage { get; }

        protected WishItem(int wid, string name, int rarity, Banner banners)
        {
            WID = wid;
            Name = name;
            Rarity = rarity;
            Banners = banners;
            Icon = new Bitmap(GetIcon());
            RarityImage = new Bitmap(GetRarityImage());

            try
            {
                WishArt = new Bitmap(GetMultiWishSplashArt());
            } 
            catch (FileNotFoundException)
            {
            }
        }

        public string GetFormattedName(int allowedLength)
        {
            string format = Rarity switch
            {
                3 => "[{0}]",
                4 => "\"{0}\"",
                5 => "\"[{0}]\"",
                _ => "{0}"
            };

            string toFill = "...";
            int max = allowedLength - (format.Length - 3);
            string name;

            if (Name.Length > max)
                name = Name.Substring(0, max - toFill.Length) + toFill;
            else
                name = Name;

            return string.Format(format, name);
        }

        private string GetRarityImage() => $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Rarity{Path.DirectorySeparatorChar}{Rarity}.png";

        public abstract string GetNameWithEmotes();

        protected abstract string GetMultiWishSplashArt();

        protected abstract string GetIcon();
    }
}
