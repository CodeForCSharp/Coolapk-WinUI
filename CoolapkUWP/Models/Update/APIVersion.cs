using System.Text.RegularExpressions;

namespace CoolapkUWP.Models.Update
{
    public partial class APIVersion
    {
        public string Version { get; set; }
        public string VersionCode { get; set; }

        public APIVersion(string version, string versionCode)
        {
            Version = version;
            VersionCode = versionCode;
        }

        [GeneratedRegex(@"\+CoolMarket/(.*)-universal")]
        private static partial Regex CoolMarketRegex();

        public static APIVersion Parse(string line)
        {
            Match match = CoolMarketRegex().Match(line);
            if (match.Success && match.Groups.Count >= 2)
            {
                string value = match.Groups[1].Value;
                string[] lines = value.Split('-');
                if (lines.Length >= 2)
                {
                    return new APIVersion(lines[0].Trim(), lines[1].Trim());
                }
            }
            return null;
        }

        public override string ToString() => $"+CoolMarket/{Version}-{VersionCode}-universal";
    }
}
