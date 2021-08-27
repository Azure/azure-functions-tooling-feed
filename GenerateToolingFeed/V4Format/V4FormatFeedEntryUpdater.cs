using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GenerateToolingFeed.V4Format
{
    internal class V4FormatFeedEntryUpdater : IFeedEntryUpdater
    {
        private static readonly IDictionary<string, string> _dotnetToTemplatesPrefix = new Dictionary<string, string>()
        {
            { "net6-isolated", "Microsoft.Azure.Functions.Worker" },
            { "net6", "Microsoft.Azure.WebJobs" },
            { "net5-isolated", "Microsoft.Azure.Functions.Worker" },
            { "netcore3", "Microsoft.Azure.WebJobs" },
            { "netcore2", "Microsoft.Azure.WebJobs" },
            { "netframework", "Microsoft.Azure.WebJobs" }
        };

        public JObject GetUpdatedFeedEntry(JObject feed, CoreToolsInfo coreToolsInfo)
        {
            V4FormatFeedEntry feedEntry = feed.ToObject<V4FormatFeedEntry>();

            UpdateCoreToolsReferences(feedEntry.coreTools, coreToolsInfo.ArtifactsDirectory, coreToolsInfo.Version);
            UpdateDotnetTemplatesToLatest(feedEntry.workerRuntimes, coreToolsInfo.MajorVersion);

            Helper.MergeObjectToJToken(feed, feedEntry);

            return feed;
        }

        private static void UpdateCoreToolsReferences(V4FormatCliEntry[] cliEntries, string coreToolsArtifactsDirectory, string cliVersion)
        {
            foreach (var cliEntry in cliEntries)
            {
                bool minified = ShouldBeMinified(cliEntry);

                cliEntry.sha2 = Helper.GetShaFileContent(cliEntry.OS, cliEntry.Architecture, cliVersion, coreToolsArtifactsDirectory, minified);
                cliEntry.downloadLink = Helper.GetDownloadLink(cliEntry.OS, cliEntry.Architecture, cliVersion, minified);
            }
        }

        private static bool ShouldBeMinified(V4FormatCliEntry cliEntry)
        {
            return !string.IsNullOrEmpty(cliEntry.size)
                && string.Equals(cliEntry.size, "minified", StringComparison.OrdinalIgnoreCase);
        }

        private static void UpdateDotnetTemplatesToLatest(IDictionary<string, IDictionary<string, object>> workerRuntimes, int coreToolsMajor)
        {
            if (!workerRuntimes.TryGetValue("dotnet", out IDictionary<string, object> dotnetInfo))
            {
                throw new Exception("Could not find 'dotnet' worker runtime information in the feed.");
            }

            foreach (KeyValuePair<string, object> keyValInfo in dotnetInfo)
            {
                string dotnetEntryLabel = keyValInfo.Key;
                JObject dotnetEntryToken = keyValInfo.Value as JObject;

                V4FormatDotnetEntry dotnetEntry = dotnetEntryToken?.ToObject<V4FormatDotnetEntry>() ?? throw new Exception($"Cannot parse 'dotnet' object in the feed with label '{dotnetEntryLabel}'");

                if (!_dotnetToTemplatesPrefix.TryGetValue(dotnetEntryLabel, out string templatePrefix))
                {
                    throw new Exception($"Cannot find the template package: Unidentified dotnet label '{dotnetEntryLabel}'.");
                }

                dotnetEntry.itemTemplates = Helper.GetTemplateUrl($"{templatePrefix}.ItemTemplates", coreToolsMajor);
                dotnetEntry.projectTemplates = Helper.GetTemplateUrl($"{templatePrefix}.ProjectTemplates", coreToolsMajor);

                Helper.MergeObjectToJToken(dotnetEntryToken, dotnetEntry);
            }
        }
    }
}
