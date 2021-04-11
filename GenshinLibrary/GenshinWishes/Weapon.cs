using Newtonsoft.Json;
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
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetWeaponEmote(Type)} **{Name}**";
        }

        public override string GetMultiWishSplashArt()
        {
            return $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Weapons{Path.DirectorySeparatorChar}{Name}.png";
        }

        public override string GetIcon()
        {
            return $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Type}.png";
        }
    }
}
