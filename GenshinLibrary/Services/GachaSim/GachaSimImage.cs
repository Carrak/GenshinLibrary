using GenshinLibrary.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Linq;

namespace GenshinLibrary.Services.GachaSim
{
    public class GachaSimImage
    {
        private static readonly string gachasimMainPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Main{Path.DirectorySeparatorChar}";
        private static readonly string backgroundPath = $"{gachasimMainPath}Bg.jpg";
        private static readonly string maskPath = $"{gachasimMainPath}mask.png";
        private static readonly string framePath = $"{gachasimMainPath}frame.png";

        private static readonly int indent = 2;
        private static readonly Size weaponSize = new(192, 384);
        private static readonly Size characterSize = new(134, 430);

        private readonly GachaSimWishItemRecord[] _items;

        public GachaSimImage(GachaSimWishItemRecord[] items)
        {
            _items = items.OrderByDescending(x => x.WishItem.Rarity).ThenByDescending(x => x.WishItem is Character).ToArray();
        }

        public Stream GetImage()
        {
            using Image bitmap = Image.Load(backgroundPath);
            using Image mask = Image.Load(maskPath);
            using Image frame = Image.Load(framePath);

            int startingX = (bitmap.Width - (_items.Length - 1) * (indent + frame.Width)) / 2 + 1;

            for (int i = 0; i < _items.Length; i++)
            {
                int xPos = startingX + i * (frame.Width + indent);
                using var glow = Image.Load($"{gachasimMainPath}glow{_items[i].WishItem.Rarity}.png");
                bitmap.Mutate(x => x.DrawImage(glow, new Point(xPos - glow.Width / 2, bitmap.Height / 2 - glow.Height / 2), PixelColorBlendingMode.Lighten, 1));
            }

            for (int i = 0; i < _items.Length; i++)
            {
                int xPos = startingX + i * (frame.Width + indent);

                using var framedWishArt = frame.Clone(x => { });
                using var wishArt = Image.Load(_items[i].WishItem.WishArtPath);

                wishArt.Mutate(x => x
                    .Resize(_items[i].WishItem is Weapon ? weaponSize : characterSize)
                    .Crop(new Rectangle(wishArt.Width / 2 - mask.Width / 2, wishArt.Height / 2 - mask.Height / 2, mask.Width, mask.Height))
                    .DrawImage(mask, PixelColorBlendingMode.Multiply, PixelAlphaCompositionMode.DestIn, 1)
                    );

                if (_items[i].WishItem is Weapon)
                {
                    int offset = 8;
                    using var clone = wishArt.Clone(x => x
                        .Brightness(0)
                        .Crop(new Rectangle(0, 0, wishArt.Width - offset, wishArt.Height - offset))
                        .DrawImage(mask, PixelColorBlendingMode.Multiply, PixelAlphaCompositionMode.DestIn, 1)
                        );

                    framedWishArt.Mutate(x => x.DrawImage(clone, new Point(offset + 2, offset), 0.7f));
                }

                string iconPath = _items[i].WishItem switch
                {
                    Character c => c.ElementIconPath,
                    Weapon w => w.WeaponTypeIconPath,
                    _ => throw new System.Exception("Unknown type.")
                };

                using var rarityImage = Image.Load(_items[i].WishItem.RarityImagePath);
                using var icon = Image.Load(iconPath);
                icon.Mutate(x => x.Resize(55, 55));

                framedWishArt.Mutate(x => x
                    .DrawImage(wishArt, new Point(frame.Width / 2 - wishArt.Width / 2, frame.Height / 2 - wishArt.Height / 2), 1)
                    .DrawImage(icon, new Point(framedWishArt.Width / 2 - icon.Width / 2, framedWishArt.Height - icon.Width - 65), 1)
                    .DrawImage(rarityImage, new Point(framedWishArt.Width / 2 - rarityImage.Width / 2, framedWishArt.Height - rarityImage.Height - 45), 1)
                    );

                bitmap.Mutate(ctx => ctx.DrawImage(framedWishArt, new Point(xPos - framedWishArt.Width / 2, bitmap.Height / 2 - framedWishArt.Height / 2), 1));
            }

            MemoryStream stream = new();
            bitmap.SaveAsPng(stream); ;
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
