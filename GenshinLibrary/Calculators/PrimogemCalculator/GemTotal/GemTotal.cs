using Discord;
using System.Collections.Generic;
using System.Linq;

namespace GenshinLibrary.ReactionCallback.PrimogemCalculator
{
    public class GemTotal
    {
        public string Name { get; set; }
        public Emote Emote { get; }
        public List<Reward> Rewards { get; }

        public GemTotal(Emote emote, string name, params Reward[] totals)
        {
            Name = name;
            Emote = emote;
            Rewards = new List<Reward>(totals);
        }

        public override string ToString()
        {
            return string.Join('\n', Rewards.Select(x => x.Convert()));
        }

        public EmbedFieldBuilder ToField()
        {
            EmbedFieldBuilder field = new EmbedFieldBuilder();
            field.WithName($"{Emote} {Name}")
                .WithValue(ToString());
            return field;
        }

        public int[] GetTotalCurrencies()
        {
            int[] currencies = new int[3];
            foreach (var reward in Rewards)
                switch (reward.Currency)
                {
                    case Currency.Primogems: currencies[0] += reward.GetTotal(); break;
                    case Currency.Acquaint: currencies[1] += reward.GetTotal(); break;
                    case Currency.Intertwined: currencies[2] += reward.GetTotal(); break;
                }
            return currencies;
        }
    }
}
