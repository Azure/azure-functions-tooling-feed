using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using GenerateToolingFeed;
using System.IO.IsolatedStorage;
using Moq;
using Newtonsoft.Json;

namespace TestGenerateToolingFeed.V4Format
{
    public class HelperTests
    {
        [Theory]
        [InlineData("Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore", 4, "https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore/4.0.2945")]
        [InlineData("Microsoft.Azure.Functions.Worker.ProjectTemplates", 4, "https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ProjectTemplates/4.0.2945")]
        [InlineData("Microsoft.Azure.WebJobs.ItemTemplates", 3, "https://www.nuget.org/api/v2/package/Microsoft.Azure.WebJobs.ItemTemplates/3.1.2945")]
        [InlineData("Microsoft.Azure.WebJobs.ProjectTemplates", 3, "https://www.nuget.org/api/v2/package/Microsoft.Azure.WebJobs.ProjectTemplates/3.1.2945")]
        [InlineData("Microsoft.Azure.Functions.Worker.ItemTemplates.NetFx", 4, "https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ItemTemplates.NetFx/4.0.2945")]
        [InlineData("Microsoft.Azure.Functions.Worker.ItemTemplates", 4, "https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ItemTemplates/4.0.2945")]
        public void Test_GetTemplateUrl(string packageId, int cliMajor, string expectedVersion)
        {
            var version = Helper.GetTemplateUrl(packageId, cliMajor);
            Assert.Equal(expectedVersion, version);
        }

        [Theory]
        [InlineData("https://raw.githubusercontent.com/Azure/azure-functions-tooling-feed/main/cli-feed-v4.json", "v4")]
        [InlineData("https://raw.githubusercontent.com/Azure/azure-functions-tooling-feed/main/cli-feed-v4.json", "v4-prerelease")]
        public void Test_GetReleaseVersionFromTag(string url, string tag)
        {
            using (var httpClient = new HttpClient())
            {
                var json = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                var feed = JObject.Parse(json);
                var resultVersion = Helper.GetReleaseVersionFromTag(feed, tag);

                Assert.True(resultVersion != null);
            }
        }
        
