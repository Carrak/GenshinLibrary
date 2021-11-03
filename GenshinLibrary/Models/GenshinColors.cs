using Discord;
using System;

namespace GenshinLibrary.Models
{
    public static class GenshinColors
    {
        public static readonly Color Anemo = new Color(167, 244, 205);
        public static readonly Color Cryo = new Color(208, 254, 255);
        public static readonly Color Dendro = new Color(180, 238, 39);
        public static readonly Color Electro = new Color(223, 187, 255);
        public static readonly Color Geo = new Color(244, 216, 96);
        public static readonly Color Hydro = new Color(1, 232, 255);
        public static readonly Color Pyro = new Color(255, 170, 114);
        public static readonly Color NoElement = new Color(197, 197, 197);

        public static readonly Color ThreeStar = Color.Blue;
        public static readonly Color FourStar = Color.Purple;
        public static readonly Color FiveStar = Color.Gold;

        public static Color GetElementColor(Element vision)
        {
            return vision switch
            {
                Element.Anemo => Anemo,
                Element.Cryo => Cryo,
                Element.Dendro => Dendro,
                Element.Electro => Electro,
                Element.Geo => Geo,
                Element.Hydro => Hydro,
                Element.Pyro => Pyro,
                _ => throw new NotImplementedException()
            };
        }

        public static Color GetRarityColor(int rarity)
        {
            return rarity switch {
                3 => Color.Blue,
                4 => Color.Purple,
                5 => Color.Gold,
                _ => throw new NotImplementedException()
            };
        }
    }
}
