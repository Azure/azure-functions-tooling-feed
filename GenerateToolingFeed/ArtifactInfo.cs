using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateToolingFeed
{
    public class ArtifactInfo
    {
        public string InProcArtifactVersion { get; set; }
        public string ConsolidatedBuildId { get; set; }
        public string DefaultArtifactVersion { get; set; }
    }
}
