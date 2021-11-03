using Newtonsoft.Json;
using System.Collections.Generic;

namespace GenshinLibrary.Analytics
{
    public class WishItemSummary
    {
        public int Count { get; }
        public IEnumerable<BannerCount> GroupedCounts { get; }

        public WishItemSummary(
            [JsonProperty("count")] int count,
            [JsonProperty("grouped_counts")] IEnumerable<BannerCount> groupedCounts
            )
        {
            Count = count;
            GroupedCounts = groupedCounts;
        }
    }
}
