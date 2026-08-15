[CmdletBinding(DefaultParameterSetName = 'WorkingTree')]
param(
    [Parameter(ParameterSetName = 'GitRange', Mandatory)] [string] $BaseRef,
    [Parameter(ParameterSetName = 'GitRange', Mandatory)] [string] $HeadRef,
    [Parameter(ParameterSetName = 'ExplicitFiles', Mandatory)] [string[]] $Path,
    [string] $Mode = 'ReportOnly',
    [string] $JsonOutputPath,
    [string] $ExceptionRegistryPath,
    [string] $RepositoryRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$toolingFailureExitCode = 2
$conformanceFailureExitCode = 1
trap {
    Write-Output "TOOL ERROR: $($_.Exception.Message)"
    Write-Output 'Final result: TOOL ERROR'
    exit $toolingFailureExitCode
}

if ($Mode -notin @('ReportOnly', 'Strict')) { throw "Unsupported checker mode: $Mode. Supported modes: ReportOnly, Strict." }

if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) { $RepositoryRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot) }

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
    $script:findings.Add([pscustomobject]@{ RuleId=$Rule; Level=$Level; Applicability=$Applicability; File=$File; Line=$Line; Evidence=$Evidence; Reason=$Reason; RecommendedAction=$Action; Suppressed=$false; ExceptionId=$null })
}
function Get-ExceptionRegistryPath {
    if ([string]::IsNullOrWhiteSpace($ExceptionRegistryPath)) { return Join-Path $root 'docs/engineering/QBE_EXCEPTIONS.json' }
    if ([IO.Path]::IsPathRooted($ExceptionRegistryPath)) { return [IO.Path]::GetFullPath($ExceptionRegistryPath) }
    return [IO.Path]::GetFullPath((Join-Path $root $ExceptionRegistryPath))
}
function Get-ValidatedExceptions {
    $registryPath = Get-ExceptionRegistryPath
    if (-not (Test-Path -LiteralPath $registryPath -PathType Leaf)) {
        if ([string]::IsNullOrWhiteSpace($ExceptionRegistryPath)) { return @() }
        throw "Exception registry not found: $registryPath"
    }
    try { $registry = Get-Content -Raw -LiteralPath $registryPath | ConvertFrom-Json } catch { throw "Malformed exception registry: $registryPath" }
    if ($null -eq $registry -or [string]::IsNullOrWhiteSpace($registry.schemaVersion) -or $null -eq $registry.exceptions) { throw "Malformed exception registry: schemaVersion and exceptions are required." }
    $validated = @()
    foreach ($exception in @($registry.exceptions)) {
        foreach ($field in @('ExceptionId','RuleId','Scope','Reason','Status')) { if ([string]::IsNullOrWhiteSpace([string]$exception.$field)) { throw "Invalid exception record: $field is required." } }
        $approvedBy = if ($null -ne $exception.PSObject.Properties['ApprovedBy']) { [string]$exception.ApprovedBy } else { '' }
        $approvalReference = if ($null -ne $exception.PSObject.Properties['ApprovalReference']) { [string]$exception.ApprovalReference } else { '' }
        $expiresAtValue = if ($null -ne $exception.PSObject.Properties['ExpiresAt']) { [string]$exception.ExpiresAt } else { '' }
        $noExpiryRationale = if ($null -ne $exception.PSObject.Properties['NoExpiryRationale']) { [string]$exception.NoExpiryRationale } else { '' }
        if ([string]::IsNullOrWhiteSpace($approvedBy) -and [string]::IsNullOrWhiteSpace($approvalReference)) { throw "Invalid exception record $($exception.ExceptionId): ApprovedBy or ApprovalReference is required." }
        if ($exception.RuleId -notin $implementedRules) { throw "Invalid exception record $($exception.ExceptionId): unknown RuleId $($exception.RuleId)." }
        if ($exception.Status -notin @('ACTIVE','EXPIRED','REVOKED')) { throw "Invalid exception record $($exception.ExceptionId): unsupported Status $($exception.Status)." }
        $scope = ([string]$exception.Scope).Replace('\\','/')
        if ([IO.Path]::IsPathRooted($scope) -or $scope -match '(^|/)\.\.(/|$)' -or $scope -match '[*?]' -or [string]::IsNullOrWhiteSpace($scope)) { throw "Invalid exception record $($exception.ExceptionId): Scope must be a specific repository-relative file." }
        if ([string]::IsNullOrWhiteSpace($expiresAtValue) -and [string]::IsNullOrWhiteSpace($noExpiryRationale)) { throw "Invalid exception record $($exception.ExceptionId): ExpiresAt or NoExpiryRationale is required." }
        $expiresAt = $null
        if (-not [string]::IsNullOrWhiteSpace($expiresAtValue)) { try { $expiresAt = [datetime]::Parse($expiresAtValue).ToUniversalTime() } catch { throw "Invalid exception record $($exception.ExceptionId): ExpiresAt is invalid." } }
        $validated += [pscustomobject]@{ ExceptionId=[string]$exception.ExceptionId; RuleId=[string]$exception.RuleId; Scope=$scope; Status=[string]$exception.Status; ExpiresAt=$expiresAt }
    }
    return $validated
}
function Write-StructuredResult([string]$result, [object[]]$blockingRules) {
    if ([string]::IsNullOrWhiteSpace($JsonOutputPath)) { return }
    $outputPath = if ([IO.Path]::IsPathRooted($JsonOutputPath)) { [IO.Path]::GetFullPath($JsonOutputPath) } else { [IO.Path]::GetFullPath((Join-Path $root $JsonOutputPath)) }
    $parent = Split-Path -Parent $outputPath
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) { throw "JSON output directory not found: $parent" }
    $json = [pscustomobject]@{
        schemaVersion = '1.0'
        checkerVersion = 'G6-E2B'
        mode = $Mode
        scope = $scope
        baseRef = if ($scope -eq 'GitRange') { $BaseRef } else { $null }
        headRef = if ($scope -eq 'GitRange') { $HeadRef } else { $null }
        filesEvaluated = $files.Count
        violationCount = @($findings | Where-Object Level -eq 'VIOLATION').Count
        reviewCount = @($findings | Where-Object Level -eq 'REVIEW').Count
        infoCount = @($findings | Where-Object Level -eq 'INFO').Count
        suppressedViolationCount = @($findings | Where-Object { $_.Level -eq 'VIOLATION' -and $_.Suppressed }).Count
        blockingRuleIds = @($blockingRules)
        result = $result
        findings = @($findings | ForEach-Object { [pscustomobject]@{ ruleId=$_.RuleId; level=$_.Level; file=$_.File; line=$_.Line; reason=$_.Reason; evidence=$_.Evidence; recommendedAction=$_.RecommendedAction; suppressed=$_.Suppressed; exceptionId=$_.ExceptionId } })
    }
    $json | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $outputPath -Encoding utf8
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
$exceptions = @(Get-ValidatedExceptions)
$exceptionNotices = [System.Collections.Generic.List[string]]::new()
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

