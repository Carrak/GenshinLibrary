using Discord.Commands;
using System;

namespace GenshinLibrary.Calculators.PrimogemCalculator
{
    [NamedArgumentType]
    public class PrimogemCalculatorSettings
    {
        // Current
        public int Primogems { get; set; }
        public int Acquaint { get; set; }
        public int Intertwined { get; set; }
        // Custom
        public int Abyss { get; set; }
        public int CurrSojourner { get; set; }
        public int CurrGnostic { get; set; }
        public int Hoyolab { get; set; }
        public bool Events { get; set; }
        // Purchaseables
        public int Welkin { get; set; }
        public int Gnostic { get; set; }

        public void Validate()
        {
            if (Primogems < 0)
                throw new Exception($"`{nameof(Primogems)}` must be above 0.");

            if (Acquaint < 0)
                throw new Exception($"`{nameof(Acquaint)}` must be above 0.");

            if (Intertwined < 0)
                throw new Exception($"`{nameof(Intertwined)}` must be above 0");

            if (Abyss % 50 != 0 || Abyss < 0 || Abyss > 600)
                throw new Exception($"`{nameof(Abyss)}` must be a number that divides by 50 in range from 0 to 600");

            if (CurrSojourner != 0 && CurrGnostic != 0)
                throw new Exception($"Only specify one of the two: `{nameof(CurrSojourner)}` or `{nameof(CurrGnostic)}`");

            if (CurrSojourner < 0 || CurrSojourner > 49)
                throw new Exception($"`{nameof(CurrSojourner)}` must be from 0 to 49.");

            if (CurrGnostic < 0 || CurrGnostic > 49)
                throw new Exception($"`{nameof(CurrGnostic)}` must be from 0 to 49.");

            if (Welkin < 0 || Welkin > 180)
                throw new Exception($"`{nameof(Welkin)}` must be in range from 0 to 180.");

            if (Gnostic < 0 || Gnostic > 10)
                throw new Exception($"`{nameof(Gnostic)}` must be from 1 to 10");

            if (Hoyolab < 0 || Hoyolab > 30)
                throw new Exception($"`{nameof(Hoyolab)}` must be between 0 and 30");

            if (Hoyolab > DateTime.UtcNow.Day)
                throw new Exception($"`{nameof(Hoyolab)}` can't be bigger than the current date.");
        }
    }
}
