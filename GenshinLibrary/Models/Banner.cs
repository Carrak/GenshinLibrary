using System;

namespace GenshinLibrary.GenshinWishes
{
    [Flags]
    public enum Banner
    {
        Character = 1 << 0,
        Weapon = 1 << 1,
        Standard = 1 << 2,
        Beginner = 1 << 3
    }
}