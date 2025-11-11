# Running GenerateToolingFeed

This tool creates a new version of the tooling feed with new Core Tools versions and templates. It reads version metadata, increments the minor version, updates download links and SHA2 hashes, fetches latest templates from NuGet, and updates prerelease tags.

## Prerequisites

Within the `GenerateToolingFeed` directory, create a new artifact directory with the files described in this section.

### metadata.json

This is a version information file. Remove the comments in the example below, and update the versions and build ID as needed.

```jsonc
{
  "DefaultArtifactVersion": "4.0.9999", // Core Tools version
  "InProcArtifactVersion": "4.0.9999-inproc", // In-process Core Tools version`
  "ConsolidatedBuildId": "12345678" // Build ID from Azure DevOps pipeline
}
```

### SHA2 files

10 files containing 64-character SHA256 hashes (one per line, no newline):

- Isolated builds (v4 tag): `Azure.Functions.Cli.{rid}.{version}.zip.sha2` for linux-x64, osx-x64, osx-arm64, min.win-x64, and min.win-arm64 runtime identifiers (RIDs)
- In-process builds (v0 tag): `Azure.Functions.Cli.{rid}_inproc.{version}-inproc.zip.sha2` for the same RIDs

## Running the Tool

> [!IMPORTANT]
> Run from the `GenerateToolingFeed/` directory. The tool expects feed files at `../cli-feed-*.json`.

Basic usage:

```powershell
cd GenerateToolingFeed
dotnet run -- <path-to-artifact-directory>
```

Example:

```powershell
cd GenerateToolingFeed
dotnet run -- .\test-artifacts\
```

Bypass download validation (for testing with unpublished builds):

```powershell
cd GenerateToolingFeed
$env:bypassDownloadLinkValidation="1"
dotnet run -- .\test-artifacts\
```

## Creating Test Artifacts

Quick setup script:

```powershell
cd GenerateToolingFeed
New-Item -ItemType Directory -Force -Path .\test-artifacts

@{
    DefaultArtifactVersion = "4.0.9999"
    InProcArtifactVersion = "4.0.9999-inproc"
    ConsolidatedBuildId = "12345678"
} | ConvertTo-Json | Out-File .\test-artifacts\metadata.json -Encoding UTF8

# Create dummy SHA2 files
$dummyHash = "1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef"
$rids = @("linux-x64", "osx-x64", "osx-arm64", "min.win-x64", "min.win-arm64")

foreach ($rid in $rids) {
    $dummyHash | Out-File ".\test-artifacts\Azure.Functions.Cli.$rid.4.0.9999.zip.sha2" -NoNewline
    $dummyHash | Out-File ".\test-artifacts\Azure.Functions.Cli.${rid}_inproc.4.0.9999-inproc.zip.sha2" -NoNewline
}

# Run with validation bypass
$env:bypassDownloadLinkValidation="1"
dotnet run -- .\test-artifacts\
```

## Output

The tool writes updated feed files to the artifact directory (e.g., `<artifact-directory>/cli-feed-v4.json`). Copy these back to the repository root to commit them. Console output shows which feeds were updated, version increments (e.g., `4.XXX.0` → `4.YYY.0`), and template package updates.

## Troubleshooting

**Wrong directory error:** Run from `GenerateToolingFeed/` not repo root.

**Missing SHA2 files:** Ensure all 10 .sha2 files exist with correct naming: `Azure.Functions.Cli.{rid}.{version}.zip.sha2` and `Azure.Functions.Cli.{rid}_inproc.{version}-inproc.zip.sha2`.

**Timeout/hanging:** Set `$env:bypassDownloadLinkValidation="1"` to skip CDN URL validation during testing.
