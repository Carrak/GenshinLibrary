using Newtonsoft.Json;

namespace GenshinLibrary.GenshinWishes
{
    public class ServerInfo
    {
        [JsonProperty("sid")]
        public int ServerID { get; set; }
        [JsonProperty("name")]
        public string ServerName { get; set; }
        [JsonProperty("timezone")]
        public string ServerTimezone { get; set; }
    }
}
