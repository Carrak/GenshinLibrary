using Discord;
using GenshinLibrary.Models;
using System;

namespace GenshinLibrary.ReactionCallback.PrimogemCalculator
{
    public class Reward
    {
        public Currency Currency { get; set; }
        public int Quantity { get; set; }
        public int Amount { get; set; }

        public Reward(Currency currency, int quantity, int amount)
        {
            Currency = currency;
            Quantity = quantity;
            Amount = amount;
        }

        public string Convert() => $"{Quantity} × **{Amount}**{GetEmote()} = **{Quantity * Amount}**{GetEmote()}";

        public Emote GetEmote()
        {
            return Currency switch
            {
                Currency.Primogems => GenshinEmotes.Primogem,
                Currency.Acquaint => GenshinEmotes.Acquaint,
                Currency.Intertwined => GenshinEmotes.Intertwined,
                _ => throw new NotImplementedException()
            };
        }

        public int GetTotal() => Quantity * Amount;
    }
}
