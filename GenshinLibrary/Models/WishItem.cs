using System.Collections.Generic;
using System.IO;

namespace GenshinLibrary.Models
{
    public abstract class WishItem
    {
        public int WID { get; }
        public string Name { get; }
        public int Rarity { get; }
        public Banner Banners { get; }
        public IEnumerable<string> Aliases { get; }

        public abstract string WishArtPath { get; }
        public string RarityImagePath { get; }

        protected WishItem(int wid, string name, int rarity, Banner banners, IEnumerable<string> aliases)
        {
            WID = wid;
            Name = name;
            Rarity = rarity;
            Banners = banners;
            Aliases = aliases;
            RarityImagePath = $"{Globals.ProjectDirectory}Sprites{Path.DirectorySeparatorChar}GachaSim{Path.DirectorySeparatorChar}Rarity{Path.DirectorySeparatorChar}{Rarity}.png";
        }

        public abstract string GetNameWithEmotes();
    }
}
