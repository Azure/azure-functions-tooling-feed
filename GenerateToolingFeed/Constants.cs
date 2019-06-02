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
                { "x86" ,"x86" }
            };
    }
}
