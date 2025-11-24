---
applyTo: "cli-feed-v4.json"
---

# Reviewing Feed Updates

## Review Workflow

**When reviewing any PR:**
1. Identify change types by examining the diff
2. Apply relevant checklist(s) - multiple may apply to a single PR
3. Verify critical invariants: version immutability, ordering (prerelease ≥ release), platform coverage
4. Report findings:
   - Bot PRs: Factual statements only (e.g., "All platforms present. SHA2 hashes verified.")
   - Human PRs: Constructive feedback with context

**Reject immediately if:**
- Existing version blocks modified (unless security fix with justification)
- Version blocks deleted (append-only model)
- Prerelease < release after changes (e.g., prerelease: `4.120.0`, release: `4.121.0`)
- The PR makes the feed invalid JSON

**Request changes if:**
- Execution model mismatch (isolated using in-process templates or vice versa)
- Missing SHA2 hashes in Core Tools downloads
- Unpaired updates (only v4 or v0 updated, or only standard without in-process)
- Missing platforms (requires all 5: linux-x64, osx-x64, osx-arm64, min.win-x64, min.win-arm64)
- .NET Framework isolated with non-empty `localContainerBaseImage`

**Diff verification:**
- Human PRs: Request diff/gist if not included in description
- Bot PRs (`azure-functions-release`): Skip diff check

**Security fix exception for immutability:** Allow modifications to existing version blocks only if ALL met:
- PR states "security fix", "critical correction", or "hash correction" with clear justification
- Changes limited to: URLs, hashes, or security fields only
- No changes to: version numbers, platform identifiers, or structure
- When applicable, note that all criteria were verified

## Review Checklists

> **IMPORTANT**: The following checklists correspond to different types of feed updates. If a PR contains multiple types of changes (e.g., both Core Tools updates and template updates), you must apply all relevant checklists. Do not refer to the checklists directly in your review comments; focus instead on findings.

**How to apply checklists:**
1. Examine the PR diff to identify change types
2. Apply each matching checklist independently
3. If any checklist item fails, note it in your review

**When multiple checklists apply:**
- Apply all relevant checklists independently (each checklist validates different aspects)
- If checklists have conflicting requirements, flag the PR for human review with details
- Always prioritize version ordering and immutability rules over other checks

### Core Tools Updates (Automated Bot PRs)

**When:** PR adds new version blocks (e.g., `"4.122.0": { ... }` and/or `"4.122.0-inprocess": { ... }`) with new Core Tools downloads
- Both standard and in-process releases updated together
- All 5 platforms present (linux-x64, osx-x64, osx-arm64, min.win-x64, min.win-arm64)
- Zip naming: standard `Azure.Functions.Cli.{platform}.{version}.zip`, in-process includes `_inproc` suffix
- Size fields: "full" for Linux/macOS, "minified" for Windows
- SHA2 hashes present (64-char hex strings)
- Download URL base path consistent across release

### Tag Promotion (Prerelease to Release)

**When:** PR only changes `release` field values in tag objects (`v4`, `v0`) without adding/modifying version blocks
- Only `v4.release` and `v0.release` changed
- Both v4 and v0 tags updated together
- Target version blocks already exist
- Version numbers match: standard (e.g., `4.121.0`) and in-process (`4.121.0-inprocess`)
- Prerelease tags unchanged
- Version ordering maintained: prerelease ≥ release
- No new version blocks or content modifications

### Template Updates

**When:** PR updates `itemTemplates` or `projectTemplates` URLs across any number of .NET versions
- All currently supported .NET versions updated together (unless noted in the PR description)
- EOL versions unchanged
- Isolated entries use `Microsoft.AzureFunctions.ProjectTemplate.CSharp.Isolated.3.x`
- In-process entries use `Microsoft.AzureFunctions.ProjectTemplate.CSharp.3.x`
- Item templates match execution model:
  - .NET Core isolated: `Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore`
  - .NET Framework isolated: `Microsoft.Azure.Functions.Worker.ItemTemplates.NetFx`
  - In-process: `Microsoft.Azure.WebJobs.ItemTemplates`
- Template versions consistent across supported versions

### Adding New .NET Version

**When:** PR adds a new `net{X}-isolated` entry to the `workerRuntimes.dotnet` object

**Naming:**
- Entry key: `net{major}-isolated`, `toolingSuffix` matches, `targetFramework`: `net{major}.0`

**Display info:**
- `displayName`: ".NET {major}.0", `targetFramework`: ".NET {major}"
- `description`: "Isolated Preview" (preview) or "Isolated LTS"/"Isolated" (GA)
- `hidden`: `true` (unannounced preview), `false` (announced/GA)
- `endOfLifeDate`: Official Microsoft EOL date

