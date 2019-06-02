using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateToolingFeed
{
    public class FeedEntry
    {
        public string cli { get; set; }

        public string sha2 { get; set; }

        public string itemTemplates { get; set; }

        public string projectTemplates { get; set; }

        public CliEntry[] standaloneCli { get; set; }
    }

    public class CliEntry
    {
        public string OperatingSystem { get; set; }
        public string OS { get; set; }
        public string Architecture { get; set; }
        public string downloadLink { get; set; }
        public string sha2 { get; set; }
    }
}
