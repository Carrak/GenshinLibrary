using System;

namespace GenshinLibrary.Services.Resin
{
    /// <summary>
    ///     Represents an update made to a user's current resin.
    /// </summary>
    public class ResinUpdate
    {
        public static int MaxResin { get; } = 160;
        public static int RechargeRateMinutes { get; } = 8;

        public ulong UserID { get; }
        public DateTime UpdatedAt { get; }
        public int Value { get; }

        public DateTime FullyRefillsAt { get; }

        public ResinUpdate(ulong userId, DateTime updatedAt, int value)
        {
            UserID = userId;
            UpdatedAt = updatedAt;
            Value = value;
            FullyRefillsAt = updatedAt + (MaxResin - value) * TimeSpan.FromMinutes(RechargeRateMinutes);
        }

        public TimeSpan TimeBeforeFullRefill() => FullyRefillsAt - DateTime.UtcNow;
        public int GetCurrentResin() => Math.Min(MaxResin, Value + (int)((DateTime.UtcNow - UpdatedAt) / TimeSpan.FromMinutes(RechargeRateMinutes)));
        public static string GetResinString(int value) => $"**{value} / {MaxResin}**";
    }
}