**SDK and templates:**
- `sdk.name`: `Microsoft.Azure.Functions.Worker.Sdk`, `sdk.version` supports target framework
- `capabilities`: includes new version, builds on prior versions
- `itemTemplates`: `.NetCore` variant for .NET 6+, `projectTemplateId.csharp`: `.Isolated.3.x`

**Container config:**
- `localContainerBaseImage`: `dotnet-isolated:4-dotnet-isolated{major}.0-appservice`
- `linuxFxVersion`: `DOTNET-ISOLATED|{major}.0`, `netFrameworkVersion`: `v{major}.0`

### Promoting Preview to GA

**When:** PR changes `displayInfo.description` from `"Isolated Preview"` to `"Isolated LTS"` or `"Isolated"` for an existing .NET version entry
- `description`: "Isolated Preview" → "Isolated LTS" (even-numbered) or "Isolated" (odd-numbered)
- SDK version updated if needed (to GA-ready version)
- Template packages updated if appropriate
- `hidden`: remains or becomes `false`
- `default`: typically remains `false`; only `true` when making it the new default
- Other fields unchanged (capabilities, container images, etc.)

### Changing Default .NET Version

**When:** PR changes which .NET version is the default

- New version's `default` set to `true`, previous default's `default` set to `false`
- Both changes in same PR
- Only one .NET runtime entry per release has `default: true`
- PR typically standalone

### EOL Version Updates

**When:** PR modifies entries for end-of-life .NET versions

- `hidden` remains `false` (ensures existing tools continue functioning)
- **Exception**: Legacy net6 in-process entry in v4 releases has `hidden: true`
- SDK/template versions not updated for EOL entries
- EOL versions included until major version retired

---

## Domain Reference

The sections below provide detailed specifications for understanding feed structure and conventions. Refer to these when validating specific fields or patterns during review.

### Tag Versions

**Tags**: `v4`, `v4-prerelease`, `v0`, `v0-prerelease`

**Meanings**:
- **v4**: Isolated worker model + one legacy in-process .NET 6 entry
- **v0**: Legacy in-process model only (EOL when .NET 8 reaches EOL)

**Invariants**:
- Standard releases (e.g., `4.121.0`) and in-process variants (e.g., `4.121.0-inprocess`) always paired
- Prerelease tags ≥ release tags

### Tag Promotion Approaches

**Staggered (two-phase)** - Most common, by `azure-functions-release` bot:
1. Prerelease PR: Updates `v4-prerelease` and `v0-prerelease`, adds new version blocks. Release tags unchanged.
2. Release PR: Updates only `v4.release` and `v0.release`. No new blocks, prerelease tags unchanged.

**Simultaneous (single-phase)** - For pre-validated human PRs: Updates all four tags and adds version blocks in same PR. Use when Core Tools already validated, metadata corrections need immediate consistency, or template updates need synchronized deployment.

### .NET Version Field Reference

**Naming:** Entry key `net{major}-isolated`, `toolingSuffix` matches, `targetFramework` is `net{major}.0`

**SDK:** Isolated uses `Microsoft.Azure.Functions.Worker.Sdk`, in-process uses `Microsoft.NET.Sdk.Functions`

**displayInfo:** `displayName` is ".NET {major}.0", `targetFramework` is ".NET {major}", `description` and `hidden` per table below, `endOfLifeDate` from Microsoft

**Description and hidden field values:**

| Version Type | `description` | Initial `hidden` | Can Change To |
|--------------|---------------|------------------|---------------|
| Preview (not announced) | `"Isolated Preview"` | `true` | `false` when ready for public use |
| Preview (announced) | `"Isolated Preview"` | `false` | No change |
| GA LTS (even #) | `"Isolated LTS"` | `false` | `true` when EOL (with exceptions*) |
| GA STS (odd #) | `"Isolated"` | `false` | `true` when EOL (with exceptions*) |

*Exception: Legacy net6 in-process entry in v4 releases is `hidden: true` to prevent duplication with v0 feed.

**Capabilities:** Add new version progressively (e.g., `"isolated,net6,net7,net8,net9,net10"`)

**Container patterns:**
- Isolated (.NET Core): `localContainerBaseImage` = `DOCKER|mcr.../dotnet-isolated:4-dotnet-isolated{major}.0-appservice`, `linuxFxVersion` = `DOTNET-ISOLATED|{major}.0`, `netFrameworkVersion` = `v{major}.0`
- Isolated (.NET Framework): `localContainerBaseImage` empty (Windows-only), `netFrameworkVersion` = `v6.0`
- In-process: `localContainerBaseImage` = `DOCKER|mcr.../dotnet:4-dotnet{major}.0-appservice`, `linuxFxVersion` = `DOTNET|{major}.0`, `netFrameworkVersion` = `v{major}.0`
