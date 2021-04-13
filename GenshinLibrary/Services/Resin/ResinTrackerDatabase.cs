using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenshinLibrary.Services.Resin
{
    partial class ResinTrackerService
    {
        public async Task<IEnumerable<ResinUpdate>> GetUpdatesAsync()
        {
            string query = @"
            SELECT userid, updated_at, value FROM resin_updates
            ";

            await using var cmd = _database.GetCommand(query);
            await using var reader = await cmd.ExecuteReaderAsync();

            List<ResinUpdate> updates = new List<ResinUpdate>();
            while (await reader.ReadAsync())
                updates.Add(new ResinUpdate((ulong)reader.GetInt64(0), reader.GetDateTime(1), reader.GetInt32(2)));

            return updates;
        }

        public async Task UpdateResinAsync(ResinUpdate ru)
        {
            string query = @"
            INSERT INTO resin_updates (userid, updated_at, value) VALUES (@uid, @updated_at, @value)
            ON CONFLICT (userid) DO
                UPDATE SET updated_at = @updated_at, value = @value
            ";

            await using var cmd = _database.GetCommand(query);
            cmd.Parameters.AddWithValue("uid", (long)ru.UserID);
            cmd.Parameters.AddWithValue("updated_at", ru.UpdatedAt);
            cmd.Parameters.AddWithValue("value", ru.Value);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
