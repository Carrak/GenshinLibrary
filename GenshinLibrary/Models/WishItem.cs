using System.IO;

namespace GenshinLibrary.GenshinWishes
{
    public abstract class WishItem
    {
        public int WID { get; }
        public string Name { get; }
        public int Rarity { get; }
        public Banner Banners { get; }

        public abstract string IconPath { get; }
        public abstract string WishArtPath { get; }
        public string RarityImagePath { get; }

        protected WishItem(int wid, string name, int rarity, Banner banners)
        {
            WID = wid;
            Name = name;
            Rarity = rarity;
            Banners = banners;
            RarityImagePath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Rarity{Path.DirectorySeparatorChar}{Rarity}.png";
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

        public abstract string GetNameWithEmotes();
    }
}
