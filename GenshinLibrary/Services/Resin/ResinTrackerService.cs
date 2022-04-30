using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenshinLibrary.Services.Resin
{
    /// <summary>
    ///     The service to keep track of users' resin.
    /// </summary>
    public partial class ResinTrackerService
    {
        private readonly DatabaseService _database;
        private Dictionary<ulong, ResinUpdate> ResinUpdates { get; } = new Dictionary<ulong, ResinUpdate>();

        public ResinTrackerService(DatabaseService database)
        {
            _database = database;
        }

        public async Task InitAsync()
        {
            var resinUpdates = await GetUpdatesAsync();
            foreach (var ru in resinUpdates)
                ResinUpdates[ru.UserID] = ru;
        }

        public async Task<ResinUpdate> SetValueAsync(ulong userId, DateTime dt, int value)
        {
            var update = new ResinUpdate(userId, dt, value);
            ResinUpdates[userId] = update;
            await UpdateResinAsync(update);
            return update;
        }

        public ResinUpdate GetResinUpdate(ulong userId) => ResinUpdates.TryGetValue(userId, out var update) ? update : null;
    }
}