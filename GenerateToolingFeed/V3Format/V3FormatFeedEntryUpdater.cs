﻿using Newtonsoft.Json.Linq;
using System;

namespace GenerateToolingFeed.V3Format
{
    internal class V3FormatFeedEntryUpdater : IFeedEntryUpdater
    {
        public JObject GetUpdatedFeedEntry(JObject feed, CoreToolsInfo coreToolsInfo)
        {
            var feedEntry = feed.ToObject<V3FormatFeedEntry>();

            UpdateCoreToolsReferences(feedEntry, coreToolsInfo);
            UpdateDotnetTemplatesToLatest(feedEntry, coreToolsInfo.MajorVersion);

            Helper.MergeObjectToJToken(feed, feedEntry);

            return feed;
        }

        private void UpdateCoreToolsReferences(V3FormatFeedEntry feedEntry, CoreToolsInfo coreToolsInfo)
        {
            var cliEntry = new V3FormatCliEntry { OS = "windows", Architecture = "x86" };

            feedEntry.cli = GetDownloadLink(cliEntry.OS ?? cliEntry.OperatingSystem,
                    cliEntry.Architecture, coreToolsInfo.Version, isMinified: true);

            feedEntry.sha2 = Helper.GetShaFileContent(cliEntry.OS ?? cliEntry.OperatingSystem,
                    cliEntry.Architecture, coreToolsInfo.Version, coreToolsInfo.ArtifactsDirectory, isMinified: true);

            feedEntry.standaloneCli = UpdateStandaloneCli(feedEntry.standaloneCli, coreToolsInfo.ArtifactsDirectory, coreToolsInfo.Version);
        }

        private void UpdateDotnetTemplatesToLatest(V3FormatFeedEntry feedEntry, int coreToolsMajor)
        {
            feedEntry.itemTemplates = Helper.GetTemplateUrl("Microsoft.Azure.WebJobs.ItemTemplates", coreToolsMajor);
            feedEntry.projectTemplates = Helper.GetTemplateUrl("Microsoft.Azure.WebJobs.ProjectTemplates", coreToolsMajor);
        }

        private static V3FormatCliEntry[] UpdateStandaloneCli(V3FormatCliEntry[] cliEntries, string coreToolsArtifactsDirectory, string cliVersion)
        {
            foreach (var cliEntry in cliEntries)
            {
                bool minified = ShouldBeMinified(cliEntry);

                cliEntry.sha2 = Helper.GetShaFileContent(cliEntry.OS ?? cliEntry.OperatingSystem,
                    cliEntry.Architecture, cliVersion, coreToolsArtifactsDirectory, minified);

                cliEntry.downloadLink = GetDownloadLink(cliEntry.OS ?? cliEntry.OperatingSystem,
                    cliEntry.Architecture, cliVersion, minified);
            }

            return cliEntries;
        }

        private static bool ShouldBeMinified(V3FormatCliEntry cliEntry)
        {
            string os = cliEntry.OS ?? cliEntry.OperatingSystem;

            return os.Equals("Windows", StringComparison.OrdinalIgnoreCase)
                && cliEntry.Architecture.Equals("x64", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDownloadLink(string os, string architecture, string cliVersion, bool isMinified = false, string linkSuffix = "")
        {
            string rid = isMinified ? "min." : string.Empty;

            rid += Helper.GetRuntimeIdentifier(false, os, architecture);
            var url = $"https://functionscdn.azureedge.net/public/{cliVersion}/Azure.Functions.Cli.{rid}{linkSuffix}.{cliVersion}.zip";

            string bypassDownloadLinkValidation = Environment.GetEnvironmentVariable("bypassDownloadLinkValidation");
            if (bypassDownloadLinkValidation != "1" && !Helper.IsValidDownloadLink(url))
            {
                throw new Exception($"{url} is not valid or no found. Cannot generate cli feed file");
            }
            return url;
        }
    }
}
