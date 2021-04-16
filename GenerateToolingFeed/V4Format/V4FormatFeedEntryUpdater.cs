using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GenerateToolingFeed.V4Format
{
    internal class V4FormatFeedEntryUpdater : IFeedEntryUpdater
    {
        private static readonly IDictionary<string, string> _dotnetToTemplatesPrefix = new Dictionary<string, string>()
        {
            { "net5-isolated", "Microsoft.Azure.Functions.Worker" },
            { "netcore3", "Microsoft.Azure.WebJobs" },
            { "netcore2", "Microsoft.Azure.WebJobs" },
            { "netframework", "Microsoft.Azure.WebJobs" }
        };

        public JToken GetUpdatedFeedEntry(JToken feed, CoreToolsInfo coreToolsInfo)
        {
            V4FormatFeedEntry feedEntry = feed.ToObject<V4FormatFeedEntry>();

            UpdateCoreToolsReferences(feedEntry.coreTools, coreToolsInfo.ArtifactsDirectory, coreToolsInfo.Version);
            UpdateDotnetTemplatesToLatest(feedEntry.workerRuntimes, coreToolsInfo.MajorVersion);

            return JToken.FromObject(feedEntry);
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
                && string.Equals(cliEntry.size, "minifed", StringComparison.OrdinalIgnoreCase);
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
                V4FormatDotnetEntry dotnetEntry = keyValInfo.Value as V4FormatDotnetEntry ?? throw new Exception("Cannot parse 'dotnet' object in the feed");

                if (!_dotnetToTemplatesPrefix.TryGetValue(dotnetEntryLabel, out string templatePrefix))
                {
                    throw new Exception($"Cannot find the template package: Unidentified dotnet label '{dotnetEntryLabel}'.");
                }

                dotnetEntry.itemTemplates = Helper.GetTemplateUrl($"{templatePrefix}.ItemTemplates", coreToolsMajor);
                dotnetEntry.projectTemplates = Helper.GetTemplateUrl($"{templatePrefix}.ProjectTemplates", coreToolsMajor);
            }
        }
    }
}
