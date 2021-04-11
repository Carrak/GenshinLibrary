using System.Collections.Generic;

namespace GenshinLibrary.GenshinWishes.Profiles
{
    public class Profile
    {
        public IEnumerable<WishCount> Weapons { get; }
        public IEnumerable<WishCount> Characters { get; }
        public Character Avatar { get; }

        public Profile(IEnumerable<WishCount> weapons, IEnumerable<WishCount> characters, Character avatar)
        {
            Weapons = weapons;
            Characters = characters;
            Avatar = avatar;
        }
    }
}
