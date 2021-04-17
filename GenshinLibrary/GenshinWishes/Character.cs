using Newtonsoft.Json;
using System.Drawing;
using System.IO;

namespace GenshinLibrary.GenshinWishes
{
    public class Character : WishItem
    {
        public static Bitmap DefaultAvatar = new Bitmap($"{Globals.ProjectDirectory}Characters{Path.DirectorySeparatorChar}Aether.png");

        public Element Vision { get; }
        public WeaponType WieldedWeapon { get; }
        public Bitmap AvatarImage { get; }

        [JsonConstructor]
        public Character(int wid, string name, Element vision, WeaponType weapon, int rarity, Banner banners) : base(wid, name, rarity, banners)
        {
            Vision = vision;
            WieldedWeapon = weapon;
            WishArt = new Bitmap($"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}SplashArtsPartial{Path.DirectorySeparatorChar}{Name}.png");
            Icon = new Bitmap($"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Vision}.png");
            AvatarImage = new Bitmap($"{Globals.ProjectDirectory}Characters{Path.DirectorySeparatorChar}{Name}.png");
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetElementEmote(Vision)}{GenshinEmotes.GetWeaponEmote(WieldedWeapon)} **{Name}**";
        }

        public Stream GetAvatar()
        {
            var stream = new MemoryStream();

            if (AvatarImage != null)
                AvatarImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            else
                DefaultAvatar.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            stream.Position = 0;
            return stream;
        }
    }
}
