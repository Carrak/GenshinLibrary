using Newtonsoft.Json;
using System.Drawing;
using System.IO;

namespace GenshinLibrary.GenshinWishes
{
    public class Weapon : WishItem
    {
        public WeaponType Type { get; }

        [JsonConstructor]
        public Weapon(int wid, string name, WeaponType type, int rarity, Banner banners) : base(wid, name, rarity, banners)
        {
            Type = type;
            WishArt = new Bitmap($"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Weapons{Path.DirectorySeparatorChar}{Name}.png");
            Icon = new Bitmap($"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Type}.png");
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetWeaponEmote(Type)} **{Name}**";
        }
    }
}
