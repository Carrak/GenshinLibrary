using GenshinLibrary.Models;
using GenshinLibrary.Services.Wishes.Images;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace GenshinLibrary.Pagers
{
    public class WishHistoryPager : Pager, IDisposable
    {
        public string BannerName { get; }

        private readonly ReadOnlyCollection<CompleteWishItemRecord> _items;
        private readonly WishImage[] _images;

        public WishHistoryPager(IEnumerable<CompleteWishItemRecord> items, string bannerName) : base((int)Math.Ceiling(items.Count() / (float) WishImage.WISHES_DISPLAYED))
        {
            _items = items.ToList().AsReadOnly();
            BannerName = bannerName;

            _images = new WishImage[TotalPages];
            for (int page = 0; page < TotalPages; page++)
            {
                int diff = page * WishImage.WISHES_DISPLAYED;
                int low = diff;
                int high = Math.Min(diff + WishImage.WISHES_DISPLAYED, _items.Count);
                CompleteWishItemRecord[] records = new CompleteWishItemRecord[high - low];
                for (int j = low; j < high; j++)
                    records[j - diff] = _items[j];
                _images[page] = WishImage.GetHistoryWishImage(records);
            }
        }

        public Stream GetPageImage()
        {
            return _images[Page].GetStream();
        }

        public void Dispose()
        {
            foreach (var img in _images)
                img?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
