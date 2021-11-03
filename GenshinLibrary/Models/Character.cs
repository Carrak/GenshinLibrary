﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace GenshinLibrary.Models
{
    public class Character : WishItem
    {
        public static string DefaultAvatarPath = $"{Globals.ProjectDirectory}Characters{Path.DirectorySeparatorChar}Aether.png";

        public Element Vision { get; }
        public WeaponType WieldedWeapon { get; }

        public override string IconPath { get; }
        public override string WishArtPath { get; }
        public string AvatarImagePath { get; }

        [JsonConstructor]
        public Character(int wid, string name, Element vision, WeaponType weapon, int rarity, Banner banners, IEnumerable<string> aliases) : base(wid, name, rarity, banners, aliases)
        {
            Vision = vision;
            WieldedWeapon = weapon;
            WishArtPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}SplashArtsPartial{Path.DirectorySeparatorChar}{Name}.png";
            IconPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Icons{Path.DirectorySeparatorChar}{Vision}.png";
            AvatarImagePath = $"{Globals.ProjectDirectory}Characters{Path.DirectorySeparatorChar}{Name}.png";
        }

        public override string GetNameWithEmotes()
        {
            return $"{GenshinEmotes.GetElementEmote(Vision)}{GenshinEmotes.GetWeaponEmote(WieldedWeapon)} **{Name}**";
        }
    }
}
