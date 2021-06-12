using GenshinLibrary.Models;
using Newtonsoft.Json;

namespace GenshinLibrary.Analytics
{
    public class BannerStats
    {
        public Banner Banner { get; }
        public int TotalWishes { get; }
        public int ThreeStarWishes { get; }
        public int FourStarWishes { get; }
        public int FiveStarWishes { get; }
        public int FourStarCharacters { get; }
        public int FiveStarCharacters { get; }
        public int FourStarWeapons { get; }
        public int FiveStarWeapons { get; }

        [JsonConstructor]
        public BannerStats(
            [JsonProperty("banner")] Banner banner,
            [JsonProperty("total")] int total,
            [JsonProperty("threestar")] int threestar,
            [JsonProperty("fourstar")] int fourstar,
            [JsonProperty("fivestar")] int fivestar,
            [JsonProperty("fourstar_characters")] int fourstarCharacters,
            [JsonProperty("fivestar_characters")] int fivestarCharacters,
            [JsonProperty("fourstar_weapons")] int fourstarWeapons,
            [JsonProperty("fivestar_weapons")] int fivestarWeapons
            )
        {
            Banner = banner;
            TotalWishes = total;
            ThreeStarWishes = threestar;
            FourStarWishes = fourstar;
            FiveStarWishes = fivestar;
            FourStarCharacters = fourstarCharacters;
            FiveStarCharacters = fivestarCharacters;
            FourStarWeapons = fourstarWeapons;
            FiveStarWeapons = fivestarWeapons;
        }

        public string GetGeneralInfo(bool displayFourstar, bool displayFivestar)
        {
            return
                $"Total: **{TotalWishes}**\n" +
                $"3★: **{ThreeStarWishes}**\n" +
                $"4★: **{FourStarWishes}**{(displayFourstar ? $" ({FourStarWeapons}W, {FourStarCharacters}C)" : "")}\n" +
                $"5★: **{FiveStarWishes}**{(displayFivestar ? $" ({FiveStarWeapons}W, {FiveStarCharacters}C)" : "")}\n\n" +
                $"4★: **{FourStarWishes / (float)TotalWishes:0.00%}**\n" +
                $"5★: **{FiveStarWishes / (float)TotalWishes:0.00%}**\n";
        }
    }
}
