using GenshinLibrary.Calculators.PrimogemCalculator;
using Newtonsoft.Json;

namespace GenshinLibrary
{
    public class Config
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("connection")]
        public string Connection { get; set; }
        [JsonProperty("current_version")]
        public GameVersion Version { get; set; }
        [JsonProperty("topgg_token")]
        public string TopGGToken { get; set; }
        [JsonProperty("log_channel")]
        public ulong LogChannelId { get; set; }
        [JsonProperty("main_guild")]
        public ulong MainGuildId { get; set; }
        [JsonProperty("changelog_channel")]
        public ulong ChangelogChannelId { get; set; }
    }
}
