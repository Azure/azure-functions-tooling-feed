using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace GenerateToolingFeed
{
    class Program
    {
        private static readonly Dictionary<string, string> _feedNameToLink = new Dictionary<string, string>()
        {
            { Constants.FeedAllVersions, "https://raw.githubusercontent.com/Azure/azure-functions-tooling-feed/main/cli-feed-v3.json" },
            { Constants.FeedV1AndV2Only, "https://raw.githubusercontent.com/Azure/azure-functions-tooling-feed/main/cli-feed-v3-2.json" },
            { Constants.FeedV4FormatAllVersions, "https://raw.githubusercontent.com/Azure/azure-functions-tooling-feed/main/cli-feed-v4.json" }
        };

        private static readonly Dictionary<string, FeedFormat> _feedNameToFormat = new Dictionary<string, FeedFormat>()
        {
            { Constants.FeedAllVersions, FeedFormat.V3 },
            { Constants.FeedV1AndV2Only, FeedFormat.V3 },
            { Constants.FeedV4FormatAllVersions, FeedFormat.V4 }
        };

        static void Main(string[] args)
        {
            string coreToolsVersion = GetCliVersion(args[0]);
            if (NuGetVersion.TryParse(coreToolsVersion, out NuGetVersion ver))
            {
                var coreToolsInfo = new CoreToolsInfo(coreToolsVersion, ver.Major, args[0]);

                // update this feed for Functions v2 only.
                if (ver.Major == 2)
                {
                    GenerateNewFeed(Constants.FeedV1AndV2Only, coreToolsInfo);
                }

                // update this feed for Functions v2 and v3
                GenerateNewFeed(Constants.FeedAllVersions, coreToolsInfo);

                // update v4 format feed for Functions v2 and v3
                GenerateNewFeed(Constants.FeedV4FormatAllVersions, coreToolsInfo);
            }
            else
            {
                throw new Exception($"Could not parse the version for core tools located at '{args[0]}'");
            }
        }

        public static void GenerateNewFeed(string feedName, CoreToolsInfo coreToolsInfo)
        {
            Console.WriteLine($"Preparing CLI feed for version: '{coreToolsInfo.Version}' for feed: '{feedName}'");

            JObject feedJson = GetFeedJSON(feedName);
            FeedFormat format = _feedNameToFormat[feedName];

            UpdateFeedWithNewToolsAndTemplates(feedJson, format, coreToolsInfo);

            string path = Path.Combine(coreToolsInfo.ArtifactsDirectory, feedName);
            WriteToJsonFile(feedJson, path);
        }

        private static void UpdateFeedWithNewToolsAndTemplates(JObject feed, FeedFormat format, CoreToolsInfo coreToolsInfo)
        {
            JObject currentReleaseEntryJson = GetCurrentReleaseEntry(feed, coreToolsInfo.MajorVersion);
            JObject newReleaseEntryJson = GetNewReleaseEntryJson(currentReleaseEntryJson, format, coreToolsInfo);

            AddNewReleaseToFeed(feed, newReleaseEntryJson, coreToolsInfo.MajorVersion);
        }

        private static JObject GetNewReleaseEntryJson(JObject currentReleaseEntry, FeedFormat format, CoreToolsInfo coreToolsInfo)
        {
            IFeedEntryUpdater feedEntryUpdater = FeedEntryUpdaterFactory.GetFeedEntryUpdater(format);
            return feedEntryUpdater.GetUpdatedFeedEntry(currentReleaseEntry, coreToolsInfo);
        }

        private static void AddNewReleaseToFeed(JObject feed, JObject newRelease, int majorVersion)
        {
            JObject feedReleases = feed["releases"] as JObject;
            string newReleaseVersion = Helper.GetReleaseVersion(feedReleases, majorVersion);

            feedReleases.Add(newReleaseVersion, newRelease);
            UpdateFeedTagToNewVersion(feed, majorVersion, newReleaseVersion);
        }

        private static void UpdateFeedTagToNewVersion(JObject feed, int majorVersion, string newVersion)
        {
            string prereleaseTag = majorVersion == 2 ? "v2-prerelease" : "v3-prerelease";
            feed["tags"][prereleaseTag]["release"] = newVersion;
        }

        private static void WriteToJsonFile(JObject content, string path)
        {
            string feedString = JsonConvert.SerializeObject(content, Formatting.Indented);

            Console.WriteLine("Writing File\n" + feedString);
            File.WriteAllText(path, feedString);
        }

        private static JObject GetFeedJSON(string feedName)
        {
            string feedContent = Helper.HttpClient.GetStringAsync(_feedNameToLink[feedName]).Result;
            return JObject.Parse(feedContent);
        }

        private static JObject GetCurrentReleaseEntry(JObject feed, int majorVersion)
        {
            string tag = Helper.GetTagFromMajorVersion(majorVersion);
            string releaseVersion = Helper.GetReleaseVersionFromTag(feed, tag);

            return feed["releases"][releaseVersion] as JObject;
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
    }
}