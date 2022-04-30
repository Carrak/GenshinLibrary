using GenshinLibrary.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenshinLibrary.Services.Wishes.Images
{
    public class WishImage : IDisposable
    {
        public const int WISHES_DISPLAYED = 12;
        public const int HEIGHT = 702;
        public const int WIDTH = 910;

        private const int FONT_SIZE = 21;
        private const int LINE_THICKNESS = 2;
        private const int DY = HEIGHT / (WISHES_DISPLAYED + 1);

        private static readonly Rgba32 bgColor = new Rgba32(235, 235, 235);
        private static readonly Rgba32 frameColor = new Rgba32(171, 171, 171);
        private static readonly Rgba32 headerColor = new Rgba32(219, 215, 211);
        private static readonly Rgba32 textColor = new Rgba32(150, 150, 150);
        private static readonly Rgba32 boldTextColor = new Rgba32(130, 130, 130);
        private static readonly Rgba32 fourstarColor = new Rgba32(166, 94, 226);
        private static readonly Rgba32 fivestarColor = new Rgba32(191, 109, 55);

        private readonly IReadOnlyList<WishImageColumn> _columns;
        private readonly IReadOnlyList<WishImageRow> _rows;

        private Stream Image { get; set; }

        private WishImage(IEnumerable<WishImageColumn> columns, IEnumerable<WishImageRow> rows) 
        {
            _columns = columns.ToList().AsReadOnly();
            _rows = rows.ToList().AsReadOnly();
        }

        public static WishImage GetRecordsWishImage(IEnumerable<WishItemRecord> records)
        {
            WishImageColumn[] columns = new[]
            {
                new WishImageColumn("Item Type", 150),
                new WishImageColumn("Item Name", 356),
                new WishImageColumn("Wish Type", 162),
                new WishImageColumn("Time Received", 242)
            };

            List<WishImageRow> rows = new List<WishImageRow>();
            foreach (var record in records)
            {
                var rarityColor = record.WishItem.Rarity switch
                {
                    3 => textColor,
                    4 => fourstarColor,
                    5 => fivestarColor,
                    _ => throw new Exception("Unknown rarity.")
                };

                var name = record.WishItem.Rarity switch
                {
                    3 => record.WishItem.Name,
                    4 => $"{record.WishItem.Name} (4-Star)",
                    5 => $"{record.WishItem.Name} (5-Star)",
                    _ => throw new Exception("Unknown rarity.")
                };

                WishImageRowElement[] elements = new WishImageRowElement[4];
                elements[0] = new WishImageRowElement(record.WishItem.GetType().Name, textColor);
                elements[1] = new WishImageRowElement(name, rarityColor);
                elements[2] = new WishImageRowElement(record.Banner.ToString(), textColor);
                elements[3] = new WishImageRowElement(record.DateTime.ToString("yyyy-MM-dd hh:mm:ss"), textColor);

                rows.Add(new WishImageRow(elements));
            }

            return new WishImage(columns, rows);
        }

        public static WishImage GetHistoryWishImage(IEnumerable<CompleteWishItemRecord> records)
        {
            WishImageColumn[] columns = new[]
            {
                new WishImageColumn("Pity 5", 75),
                new WishImageColumn("Pity 4", 75),
                new WishImageColumn("Item Name", 356),
                new WishImageColumn("Wish Type", 162),
                new WishImageColumn("Time Received", 242)
            };

            List<WishImageRow> rows = new List<WishImageRow>();
            foreach(var record in records)
            {
                var rarityColor = record.WishItem.Rarity switch
                {
                    3 => textColor,
                    4 => fourstarColor,
                    5 => fivestarColor,
                    _ => throw new Exception("Unknown rarity.")
                };

                var name = record.WishItem.Rarity switch
                {
                    3 => record.WishItem.Name,
                    4 => $"{record.WishItem.Name} (4-Star)",
                    5 => $"{record.WishItem.Name} (5-Star)",
                    _ => throw new Exception("Unknown rarity.")
                };

                WishImageRowElement[] elements = new WishImageRowElement[5];
                elements[0] = new WishImageRowElement(record.PityFive.ToString(), record.WishItem.Rarity == 5 ? fivestarColor : Mix(textColor, fivestarColor, record.PityFive / 90f / 1.75f));
                elements[1] = new WishImageRowElement(record.PityFour.ToString(), record.WishItem.Rarity == 4 ? fourstarColor : Mix(textColor, fourstarColor, record.PityFour / 10f / 1.75f));
                elements[2] = new WishImageRowElement(name, rarityColor);
                elements[3] = new WishImageRowElement(record.Banner.ToString(), textColor);
                elements[4] = new WishImageRowElement(record.DateTime.ToString("yyyy-MM-dd hh:mm:ss"), textColor);

                rows.Add(new WishImageRow(elements));
            }

            return new WishImage(columns, rows);
        }

        public Stream GetStream()
        {
            if (Image != null)
            {
                Image.Seek(0, SeekOrigin.Begin);
                return Image;
            }

            using var canvas = GetCanvas();

            var font = SystemFonts.CreateFont("Arial", FONT_SIZE);
            TextOptions to = new TextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            for (int i = 0; i < _rows.Count; i++)
            {
                var rowElements = _rows[i].Elements;
                float y = DY * (i + 1.5f);

                int x = 0;
                int j = 0;
                foreach(var element in rowElements)
                {
                    to.Origin = new PointF(x + _columns[j].Width / 2, y);
                    canvas.Mutate(ctx => ctx.DrawText(to, element.Text, element.Color));
                    x += _columns[j].Width;
                    j++;
                }
            }

            MemoryStream stream = new MemoryStream();
            canvas.SaveAsPng(stream);
            stream.Seek(0, SeekOrigin.Begin);

            Image = stream;
            return Image;
        }

        private static Rgba32 Mix(Rgba32 a, Rgba32 b, float percent)
        {
            float amountFrom = 1.0f - percent;

            return new Rgba32(
                (byte)(a.R * amountFrom + b.R * percent),
                (byte)(a.G * amountFrom + b.G * percent),
                (byte)(a.B * amountFrom + b.B * percent)
                );
        }

        public void Dispose()
        {
            Image?.Dispose();
        }

        private Image<Rgba32> GetCanvas()
        {
            Image<Rgba32> canvas = new Image<Rgba32>(WIDTH, DY * (_rows.Count+1), bgColor);

            canvas.Mutate(x => x.Fill(headerColor, new RectangleF(0, 0, canvas.Width, DY)));

            // Drawing column lines
            int colX = 0;
            List<IPath> paths = new List<IPath>();
            foreach (var column in _columns)
            {
                colX += column.Width;
                paths.Add(new PathBuilder().AddLine(new PointF(colX, 0), new PointF(colX, canvas.Height)).Build());
            }
            canvas.Mutate(x => x.Draw(frameColor, LINE_THICKNESS, new PathCollection(paths)));

            // Draw row lines
            paths.Clear();
            for (int i = 0; i < _rows.Count; i++)
            {
                int y = (i + 1) * DY;
                paths.Add(new PathBuilder().AddLine(new PointF(0, y), new PointF(canvas.Width, y)).Build());
            }
            canvas.Mutate(x => x.Draw(frameColor, LINE_THICKNESS, new PathCollection(paths)));

            // Create font and text options
            var font = SystemFonts.CreateFont("Arial", FONT_SIZE, FontStyle.Bold);
            TextOptions to = new TextOptions(font)
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Draw header
            int x = 0;
            foreach(var column in _columns)
            {
                to.Origin = new PointF(x + column.Width / 2, DY / 2);
                canvas.Mutate(x => x.DrawText(to, column.Name, boldTextColor));
                x += column.Width;
            }

            return canvas;
        }
    }
}
