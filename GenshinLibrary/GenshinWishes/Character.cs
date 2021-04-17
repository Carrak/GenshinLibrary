using Discord;
using Newtonsoft.Json;
using System.IO;

namespace GenshinLibrary.GenshinWishes
{
    public class Character : WishItem
    {
        public Element Vision { get; }
        public WeaponType WieldedWeapon { get; }

        [JsonConstructor]
        public Character(int wid, string name, Element vision, WeaponType weapon, int rarity, Banner banners) : base(wid, name, rarity, banners)
        {
            Vision = vision;
            WieldedWeapon = weapon;
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetElementEmote(Vision)}{GenshinEmotes.GetWeaponEmote(WieldedWeapon)} **{Name}**";
        }

        public Image GetImage()
        {
            return new Image($"{Globals.ProjectDirectory}Characters{Path.DirectorySeparatorChar}{Name.Replace(" ", "")}.png");
        }

        public static Image GetDefaultImage()
        {
            return new Image($"{Globals.ProjectDirectory}Characters{Path.DirectorySeparatorChar}Aether.png");
        }

        protected override string GetMultiWishSplashArt()
        {
            return $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}SplashArtsPartial{Path.DirectorySeparatorChar}{Name}.png";
        }

        protected override string GetIcon()
        {
            return $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Vision}.png";
        }
    }
}
