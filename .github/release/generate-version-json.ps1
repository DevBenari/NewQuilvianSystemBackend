param(
    [string]$MetadataFile = ".github/release/release-metadata.json",
    [string]$PolicyFile = ".github/release/version-policy.json",
    [string]$Output = ".github/release/version.json"
)


Write-Host "=== Quilvian Version Generator ==="


# ======================================
# Load $metadata = Get-Content `
# ======================================

$metadata = Get-Content `
    $MetadataFile `
    -Raw |
    ConvertFrom-Json

$policy = Get-Content `
    $PolicyFile `
    -Raw |
    ConvertFrom-Json


$modules = @($metadata.modules)

$features = @($metadata.features)

$migrationCount = $metadata.migrationCount

$currentVersion = [string]$policy.currentVersion

Write-Host "Current Version:"
Write-Host $currentVersion


$currentVersion = $currentVersion.Trim()

if ($currentVersion -notmatch '^(\d+)\.(\d+)\.(\d+)$')
{
    throw "Invalid version format: $currentVersion"
}

$major = [int]$matches[1]
$minor = [int]$matches[2]
$patch = [int]$matches[3]

Write-Host "DEBUG VERSION PARTS"
Write-Host "Major : $major"
Write-Host "Minor : $minor"
Write-Host "Patch : $patch"


if ($migrationCount -gt 0 -or $modules.Count -gt 0)
{
    $minor++
    $patch = 0
}
else
{
    $patch++
}

$Version = "{0}.{1}.{2}" -f $major,$minor,$patch

Write-Host "Calculated Version:"
Write-Host $Version

Write-Host ""
Write-Host "Detected Modules:"
$modules


Write-Host ""
Write-Host "Migration Count:"
$migrationCount



# ======================================
# Generate Release Name
# ======================================

if ($modules.Count -gt 3)
{
    $releaseName = "Quilvian $Version - Enterprise Platform Enhancement"
}
elseif ($modules.Count -gt 0)
{
    $releaseName = "Quilvian $Version - " + ($modules -join " and ") + " Enhancement"
}
else
{
    $releaseName = "Quilvian $Version Maintenance Release"
}



# ======================================
# Generate Description
# ======================================

$moduleText =
    ($modules -join ", ")


$featureText = ($features -join ", ")


$description =
"Release $Version enhances $moduleText capabilities."


if ($features.Count -gt 0)
{
    $description +=
    " Includes improvements across $featureText."
}


if ($migrationCount -gt 0)
{
    $description +=
    " Includes $migrationCount database migration update(s)."
}



# ======================================
# Create Version JSON
# ======================================

$result = @{
    backendVersion = $Version

    releaseName = $releaseName

    description = $description

    releaseMetadata = @{
        modules = $modules

        features = $features

        migrationCount = $migrationCount

        migrations = $metadata.migrations

        generatedAt =
            (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")

        generatedBy =
            "Quilvian Release Generator"
    }
}



$result |
ConvertTo-Json -Depth 10 |
Set-Content $Output



Write-Host ""
Write-Host "Generated:"
Write-Host $Output