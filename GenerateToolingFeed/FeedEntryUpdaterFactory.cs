using GenerateToolingFeed.V3Format;
using GenerateToolingFeed.V4Format;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateToolingFeed
{
    internal static class FeedEntryUpdaterFactory
    {
        public static IFeedEntryUpdater GetFeedEntryUpdater(FeedFormat format)
        {
            switch (format)
            {
                case FeedFormat.V3:
                    return new V3FormatFeedEntryUpdater();
                case FeedFormat.V4:
                    return new V4FormatFeedEntryUpdater();
                default:
                    throw new InvalidOperationException($"Unidentified feed format '{format}'");
            }
        }
    }
}
