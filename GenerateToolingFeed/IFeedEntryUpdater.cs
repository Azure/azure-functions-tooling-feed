using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenerateToolingFeed
{
    interface IFeedEntryUpdater
    {
        JToken GetUpdatedFeedEntry(JToken feed, CoreToolsInfo coreToolsInfo);
    }
}
