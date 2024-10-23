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
        private static readonly Dictionary<string, FeedFormat> _feedNameToFormat = new Dictionary<string, FeedFormat>()
        {
            { Constants.CliFeedV3, FeedFormat.V3 }, // update this feed for Functions v2 only.
            { Constants.CliFeedV32, FeedFormat.V3 }, // update this feed for Functions v2 and v3
            { Constants.CliFeedV4, FeedFormat.V4 }, // update v4 format feed for Functions v2, v3 and v4
        };

        static void Main(string[] args)
        {
            string artifactDirectoryPath = args[0];
            var artifactInfo = GetArtifactInfo(artifactDirectoryPath);
            var coreToolsInfo = new CoreToolsInfo(artifactInfo, artifactDirectoryPath);

            // update this feed for Functions v2 only.
            if (coreToolsInfo.MajorVersion == 2)
            {
                GenerateNewFeed(Constants.CliFeedV32, coreToolsInfo);
            }

            // update this feed for Functions v2 and v3
            GenerateNewFeed(Constants.CliFeedV3, coreToolsInfo);

            //update v4 format feed for Functions v2, v3 and v4
           GenerateNewFeed(Constants.CliFeedV4, coreToolsInfo);

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
            else
            {
                Console.WriteLine($"WARNING: No existing entries found for version {coreToolsInfo.MajorVersion} in {feedName}. You may have to manually add a version before this tool will work. Skipping this feed.");
            }
        }

        private static bool TryUpdateFeedWithNewToolsAndTemplates(JObject feed, FeedFormat format, CoreToolsInfo coreToolsInfo)
        {
            try
            {
                List<string> tags = Helper.GetTagsFromMajorVersion(coreToolsInfo.MajorVersion);

                bool result = true;
                foreach (string tag in tags)
                {
                    string releaseVersion = Helper.GetReleaseVersionFromTag(feed, tag);

                    // Get a cloned object to not modify the exisiting release
                    JObject currentReleaseEntryJson = feed["releases"][releaseVersion].DeepClone() as JObject;

                    JObject newReleaseEntryJson = GetNewReleaseEntryJson(currentReleaseEntryJson, format, coreToolsInfo, tag);

                    result &= TryAddNewReleaseToFeed(feed, newReleaseEntryJson, coreToolsInfo.MajorVersion, tag);
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        private static JObject GetNewReleaseEntryJson(JObject currentReleaseEntry, FeedFormat format, CoreToolsInfo coreToolsInfo, string tag)
        {
            IFeedEntryUpdater feedEntryUpdater = FeedEntryUpdaterFactory.GetFeedEntryUpdater(format, tag);
            return feedEntryUpdater.GetUpdatedFeedEntry(currentReleaseEntry, coreToolsInfo);
        }

        private static bool TryAddNewReleaseToFeed(JObject feed, JObject newRelease, int majorVersion, string tag)
        {
            JObject feedReleases = feed["releases"] as JObject;
            string newReleaseVersion = Helper.GetNewReleaseVersion(feedReleases, majorVersion);

            if (newReleaseVersion == null)
            {
                return false;
            }

            string versionSuffix = Constants.ReleaseVersionSuffix.ContainsKey(tag) ? Constants.ReleaseVersionSuffix[tag] : string.Empty;
            newReleaseVersion = $"{newReleaseVersion}{versionSuffix}";

            feedReleases.Add(newReleaseVersion, newRelease);
            string prereleaseTag = $"{tag}-prerelease";
            feed["tags"][prereleaseTag]["release"] = newReleaseVersion;
            return true;
        }

        private static void WriteToJsonFile(JObject content, string path)
        {
            string feedString = JsonConvert.SerializeObject(content, Formatting.Indented);

            Console.WriteLine("Writing File\n" + feedString);
            File.WriteAllText(path, feedString);
        }

        private static JObject GetFeedJSON(string feedName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "..", feedName);
            string feedContent = File.ReadAllText(path);
            return JObject.Parse(feedContent);
        }

        private static ArtifactInfo GetArtifactInfo(string path)
        {
            string filePath = Path.Combine(path, Constants.VersionMetadataFileName);
            ArtifactInfo artifactInfo = null;
            if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                artifactInfo = JObject.Parse(jsonString).ToObject<ArtifactInfo>();
            }
            return artifactInfo;
        }
    }
}