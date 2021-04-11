using Discord.Commands;

namespace GenshinLibrary.GenshinWishes
{
    [NamedArgumentType]
    public class WishHistoryFilterValues
    {
        public string Rarity { get; set; }
        public string DateTime { get; set; }
        public string Name { get; set; }
        public string Pity { get; set; }
        public string Order { get; set; }
    }
}
