using Newtonsoft.Json;
using System.IO;

namespace GenshinLibrary.Models
{
    public class Weapon : WishItem
    {
        public WeaponType Type { get; }
        public override string IconPath { get; }
        public override string WishArtPath { get; }

        [JsonConstructor]
        public Weapon(int wid, string name, WeaponType type, int rarity, Banner banners) : base(wid, name, rarity, banners)
        {
            Type = type;
            WishArtPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Weapons{Path.DirectorySeparatorChar}{Name}.png";
            IconPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Type}.png";
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetWeaponEmote(Type)} **{Name}**";
        }
    }
}
