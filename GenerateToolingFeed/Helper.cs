using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace GenerateToolingFeed
{
    public class Helper
    {
        public static System.Net.Http.HttpClient HttpClient => _lazyClient.Value;

        public static string GetDownloadLink(string os, string architecture, string cliVersion, bool isMinified = false)
        {
            string rid = string.Empty;
            if (isMinified)
            {
                rid = "min.";
            }
            rid += GetRuntimeIdentifier(false, os, architecture);
            var url = $"https://functionscdn.azureedge.net/public/{cliVersion}/Azure.Functions.Cli.{rid}.{cliVersion}.zip";

            // bypassing validation for now

            if (!IsValidDownloadLink(url))
            {
                throw new Exception($"{url} is not valid or no found. Cannot generate cli feed file");
            }
            return url;
        }

        public static string GetReleaseVersion(JToken jToken, int majorReleaseVersion)
        {
            List<String> versions = new List<string>();
            foreach (JProperty item in jToken)
            {
                string name = item.Name;
                if (name.StartsWith($"{majorReleaseVersion}."))
                {
                    versions.Add(item.Name);
                }
            }

            var nuGetVersions = versions.Select(p =>
            {
                Version.TryParse(p, out Version version);
                return version;
            }).Where(v => v != null);
            var maxVersion = nuGetVersions.OrderByDescending(p => p).FirstOrDefault();
            Version releaseVersion = new Version(maxVersion.Major, maxVersion.Minor + 1, 0);
            return releaseVersion.ToString();
        }

        public static string GetShaFileContent(string os, string architecture, string cliVersion, string filePath, bool isMinified = false)
        {
            string rid = GetRuntimeIdentifier(isMinified, os, architecture);
            string fileName = $"Azure.Functions.Cli.{rid}.{cliVersion}.zip.sha2";

            string path = Path.Combine(filePath, fileName);
            return File.ReadAllText(path);
        }

        public static string GetLatestPackageVersion(string packageId, int cliMajor)
        {
            string url = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLower()}/index.json";
            var response = HttpClient.GetStringAsync(url).Result;
            var versionsObject = JObject.Parse(response);

            var versions = JsonConvert.DeserializeObject<IEnumerable<string>>(versionsObject["versions"].ToString());

            var nuGetVersions = versions.Select(p =>
            {
                if (NuGetVersion.TryParse(p, out NuGetVersion nuGetVersion) && nuGetVersion.Major == cliMajor)
                {
                    return nuGetVersion;
                }
                return null;
            }).Where(v => v != null);
            return nuGetVersions.OrderByDescending(p => p.Version).FirstOrDefault()?.ToString();
        }

        public static string GetTemplateUrl(string packageId, int cliMajor)
        {
            string version = GetLatestPackageVersion(packageId, cliMajor);
            return $"https://www.nuget.org/api/v2/package/{packageId}/{version}";
        }

        public static string GetRuntimeIdentifier(bool isMinified, string os, string architecture)
        {
            string rid = string.Empty;
            if (isMinified)
            {
                rid = "min.";
            }

            os = os.ToLower();
            if (Constants.OperatingSystem.TryGetValue(os, out string osValue))
            {
                rid = rid + osValue;
            }
            else
            {
                throw new Exception($"Could not find matching value for {os} to generate rid");
            }


            if (Constants.Architecture.TryGetValue(architecture, out string architectureValue))
            {
                rid = rid + "-" + architectureValue;
            }
            else
            {
                throw new Exception($"Could not find matching value for {architecture} to generate rid");
            }

            return rid;
        }

        public static bool IsValidDownloadLink(string url)
        {
            var result = HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
            return !(result.StatusCode == HttpStatusCode.NotFound || !result.IsSuccessStatusCode);
        }

        private static Lazy<System.Net.Http.HttpClient> _lazyClient = new Lazy<System.Net.Http.HttpClient>(() =>
        {
            return new System.Net.Http.HttpClient(new HttpClientHandler
            {
                MaxConnectionsPerServer = 50
            });
        });

        public static string GetReleaseVersionFromTag(JObject json, string tag)
        {
            var releaseVersion = json?["tags"]?[tag]?["release"].ToString();
            return releaseVersion;
        }

        public static string GetTagFromMajorVersion(int majorVersion)
        {
            switch (majorVersion)
            {
                case 2:
                    return "v2";
                case 3:
                    return "v3";
                default:
                    return "v2";
            }
            
        }

    }
}
