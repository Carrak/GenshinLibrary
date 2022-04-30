using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace GenshinLibrary.Models
{
    public class Weapon : WishItem
    {
        public WeaponType Type { get; }
        public string WeaponTypeIconPath { get; }
        public string WeaponIconPath { get; }

        public override string WishArtPath { get; }

        [JsonConstructor]
        public Weapon(int wid, string name, WeaponType type, int rarity, Banner banners, IEnumerable<string> aliases) : base(wid, name, rarity, banners, aliases)
        {
            Type = type;
            WishArtPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}WishArtworks{Path.DirectorySeparatorChar}{Name}.png";
            WeaponTypeIconPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Type}.png";
            WeaponIconPath = $"{Globals.ProjectDirectory}Weapons{Path.DirectorySeparatorChar}{Name}.png";
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetWeaponEmote(Type)} **{Name}**";
        }
    }
}
