using Discord;
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
        private Dictionary<ulong, ResinUpdate> _resinUpdates { get; } = new Dictionary<ulong, ResinUpdate>();

        public ResinTrackerService(DatabaseService database)
        {
            _database = database;
        }

        public async Task InitAsync()
        {
            var resinUpdates = await GetUpdatesAsync();
            foreach (var ru in resinUpdates)
                _resinUpdates[ru.UserID] = ru;
        }

        public async Task<ResinUpdate> SetValueAsync(IUser user, DateTime dt, int value)
        {
            var update = new ResinUpdate(user.Id, dt, value);
            _resinUpdates[user.Id] = update;
            await UpdateResinAsync(update);
            return update;
        }

        public ResinUpdate GetValue(IUser user) => _resinUpdates.TryGetValue(user.Id, out var update) ? update : null;
    }
}