using GenshinLibrary.Models;
using System.Collections.Generic;

namespace GenshinLibrary.Services.GachaSim
{
    class WishCounts
    {
        public IReadOnlyList<WishItemCount> Fivestars { get; }
        public IReadOnlyList<WishItemCount> Fourstars { get; }
        public IReadOnlyList<WishItemCount> Threestars { get; }

        public WishCounts(IEnumerable<GachaSimWishItemRecord> wishItems)
        {
            Dictionary<WishItem, int> counts = new Dictionary<WishItem, int>();
            foreach (var wi in wishItems)
                counts[wi.WishItem] = counts.TryGetValue(wi.WishItem, out var count) ? ++count : 1;

            List<WishItemCount> threestars = new List<WishItemCount>();
            List<WishItemCount> fourstars = new List<WishItemCount>();
            List<WishItemCount> fivestars = new List<WishItemCount>();

            foreach (var entry in counts)
            {
                var wi = entry.Key;
                var count = entry.Value;

                var wishItemCount = new WishItemCount(wi, count);
                switch (wi.Rarity)
                {
                    case 3: threestars.Add(wishItemCount); break;
                    case 4: fourstars.Add(wishItemCount); break;
                    case 5: fivestars.Add(wishItemCount); break;
                }
            }

            threestars.Sort((item1, item2) => item2.Count - item1.Count);
            fourstars.Sort((item1, item2) => item2.Count - item1.Count);
            fivestars.Sort((item1, item2) => item2.Count - item1.Count);

            Fivestars = fivestars.AsReadOnly();
            Fourstars = fourstars.AsReadOnly();
            Threestars = threestars.AsReadOnly();
        }
    }
}
