using Discord;
using GenshinLibrary.Analytics;
using GenshinLibrary.GenshinWishes;
using GenshinLibrary.GenshinWishes.Profiles;
using GenshinLibrary.Services.GachaSim.Banners;
using GenshinLibrary.Services.Wishes.Filtering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace GenshinLibrary.Services.Wishes
{
    partial class WishService
    {
        public async Task InitAsync()
        {
            string query = @"
            SELECT json_agg(get_character(wid)) FROM wish_items WHERE type = 'character';
            SELECT json_agg(get_weapon(wid)) FROM wish_items WHERE type = 'weapon';
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
            }
        }

        public async Task<IEnumerable<BannerStats>> GetAnalyticsAsync(IUser user)
        public async Task<Dictionary<Banner, BannerStats>> GetAnalyticsAsync(IUser user)
        {
            string query = @"
            SELECT get_analytics(@uid);
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
            DELETE FROM wishes WHERE wishid IN (SELECT * FROM unnest(@wishids))
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("wishids", records.Select(x => x.WishID).ToArray());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IEnumerable<CompleteWishItemRecord>> GetRecentRecordsAsync(IUser user, Banner banner, int limit)
        {
            string query = @"
            SELECT wid, wishid, datetime FROM
                (SELECT wid, wishid, datetime FROM wishes 
                WHERE userid = @uid AND banner = @banner
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

                records.Add(new CompleteWishItemRecord(dt, WishItemsByWID[wid], 0, wishid));
            }

            return records;
        }

        public async Task<IEnumerable<CompleteWishItemRecord>> GetRecordsAsync(IUser user, Banner banner, QueryCondition queryCondition = null)
        {
            string query = @$"
            SELECT wid, datetime, wishid, pity FROM get_detailed_wishes(@uid, @banner)
            ";

            await using var cmd = _database.GetCommand(query);

            if (queryCondition != null && !queryCondition.IsEmpty)
            {
                cmd.CommandText += queryCondition.Conditions;
                foreach (var param in queryCondition.Parameters)
                    cmd.Parameters.Add(param);
            }

            cmd.Parameters.AddWithValue("uid", (long)user.Id);
            cmd.Parameters.AddWithValue("banner", banner);

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

                records.Add(new CompleteWishItemRecord(dt, WishItemsByWID[wid], pity, wishid));
            }

            return records;
        }

        public async Task AddWishAsync(IUser user, WishItem item, Banner banner, DateTime datetime)
        {
            string query = @"
            INSERT INTO wishes (userid, wid, banner, datetime) VALUES (@uid, @wid, @banner, @dt); 
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
            INSERT INTO wishes (userid, banner, datetime, wid)
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
            SELECT get_profile(@uid);
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
            INSERT INTO avatars (userid, wid) VALUES (@uid, @wid)
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
            DELETE FROM avatars WHERE userid = @uid
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)user.Id);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Pities> GetPities(IUser user)
        {
            string query = @"
            SELECT get_pities(@uid);
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
            SELECT get_event_wishes()
            ";

            await using var cmd = _database.GetCommand(query);
            await using var reader = await cmd.ExecuteReaderAsync();

            await reader.ReadAsync();

            IEnumerable<EventWishRaw> eventWishes = JsonConvert.DeserializeObject<IEnumerable<EventWishRaw>>(reader.GetString(0));
            return eventWishes;
        }
    }
}
