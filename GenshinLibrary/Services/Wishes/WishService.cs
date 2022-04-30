using Fastenshtein;
using GenshinLibrary.Models;
using GenshinLibrary.Services.GachaSim;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.Services.Wishes
{
    public partial class WishService
    {
        public IReadOnlyDictionary<string, WishItem> WishItemsByName { get; private set; }
        public IReadOnlyDictionary<int, WishItem> WishItemsByWID { get; private set; }
        public IReadOnlyDictionary<int, WishBanner> BannersByBID { get; private set; }
        public IReadOnlyDictionary<int, ServerInfo> ServersBySID { get; private set; }

        private readonly DatabaseService _database;

        public readonly int BeginnerBID = -1;
        public readonly int StandardBID = 0;

        public WishService(DatabaseService database)
        {
            _database = database;
        }

        public WishItem GetBestSuggestion(string name, float threshold = 0.6f)
        {
            var wishitems = WishItemsByName.ToList();
            WishItem minWishItem = null;
            float maxScore = 0;

            for (int i = 0; i < wishitems.Count; i++)
            {
                var keyValuePair = wishitems[i];

                var score = GetLevenshteinScore(name, keyValuePair.Key);
                if (score >= threshold && score > maxScore)
                {
                    minWishItem = keyValuePair.Value;
                    maxScore = score;
                }
            }

            return minWishItem;

            static float GetLevenshteinScore(string s1, string s2) => 1 - (float)Levenshtein.Distance(s1.ToLower(), s2.ToLower()) / Math.Max(s1.Length, s2.Length);
        }
    }
}
