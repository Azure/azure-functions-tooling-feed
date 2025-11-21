# Update Feed for .NET Version

This guide explains how to update the `cli-feed-v4.json` file to upgrade a .NET version by copying the current release block and applying version updates.

---

## 📥 Required User Input

Before starting, gather the following information:

1. **Target .NET Version** (e.g., `9`, `10`, `11`)
   - The .NET version number you want to upgrade to

2. **Item/Project Templates Version** (e.g., `4.0.5331`)
   - The version to use for both:
     - `Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore`
     - `Microsoft.Azure.Functions.Worker.ProjectTemplates`

3. **SDK Version** (e.g., `2.0.5`)
   - The latest version of `Microsoft.Azure.Functions.Worker.Sdk` compatible with the target .NET version

4. **Description Type**
   - Choose one:
     - `Isolated` - for Standard Term Support (STS) versions
     - `Isolated LTS` - for Long Term Support (LTS) versions

---

## ✅ Steps Overview

### Step 1: Copy Current Release Block
- Duplicate the **latest release block** (e.g., `4.118.0` and `4.118.0-inprocess`).
- Place the copy **right below the latest release**.
- **Commit this change as Commit 1**.

### Step 2: Increment Version Numbers
Update the version numbers in both release blocks:
- Change `4.118.0` → `4.119.0`
- Change `4.118.0-inprocess` → `4.119.0-inprocess`

### Step 3: Update or Add .NET Runtime Configuration

**Check if the target .NET version already exists** in the copied `4.119.0` block:

#### **Case A: Preview Block Already Exists** (e.g., `net10-isolated` with `Isolated Preview`)
If the .NET version already exists as a preview:

1. **Locate the existing runtime block** (e.g., `net10-isolated`)
2. **Update description only**:
   - Change from `Isolated Preview` to:
     - `Isolated` (for STS)
     - `Isolated LTS` (for LTS)
3. **Update itemTemplates version**: Replace with the version **provided by the user**
   - Format: `https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ItemTemplates.NetCore/4.0.XXXX`
4. **Update projectTemplates version**: Replace with the version **provided by the user**
   - Format: `https://www.nuget.org/api/v2/package/Microsoft.Azure.Functions.Worker.ProjectTemplates/4.0.XXXX`
5. **Update SDK version**: Use the version **provided by the user**
   - Example: `"version": "2.0.5"`
7. **Skip creating a new block** - the runtime configuration already exists

#### **Case B: Runtime Block Does NOT Exist** (New .NET version)
If this is a completely new .NET version not yet in the feed:

1. **Copy the previous .NET isolated block** (e.g., copy `net9-isolated` to create `net10-isolated`)
2. **Update the key name**: `net9-isolated` → `net10-isolated`
3. **Update displayInfo**:
   - `displayName`: `.NET 9.0` → `.NET 10.0`
   - `targetFramework`: `.NET 9` → `.NET 10`
   - `description`: Set based on user input:
     - `Isolated` (for STS)
     - `Isolated LTS` (for LTS)
   - `endOfLifeDate`: Update to the new version's EOL date
4. **Update capabilities**: Add the new version to the capabilities list
   - Example: `isolated,net6,net7,net8,net9` → `isolated,net6,net7,net8,net9,net10`
5. **Update sdk version**: Use the version **provided by the user**
6. **Update toolingSuffix**: `net9-isolated` → `net10-isolated`
7. **Update targetFramework**: `net9.0` → `net10.0`
8. **Update itemTemplates version**: Replace with the version **provided by the user**
9. **Update projectTemplates version**: Replace with the version **provided by the user**
10. **Update localContainerBaseImage**: Update the .NET version in the image path
    - Example: `dotnet-isolated9.0-appservice` → `dotnet-isolated10.0-appservice`
11. **Update windowsSiteConfig**:
    - `netFrameworkVersion`: `v9.0` → `v10.0`
12. **Update linuxSiteConfig**:
    - `linuxFxVersion`: `DOTNET-ISOLATED|9.0` → `DOTNET-ISOLATED|10.0`

### Step 4: Update In-Process Runtime Templates (4.119.0-inprocess block)

**Note**: In-process runtimes are no longer actively supported for new .NET versions. Only update template versions for existing in-process runtimes.

In the `4.119.0-inprocess` block:

