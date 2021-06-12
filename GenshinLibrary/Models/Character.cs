using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;

namespace GenshinLibrary.Models
{
    public class Character : WishItem
    {
        public static Bitmap DefaultAvatar = new Bitmap($"{Globals.ProjectDirectory}Characters{Path.DirectorySeparatorChar}Aether.png");

        public Element Vision { get; }
        public WeaponType WieldedWeapon { get; }
        public Bitmap AvatarImage { get; }

        public override string IconPath { get; }
        public override string WishArtPath { get; }

        [JsonConstructor]
        public Character(int wid, string name, Element vision, WeaponType weapon, int rarity, Banner banners) : base(wid, name, rarity, banners)
        {
            Vision = vision;
            WieldedWeapon = weapon;
            WishArtPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}SplashArtsPartial{Path.DirectorySeparatorChar}{Name}.png";
            IconPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Vision}.png";

            try
            {
                AvatarImage = new Bitmap($"{Globals.ProjectDirectory}Characters{Path.DirectorySeparatorChar}{Name}.png");
            }
            catch
            {
                Console.WriteLine($"Image missing/no access to AvatarImage for {Name}");
            }
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetElementEmote(Vision)}{GenshinEmotes.GetWeaponEmote(WieldedWeapon)} **{Name}**";
        }

        public Stream GetAvatar()
        {
            var stream = new MemoryStream();
            AvatarImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            return stream;
        }
    }
}
