param(
    [string]$BaseCommit = "HEAD~1"
)

$ModuleMapPath = ".github/release/module-map.json"
$MigrationMapPath = ".github/release/migration-map.json"
$FeatureMapPath = ".github/release/feature-map.json"

$Output = ".github/release/release-metadata.json"


Write-Host "=== Quilvian Release Analyzer ==="


$moduleMap = Get-Content $ModuleMapPath -Raw | ConvertFrom-Json
$migrationMap = Get-Content $MigrationMapPath -Raw | ConvertFrom-Json
$featureMap = Get-Content $FeatureMapPath -Raw | ConvertFrom-Json


# ======================================
# 1. Detect changed files
# ======================================

$changedFiles = git diff `
    --name-only `
    $BaseCommit `
    HEAD


Write-Host ""
Write-Host "Changed Files:"
$changedFiles


$detectedModules = New-Object System.Collections.Generic.List[string]


foreach ($property in $moduleMap.PSObject.Properties)
{
    $moduleName = $property.Name
    $patterns = $property.Value


    foreach ($pattern in $patterns)
    {
        if ($changedFiles -match [regex]::Escape($pattern))
        {
            if (-not $detectedModules.Contains($moduleName))
            {
                $detectedModules.Add($moduleName)
            }
        }
    }
}

# ======================================
# Detect Features
# ======================================

$detectedFeatures =
New-Object System.Collections.Generic.List[string]


foreach ($property in $featureMap.PSObject.Properties)
{
    $featureName = $property.Name
    $patterns = $property.Value


    foreach ($pattern in $patterns)
    {
        if ($changedFiles -match [regex]::Escape($pattern))
        {
            if (-not $detectedFeatures.Contains($featureName))
            {
                $detectedFeatures.Add($featureName)
            }
        }
    }
}

# ======================================
# 2. Detect migrations
# ======================================

$changedMigrationFiles = git diff `
    --name-only `
    $BaseCommit `
    HEAD |
    Where-Object {
        $_ -like "Migrations/*.cs" `
        -and $_ -notlike "*.Designer.cs" `
        -and $_ -notlike "*Snapshot.cs"
    }


$newMigrations = $changedMigrationFiles


$migrationNames = $newMigrations |
    ForEach-Object {
        Split-Path $_ -Leaf
    }



$detectedMigrationModules =
New-Object System.Collections.Generic.List[string]


foreach ($migration in $newMigrations)
{

    foreach ($rule in $migrationMap.PSObject.Properties)
    {
        if ($migration -match $rule.Name)
        {
            if (-not $detectedMigrationModules.Contains($rule.Value))
            {
                $detectedMigrationModules.Add($rule.Value)
            }
        }
    }

}



# ======================================
# 3. Combine result
# ======================================


$allModules =
(
    $detectedModules +
    $detectedMigrationModules
) |
Sort-Object -Unique



$result = @{
    generatedAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss")

    modules = @($allModules)

    sourceModules = $detectedModules

    migrationModules = $detectedMigrationModules

    features = $detectedFeatures

    migrationCount = $migrationNames.Count

    migrations = @($migrationNames)

    changedFiles = @($changedFiles)
}



$result |
ConvertTo-Json -Depth 10 |
Set-Content $Output



Write-Host ""
Write-Host "Generated:"
Write-Host $Output