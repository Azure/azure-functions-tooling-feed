using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace GenerateToolingFeed.V4Format
{
    internal class V4FormatFeedEntryUpdater : IFeedEntryUpdater
    {
        private readonly string _tag;

        private readonly IDictionary<string, string> _dotnetToItemTemplates = new Dictionary<string, string>()
        {
            { "net9-isolated", "Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore" },
            { "net8", "Microsoft.Azure.WebJobs.ItemTemplates" },
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

        private readonly IDictionary<string, string> _dotnetToProjectTemplates = new Dictionary<string, string>()
        {
            { "net9-isolated", "Microsoft.Azure.Functions.Worker.ProjectTemplates" },
            { "net8", "Microsoft.Azure.WebJobs.ProjectTemplates" },
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

        private static readonly IDictionary<string, string> _linkSuffix = new Dictionary<string, string>()
        {
            {
                "v0", "_inproc"
            }
        };

        public V4FormatFeedEntryUpdater(string tag)
        {
            _tag = tag;
        }

        public JObject GetUpdatedFeedEntry(JObject feed, CoreToolsInfo coreToolsInfo)
        {
            V4FormatFeedEntry feedEntry = feed.ToObject<V4FormatFeedEntry>();

            UpdateCoreToolsReferences(feedEntry.coreTools, coreToolsInfo);
            UpdateDotnetTemplatesToLatest(feedEntry.workerRuntimes, coreToolsInfo.MajorVersion);

            Helper.MergeObjectToJToken(feed, feedEntry);

            return feed;
        }

        public void UpdateCoreToolsReferences(V4FormatCliEntry[] cliEntries, CoreToolsInfo coreToolsInfo)
        {
            foreach (var cliEntry in cliEntries)
            {
                bool minified = Helper.ShouldBeMinified(cliEntry);

                string zipFileName = GetZipFileName(cliEntry.OS, cliEntry.Architecture, coreToolsInfo, _tag, minified);
                cliEntry.sha2 = GetShaFileContent(coreToolsInfo.ArtifactsDirectory, zipFileName);
                cliEntry.downloadLink = GetDownloadLink(cliEntry.OS, cliEntry.Architecture, coreToolsInfo, _tag, minified);
            }
        }

        private static string GetZipFileName(string os, string architecture, CoreToolsInfo coreToolsInfo, string tag, bool isMinified = false)
        {
            string rid = isMinified ? "min." : string.Empty;
            rid += Helper.GetRuntimeIdentifier(false, os, architecture);

            string linkSuffix = _linkSuffix.ContainsKey(tag) ? _linkSuffix[tag] : string.Empty;
            string version = _linkSuffix.ContainsKey(tag) ? coreToolsInfo.InprocVersion : coreToolsInfo.Version;
            return $"Azure.Functions.Cli.{rid}{linkSuffix}.{version}.zip";
        }

        private static string GetDownloadLink(string os, string architecture, CoreToolsInfo coreToolsInfo, string tag, bool isMinified = false)
        {
            string linkSuffix = _linkSuffix.ContainsKey(tag) ? _linkSuffix[tag] : string.Empty;
            string version = _linkSuffix.ContainsKey(tag) ? coreToolsInfo.InprocVersion : coreToolsInfo.Version;

            string rid = isMinified ? "min." : string.Empty;

            rid += Helper.GetRuntimeIdentifier(false, os, architecture);

            string containerName = $"{coreToolsInfo.MajorVersion}.0.{coreToolsInfo.BuildId}";
            var url = $"https://functionscdn.azureedge.net/public/{containerName}/Azure.Functions.Cli.{rid}{linkSuffix}.{version}.zip";

            string bypassDownloadLinkValidation = Environment.GetEnvironmentVariable("bypassDownloadLinkValidation");
            if (bypassDownloadLinkValidation != "1" && !Helper.IsValidDownloadLink(url))
            {
                throw new Exception($"{url} is not valid or no found. Cannot generate cli feed file");
            }
            return url;
        }

        public static string GetShaFileContent(string zipfilePath, string fileName)
        {
            string path = Path.Combine(zipfilePath, fileName);
            string shaFilePath = $"{path}.sha2";
            return File.ReadAllText(shaFilePath);
        }

        private void UpdateDotnetTemplatesToLatest(IDictionary<string, IDictionary<string, object>> workerRuntimes, int coreToolsMajor)
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
