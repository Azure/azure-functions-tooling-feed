# Update Feed for .NET Version

This guide explains how to update the `cli-feed-v4.json` file. There are **two distinct scenarios** for updating the feed.

---

## 🔀 Choose Your Scenario

### **Scenario 1: Add a New .NET Version** 
Adding a completely new .NET version to the feed (e.g., adding .NET 11 for the first time).

**Example**: [See this gist](https://gist.github.com/kshyju/7a230fa42e4bad1f2b587711d0138271/revisions#diff-9a27a8941d5a3e9c63838706f8607bd4d763297ceaeb0aa1df418daccb80bdb8)

### **Scenario 2: Bump Template Versions**
Updating item/project template versions for existing .NET versions. May also include:
- Upgrading a preview version to GA (update description)
- Updating the Worker SDK to the latest version

---

## 📥 Scenario 1: Add a New .NET Version

### Required User Input

Gather the following information before starting:

1. **displayName** (e.g., `.NET 11.0`)
2. **targetFramework** (e.g., `.NET 11`)
3. **description** (e.g., `Isolated`, `Isolated LTS`, or `Isolated Preview`)
4. **endOfLifeDate** (e.g., `2029-11-10T00:00:00Z`)
5. **Item/Project Templates Version** (e.g., `4.0.5400`)
6. **SDK Version** (e.g., `2.1.0`)

### Steps Overview

#### **Commit 1: Copy Current Release Block**
1. Locate the **latest release block** in `cli-feed-v4.json` (e.g., `4.118.0`)
2. **Copy only the main block** (NOT the `-inprocess` variant, as we don't support new in-process versions)
3. Paste the copied block **directly below** the original
4. **Do not modify anything** - just duplicate it
5. **Commit this change as Commit 1**

#### **Commit 2: Update the Copied Block**

1. **Increment the version number**:
   - Change `4.118.0` → `4.119.0`

2. **Add the new .NET runtime configuration**:
   - Copy the most recent .NET isolated block (e.g., `net10-isolated`)
   - Rename it to match the new version (e.g., `net11-isolated`)

3. **Update all fields in the new runtime block**:
   - **displayInfo**:
     - `displayName`: Use provided value (e.g., `.NET 11.0`)
     - `targetFramework`: Use provided value (e.g., `.NET 11`)
     - `description`: Use provided value (e.g., `Isolated`, `Isolated LTS`, or `Isolated Preview`)
     - `endOfLifeDate`: Use provided value
     - `hidden`: Set to `false`
   
   - **capabilities**: Add the new version to the list
     - Example: `isolated,net6,net7,net8,net9,net10` → `isolated,net6,net7,net8,net9,net10,net11`
   
   - **sdk**:
     - `version`: Use provided SDK version if provided in input
   
   - **toolingSuffix**: Update to match new version (e.g., `net11-isolated`)
   
   - **targetFramework**: Update to match new version (e.g., `net11.0`)
   
   - **itemTemplates**: Update version in URL if provided in input
     - Format: `https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore/4.0.XXXX`
   
   - **projectTemplates**: Update version in URL
     - Format: `https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ProjectTemplates/4.0.XXXX`
   
   - **localContainerBaseImage**: Update .NET version
     - Example: `dotnet-isolated10.0-appservice` → `dotnet-isolated11.0-appservice`
   
   - **windowsSiteConfig**:
     - `netFrameworkVersion`: Update (e.g., `v10.0` → `v11.0`)
   
   - **linuxSiteConfig**:
     - `linuxFxVersion`: Update (e.g., `DOTNET-ISOLATED|10.0` → `DOTNET-ISOLATED|11.0`)

4. **Commit all changes as Commit 2**

---

## 📥 Scenario 2: Bump Template Versions

### Required User Input

Gather the following information before starting:

1. **Item/Project Templates Version** (e.g., `4.0.5331`)
   - The new version for:
     - `Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore` (isolated)
     - `Microsoft.Azure.Functions.Worker.ProjectTemplates` (isolated)
     - `Microsoft.Azure.WebJobs.ItemTemplates` (in-process)
     - `Microsoft.Azure.WebJobs.ProjectTemplates` (in-process)

2. **SDK Version** (optional - if updating)
   - Latest version of `Microsoft.Azure.Functions.Worker.Sdk`

3. **Description Update and .NET version** (optional - if upgrading preview to GA for specific .NET version)
   - New description: `Isolated` or `Isolated LTS`

### Steps Overview

#### **Commit 1: Copy Current Release Block**
1. Locate the **latest release block** in `cli-feed-v4.json` (e.g., `4.118.0` and `4.118.0-inprocess`)
2. **Copy both blocks** (main and `-inprocess`)
3. Paste both copied blocks **directly below** the originals
4. **Do not modify anything** - just duplicate them
5. **Commit this change as Commit 1**

#### **Commit 2: Update Template Versions**

1. **Increment version numbers**:
   - Change `4.118.0` → `4.119.0`
   - Change `4.118.0-inprocess` → `4.119.0-inprocess`

2. **In both the NEW `4.119.0` and `4.119.0-inprocess` block (isolated runtimes)**:
   - Update `itemTemplates` version for all isolated runtime blocks (e.g., `net8-isolated`, `net9-isolated`, `net10-isolated`)
     - Format: `https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore/4.0.XXXX`
   - Update `projectTemplates` version for all isolated runtime blocks
     - Format: `https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ProjectTemplates/4.0.XXXX`

4. **If upgrading preview to GA** (e.g., changing `.NET 10.0` from preview to LTS) (optional):
   - Locate the preview runtime block (e.g., `net10-isolated`)
   - Update `description`: Change from `Isolated Preview` to `Isolated` or `Isolated LTS`
   - Update `endOfLifeDate` if necessary

5. **If updating Worker SDK** (optional):
   - Update `sdk.version` for the relevant isolated runtime blocks

6. **Commit all changes as Commit 2**

---

## 📋 Example Workflows

### Example 1: Adding .NET 11.0 (Scenario 1)

**User provides:**
- displayName: `.NET 11.0`
- targetFramework: `.NET 11`
- description: `Isolated Preview`
- endOfLifeDate: `2029-11-10T00:00:00Z`
- Item/Project templates version: `4.0.5400`
- SDK version: `2.1.0`

**Commit 1:**
- Copy `4.118.0` block only (not `-inprocess`)

**Commit 2:**
- Bump version to `4.119.0`
- Add `net11-isolated` block with all the provided information
- Update all version-specific fields as described above

---

### Example 2: Bumping Template Versions + Upgrading .NET 10 to LTS (Scenario 2)

**User provides:**
- Item/Project templates version: `4.0.5331`
- SDK version: `2.0.6`
- Description for .NET 10: `Isolated LTS`

**Commit 1:**
- Copy both `4.118.0` and `4.118.0-inprocess` blocks

**Commit 2:**
- Bump versions to `4.119.0` and `4.119.0-inprocess`
- Update all `itemTemplates` and `projectTemplates` URLs to version `4.0.5331`
- Update `net10-isolated` description from `Isolated Preview` → `Isolated LTS`
- Update `net10-isolated` SDK version to `2.0.6`
- Update `net10-isolated` endOfLifeDate if needed

---

## ⚠️ Important Notes

### For Scenario 1 (Adding New .NET Version):
- **Only copy the main block** in Commit 1 - do NOT copy the `-inprocess` variant
- New .NET versions are isolated-only; in-process is not supported for new versions
- Verify container image tags exist before adding the new version
- Add the new version to the `capabilities` list of all relevant runtime blocks
- Use the exact values provided for displayName, targetFramework, description, and endOfLifeDate

### For Scenario 2 (Bumping Templates):
- **Copy both main and `-inprocess` blocks** in Commit 1
- Update template versions for ALL runtime blocks (both isolated and in-process)
- When upgrading preview to GA, update the description and EOL date
- SDK version updates are optional but recommended for the latest .NET versions

### General:
- Always use the two-commit workflow: Commit 1 (copy) + Commit 2 (modify)
- Test that SDK versions are compatible with their respective .NET versions
- Verify all URL changes point to valid NuGet packages
- Double-check version number increments are consistent

---

## 🚀 Quick Reference Checklist

### Scenario 1: Adding New .NET Version
**Before you start:**
- [ ] displayName (e.g., `.NET 11.0`)
- [ ] targetFramework (e.g., `.NET 11`)
- [ ] description (Isolated/Isolated LTS/Isolated Preview)
- [ ] endOfLifeDate
- [ ] Item/Project templates version
- [ ] SDK version

**Workflow:**
1. **Commit 1**: Copy main block only (NOT `-inprocess`)
2. **Commit 2**: Bump version + add new runtime configuration

---

### Scenario 2: Bumping Template Versions
**Before you start:**
- [ ] Item/Project templates version
- [ ] SDK version (if updating)
- [ ] Description update (if upgrading preview)

**Workflow:**
1. **Commit 1**: Copy both main and `-inprocess` blocks
2. **Commit 2**: Bump versions + update all template URLs (+ optional preview→GA upgrade)