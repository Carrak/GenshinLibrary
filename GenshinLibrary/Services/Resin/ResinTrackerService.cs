using Discord;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace GenshinLibrary.Services.Resin
{
    /// <summary>
    ///     The service to keep track of users' resin.
    /// </summary>
    public class ResinTrackerService
    {
        /// <summary>
        ///     The cache to store resin updates in.
        /// </summary>
        readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        ///     Sets a <see cref="ResinUpdate"/> instance to the cache that expires once the value of that instance is meant to reach <see cref="ResinUpdate.MaxResin"/>
        /// </summary>
        /// <param name="user">The user to use for identification.</param>
        /// <param name="dt">The datetime of the update.</param>
        /// <param name="value">The updated value.</param>
        /// <returns>The cached ResinUpdate instance.</returns>
        public ResinUpdate SetValue(IUser user, DateTime dt, int value)
        {
            var update = new ResinUpdate(dt, value);
            _cache.Set(user.Id, update, update.FullyRefillsAt);
            return update;
        }

        /// <summary>
        ///     Gets the <see cref="ResinUpdate"/> instance from the cache at the ID of the given user, or null if it is not found.
        /// </summary>
        /// <param name="user">The user to get the object for.</param>
        /// <returns>The ResinUpdate instance.</returns>
        public ResinUpdate GetValue(IUser user) => _cache.TryGetValue<ResinUpdate>(user.Id, out var update) ? update : null;
    }
}