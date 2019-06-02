using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace GenerateToolingFeed
{
    class Program
    {
        private const string _feedUrl = "https://raw.githubusercontent.com/Azure/azure-functions-tooling-feed/master/cli-feed-v3.json";

        static void Main(string[] args)
        {
            string cliVersion = Environment.GetEnvironmentVariable("cliVersion");
            Console.WriteLine($"Preparing CLI feed for version:{cliVersion}");

            string coreToolsArtifactsDirectory = Environment.GetEnvironmentVariable("coreToolsArtifactsDirectory");
            Console.WriteLine($"Preparing CLI feed for version:{coreToolsArtifactsDirectory}");

            string feedReleaseVersion = Environment.GetEnvironmentVariable("feedReleaseVersion");
            Console.WriteLine($"Update v2-prerelease to version:{feedReleaseVersion}");

            if (string.IsNullOrEmpty(feedReleaseVersion))
            {
                throw new Exception("feedReleaseVersion not set. Please set the feedReleaseVersion");
            }

            var currentFeed = Helper.HttpClient.GetStringAsync(_feedUrl).Result;
            var currentFeedJson = JObject.Parse(currentFeed);
            var targetFeedJson = JObject.Parse(currentFeed);

            string releaseVersion = Helper.GetReleaseVersionFromTag(currentFeedJson, tag: "v2");

            var releaseJson = currentFeedJson["releases"][releaseVersion];
            var feedEntry = releaseJson.ToObject<FeedEntry>();

            // Update the standalone entries
            var updatedCliEntry = UpdateStandaloneCli(feedEntry, coreToolsArtifactsDirectory, cliVersion);

            var cliEtnry = new CliEntry { OS = "windows", Architecture = "x86" };
            releaseJson["cli"] = Helper.GetDownloadLink(cliEtnry, cliVersion, isMinified: true);
            releaseJson["sha2"] = Helper.GetShaFileContent(cliEtnry, cliVersion, coreToolsArtifactsDirectory, isMinified: true);
            releaseJson["standaloneCli"] = JArray.FromObject(updatedCliEntry);
            releaseJson["itemTemplates"] = Helper.GetTemplateUrl("Microsoft.Azure.WebJobs.ItemTemplates");
            releaseJson["projectTemplates"] = Helper.GetTemplateUrl("Microsoft.Azure.WebJobs.ProjectTemplates");

            targetFeedJson.Add(feedReleaseVersion, releaseJson);
            targetFeedJson["tags"]["v2-prerelease"]["release"] = feedReleaseVersion;

            string path = Path.Combine(coreToolsArtifactsDirectory, "cli-feed-v3.json");
            string feedString = JsonConvert.SerializeObject(targetFeedJson, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            Console.WriteLine("Writing File\n" + feedString);
            File.WriteAllText(path, feedString);
        }

        private static CliEntry[] UpdateStandaloneCli(FeedEntry feedEntry, string coreToolsArtifactsDirectory, string cliVersion)
        {
            foreach (var cliEntry in feedEntry.standaloneCli)
            {
                // Updating sha2
                cliEntry.sha2 = Helper.GetShaFileContent(cliEntry, cliVersion, coreToolsArtifactsDirectory);
                cliEntry.downloadLink = Helper.GetDownloadLink(cliEntry, cliVersion);
            }
            return feedEntry.standaloneCli;
        }
    }
}