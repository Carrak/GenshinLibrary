using GenshinLibrary.GenshinWishes;
using Newtonsoft.Json;

namespace GenshinLibrary.Analytics
{
    public class BannerStats
    {
        [JsonProperty("banner")]
        public Banner Banner { get; set; }
        [JsonProperty("total")]
        public int TotalWishes { get; set; }
        [JsonProperty("threestar")]
        public int ThreeStarWishes { get; set; }
        [JsonProperty("fourstar")]
        public int FourStarWishes { get; set; }
        [JsonProperty("fivestar")]
        public int FiveStarWishes { get; set; }
        [JsonProperty("fourstar_characters")]
        public int FourStarCharacters { get; set; }
        [JsonProperty("fourstar_weapons")]
        public int FourStarWeapons { get; set; }
        [JsonProperty("fivestar_characters")]
        public int FiveStarCharacters { get; set; }
        [JsonProperty("fivestar_weapons")]
        public int FiveStarWeapons { get; set; }

        public string GetGeneralInfo(bool displayFourstar, bool displayFivestar)
        {
            return
                $"Total: **{TotalWishes}**\n" +
                $"3-star: **{ThreeStarWishes}**\n" +
                $"4-star: **{FourStarWishes}**{(displayFourstar ? $" ({FourStarWeapons}W, {FourStarCharacters}C)" : "")}\n" +
                $"5-star: **{FiveStarWishes}**{(displayFivestar ? $" ({FiveStarWeapons}W, {FiveStarCharacters}C)" : "")}\n\n" +
                $"4-star: **{FourStarWishes / (float)TotalWishes:0.00%}**\n" +
                $"5-star: **{FiveStarWishes / (float)TotalWishes:0.00%}**\n";
        }
    }
}
