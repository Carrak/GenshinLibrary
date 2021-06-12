using Newtonsoft.Json;
using System.Collections.Generic;

namespace GenshinLibrary.Models.Profiles
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
