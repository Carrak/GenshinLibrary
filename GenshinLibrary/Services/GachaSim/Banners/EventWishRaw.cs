using GenshinLibrary.GenshinWishes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GenshinLibrary.Services.GachaSim.Banners
{
    public class EventWishRaw
    {
        [JsonProperty("bid")]
        public int BID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("type")]
        public Banner Type;
        [JsonProperty("rateup_pool")]
        public IEnumerable<int> RateUpWIDs { get; set; }
    }
}
