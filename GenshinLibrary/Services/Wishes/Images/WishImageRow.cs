using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenshinLibrary.Services.Wishes.Images
{
    public class WishImageRow
    {
        public IEnumerable<WishImageRowElement> Elements { get; }

        public WishImageRow(IEnumerable<WishImageRowElement> elements)
        {
            Elements = elements;
        }
    }

    public class WishImageRowElement
    {
        public string Text { get; }
        public Rgba32 Color { get; }

        public WishImageRowElement(string text, Rgba32 color)
        {
            Text = text;
            Color = color;
        }
    }
}
