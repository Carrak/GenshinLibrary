using GenshinLibrary.GenshinWishes;
using Newtonsoft.Json;

namespace GenshinLibrary.Analytics
{
    class EventBannerStats : BannerStats
    {
        public int RateUpFourStar { get; }
        public int RateUpFiveStar { get; }
        public int NonRateUpFourStar { get; }
        public int NonRateUpFiveStar { get; }
        public bool FourStarRateupGuarantee { get; }
        public bool FiveStarRateupGuarantee { get; }

        public EventBannerStats(
            [JsonProperty("banner")] Banner banner,
            [JsonProperty("total")] int total,
            [JsonProperty("threestar")] int threestar,
            [JsonProperty("fourstar")] int fourstar,
            [JsonProperty("fivestar")] int fivestar,
            [JsonProperty("fourstar_characters")] int fourstarCharacters,
            [JsonProperty("fivestar_characters")] int fivestarCharacters,
            [JsonProperty("fourstar_weapons")] int fourstarWeapons,
            [JsonProperty("fivestar_weapons")] int fivestarWeapons,
            [JsonProperty("rateup_fourstar")] int rateupFourstar,
            [JsonProperty("rateup_fivestar")] int rateupFivestar,
            [JsonProperty("non_rateup_fourstar")] int nonRateupFourstar,
            [JsonProperty("non_rateup_fivestar")] int nonRateupFivestar,
            [JsonProperty("fourstar_rateup_guarantee")] bool fourstarRateupGuarantee,
            [JsonProperty("fivestar_rateup_guarantee")] bool fivestarRateupGuarantee
            ) : base(banner, total, threestar, fourstar, fivestar, fourstarCharacters, fivestarCharacters, fourstarWeapons, fivestarWeapons)
        {
            RateUpFourStar = rateupFourstar;
            RateUpFiveStar = rateupFivestar;
            NonRateUpFourStar = nonRateupFourstar;
            NonRateUpFiveStar = nonRateupFivestar;
            FourStarRateupGuarantee = fourstarRateupGuarantee;
            FiveStarRateupGuarantee = fivestarRateupGuarantee;
        }

        public string RateUpStats()
        {
            return $"Rate up stats:\n" +
                $"4★: **{RateUpFourStar}** / **{NonRateUpFourStar}** {(FourStarWishes != 0 ? $"(**{RateUpFourStar / (float)FourStarWishes:0.00%}**)" : "")}\n" +
                $"5★: **{RateUpFiveStar}** / **{NonRateUpFiveStar}** {(FiveStarWishes != 0 ? $"(**{RateUpFiveStar / (float)FiveStarWishes:0.00%}**)" : "")}\n";
        }
        
        public string RateUpGuarantees()
        {
            return $"{Banner}\n5★: {GetSign(FiveStarRateupGuarantee)} | 4★: {GetSign(FourStarRateupGuarantee)}";
        }

        private string GetSign(bool guarantee) => guarantee ? "**Yes**" : "**No**";
    }
}
