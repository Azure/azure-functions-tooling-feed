using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateToolingFeed
{
    public class V3FormatFeedEntry
    {
        public string cli { get; set; }

        public string sha2 { get; set; }

        public string itemTemplates { get; set; }

        public string projectTemplates { get; set; }

        public V3FormatCliEntry[] standaloneCli { get; set; }
    }
}
