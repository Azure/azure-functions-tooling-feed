﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GenerateToolingFeed.V4Format
{
    internal class V4FormatFeedEntryUpdater : IFeedEntryUpdater
    {
        private static readonly IDictionary<string, string> _dotnetToItemTemplates = new Dictionary<string, string>()
        {
            { "net8-isolated", "Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore" },
            { "net7-isolated",  "Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore"  },
            { "net6-isolated", "Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore"},
            { "net6", "Microsoft.Azure.WebJobs.ItemTemplates" },
            { "net5-isolated", "Microsoft.Azure.Functions.Worker.ItemTemplates"},
            { "netcore3", "Microsoft.Azure.WebJobs.ItemTemplates" },
            { "netcore2", "Microsoft.Azure.WebJobs.ItemTemplates" },
            { "netframework", "Microsoft.Azure.WebJobs.ItemTemplates" },
            { "netfx-isolated", "Microsoft.Azure.Functions.Worker.ItemTemplates.NetFx" }
        };

        private static readonly IDictionary<string, string> _dotnetToProjectTemplates = new Dictionary<string, string>()
        {
            { "net8-isolated", "Microsoft.Azure.Functions.Worker.ProjectTemplates" },
            { "net7-isolated", "Microsoft.Azure.Functions.Worker.ProjectTemplates" },
            { "net6-isolated", "Microsoft.Azure.Functions.Worker.ProjectTemplates" },
            { "net6", "Microsoft.Azure.WebJobs.ProjectTemplates" },
            { "net5-isolated", "Microsoft.Azure.Functions.Worker.ProjectTemplates" },
            { "netcore3", "Microsoft.Azure.WebJobs.ProjectTemplates" },
            { "netcore2", "Microsoft.Azure.WebJobs.ProjectTemplates" },
            { "netframework", "Microsoft.Azure.WebJobs.ProjectTemplates" },
            { "netfx-isolated", "Microsoft.Azure.Functions.Worker.ProjectTemplates" }
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

                if (!_dotnetToItemTemplates.TryGetValue(dotnetEntryLabel, out string itemTemplates))
                {
                    throw new Exception($"Cannot find the template package: Unidentified dotnet label '{dotnetEntryLabel}'.");
                }
                
                dotnetEntry.itemTemplates = Helper.GetTemplateUrl($"{itemTemplates}", coreToolsMajor);
                
                if (!_dotnetToProjectTemplates.TryGetValue(dotnetEntryLabel, out string projecTemplate))
                {
                    throw new Exception($"Cannot find the template package: Unidentified dotnet label '{dotnetEntryLabel}'.");
                }
                dotnetEntry.projectTemplates = Helper.GetTemplateUrl($"{projecTemplate}", coreToolsMajor);

                Helper.MergeObjectToJToken(dotnetEntryToken, dotnetEntry);
            }
        }
    }
}
