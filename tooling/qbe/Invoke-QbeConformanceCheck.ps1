[CmdletBinding(DefaultParameterSetName = 'WorkingTree')]
param(
    [Parameter(ParameterSetName = 'GitRange', Mandatory)] [string] $BaseRef,
    [Parameter(ParameterSetName = 'GitRange', Mandatory)] [string] $HeadRef,
    [Parameter(ParameterSetName = 'ExplicitFiles', Mandatory)] [string[]] $Path,
    [string] $RepositoryRoot = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$requiredAuthority = @(
    'AGENTS.md',
    'docs/engineering/BACKEND_ENGINEERING_CONTRACT.md',
    'docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md'
)
$implementedRules = @('QBE-ENT-001','QBE-NAM-001','QBE-CFG-001','QBE-CODE-002','QBE-CODE-003','QBE-MOD-002','QBE-SVC-001')
$root = [IO.Path]::GetFullPath($RepositoryRoot)
foreach ($authority in $requiredAuthority) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $authority) -PathType Leaf)) { throw "Canonical governance missing: $authority" }
}
$contract = Get-Content -Raw -LiteralPath (Join-Path $root 'docs/engineering/BACKEND_ENGINEERING_CONTRACT.md')
foreach ($rule in $implementedRules) { if ($contract -notmatch [regex]::Escape($rule)) { throw "Canonical contract does not define $rule" } }

