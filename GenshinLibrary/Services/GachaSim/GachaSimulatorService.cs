using Discord;
using GenshinLibrary.GenshinWishes;
using GenshinLibrary.Services.GachaSim.Banners;
using GenshinLibrary.Services.Wishes;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Services.GachaSim
{
    public class GachaSimulatorService
    {
        private readonly WishService _wishes;
        private readonly MemoryCache profiles = new MemoryCache(new MemoryCacheOptions());

        public Dictionary<int, WishBanner> Banners = new Dictionary<int, WishBanner>();

        public readonly int BeginnerBID = -1;
        public readonly int StandardBID = 0;

        public GachaSimulatorService(WishService wishes)
        {
            _wishes = wishes;
        }

        public GachaSimulatorProfile GetOrCreateProfile(IUser user)
        {
            if (profiles.TryGetValue<GachaSimulatorProfile>(user.Id, out var profile))
                return profile;

            var newProfile = new GachaSimulatorProfile(Banners.Values.First());
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

        public async Task InitAsync()
        {
            foreach(var wi in _wishes.WishItems.Values.Distinct())
                if (wi.WishArt is null)
                    Console.WriteLine($"{wi.Name} does not have a wish art image.");

            float baseFivestarChance = 0.006f;
            float baseFourstarChance = 0.051f;
            int baseHardpity = 90;

            float weaponFivestarChance = 0.007f;
            float weaponFourstarChance = 0.06f;
            int weaponHardpity = 80;

            var wishItems = _wishes.WishItems;
            var standardWishes = wishItems.Values.Where(x => x.Banners.HasFlag(Banner.Standard));

            Banners[StandardBID] = new StandardWish(StandardBID, "Standard", standardWishes, baseFivestarChance, baseFourstarChance, baseHardpity);

            string[] starterNames = { "Amber", "Kaeya", "Lisa" };
            var standardNoStarters = standardWishes.Where(x => !starterNames.Contains(x.Name));
            var standardNoWeapons = standardNoStarters.Where(x => !(x.Rarity == 5 && x is Weapon));
            var standardNoCharacters = standardNoStarters.Where(x => !(x.Rarity == 5 && x is Character));
            var beginnerPool = standardNoWeapons.Where(x => !(x.Rarity == 4 && x is Weapon) && x.Name != "Noelle");

            Banners[BeginnerBID] = new BeginnerWish(BeginnerBID, "Beginner", beginnerPool, wishItems["Noelle"], baseFivestarChance, baseFourstarChance, baseHardpity);

            IEnumerable<EventWishRaw> eventWishes = await _wishes.GetEventWishesAsync();
            foreach (var eventWish in eventWishes)
            {
                var rateupPool = eventWish.RateUpWIDs.Select(x => _wishes.WishItemsByWID[x]);
                switch (eventWish.Type)
                {
                    case Banner.Character:
                        Banners[eventWish.BID] = new EventWish(eventWish.BID, eventWish.Name, eventWish.Date, 0.5f, eventWish.Type, rateupPool, standardNoWeapons, baseFivestarChance, baseFourstarChance, baseHardpity);
                        break;
                    case Banner.Weapon:
                        Banners[eventWish.BID] = new EventWish(eventWish.BID, eventWish.Name, eventWish.Date, 0.75f, eventWish.Type, rateupPool, standardNoCharacters, weaponFivestarChance, weaponFourstarChance, weaponHardpity);
                        break;
                }
            }
        }
    }
}
