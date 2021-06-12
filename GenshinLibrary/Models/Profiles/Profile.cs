using System.Collections.Generic;
using System.IO;

namespace GenshinLibrary.Models.Profiles
{
    public class Profile
    {
        public IEnumerable<WishCount> Weapons { get; }
        public IEnumerable<WishCount> Characters { get; }
        public Character Character { get; }

        public Profile(IEnumerable<WishCount> weapons, IEnumerable<WishCount> characters, Character avatar)
        {
            Weapons = weapons;
            Characters = characters;
            Character = avatar;
        }

        public Stream GetAvatar()
        {
            var stream = new MemoryStream();

            if (Character?.AvatarImage != null)
                Character.AvatarImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            else
                Character.DefaultAvatar.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            stream.Position = 0;
            return stream;
        }
    }
}
