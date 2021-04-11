using Discord;
using System;

namespace GenshinLibrary.GenshinWishes
{
    public static class GenshinEmotes
    {
        public static readonly Emote Primogem = Emote.Parse("<:Primogem:826104919622025276>");
        public static readonly Emote Acquaint = Emote.Parse("<:Acquaint:826104944268017694>");
        public static readonly Emote Intertwined = Emote.Parse("<:Intertwined:826104993035059204>");
        public static readonly Emote Starglitter = Emote.Parse("<:Starglitter:826085369358712853>");
        public static readonly Emote Events = Emote.Parse("<:Events:826095667772391424>");
        public static readonly Emote Welkin = Emote.Parse("<:Welkin:826088391144898610>");
        public static readonly Emote Daily = Emote.Parse("<:Daily:826138950800375848>");
        public static readonly Emote Abyss = Emote.Parse("<:Abyss:826139285937192980>");
        public static readonly Emote Stardust = Emote.Parse("<:Stardust:826139780957077504>");
        public static readonly Emote Sojourner = Emote.Parse("<:Sojourner:826142297154125864>");
        public static readonly Emote Gnostic = Emote.Parse("<:Gnostic:826142282792829008>");
        public static readonly Emote Battlepass = Emote.Parse("<:Battlepass:826514967129620480>");

        public static readonly Emote Pyro = Emote.Parse("<:Pyro:820689358088568832>");
        public static readonly Emote Cryo = Emote.Parse("<:Cryo:820689413813698610>");
        public static readonly Emote Hydro = Emote.Parse("<:Hydro:820689376844841013>");
        public static readonly Emote Electro = Emote.Parse("<:Electro:820689395555631164>");
        public static readonly Emote Anemo = Emote.Parse("<:Anemo:820689405378560100>");
        public static readonly Emote Geo = Emote.Parse("<:Geo:820689422462091289>");
        public static readonly Emote Dendro = Emote.Parse("<:Dendro:820689385014951966>");

        public static readonly Emote Claymore = Emote.Parse("<:Claymore:828002792270331946>");
        public static readonly Emote Sword = Emote.Parse("<:Sword:828002817192230934>");
        public static readonly Emote Polearm = Emote.Parse("<:Polearm:828002779322384474>");
        public static readonly Emote Bow = Emote.Parse("<:Bow:828002766458322996>");
        public static readonly Emote Catalyst = Emote.Parse("<:Catalyst:828002806148628571>");

        public static Emote GetElementEmote(Element element)
        {
            return element switch
            {
                Element.Pyro => Pyro,
                Element.Cryo => Cryo,
                Element.Hydro => Hydro,
                Element.Electro => Electro,
                Element.Anemo => Anemo,
                Element.Geo => Geo,
                Element.Dendro => Dendro,
                _ => throw new NotImplementedException()
            };
        }

        public static Emote GetWeaponEmote(WeaponType weapon)
        {
            return weapon switch
            {
                WeaponType.Claymore => Claymore,
                WeaponType.Sword => Sword,
                WeaponType.Polearm => Polearm,
                WeaponType.Bow => Bow,
                WeaponType.Catalyst => Catalyst,
                _ => throw new NotImplementedException()
            };
        }
    }
}
