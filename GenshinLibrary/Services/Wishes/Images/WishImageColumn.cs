using System;
using System.Collections.Generic;
using System.Text;

namespace GenshinLibrary.Services.Wishes.Images
{
    public class WishImageColumn
    {
        public string Name { get; }
        public int Width { get; }

        public WishImageColumn(string name, int width)
        {
            Name = name;
            Width = width;
        }
    }
}
