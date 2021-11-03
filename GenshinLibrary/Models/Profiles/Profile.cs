using System.Collections.Generic;

namespace GenshinLibrary.Models.Profiles
{
    public class Profile
    {
        public IEnumerable<WishCount> Weapons { get; }
        public IEnumerable<WishCount> Characters { get; }
        public Character ProfileCharacter { get; }

        public Profile(IEnumerable<WishCount> weapons, IEnumerable<WishCount> characters, Character avatar)
        {
            Weapons = weapons;
            Characters = characters;
            ProfileCharacter = avatar;
        }
    }
}
