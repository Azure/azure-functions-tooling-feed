using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

namespace GenerateToolingFeed
{
    public class Helper
    {
        public static System.Net.Http.HttpClient HttpClient => _lazyClient.Value;

        public static string newReleaseVersion = string.Empty;

        public static string GetDownloadLink(string os, string architecture, string cliVersion, bool isMinified = false, string linkSuffix = "")
        {
            string rid = isMinified ? "min." : string.Empty;

            rid += GetRuntimeIdentifier(false, os, architecture);
            var url = $"https://functionscdn.azureedge.net/public/{cliVersion}/Azure.Functions.Cli.{rid}{linkSuffix}.{cliVersion}.zip";

            string bypassDownloadLinkValidation = Environment.GetEnvironmentVariable("bypassDownloadLinkValidation");
            if (bypassDownloadLinkValidation != "1" && !IsValidDownloadLink(url))
            {
                throw new Exception($"{url} is not valid or no found. Cannot generate cli feed file");
            }
            return url;
        }

        public static void UpdateCoreToolsReferences(V4FormatCliEntry[] cliEntries, string coreToolsArtifactsDirectory, string cliVersion, string linkSuffix)
        {
            foreach (var cliEntry in cliEntries)
            {
                bool minified = ShouldBeMinified(cliEntry);

                cliEntry.sha2 = GetShaFileContent(cliEntry.OS, cliEntry.Architecture, cliVersion, coreToolsArtifactsDirectory, minified, linkSuffix);
                cliEntry.downloadLink = GetDownloadLink(cliEntry.OS, cliEntry.Architecture, cliVersion, minified, linkSuffix);
            }
        }

        public static bool ShouldBeMinified(V4FormatCliEntry cliEntry)
        {
            return !string.IsNullOrEmpty(cliEntry.size)
                && string.Equals(cliEntry.size, "minified", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetNewReleaseVersion(JToken jToken, int majorReleaseVersion)
        {
            if (!string.IsNullOrEmpty(newReleaseVersion))
            {
                return newReleaseVersion;
            }

            List<String> versions = new List<string>();
            foreach (JProperty item in jToken)
            {
                string name = item.Name.Split("-").First();
                if (name.StartsWith($"{majorReleaseVersion}."))
                {
                    versions.Add(item.Name);
                }
            }

            if (!versions.Any())
            {
                return null;
            }

            var nuGetVersions = versions.Select(p =>
            {
                Version.TryParse(p, out Version version);
                return version;
            }).Where(v => v != null);
            var maxVersion = nuGetVersions.OrderByDescending(p => p).FirstOrDefault();
            Version releaseVersion = new Version(maxVersion.Major, maxVersion.Minor + 1, 0);
            newReleaseVersion = releaseVersion.ToString();
            return newReleaseVersion;
        }

        public static string GetShaFileContent(string os, string architecture, string cliVersion, string filePath, bool isMinified = false, string linkSuffix = "")
        {
            string rid = GetRuntimeIdentifier(isMinified, os, architecture);
            string fileName = $"Azure.Functions.Cli.{rid}{linkSuffix}.{cliVersion}.zip.sha2";

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

        public static void MergeObjectToJToken(JObject source, object toMerge)
        {
            // Clone source for iterating. That way we can modify acutal source in place
            JObject cloneSource = source.DeepClone() as JObject;

            foreach (var jsonItem in cloneSource)
            {
                string tokenName = jsonItem.Key;
                JToken tokenValue = jsonItem.Value;

                foreach (var prop in toMerge.GetType().GetProperties())
                {
                    if (string.Equals(tokenName, prop.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        source[tokenName] = JToken.FromObject(prop.GetValue(toMerge));
                        break;
                    }
                }
            }
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

        public static List<string> GetTagsFromMajorVersion(int majorVersion) => majorVersion switch
        {
            2 => ["v2"],
            3 => ["v3"],
            4 => ["v4", "v0"],
            _ => throw new ArgumentNullException($"{majorVersion} is not a supported version.", nameof(majorVersion))
        };
    }
}