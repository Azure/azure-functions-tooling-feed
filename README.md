# Azure Functions Tooling Feedss

The Azure Functions Tooling Feed is consumed by Visual Studio and Visual Studio code to make sure they are always using the latest [core tools](https://github.com/Azure/azure-functions-core-tools) and matching [templates](https://github.com/Azure/azure-functions-templates). The Azure Functions Tooling Feed is hosted at https://functionscdn.azureedge.net/public/cli-feed-v3.json.

## Structure
``` js
{
  "tags": {                                                    // Tags acts as a pointer to various release trains
    "v1": {                                                    // Release train for v1 Functions runtime (prod)
      "release": "1.2.0",                                      // Target release for this tag
      "displayName": "Azure Functions v1 (.NET Framework)",    // The fallback display text in case localized strings are not present
      "displayVersion": "v1",                                  // Strings used to construct localized display text
      "releaseQuality": "GA",                                  // Strings used to construct localized display text
      "targetFramework": ".NET Framework",                     // Strings used to construct localized display text
      "hidden": false                                          // Indicates whether the release train is publically visible
    }
  },
  "releases": {
    "1.2.0": {
      "cli": "",                                           // URL of the zip file contiaining the corresponding CLI
      "localEntryPoint": "func.exe",                       // How to start the CLI i.e. via dotnet.exe for func.dll or directly incase for func.exe
      "itemTemplates": "",                                 // URL of the NuGet Package for Item templates consumed by Visual Studio
      "projectTemplates": "",                              // URL of the NuGet Package for Item templates consumed by Visual Studio
      "templateApiZip": "",                                // URL of the zip file containing templates into JSON files.
      "sha2": ""                                           // SHA2 for the CLI zip
    }
  }
}
```
## Update Guidelines
1. Feed additive, so new releases are added to the feed. Existing releases are never removed from the feed.
2. Consider a version format of X.Y.Z (Major.Minor.Patch). 
    - Minor version update
        - For v1: "1.0.12” was updated to “1.1.0” when cli was updated from 1.0.12 to 1.0.13
        - For v2:  "2.0.1” was updated to “2.1.0” when cli was updated from 2.0.1-beta.25 to 2.0.1-beta.26 
    - Patch version update
        - For v1: "1.1.0” was updated to “1.1.1” when templates were updated from 1.0.3.10186 to 1.0.3.10187.
        - For v2:  "2.1.0” was updated to “2.1.1” when templates were updated from 2.0.0-beta-10177 to 2.0.0-beta-10178

## Related GitHub Repositories
- [Azure Functions Templates](https://github.com/Azure/azure-functions-templates)
- [Azure Functions Core Tools](https://github.com/Azure/azure-functions-core-tools)

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
