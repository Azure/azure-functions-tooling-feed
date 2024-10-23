using NuGet.Versioning;
using System;

namespace GenerateToolingFeed
{
    public class CoreToolsInfo
    {
        public CoreToolsInfo(ArtifactInfo artifactInfo, string artifactsDirectory)
        {
            if (artifactInfo == null)
            {
                Version = Helper.GetCliVersion(artifactsDirectory);
            }
            else
            {
                Version = artifactInfo?.DefaultArtifactVersion;
                InprocVersion = artifactInfo?.InProcArtifactVersion;
                BuildId = artifactInfo.ConsolidatedBuildId;
            }
            ArtifactsDirectory = artifactsDirectory ?? throw new ArgumentNullException(nameof(artifactsDirectory));

            var coreToolsVersion = NuGetVersion.Parse(Version);
            MajorVersion = coreToolsVersion.Major;
        }

        public string Version { get; }

        public string InprocVersion { get; }

        public int MajorVersion { get; }

        public string ArtifactsDirectory { get; }

        public string BuildId { get; }
    }
}
