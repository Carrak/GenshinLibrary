using Newtonsoft.Json;

namespace GenshinLibrary.GenshinWishes.Profiles
{
    public class RawWishCount
    {
        [JsonProperty("wid")]
        public int WID { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