function Get-RelativePath([string]$file) {
    $full = [IO.Path]::GetFullPath($file)
    if ($full.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) { return $full.Substring($root.Length).TrimStart('\','/') }
    return $file
}
function Add-Finding([string]$Rule, [string]$Level, [string]$Applicability, [string]$File, [int]$Line, [string]$Evidence, [string]$Reason, [string]$Action) {
    $script:findings.Add([pscustomobject]@{ RuleId=$Rule; Level=$Level; Applicability=$Applicability; File=$File; Line=$Line; Evidence=$Evidence; Reason=$Reason; RecommendedAction=$Action })
}
function Get-AddedLines([string]$relative, [string]$base, [string]$head) {
    $output = if ($head -eq 'WORKTREE') { & git -C $root diff --unified=0 $base -- $relative 2>$null } else { & git -C $root diff --unified=0 "$base..$head" -- $relative 2>$null }
    $lines = @(); $lineNumber = 0
    foreach ($line in $output) {
        if ($line -match '^\+\+\+') { continue }
        if ($line -match '^@@ .*\+(\d+)(?:,(\d+))?') { $lineNumber = [int]$Matches[1]; continue }
        if ($line.StartsWith('+')) { $lines += [pscustomobject]@{ Number=$lineNumber; Text=$line.Substring(1) }; $lineNumber++; continue }
        if (-not $line.StartsWith('-') -and $lineNumber -gt 0) { $lineNumber++ }
    }
    return $lines
}
function Test-PersistedEntity([string]$content, [string]$name) {
    $hasDbSet = @((Get-ChildItem -LiteralPath $root -Recurse -Filter '*.cs' | Select-String -SimpleMatch "DbSet<$name>")).Count -gt 0
    return $content -match "class\s+$([regex]::Escape($name))\b" -and ($content -match 'IdentityModel|\[Table\(|DbSet<' -or $hasDbSet)
}
function Test-Configuration([string]$name) {
    return @((Get-ChildItem -LiteralPath $root -Recurse -Filter '*.cs' | Select-String -Pattern "IEntityTypeConfiguration\s*<\s*$([regex]::Escape($name))\s*>" )).Count -gt 0
}
function Test-RegistryOwnership([string]$relative) {
    $registry = Get-Content -Raw -LiteralPath (Join-Path $root 'docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md')
    $segments = $relative -split '[\\/]'
    foreach ($segment in $segments) { if ($segment -match 'Management$' -and $registry -match [regex]::Escape($segment)) { return $true } }
    return $false
}

$scope = $PSCmdlet.ParameterSetName
$files = @()
$addedByFile = @{}
switch ($scope) {
    'WorkingTree' {
        $tracked = & git -C $root diff --name-only HEAD
        $untracked = & git -C $root ls-files --others --exclude-standard
        $files = @($tracked + $untracked | Where-Object { $_ -match '\.cs$' } | Select-Object -Unique)
        foreach ($file in $files) {
            $full = Join-Path $root $file
            if ((& git -C $root ls-files --error-unmatch -- $file 2>$null)) { $addedByFile[$file] = @(Get-AddedLines $file 'HEAD' 'WORKTREE') }
            elseif (Test-Path -LiteralPath $full) { $i=0; $addedByFile[$file]=@(Get-Content $full | ForEach-Object { $i++; [pscustomobject]@{Number=$i;Text=$_} }) }
        }
    }
    'GitRange' {
        $files = @(& git -C $root diff --name-only "$BaseRef..$HeadRef" -- '*.cs')
        foreach ($file in $files) { $addedByFile[$file] = @(Get-AddedLines $file $BaseRef $HeadRef) }
    }
    'ExplicitFiles' {
        $files = @($Path | ForEach-Object { Get-RelativePath $_ } | Where-Object { $_ -match '\.cs$' } | Select-Object -Unique)
        foreach ($file in $files) { $full=Join-Path $root $file; if(Test-Path $full){$i=0;$addedByFile[$file]=@(Get-Content $full|ForEach-Object{$i++;[pscustomobject]@{Number=$i;Text=$_}})} }
    }
}

$findings = [System.Collections.Generic.List[object]]::new()
foreach ($file in $files) {
    $full = Join-Path $root $file
    if (-not (Test-Path -LiteralPath $full)) { continue }
    $content = Get-Content -Raw -LiteralPath $full
    $added = @($addedByFile[$file])
    $isNew = if ($scope -eq 'ExplicitFiles') { $true } elseif ($scope -eq 'GitRange') { -not ((@(& git -C $root ls-tree -r --name-only $BaseRef -- $file)) -contains $file) } else { -not (& git -C $root ls-files --error-unmatch -- $file 2>$null) }
    foreach ($line in $added) {
        if ($line.Text -match '\b(class|DbSet|IEntityTypeConfiguration)\s*<?\s*(Trx\w+)' -or ($line.Number -eq 1 -and $file -match '(^|[\\/])Trx\w+(Configuration)?\.cs$')) {
            Add-Finding 'QBE-NAM-001' 'VIOLATION' $(if($isNew){'NEW CODE'}else{'TOUCHED LEGACY'}) $file $line.Number $line.Text 'New operational Trx naming is prohibited.' 'Use the approved registry prefix.'
        }
        if ($file -match 'Controller\.cs$' -and $line.Text -match 'Generate\w*(Code|Number)|\b(Count|CountAsync|Max|MaxAsync|Last|LastOrDefault)\w*\s*\(.*\+\s*1') {
            $rule = if($line.Text -match 'Generate\w*(Code|Number)'){'QBE-CODE-002'}else{'QBE-CODE-003'}
            Add-Finding $rule 'VIOLATION' $(if($isNew){'NEW CODE'}else{'TOUCHED LEGACY'}) $file $line.Number $line.Text 'Controller-side business number allocation was introduced.' 'Move allocation to a Module Service and durable provider.'
        }
        if ($file -match 'Controller\.cs$' -and $line.Text -match 'ApplicationDbContext') {
            Add-Finding 'QBE-SVC-001' 'REVIEW' $(if($isNew){'NEW CODE'}else{'TOUCHED LEGACY'}) $file $line.Number $line.Text 'New direct ApplicationDbContext controller use requires boundary review.' 'Use a Module Service for domain CRUD/orchestration.'
        }
    }
    if ($isNew -and $file -notmatch 'Controller\.cs$' -and $content -match 'class\s+(\w+)') {
        $entity = $Matches[1]
        if (Test-PersistedEntity $content $entity) {
            if ($content -notmatch "class\s+$([regex]::Escape($entity))\s*:\s*IdentityModel") { Add-Finding 'QBE-ENT-001' 'VIOLATION' 'NEW CODE' $file 0 $entity 'New persisted entity does not inherit IdentityModel.' 'Inherit IdentityModel.' }
            if (-not (Test-Configuration $entity)) { Add-Finding 'QBE-CFG-001' 'VIOLATION' 'NEW CODE' $file 0 $entity 'New persisted entity has no dedicated IEntityTypeConfiguration<T>.' 'Add dedicated mapping configuration.' }
            if (-not (Test-RegistryOwnership $file)) {
                $level = if ($file -match '(^|[\\/])Areas[\\/].+Management[\\/]') { 'VIOLATION' } else { 'REVIEW' }
                Add-Finding 'QBE-MOD-002' $level 'NEW CODE' $file 0 $entity 'Area/Module ownership cannot be resolved to the approved registry.' 'Obtain registry decision; do not infer a prefix.'
            }
        }
    }
}

Write-Output 'QBE Conformance Report'
Write-Output "Scope: $scope"
Write-Output "Files evaluated: $($files.Count)"
foreach ($level in @('VIOLATION','REVIEW','INFO')) { Write-Output "${level}: $(@($findings | Where-Object Level -eq $level).Count)" }
if ($findings.Count -eq 0) { Write-Output 'Findings: none' } else { foreach ($finding in $findings) { Write-Output "[$($finding.Level)] $($finding.RuleId) | $($finding.File):$($finding.Line) | $($finding.Evidence) | Action: $($finding.RecommendedAction)" } }
Write-Output 'REPORT ONLY: No enforcement/blocking performed.'
