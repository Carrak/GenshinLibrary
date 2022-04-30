namespace GenshinLibrary.Calculators.PrimogemCalculator
{
    public class PrimogemCalculatorSettings
    {
        public int Primogems { get; }
        public int Acquaint { get; }
        public int Intertwined { get; }
        public int Abyss { get; }
        public int CurrSojourner { get; }
        public int CurrGnostic { get; }
        public int Hoyolab { get; }
        public bool Events { get; }
        public int Welkin { get; }
        public int Gnostic { get; }

        public PrimogemCalculatorSettings(int primogems, int acquaint, int intertwined, int abyss, int currSojourner, int currGnostic, int hoyolab, bool events, int welkin, int gnostic)
        {
            Primogems = primogems;
            Acquaint = acquaint;
            Intertwined = intertwined;
            Abyss = abyss;
            CurrSojourner = currSojourner;
            CurrGnostic = currGnostic;
            Hoyolab = hoyolab;
            Events = events;
            Welkin = welkin;
            Gnostic = gnostic;
        }
    }
}
