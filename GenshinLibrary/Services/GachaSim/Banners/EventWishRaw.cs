using GenshinLibrary.Models;
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
        [JsonProperty("date_started")]
        public DateTime DateStarted { get; set; }
        [JsonProperty("date_ended")]
        public DateTime DateEnded { get; set; }
        [JsonProperty("type")]
        public Banner Type;
        [JsonProperty("rateup_pool")]
        public IEnumerable<int> RateUpWIDs { get; set; }
    }
}