foreach ($finding in $findings) {
    $matching = @($exceptions | Where-Object { $_.RuleId -eq $finding.RuleId -and $_.Scope -eq $finding.File })
    foreach ($exception in $matching) {
        $isExpired = $exception.Status -eq 'EXPIRED' -or ($null -ne $exception.ExpiresAt -and $exception.ExpiresAt -lt [datetime]::UtcNow)
        if ($isExpired) { $exceptionNotices.Add("Exception $($exception.ExceptionId) is expired and did not suppress $($finding.RuleId).") ; continue }
        if ($exception.Status -eq 'REVOKED') { $exceptionNotices.Add("Exception $($exception.ExceptionId) is revoked and did not suppress $($finding.RuleId).") ; continue }
        $finding.Suppressed = $true
        $finding.ExceptionId = $exception.ExceptionId
        break
    }
}

Write-Output 'QBE Conformance Report'
Write-Output "Checker mode: $Mode"
Write-Output "Scope: $scope"
Write-Output "Files evaluated: $($files.Count)"
foreach ($level in @('VIOLATION','REVIEW','INFO')) { Write-Output "${level}: $(@($findings | Where-Object Level -eq $level).Count)" }
if ($findings.Count -eq 0) { Write-Output 'Findings: none' } else { foreach ($finding in $findings) { $suppression = if ($finding.Suppressed) { " | SUPPRESSED: $($finding.ExceptionId)" } else { '' }; Write-Output "[$($finding.Level)] $($finding.RuleId) | $($finding.File):$($finding.Line) | $($finding.Evidence) | Action: $($finding.RecommendedAction)$suppression" } }
foreach ($notice in $exceptionNotices) { Write-Output "Exception notice: $notice" }
if ($Mode -eq 'Strict') {
    $blockingRuleIds = @($findings | Where-Object { $_.Level -eq 'VIOLATION' -and -not $_.Suppressed } | Select-Object -ExpandProperty RuleId -Unique)
    if ($blockingRuleIds.Count -gt 0) {
        Write-Output 'STRICT CONFORMANCE FAILURE'
        Write-Output 'Blocking rules:'
        foreach ($ruleId in $blockingRuleIds) { Write-Output $ruleId }
        Write-Output 'Final result: CONFORMANCE FAILURE'
        Write-StructuredResult 'CONFORMANCE_FAILURE' $blockingRuleIds
        exit $conformanceFailureExitCode
    }
}

if ($Mode -eq 'ReportOnly') { Write-Output 'REPORT ONLY: No enforcement/blocking performed.' }
Write-Output 'Final result: PASS'
Write-StructuredResult 'PASS' @()
