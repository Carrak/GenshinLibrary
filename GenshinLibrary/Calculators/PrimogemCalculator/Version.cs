using System;

namespace GenshinLibrary.Calculators.PrimogemCalculator
{
    public class Version
    {
        public string VersionName { get; }
        public DateTime Start { get; }
        public DateTime End { get; }

        public Version(DateTime start, DateTime end, string versionName)
        {
            Start = start;
            End = end;
            VersionName = versionName;
        }

        public string Info()
        {
            return $"**{VersionName}** // **{Start:dd.MM.yyyy}** - **{End:dd.MM.yyyy}**";
        }

        public override string ToString()
        {
            return VersionName;
        }
    }
}
