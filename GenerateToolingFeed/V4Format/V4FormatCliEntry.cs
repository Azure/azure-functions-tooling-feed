using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateToolingFeed
{
    public class V4FormatCliEntry
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string OS { get; set; }

        public string Architecture { get; set; }

        public string downloadLink { get; set; }

        public string sha2 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string size { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string @default { get; set; }
    }
}
