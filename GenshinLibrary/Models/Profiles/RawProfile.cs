using Newtonsoft.Json;
using System.Collections.Generic;

namespace GenshinLibrary.GenshinWishes.Profiles
{
    public class RawProfile
    {
        [JsonProperty("avatar")]
        public int? AvatarWID { get; set; }
        [JsonProperty("characters")]
        public IEnumerable<RawWishCount> Characters { get; set; }
        [JsonProperty("weapons")]
        public IEnumerable<RawWishCount> Weapons { get; set; }
    }
}
