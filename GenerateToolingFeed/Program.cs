using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Versioning;

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
            Console.WriteLine();
            Console.WriteLine($"Preparing CLI feed for version: '{coreToolsInfo.Version}' for feed: '{feedName}'");

            JObject feedJson = GetFeedJSON(feedName);
            FeedFormat format = _feedNameToFormat[feedName];

            if (TryUpdateFeedWithNewToolsAndTemplates(feedJson, format, coreToolsInfo))
            {
                string path = Path.Combine(coreToolsInfo.ArtifactsDirectory, feedName);
                WriteToJsonFile(feedJson, path);
            }
        }

        private static bool TryUpdateFeedWithNewToolsAndTemplates(JObject feed, FeedFormat format, CoreToolsInfo coreToolsInfo)
        {
            // Get a cloned object to not modify the exisiting release
            JObject currentReleaseEntryJson = GetCurrentReleaseEntry(feed, coreToolsInfo.MajorVersion).DeepClone() as JObject;
            JObject newReleaseEntryJson = GetNewReleaseEntryJson(currentReleaseEntryJson, format, coreToolsInfo);

            return TryAddNewReleaseToFeed(feed, newReleaseEntryJson, coreToolsInfo.MajorVersion);
        }

        private static JObject GetNewReleaseEntryJson(JObject currentReleaseEntry, FeedFormat format, CoreToolsInfo coreToolsInfo)
        {
            IFeedEntryUpdater feedEntryUpdater = FeedEntryUpdaterFactory.GetFeedEntryUpdater(format);
            return feedEntryUpdater.GetUpdatedFeedEntry(currentReleaseEntry, coreToolsInfo);
        }

        private static bool TryAddNewReleaseToFeed(JObject feed, JObject newRelease, int majorVersion)
        {
            JObject feedReleases = feed["releases"] as JObject;
            string newReleaseVersion = Helper.GetReleaseVersion(feedReleases, majorVersion);

            if (newReleaseVersion == null)
            {
                Console.WriteLine($"WARNING: No existing entries found for version {majorVersion}. You may have to manually add a version before this tool will work. Skipping this feed.");
                return false;
            }

            feedReleases.Add(newReleaseVersion, newRelease);
            UpdateFeedTagToNewVersion(feed, majorVersion, newReleaseVersion);
            return true;
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
            Console.WriteLine($"Searching for core tools builds in {path}...");
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