        [Theory]
        [InlineData("net6-isolated", "foo-net6-isolated-item", "foo-net6-isolated-project")]
        [InlineData("netfx-isolated", "foo-netfx-isolated-item", "foo-netfx-isolated-project")]
        public void Test_UpdateDotnetTemplatesToLatest(string dotnetVersion, string updatedItem, string updatedProject)
        {
            var v4json = $@"
{{
""workerRuntimes"": {{
        ""dotnet"": {{
          ""net6-isolated"": {{
            ""displayInfo"": {{
              ""displayName"": "".NET 6.0"",
              ""hidden"": false,
              ""displayVersion"": ""v4"",
              ""targetFramework"": "".NET 6"",
              ""description"": ""Isolated LTS"",
              ""endOfLifeDate"": ""2024-11-12T00:00:00Z""
            }},
            ""capabilities"": ""isolated,net6"",
            ""sdk"": {{
              ""name"": ""Microsoft.Azure.Functions.Worker.Sdk"",
              ""version"": ""1.3.0""
            }},
            ""default"": false,
            ""toolingSuffix"": ""net6-isolated"",
            ""localEntryPoint"": ""dotnet.exe"",
            ""targetFramework"": ""net6.0"",
            ""itemTemplates"": ""https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore/4.0.2945"",
            ""projectTemplates"": ""https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ProjectTemplates/4.0.2945"",
            ""projectTemplateId"": {{
              ""csharp"": ""Microsoft.AzureFunctions.ProjectTemplate.CSharp.Isolated.3.x""
            }},
            ""localContainerBaseImage"": ""DOCKER|mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated6.0-appservice"",
            ""serviceAppSettings"": {{
              ""FUNCTIONS_EXTENSION_VERSION"": ""~4"",
              ""FUNCTIONS_WORKER_RUNTIME"": ""dotnet-isolated"",
              ""WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED"": ""1""
            }},
            ""windowsSiteConfig"": {{
              ""netFrameworkVersion"": ""v6.0""
            }},
            ""linuxSiteConfig"": {{
              ""linuxFxVersion"": ""DOTNET-ISOLATED|6.0""
            }}
          }},
          ""netfx-isolated"": {{
            ""displayInfo"": {{
              ""displayName"": "".NET Framework"",
              ""hidden"": false,
              ""displayVersion"": ""v4"",
              ""targetFramework"": "".NET Framework"",
              ""description"": ""Isolated""
            }},
            ""capabilities"": ""isolated,net6,netfxisolated"",
            ""sdk"": {{
              ""name"": ""Microsoft.Azure.Functions.Worker.Sdk"",
              ""version"": ""1.16.2""
            }},
            ""default"": false,
            ""toolingSuffix"": ""netfx-isolated"",
            ""localEntryPoint"": ""dotnet.exe"",
            ""targetFramework"": ""net48"",
            ""itemTemplates"": ""https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ItemTemplates.NetFx/4.0.2945"",
            ""projectTemplates"": ""https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ProjectTemplates/4.0.2945"",
            ""projectTemplateId"": {{
              ""csharp"": ""Microsoft.AzureFunctions.ProjectTemplate.CSharp.Isolated.3.x""
            }},
            ""localContainerBaseImage"": """",
            ""serviceAppSettings"": {{
              ""FUNCTIONS_EXTENSION_VERSION"": ""~4"",
              ""FUNCTIONS_WORKER_RUNTIME"": ""dotnet-isolated""
            }},
            ""windowsSiteConfig"": {{
              ""netFrameworkVersion"": ""v6.0""
            }}
          }}
        }}
      }},
      ""coreTools"": [
        {{
          ""OS"": ""Linux"",
          ""Architecture"": ""x64"",
          ""downloadLink"": ""https://functionscdn.azureedge.net/public/4.0.5504/Azure.Functions.Cli.linux-x64.4.0.5504.zip"",
          ""sha2"": ""a96fd30d6b7c4a04678ee20492e52ceff0a44c382aeac1a8dab0d0f61a42ebd7"",
          ""size"": ""full"",
          ""default"": ""false""
        }},
        {{
          ""OS"": ""Windows"",
          ""Architecture"": ""x86"",
          ""downloadLink"": ""https://functionscdn.azureedge.net/public/4.0.5504/Azure.Functions.Cli.min.win-x86.4.0.5504.zip"",
          ""sha2"": ""c8cc60f152dea5c79fc0ab5aab16627de99470e82743944ac61e563d25fd995f"",
          ""size"": ""minified"",
          ""default"": ""true""
        }}
      ]
}}";
            IDictionary<string, List<string>> testDotnetToItemTemplatesPrefix = new Dictionary<string, List<string>>()
            {
                { "net8-isolated", new List<string> { "Microsoft.Azure.Functions.Worker.ItemTemplates", "Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore" } },
                { "net7-isolated", new List<string> { "Microsoft.Azure.Functions.Worker.ItemTemplates", "Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore" } },
                { "net6-isolated", new List<string> { "Microsoft.Azure.Functions.Worker.ItemTemplates", "Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore" } },
                { "net6", new List<string> { "Microsoft.Azure.WebJobs.ItemTemplates" } },
                { "net5-isolated", new List<string> {"Microsoft.Azure.Functions.Worker" } },
                { "netcore3", new List<string> { "Microsoft.Azure.WebJobs.ItemTemplates" } },
                { "netcore2", new List<string> { "Microsoft.Azure.WebJobs.ItemTemplates" } },
                { "netframework", new List<string> { "Microsoft.Azure.WebJobs.ItemTemplates" } },
                { "netfx-isolated", new List<string> { "Microsoft.Azure.Functions.Worker.ItemTemplates", "Microsoft.Azure.Functions.Worker.ItemTemplates.NetFx" } }
            };
        var feedEntry = JsonConvert.DeserializeObject<V4FormatFeedEntry>(v4json);
            if (feedEntry != null && feedEntry.workerRuntimes.TryGetValue("dotnet", out IDictionary<string, object> dotnetInfo))
            {
                foreach (KeyValuePair<string, object> keyValInfo in dotnetInfo)
                {
                    string dotnetEntryLabel = keyValInfo.Key;
                    JObject? dotnetEntryToken = keyValInfo.Value as JObject;

                    V4FormatDotnetEntry dotnetEntry = dotnetEntryToken?.ToObject<V4FormatDotnetEntry>() ?? throw new Exception($"Cannot parse 'dotnet' object in the feed with label '{dotnetEntryLabel}'");
                    testDotnetToItemTemplatesPrefix.TryGetValue(dotnetEntryLabel, out List<string> itemTemplatePrefixes);
                    

                    // pre-processing asserts to make sure input parsing is fine
                    Assert.NotNull(dotnetEntryToken);
                    Assert.NotNull(dotnetEntry);
                    Assert.NotNull(dotnetEntry.itemTemplates);
                    Assert.NotNull(dotnetEntry.projectTemplates);
                    
                    dotnetEntry.itemTemplates = updatedItem;
                    dotnetEntry.projectTemplates = updatedProject;
                    Helper.MergeObjectToJToken(dotnetEntryToken, dotnetEntry);

                    // post-processing asserts to make sure input parsing is fine
                    Assert.NotNull(dotnetEntryToken);
                    V4FormatDotnetEntry dotnetEntryUpdated = dotnetEntryToken?.ToObject<V4FormatDotnetEntry>() ?? throw new Exception($"Cannot parse 'dotnet' object in the feed with label '{dotnetEntryLabel}'");
                    //testDotnetToItemTemplatesPrefix.TryGetValue(dotnetEntryLabel, out List<string> itemTemplatePrefixes);
                    Assert.NotNull(dotnetEntryUpdated);
                    Assert.NotNull(dotnetEntryUpdated.itemTemplates);
                    Assert.Equal(updatedItem, dotnetEntryUpdated.itemTemplates);
                    Assert.NotNull(dotnetEntryUpdated.projectTemplates);
                    Assert.Equal(updatedProject, dotnetEntryUpdated.projectTemplates);

                    Console.WriteLine(dotnetEntryToken);
                }
            }
        }
    }
}