using Discord;
using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Services.Wishes;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace GenshinLibrary.Services.GachaSim
{
    public class GachaSimulatorService
    {
        private readonly WishService _wishes;
        private readonly MemoryCache profiles = new MemoryCache(new MemoryCacheOptions());

        public GachaSimulatorService(WishService wishes)
        {
            _wishes = wishes;
        }

        public GachaSimulatorProfile GetOrCreateProfile(IUser user)
        {
            if (profiles.TryGetValue<GachaSimulatorProfile>(user.Id, out var profile))
                return profile;

            var newProfile = new GachaSimulatorProfile(_wishes.Banners[_wishes.StandardBID]);
            SetOrUpdate(user, newProfile);

            return newProfile;
        }

        private void SetOrUpdate(IUser user, GachaSimulatorProfile profile)
        {
            var options = new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromHours(24)
            };

            profiles.Set(user.Id, profile, options);
        }

        public void ChangeBanner(IUser user, WishBanner banner)
        {
            var profile = GetOrCreateProfile(user);
            profile.ChangeBanner(banner);
            SetOrUpdate(user, profile);
        }

        public void ResetProfile(IUser user)
        {
            var profile = GetOrCreateProfile(user);
            profile.Reset();
            SetOrUpdate(user, profile);
        }

        public WishItem[] Wish(IUser user, int count)
        {
            var profile = GetOrCreateProfile(user);
            var wishes = profile.Wish(count);
            SetOrUpdate(user, profile);
            return wishes;
        }
    }
}
