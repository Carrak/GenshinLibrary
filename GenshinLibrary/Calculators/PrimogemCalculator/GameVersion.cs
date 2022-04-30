using Newtonsoft.Json;
using System;

namespace GenshinLibrary.Calculators.PrimogemCalculator
{
    public class GameVersion
    {
        public string VersionName { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public int Major { get; }
        public int Minor { get; }

        [JsonConstructor]
        public GameVersion(
            [JsonProperty("start")] DateTime start,
            [JsonProperty("end")] DateTime end,
            [JsonProperty("version_name")] string versionName)
        {
            Start = start;
            End = end;
            VersionName = versionName;

            var split = versionName.Split('.');
            Major = int.Parse(split[0]);
            Minor = int.Parse(split[1]);
        }

        public override string ToString()
        {
            return VersionName;
        }
    }
}
