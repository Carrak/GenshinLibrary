﻿using Discord;
using GenshinLibrary.Analytics;
using GenshinLibrary.Models;
using GenshinLibrary.Models.Profiles;
using GenshinLibrary.Services.GachaSim;
using GenshinLibrary.Services.GachaSim.Banners;
using GenshinLibrary.Services.Wishes.Filtering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Services.Wishes
{
    partial class WishService
    {
        public async Task InitAsync()
        {
            await InitWishItems();
            await InitEventWishesAsync();
            await InitServers();
        }

        private async Task InitWishItems()
        {
            string query = @"
            SELECT json_agg(gl.get_character(wid)) FROM gl.wish_items WHERE type = 'character';
            SELECT json_agg(gl.get_weapon(wid)) FROM gl.wish_items WHERE type = 'weapon';
            ";

            await using var cmd = _database.GetCommand(query);
            await using var reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            IEnumerable<Character> characters = JsonConvert.DeserializeObject<IEnumerable<Character>>(reader[0].ToString());

            await reader.NextResultAsync();
            await reader.ReadAsync();

            IEnumerable<Weapon> weapons = JsonConvert.DeserializeObject<IEnumerable<Weapon>>(reader[0].ToString());

            await reader.CloseAsync();
            var wishitems = ImmutableDictionary.CreateBuilder<string, WishItem>();
            var wishitemsbywid = ImmutableDictionary.CreateBuilder<int, WishItem>();
            wishitems.KeyComparer = StringComparer.InvariantCultureIgnoreCase;

            foreach (var character in characters)
                AddWishItem(character);
            foreach (var weapon in weapons)
                AddWishItem(weapon);

            WishItems = wishitems.ToImmutable();
            WishItemsByWID = wishitemsbywid.ToImmutable();

            void AddWishItem(WishItem wi)
            {
                wishitems[wi.Name] = wi;
                wishitemsbywid[wi.WID] = wi;

                if (wi.Rarity == 4 || wi.Rarity == 5)
                    wishitems[$"{wi.Name} ({wi.Rarity}-Star)"] = wi;

                if (wi.Aliases != null)
                    foreach (var alias in wi.Aliases)
                        wishitems[alias] = wi;
            }
        }

        private async Task InitEventWishesAsync()
        {
            foreach (var wi in WishItems.Values.Distinct())
                if (!File.Exists(wi.WishArtPath))
                    Console.WriteLine($"{wi.Name} does not have a wish art image.");

            float baseFivestarChance = 0.006f;
            float baseFourstarChance = 0.051f;
            int baseHardpity = 90;

            float weaponFivestarChance = 0.007f;
            float weaponFourstarChance = 0.06f;
            int weaponHardpity = 80;

            var wishItems = WishItems.Values;
            var standardItems = wishItems.Where(x => x.Banners.HasFlag(Banner.Standard));

            var banners = ImmutableDictionary.CreateBuilder<int, WishBanner>();

            var noelle = WishItems["Noelle"];
            var beginnerItems = wishItems.Where(x => x.Banners.HasFlag(Banner.Beginner)).Where(x => x.WID != noelle.WID);
            banners[StandardBID] = new StandardWish(GachaSimAvailable(standardItems), StandardBID, "Standard", standardItems, baseFivestarChance, baseFourstarChance, baseHardpity);
            banners[BeginnerBID] = new BeginnerWish(GachaSimAvailable(beginnerItems), BeginnerBID, "Beginner", beginnerItems, noelle, baseFivestarChance, baseFourstarChance, baseHardpity);

            string[] starterNames = { "Amber", "Kaeya", "Lisa" };
            var standardNoStarters = standardItems.Where(x => !starterNames.Contains(x.Name));
            var standardNoWeapons = standardNoStarters.Where(x => !(x.Rarity == 5 && x is Weapon));
            var standardNoCharacters = standardNoStarters.Where(x => !(x.Rarity == 5 && x is Character));

            IEnumerable<EventWishRaw> eventWishes = await GetEventWishesAsync();
            foreach (var eventWish in eventWishes)
            {
                var rateupPool = eventWish.RateUpWIDs.Select(x => WishItemsByWID[x]);
                bool gachaSimAvailable = GachaSimAvailable(rateupPool);

                switch (eventWish.Type)
                {
                    case Banner.Character:
                        banners[eventWish.BID] = new EventWish(gachaSimAvailable, eventWish.BID, eventWish.Name, eventWish.DateStarted, 0.5f, eventWish.Type, rateupPool, standardNoWeapons, baseFivestarChance, baseFourstarChance, baseHardpity);
                        break;
                    case Banner.Weapon:
                        banners[eventWish.BID] = new EventWish(gachaSimAvailable, eventWish.BID, eventWish.Name, eventWish.DateStarted, 0.75f, eventWish.Type, rateupPool, standardNoCharacters, weaponFivestarChance, weaponFourstarChance, weaponHardpity);
                        break;
                }
            }

            Banners = banners.ToImmutable();
        }

        private bool GachaSimAvailable(IEnumerable<WishItem> wishItems) => !wishItems.Any(x => !File.Exists(x.WishArtPath));

        private async Task InitServers()
        {
            string query = "SELECT gl.get_servers()";

            await using var cmd = _database.GetCommand(query);
            await using var reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            Servers = JsonConvert.DeserializeObject<IEnumerable<ServerInfo>>(reader.GetString(0)).ToDictionary(x => x.ServerID);
        }

        public async Task<Dictionary<Banner, BannerStats>> GetAnalyticsAsync(IUser user)
        {
            string query = @"
            SELECT gl.get_analytics(@uid);
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            if (await reader.IsDBNullAsync(0))
                return null;

            JObject json = JObject.Parse(reader.GetString(0));
            Dictionary<Banner, BannerStats> banners = new Dictionary<Banner, BannerStats>();
            banners[Banner.Beginner] = JsonConvert.DeserializeObject<BannerStats>(json["beginner"].ToString());
            banners[Banner.Standard] = JsonConvert.DeserializeObject<BannerStats>(json["standard"].ToString());
            banners[Banner.Character] = JsonConvert.DeserializeObject<EventBannerStats>(json["character"].ToString());
            banners[Banner.Weapon] = JsonConvert.DeserializeObject<EventBannerStats>(json["weapon"].ToString());

            return banners;
        }

        public async Task RemoveWishesAsync(IEnumerable<CompleteWishItemRecord> records)
        {
            string query = @"
            DELETE FROM gl.wishes WHERE wishid IN (SELECT * FROM unnest(@wishids))
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("wishids", records.Select(x => x.WishID).ToArray());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<CompleteWishItemRecord>> GetRecentRecordsAsync(IUser user, Banner banner, int limit)
        {
            string query = @"
            SELECT wid, wishid, datetime, banner_type FROM
                (SELECT wid, wishid, datetime, banner_type FROM gl.wishes 
                WHERE userid = @uid AND gl.has_flag(banner_type, @banner)
                ORDER BY wishid DESC
                LIMIT @limit) temp_wishes
            ";

            await using var cmd = _database.GetCommand(query);

            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("banner", banner);
            cmd.Parameters.AddWithValue("limit", limit);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows)
                return null;

            List<CompleteWishItemRecord> records = new List<CompleteWishItemRecord>();
            while (await reader.ReadAsync())
            {
                int wid = reader.GetInt32(0);
                int wishid = reader.GetInt32(1);
                DateTime dt = reader.GetDateTime(2);
                int bannerType = reader.GetInt32(3);

                records.Add(new CompleteWishItemRecord(dt, WishItemsByWID[wid], 0, wishid, (Banner)bannerType));
            }

            return records;
        }

        public async Task<IEnumerable<CompleteWishItemRecord>> GetRecordsAsync(IUser user, Banner banner, WishHistoryFilters filters = null)
        {
            string query = @$"
            SELECT wid, datetime, wishid, pity, banner_type FROM gl.get_detailed_wishes(@uid, @banner, @sp)
            ";

            await using var cmd = _database.GetCommand(query);

            if (filters?.GetCondition() is QueryCondition queryCondition && queryCondition != null && !queryCondition.IsEmpty)
            {
                cmd.CommandText += queryCondition.Conditions;
                foreach (var param in queryCondition.Parameters)
                    cmd.Parameters.Add(param);
            }

            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("banner", banner);
            cmd.Parameters.AddWithValue("sp", filters?.SeparatePity ?? false);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows)
                return null;

            List<CompleteWishItemRecord> records = new List<CompleteWishItemRecord>();
            while (await reader.ReadAsync())
            {
                int wid = reader.GetInt32(0);
                DateTime dt = reader.GetDateTime(1);
                int wishid = reader.GetInt32(2);
                int pity = reader.GetInt32(3);
                int bannerType = reader.GetInt32(4);

                records.Add(new CompleteWishItemRecord(dt, WishItemsByWID[wid], pity, wishid, (Banner)bannerType));
            }

            return records;
        }

        public async Task<IEnumerable<CompleteWishItemRecord>> GetBannerWishesAsync(IUser user, EventWish eventWish, WishHistoryFilters filters = null)
        {
            string query = @$"
            SELECT wid, datetime, wishid, pity, banner_type FROM gl.get_banner_wishes(@uid, @bid, @sp)
            ";

            await using var cmd = _database.GetCommand(query);

            if (filters?.GetCondition() is QueryCondition queryCondition && queryCondition != null && !queryCondition.IsEmpty)
            {
                cmd.CommandText += queryCondition.Conditions;
                foreach (var param in queryCondition.Parameters)
                    cmd.Parameters.Add(param);
            }

            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("bid", eventWish.BID);
            cmd.Parameters.AddWithValue("sp", filters?.SeparatePity ?? false);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows)
                return null;

            List<CompleteWishItemRecord> records = new List<CompleteWishItemRecord>();
            while (await reader.ReadAsync())
            {
                int wid = reader.GetInt32(0);
                DateTime dt = reader.GetDateTime(1);
                int wishid = reader.GetInt32(2);
                int pity = reader.GetInt32(3);
                int bannerType = reader.GetInt32(4);

                records.Add(new CompleteWishItemRecord(dt, WishItemsByWID[wid], pity, wishid, (Banner)bannerType));
            }

            return records;
        }

        public async Task AddWishAsync(IUser user, WishItem item, Banner banner, DateTime datetime)
        {
            string query = @"
            INSERT INTO gl.wishes (userid, wid, banner, datetime) VALUES (@uid, @wid, @banner, @dt); 
            ";

            await using var cmd = _database.GetCommand(query);

            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("wid", item.WID);
            cmd.Parameters.AddWithValue("banner", banner);
            cmd.Parameters.AddWithValue("dt", datetime);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddWishesAsync(IUser user, Banner banner, IEnumerable<WishItemRecord> records)
        {
            string query = @"
            INSERT INTO gl.wishes (userid, banner, datetime, wid)
            SELECT @uid, @banner, * FROM unnest(@arrdt, @arritems);
            ";

            await using var cmd = _database.GetCommand(query);

            cmd.Parameters.AddWithValue("banner", banner);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("arritems", records.Select(x => x.WishItem.WID).ToArray());
            cmd.Parameters.AddWithValue("arrdt", records.Select(x => x.DateTime).ToArray());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Profile> GetProfileAsync(IUser user)
        {
            string query = @"
            SELECT gl.get_profile(@uid);
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            await using var reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            RawProfile rawProfile = JsonConvert.DeserializeObject<RawProfile>(reader.GetString(0));
            List<WishCount> weapons = new List<WishCount>();
            List<WishCount> characters = new List<WishCount>();
            Character avatar = rawProfile.AvatarWID.HasValue ? WishItemsByWID[rawProfile.AvatarWID.Value] as Character : null;

            if (rawProfile.Weapons != null)
                weapons.AddRange(rawProfile.Weapons.Select(x => new WishCount(WishItemsByWID[x.WID], x.Count)));

            if (rawProfile.Characters != null)
                characters.AddRange(rawProfile.Characters.Select(x => new WishCount(WishItemsByWID[x.WID], x.Count)));

            return new Profile(weapons, characters, avatar);
        }

        public async Task SetAvatarAsync(IUser user, Character character)
        {
            string query = @"
            INSERT INTO gl.avatars (userid, wid) VALUES (@uid, @wid)
            ON CONFLICT (userid) DO
                UPDATE SET wid = @wid
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("wid", character.WID);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveAvatarAsync(IUser user)
        {
            string query = @"
            DELETE FROM gl.avatars WHERE userid = @uid
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SetServerAsync(IUser user, int serverId)
        {
            string query = @"
            INSERT INTO gl.server_users (userid, sid) VALUES (@uid, @sid)
            ON CONFLICT (userid) DO
                UPDATE SET sid = @sid
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("sid", serverId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Pities> GetPities(IUser user)
        {
            string query = @"
            SELECT gl.get_pities(@uid);
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            await using var reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            Pities pities = JsonConvert.DeserializeObject<Pities>(reader.GetString(0));
            return pities;
        }

        public async Task<IEnumerable<EventWishRaw>> GetEventWishesAsync()
        {
            string query = @"
            SELECT gl.get_event_wishes()
            ";

            await using var cmd = _database.GetCommand(query);
            await using var reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            IEnumerable<EventWishRaw> eventWishes = JsonConvert.DeserializeObject<IEnumerable<EventWishRaw>>(reader.GetString(0));
            return eventWishes;
        }

        public async Task<WishItemSummary> GetSummaryAsync(IUser user, WishItem wi)
        {
            string query = @"
            SELECT gl.get_wish_item_summary(@uid, @wid)
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("wid", wi.WID);

            await using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            if (await reader.IsDBNullAsync(0))
                return null;

            return JsonConvert.DeserializeObject<WishItemSummary>(reader.GetString(0));
        }
    }
}
