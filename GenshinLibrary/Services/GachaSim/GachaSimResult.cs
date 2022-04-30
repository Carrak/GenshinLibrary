using Discord;

namespace GenshinLibrary.Services.GachaSim
{
    public class GachaSimResult
    {
        public GachaSimResult(Embed[] embeds, GachaSimImage image)
        {
            Embeds = embeds;
            WishImage = image;
        }

        public Embed[] Embeds { get; }
        public GachaSimImage WishImage { get; }
    }
}
