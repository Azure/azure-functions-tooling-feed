using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateToolingFeed
{
    public class Constants
    {
        public static readonly Dictionary<string, string> OperatingSystem = new Dictionary<string, string>()
            {
                { "linux" ,"linux" },
                { "macos" ,"osx" },
                { "windows" ,"win" }
            };

        public static readonly Dictionary<string, string> Architecture = new Dictionary<string, string>()
            {
                { "x64" ,"x64" },
                { "x86" ,"x86" },
                { "arm64" ,"arm64" }
            };

        public const string FeedAllVersions = "cli-feed-v3.json";
        public const string FeedV4FormatAllVersions = "cli-feed-v4.json";
        public const string FeedV1AndV2Only = "cli-feed-v3-2.json";
    }
}
