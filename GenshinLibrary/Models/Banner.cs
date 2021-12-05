using GenshinLibrary.Attributes;
using System;

namespace GenshinLibrary.Models
{
    [Flags]
    [EnumIgnore("Character1", "Character2")]
    public enum Banner
    {
        Character1 = 1,
        Character2 = 2,
        Character = 3,
        Weapon = 4,
        Standard = 8,
        Beginner = 16,
    }
}