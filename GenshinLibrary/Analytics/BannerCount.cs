using GenshinLibrary.Models;
using Newtonsoft.Json;

namespace GenshinLibrary.Analytics
{
    public class BannerCount
    {
        public Banner Banner { get; }
        public int Count { get; }

        public BannerCount(
            [JsonProperty("banner")] Banner banner,
            [JsonProperty("banner_count")] int count
            )
        {
            Banner = banner;
            Count = count;
        }
    }
}
