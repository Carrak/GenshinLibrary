namespace GenshinLibrary.Models.Profiles
{
    public class WishCount
    {
        public WishItem WishItem { get; }
        public int Count { get; }

        public WishCount(WishItem wi, int count)
        {
            WishItem = wi;
            Count = count;
        }

        public override string ToString()
        {
            return $"{WishItem.GetNameWithEmotes()} (x{Count})";
        }
    }
}