1. **Update itemTemplates version** for all in-process runtimes (e.g., `net6`, `net8`):
   - Replace with the version **provided by the user**
   - Format: `https://www.nuget.org/api/v2/package/Microsoft.Azure.WebJobs.ItemTemplates/4.0.XXXX`

2. **Update projectTemplates version** for all in-process runtimes:
   - Replace with the version **provided by the user**
   - Format: `https://www.nuget.org/api/v2/package/Microsoft.Azure.WebJobs.ProjectTemplates/4.0.XXXX`

**Do NOT add new in-process runtime blocks** - we only maintain existing ones.
---

## 🔄 Commit Structure

### **Commit 1: Copy Current Release Block**
- Duplicate `4.118.0` and `4.118.0-inprocess` blocks
- No modifications, just duplication
- Place immediately below the original blocks

### **Commit 2: Apply All Updates**
- Increment version numbers (`4.118.0` → `4.119.0`)
- **If preview exists**: Update description from `Isolated Preview` to `Isolated` or `Isolated LTS`
- **If new version**: Add new .NET runtime configuration (e.g., `net10-isolated`)
- Update item/project template versions (both isolated and in-process runtimes)
- Update SDK versions for isolated runtimes
- Update EOL dates (if adding new version)
- Update capabilities lists (if adding new version)
- Update container image references (if adding new version)
- Update framework version configs (if adding new version)

---

## 📋 Detailed Breakdown: Two Scenarios

### **Scenario 1: Upgrading Preview to GA** (Preview block already exists)

**Example**: Upgrading `.NET 10.0` from `Isolated Preview` to `Isolated LTS`

**User provides:**
- .NET version: `10`
- Item/Project templates version: `4.0.5331`
- SDK version: `2.0.5`
- Description: `Isolated LTS`

**Changes made:**
1. Copy `4.118.0` blocks → Commit 1
2. Bump to `4.119.0`
3. Locate existing `net10-isolated` block
4. Update description: `Isolated Preview` → `Isolated LTS`
5. Update item templates version: `4.0.5267` → `4.0.5331`
6. Update project templates version: `4.0.5267` → `4.0.5331`
7. Update SDK version to `2.0.5` (if changed)
8. Update EOL date to GA date
9. Update in-process template versions in `4.119.0-inprocess` block → Commit 2

### **Scenario 2: Adding New .NET Version** (No existing block)

**Example**: Adding `.NET 11.0` for the first time

**User provides:**
- .NET version: `11`
- Item/Project templates version: `4.0.5400`
- SDK version: `2.1.0`
- Description: `Isolated`

**Changes made:**
1. Copy `4.118.0` blocks → Commit 1
2. Bump to `4.119.0`
3. Copy `net10-isolated` block and rename to `net11-isolated`
4. Update all version references: `10` → `11`, `v10.0` → `v11.0`
5. Set description to `Isolated`
6. Update item/project templates to `4.0.5400`
7. Update SDK version to `2.1.0`
8. Add `net11` to capabilities across relevant runtimes
9. Update container images: `dotnet-isolated10.0-appservice` → `dotnet-isolated11.0-appservice`
10. Set appropriate EOL date
11. Update in-process template versions in `4.119.0-inprocess` block → Commit 2

---

## ⚠️ Important Notes

- **Check if preview exists first**: If the .NET version already exists as a preview, you only need to update the description and template versions - don't create a new block
- Always gather **all required user inputs** before starting the update process
- Verify the **EOL (End of Life) date** for the new or upgraded .NET version
- Confirm whether the version is **LTS** or **STS** (we don't add Preview anymore - only upgrade from Preview to GA)
- Ensure **template versions** match the user-provided versions for both isolated and in-process runtimes
- Test that **SDK versions** are compatible with the target .NET version
- For new versions: Update **capabilities** in all relevant runtime blocks to include the new version
- For new versions: Verify **container image tags** exist before updating
- **In-process runtimes**: Only update template versions - do not add new in-process runtime blocks

---

## 🚀 Quick Reference

**Before you start, have ready:**
- [ ] Target .NET version number
- [ ] Item/Project templates version
- [ ] SDK version  
- [ ] Description type (Isolated or Isolated LTS)

**Two-commit workflow:**
1. **Commit 1**: Duplicate latest release blocks (no modifications)
2. **Commit 2**: Apply all updates (version bumps, templates, SDK, description)