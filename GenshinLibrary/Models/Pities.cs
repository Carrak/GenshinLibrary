using Newtonsoft.Json;

namespace GenshinLibrary.Models
{
    public class Pities
    {
        [JsonProperty("character_fivestar")]
        public int? CharacterFivestar { get; set; }
        [JsonProperty("character_fourstar")]
        public int? CharacterFourstar { get; set; }
        [JsonProperty("weapon_fivestar")]
        public int? WeaponFivestar { get; set; }
        [JsonProperty("weapon_fourstar")]
        public int? WeaponFourstar { get; set; }
        [JsonProperty("standard_fivestar")]
        public int? StandardFivestar { get; set; }
        [JsonProperty("standard_fourstar")]
        public int? StandardFourstar { get; set; }

        public override string ToString()
        {
            return
                $"Character\n" +
                $"5★: **{NullTernary(CharacterFivestar)}** | 4★: **{NullTernary(CharacterFourstar)}**\n" +
                $"Weapon\n" +
                $"5★: **{NullTernary(WeaponFivestar)}** | 4★: **{NullTernary(WeaponFourstar)}**\n" +
                $"Standard\n" +
                $"5★: **{NullTernary(StandardFivestar)}** | 4★: **{NullTernary(StandardFourstar)}**";
        }

        private string NullTernary(int? pity)
        {
            return pity.HasValue ? pity.Value.ToString() : "-";
        }
    }
}
