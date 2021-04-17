using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using System;

namespace GenshinLibrary.GenshinWishes
{
    public class Weapon : WishItem
    {
        public WeaponType Type { get; }

        [JsonConstructor]
        public Weapon(int wid, string name, WeaponType type, int rarity, Banner banners) : base(wid, name, rarity, banners)
        {
            Type = type;
            try
            {
                WishArt = new Bitmap($"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Weapons{Path.DirectorySeparatorChar}{Name}.png");
            }
            catch
            {
                Console.WriteLine($"Image missing/no access to WishArt for {Name}");
            }

            try
            {
                Icon = new Bitmap($"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Type}.png");
            }
            catch
            {
                Console.WriteLine($"Image missing/no access to Icon for {Name}");
            }
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetWeaponEmote(Type)} **{Name}**";
        }
    }
}
