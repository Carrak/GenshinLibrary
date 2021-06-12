﻿using Fastenshtein;
using GenshinLibrary.Models;
using GenshinLibrary.Services.GachaSim;
using GenshinLibrary.Services.Wishes.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.Services.Wishes
{
    public partial class WishService
    {
        public IReadOnlyDictionary<string, WishItem> WishItems { get; private set; }
        public IReadOnlyDictionary<int, WishItem> WishItemsByWID { get; private set; }
        public IReadOnlyDictionary<int, WishBanner> Banners { get; private set; }
        public IReadOnlyDictionary<int, ServerInfo> Servers { get; private set; }

        private readonly DatabaseService _database;

        public readonly int BeginnerBID = -1;
        public readonly int StandardBID = 0;

        public WishService(DatabaseService database)
        {
            _database = database;
        }

        public WishItem GetBestSuggestion(string name)
        {
            var wishitems = WishItems.Values.ToList();
            var minWishItem = wishitems[0];
            var minDistance = Levenshtein.Distance(name, minWishItem.Name);

            for (int i = 1; i < wishitems.Count; i++)
            {
                var wishitem = wishitems[i];

                var distance = Levenshtein.Distance(name, wishitem.Name);
                if (distance < minDistance)
                {
                    minWishItem = wishitem;
                    minDistance = distance;
                }
            }

            for (int i = 0; i < wishitems.Count; i++)
            {
                var wishitem = wishitems[i];

                var distance = AutoCompleteLevenshtein.Distance(name, wishitem.Name);
                if (distance < minDistance)
                {
                    minWishItem = wishitem;
                    minDistance = distance;
                }
            }

            return minWishItem;
        }

        public WishHistoryFilters ValidateFilters(WishHistoryFilters filters, Banner banner)
        {
            if (filters.RarityFilter != null)
                foreach (var rarity in filters.RarityFilter.Constraints)
                    if (rarity.Value != 3 && rarity.Value != 4 && rarity.Value != 5)
                        throw new ArgumentException("`rarity` Rarity can only be 3, 4 or 5.");

            if (filters.PityFilter != null)
                foreach (var pity in filters.PityFilter.Constraints)
                    if (pity.Value < 1 || pity.Value > 90)
                        throw new ArgumentException("`pity` Pity must range from 1 to 90.");

            if (filters.NameFilter != null)
                for (int i = 0; i < filters.NameFilter?.Constraints.Count; i++)
                {
                    var name = filters.NameFilter.Constraints[i];

                    if (WishItems.TryGetValue(name.Value, out var wi))
                    {
                        if (!wi.Banners.HasFlag(banner))
                            throw new ArgumentException($"`name` {wi.Name} does not drop from the `{banner}` banner.");

                        filters.NameFilter.Constraints[i].Value = wi.Name;
                    }
                    else
                        throw new ArgumentException($"`name` No item called `{name.Value}` has been found. Did you mean `{GetBestSuggestion(name.Value).Name}`?");
                }

            return filters;
        }
    }
}
