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

        public int Value { get; }
        public DateTime UpdatedAt { get; }
        public DateTime FullyRefillsAt { get; }

        public ResinUpdate(DateTime updatedAt, int value)
        {
            UpdatedAt = updatedAt;
            Value = value;
            FullyRefillsAt = updatedAt + (MaxResin - value) * TimeSpan.FromMinutes(RechargeRateMinutes);
        }

        public TimeSpan TimeBeforeFullRefill() => FullyRefillsAt - DateTime.UtcNow;
        public int GetCurrentResin() => Value + (int)((DateTime.UtcNow - UpdatedAt) / TimeSpan.FromMinutes(RechargeRateMinutes));
        public static string GetResinString(int value) => $"**{value} / {MaxResin}**";
    }
}
