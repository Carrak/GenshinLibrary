using GenshinLibrary.GenshinWishes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace GenshinLibrary.Services.GachaSim
{
    class WishImage
    {
        private static readonly string backgroundPath = $"{Globals.ProjectDirectory}GachaSim{Path.DirectorySeparatorChar}Bg.jpg";
        private static readonly int width = 100;
        private static readonly int height = 316;
        private static readonly int indent = 8;
        private static readonly int iconSize = 55;

        private readonly WishItem[] _items;

        public WishImage(WishItem[] items)
        {
            _items = items.OrderByDescending(x => x.Rarity).ThenByDescending(x => x is Character).ToArray();
        }

        public Stream GetImage()
        {
            using var bitmap = new Bitmap(backgroundPath);
            using Graphics g = Graphics.FromImage(bitmap);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int startingX = (bitmap.Width - (width + indent) * _items.Length) / 2;
            int y = bitmap.Height / 2 - height / 2;

            for (int i = 0; i < _items.Length; i++)
            {
                // Current x position
                int x = startingX + i * (width + indent);

                // Colour that varies depending on rarity
                var rarityColor = _items[i].Rarity switch
                {
                    3 => Color.FromArgb(0, 200, 255),
                    4 => Color.FromArgb(240, 110, 240),
                    5 => Color.FromArgb(255, 217, 0),
                    _ => throw new NotImplementedException()
                };

                // Rectangle to place the wish result in
                Rectangle wishRect = new Rectangle(x, y, width, height);

                // Draw gradient bg
                var top = new Rectangle(wishRect.X, wishRect.Y, wishRect.Width, wishRect.Height / 2);
                var bottom = new Rectangle(wishRect.X, wishRect.Y + wishRect.Height / 2 - 1, wishRect.Width, wishRect.Height / 2);
                using LinearGradientBrush gradientBrushTop = new LinearGradientBrush(top, Color.Gray, rarityColor, 90, false);
                using LinearGradientBrush gradientBrushBottom = new LinearGradientBrush(bottom, Color.Gray, rarityColor, 270, false);
                g.FillRectangle(gradientBrushTop, top);
                g.FillRectangle(gradientBrushBottom, bottom);

                // Draw wish image
                using var wishArt = new Bitmap(_items[i].WishArtPath);
                var size = GetSize(Math.Min(width / (double)wishArt.Width, height / (double)wishArt.Height), wishArt.Width, wishArt.Height);
                var imageRect = new Rectangle(x + wishRect.Width / 2 - size.Width / 2, y + wishRect.Height / 2 - size.Height / 2, size.Width, size.Height);
                g.DrawImage(wishArt, imageRect);

                // Draw outline
                using Pen pen = new Pen(rarityColor, 2);
                g.DrawRectangle(pen, wishRect);

                // Draw rarity
                using var rarityImage = new Bitmap(_items[i].RarityImagePath);
                var raritySize = GetSize(0.2, rarityImage.Width, rarityImage.Height);
                var rarityRect = new Rectangle(x + wishRect.Width / 2 - raritySize.Width / 2, wishRect.Bottom - 20, raritySize.Width, raritySize.Height);
                g.DrawImage(rarityImage, rarityRect);

                // Draw icon
                using var icon = new Bitmap(_items[i].IconPath);
                var resizedIconSize = GetSize(iconSize / (double)icon.Width, icon.Width, icon.Height);
                var iconRect = new Rectangle(x + wishRect.Width / 2 - resizedIconSize.Width / 2, rarityRect.Y - iconSize - 5, resizedIconSize.Width, resizedIconSize.Height);
                g.DrawImage(icon, iconRect);
            }

            Stream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);


            return stream;
        }

        private static Size GetSize(double ratio, int width, int height)
        {
            return new Size((int)(width * ratio), (int)(height * ratio));
        }
    }
}
