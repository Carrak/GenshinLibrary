using Newtonsoft.Json;
using System;

namespace GenshinLibrary.Calculators.PrimogemCalculator
{
    public class Version
    {
        public string VersionName { get; }
        public DateTime Start { get; }
        public DateTime End { get; }
        public int Major { get; }
        public int Minor { get; }

        [JsonConstructor]
        public Version(
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
