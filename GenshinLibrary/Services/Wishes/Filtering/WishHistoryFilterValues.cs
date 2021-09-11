using Discord.Commands;

namespace GenshinLibrary.Models
{
    [NamedArgumentType]
    public class WishHistoryFilterValues
    {
        public string Rarity { get; set; }
        public string DateTime { get; set; }
        public string Name { get; set; }
        public string Pity { get; set; }
        public string Order { get; set; }
        public bool Sp { get; set; }
    }
}
