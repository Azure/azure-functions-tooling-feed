using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateToolingFeed
{
    public class V4FormatFeedEntry
    {
        public V4FormatCliEntry[] coreTools { get; set; }

        public IDictionary<string, IDictionary<string, object>> workerRuntimes { get; set; }
    }
}
