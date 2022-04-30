using Discord.Interactions;
using System;

namespace GenshinLibrary.Models
{
    [Flags]
    public enum Banner
    {
        [Hide]
        Character1 = 1,
        [Hide]
        Character2 = 2,
        Character = 3,
        Weapon = 4,
        Standard = 8,
        Beginner = 16,
    }
}