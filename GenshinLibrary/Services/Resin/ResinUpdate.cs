using System;

namespace GenshinLibrary.Services.Resin
{
    /// <summary>
    ///     Represents an update made to a user's current resin.
    /// </summary>
    public class ResinUpdate
    {
        public const int MAX_RESIN = 160;
        public const int RESIN_RATE_MINUTES = 8;

        public ulong UserID { get; }
        public DateTime UpdatedAt { get; }
        public int Value { get; }
        public DateTime FullyRefillsAt { get; }
        public bool IsFull => GetCurrentResin() == MAX_RESIN;

        public ResinUpdate(ulong userId, DateTime updatedAt, int value)
        {
            UserID = userId;
            UpdatedAt = updatedAt;
            Value = value;
            FullyRefillsAt = updatedAt + (MAX_RESIN - value) * TimeSpan.FromMinutes(RESIN_RATE_MINUTES);
        }

        public TimeSpan UntilFullRefill() => IsFull ? TimeSpan.Zero : FullyRefillsAt - DateTime.UtcNow;
        public TimeSpan UntilNext() => IsFull ? TimeSpan.FromMinutes(RESIN_RATE_MINUTES) : TimeSpan.FromMinutes(RESIN_RATE_MINUTES - (DateTime.UtcNow - UpdatedAt).TotalMinutes % RESIN_RATE_MINUTES);
        public int GetCurrentResin() => Math.Min(MAX_RESIN, Value + (int)((DateTime.UtcNow - UpdatedAt) / TimeSpan.FromMinutes(RESIN_RATE_MINUTES)));

        public static string GetResinString(int value) => $"**{value} / {MAX_RESIN}**";
    }
}
