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
        public Version Version { get; set; }
    }
}
