using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace GenerateToolingFeed
{
    class Program
    {
        private static Dictionary<string, string> _feeds = new Dictionary<string, string>()
        {
            { Constants.FeedAllVersions, "https://raw.githubusercontent.com/Azure/azure-functions-tooling-feed/master/cli-feed-v3.json" },
            { Constants.FeedV1AndV2Only, "https://raw.githubusercontent.com/Azure/azure-functions-tooling-feed/master/cli-feed-v3-2.json" }
        };

        static void Main(string[] args)
        {
            string cliVersion = GetCliVersion(args[0]);
            if (NuGetVersion.TryParse(cliVersion, out NuGetVersion ver))
            {
                // update this feed for v2 only.
                if (ver.Major == 2)
                {
                    GenerateFeed(cliVersion, args[0], Constants.FeedV1AndV2Only, ver.Major);
                }

                // update this feed for v2 and v3
                GenerateFeed(cliVersion, args[0], Constants.FeedAllVersions, ver.Major);
            }
        }

        public static void GenerateFeed(string cliVersion, string coreToolsArtifactsDirectory, string feedName, int majorVersion)
        {
            Console.WriteLine($"Preparing CLI feed for version:{cliVersion}");
            var currentFeed = Helper.HttpClient.GetStringAsync(_feeds[feedName]).Result;
            var currentFeedJson = JObject.Parse(currentFeed);
            var targetFeedJson = JObject.Parse(currentFeed);

            string tag = Helper.GetTagFromMajorVersion(majorVersion);
            string releaseVersion = Helper.GetReleaseVersionFromTag(currentFeedJson, tag);

            var releaseJson = currentFeedJson["releases"][releaseVersion];
            var feedEntry = releaseJson.ToObject<FeedEntry>();

            var feedReleaseVersion = Helper.GetReleaseVersion(currentFeedJson["releases"], majorVersion);

            // Update the standalone entries
            var updatedCliEntry = UpdateStandaloneCli(feedEntry, coreToolsArtifactsDirectory, cliVersion);

            var cliEtnry = new CliEntry { OS = "windows", Architecture = "x86" };
            releaseJson["cli"] = Helper.GetDownloadLink(cliEtnry, cliVersion, isMinified: true);
            releaseJson["sha2"] = Helper.GetShaFileContent(cliEtnry, cliVersion, coreToolsArtifactsDirectory, isMinified: true);
            releaseJson["standaloneCli"] = JArray.FromObject(updatedCliEntry);
            releaseJson["itemTemplates"] = Helper.GetTemplateUrl("Microsoft.Azure.WebJobs.ItemTemplates", cliVersion);
            releaseJson["projectTemplates"] = Helper.GetTemplateUrl("Microsoft.Azure.WebJobs.ProjectTemplates", cliVersion);

            var targetFeedReleases = targetFeedJson["releases"];
            ((JObject)targetFeedReleases).Add(feedReleaseVersion, releaseJson);

            string prereleaseTag = majorVersion == 2 ? "v2-prerelease" : "v3-prerelease";
            targetFeedJson["tags"][prereleaseTag]["release"] = feedReleaseVersion;

            string path = Path.Combine(coreToolsArtifactsDirectory, feedName);
            string feedString = JsonConvert.SerializeObject(targetFeedJson, Formatting.Indented);

            Console.WriteLine("Writing File\n" + feedString);
            File.WriteAllText(path, feedString);
        }

        private static string GetCliVersion(string path)
        {
            string cliVersion = string.Empty;
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                if (file.EndsWith(".zip"))
                {
                    var zip = ZipFile.OpenRead(file);
                    foreach (var entry in zip.Entries)
                    {
                        if (entry.Name == "func.dll")
                        {
                            ZipFileExtensions.ExtractToFile(entry, "func.dll", true);
                            cliVersion = FileVersionInfo.GetVersionInfo(".\\func.dll").FileVersion;
                            break;
                        }
                    }
                }
            }
            return cliVersion;
        }

        private static CliEntry[] UpdateStandaloneCli(FeedEntry feedEntry, string coreToolsArtifactsDirectory, string cliVersion)
        {
            foreach (var cliEntry in feedEntry.standaloneCli)
            {
                string os = cliEntry.OS ?? cliEntry.OperatingSystem;
                if (os.Equals("Windows", StringComparison.OrdinalIgnoreCase) &&
                    cliEntry.Architecture.Equals("x64", StringComparison.OrdinalIgnoreCase))
                {
                    // Updating sha2
                    cliEntry.sha2 = Helper.GetShaFileContent(cliEntry, cliVersion, coreToolsArtifactsDirectory, true);
                    cliEntry.downloadLink = Helper.GetDownloadLink(cliEntry, cliVersion, true);
                }
                else
                {
                    // Updating sha2
                    cliEntry.sha2 = Helper.GetShaFileContent(cliEntry, cliVersion, coreToolsArtifactsDirectory);
                    cliEntry.downloadLink = Helper.GetDownloadLink(cliEntry, cliVersion);
                }
                
            }
            return feedEntry.standaloneCli;
        }
    }
}