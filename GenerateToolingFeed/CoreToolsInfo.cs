using System;

namespace GenerateToolingFeed
{
    public class CoreToolsInfo
    {
        public CoreToolsInfo(string version, int majorVersion, string artifactsDirectory)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            ArtifactsDirectory = artifactsDirectory ?? throw new ArgumentNullException(nameof(artifactsDirectory));
            MajorVersion = majorVersion;
        }

        public string Version { get; }

        public int MajorVersion { get; }

        public string ArtifactsDirectory { get; }
    }
}
