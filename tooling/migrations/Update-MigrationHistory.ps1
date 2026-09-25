#Requires -Version 5.1
<#
.SYNOPSIS
    Memangkas volume kode migration EF Core yang ikut dikompilasi.

.DESCRIPTION
    Setiap berkas Migrations\*.Designer.cs memuat BuildTargetModel, yaitu snapshot
    penuh seluruh skema database. Isinya praktis identik antar migration dan
    berukuran sekitar 4,5 MB per berkas. Pada 204 migration totalnya 554 MB dan
    13,7 juta baris, dan Roslyn menurunkan (lowering) method-method raksasa itu
    secara paralel sampai kehabisan memori.

    Skrip ini memindahkan Designer yang sudah tidak diperlukan ke Migrations\History\
    (tidak dikompilasi), lalu menghasilkan Migrations\MigrationMetadata.g.cs berisi
    atribut [DbContext] dan [Migration] untuk migration-migration tersebut.

    Atribut itu WAJIB ada. Tanpanya EF tidak mengenali migration sama sekali:
    'migrations list' hanya melihat sebagian, dan 'migrations add' menganggap
    database kosong lalu menghasilkan migration berisi seluruh skema dari nol.
    Itulah kegagalan yang tercatat pada 4 September 2026 ketika Designer sekadar
    dikeluarkan dari kompilasi tanpa penggantinya.

    Designer yang TETAP dikompilasi:
      1. N migration terbaru       -> dibutuhkan 'dotnet ef migrations remove',
                                      yang menulis ulang snapshot dari TargetModel
                                      migration sebelumnya.
      2. Migration bermuatan data  -> InsertData/UpdateData/DeleteData memakai
                                      TargetModel untuk menentukan tipe kolom saat
                                      SQL-nya dihasilkan. Tanpa itu, replay ke
                                      database kosong bisa salah tipe.

    ApplicationDbContextModelSnapshot.cs selalu dikompilasi: itulah pembanding
    yang dipakai 'migrations add'.

.PARAMETER KeepLatest
    Banyaknya migration terbaru yang Designer-nya tetap dikompilasi. Bawaan 2.

.PARAMETER WhatIfOnly
    Hanya menampilkan rencana, tidak mengubah apa pun.

.EXAMPLE
    powershell -NoProfile -File tooling\migrations\Update-MigrationHistory.ps1

.NOTES
    Jalankan ulang setelah 'dotnet ef migrations add' bila ingin memangkas lagi.
    Tidak menjalankannya TIDAK berbahaya: migration baru lahir di Migrations\
    dan otomatis ikut dikompilasi.
#>
[CmdletBinding()]
param(
    [int]$KeepLatest = 2,
    [switch]$WhatIfOnly
)

$ErrorActionPreference = 'Stop'

$projectRoot   = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$migrationsDir = Join-Path $projectRoot 'Migrations'
$historyDir    = Join-Path $migrationsDir 'History'
$metadataFile  = Join-Path $migrationsDir 'MigrationMetadata.g.cs'
$propsFile     = Join-Path $migrationsDir 'MigrationMetadata.g.props'

if (-not (Test-Path $migrationsDir)) {
    throw "Folder Migrations tidak ditemukan di $migrationsDir"
}

# ---------------------------------------------------------------- kumpulkan
$designerFiles = @()
$designerFiles += Get-ChildItem -Path $migrationsDir -Filter '*.Designer.cs' -File
if (Test-Path $historyDir) {
    $designerFiles += Get-ChildItem -Path $historyDir -Filter '*.Designer.cs' -File
}

if ($designerFiles.Count -eq 0) {
    throw 'Tidak ada berkas *.Designer.cs yang ditemukan.'
}

$entries = foreach ($f in $designerFiles) {
    $text = Get-Content -LiteralPath $f.FullName -Raw

    $idMatch = [regex]::Match($text, '\[Migration\("([^"]+)"\)\]')
    if (-not $idMatch.Success) {
        throw "Atribut [Migration(...)] tidak terbaca di $($f.FullName)"
    }

    $clsMatch = [regex]::Match($text, '(?m)^\s*(?:public\s+|internal\s+)?partial\s+class\s+([A-Za-z_][A-Za-z0-9_]*)')
    if (-not $clsMatch.Success) {
        throw "Nama partial class tidak terbaca di $($f.FullName)"
    }

    $id = $idMatch.Groups[1].Value
    $expectedBase = $f.Name -replace '\.Designer\.cs$', ''
    if ($id -ne $expectedBase) {
        throw "Ketidakcocokan: berkas $($f.Name) tetapi atribut menyebut $id"
    }

    [pscustomobject]@{
        Id        = $id
        ClassName = $clsMatch.Groups[1].Value
        File      = $f
        MainFile  = Join-Path $migrationsDir ($id + '.cs')
    }
}

$entries = @($entries | Sort-Object Id)

$dupIds = @($entries | Group-Object Id | Where-Object { $_.Count -gt 1 })
if ($dupIds.Count -gt 0) {
    throw "ID migration ganda: $(($dupIds | ForEach-Object { $_.Name }) -join ', ')"
}
$dupCls = @($entries | Group-Object ClassName | Where-Object { $_.Count -gt 1 })
if ($dupCls.Count -gt 0) {
    throw "Nama class migration ganda: $(($dupCls | ForEach-Object { $_.Name }) -join ', ')"
}

# -------------------------------------------------- tentukan yang dipertahankan
$keepIds = New-Object 'System.Collections.Generic.HashSet[string]'

foreach ($e in @($entries | Select-Object -Last $KeepLatest)) {
    [void]$keepIds.Add($e.Id)
}

$dataDependent = @()
foreach ($e in $entries) {
    if (-not (Test-Path -LiteralPath $e.MainFile)) {
        throw "Berkas migration utama hilang untuk $($e.Id): $($e.MainFile)"
    }
    $mainText = Get-Content -LiteralPath $e.MainFile -Raw
    if ($mainText -match 'migrationBuilder\.(InsertData|UpdateData|DeleteData)') {
        [void]$keepIds.Add($e.Id)
        $dataDependent += $e.Id
    }
}

$keep    = @($entries | Where-Object { $keepIds.Contains($_.Id) })
$archive = @($entries | Where-Object { -not $keepIds.Contains($_.Id) })

Write-Host ''
Write-Host "Total migration ber-Designer : $($entries.Count)"
Write-Host "Tetap dikompilasi            : $($keep.Count)"
foreach ($e in $keep) {
    $why = if ($dataDependent -contains $e.Id) { 'muatan data' } else { "$KeepLatest terbaru" }
    Write-Host ("  - {0}  ({1})" -f $e.Id, $why)
}
Write-Host "Diarsipkan ke History\       : $($archive.Count)"

if ($WhatIfOnly) {
    Write-Host ''
    Write-Host 'WhatIfOnly aktif. Tidak ada berkas yang diubah.'
    return
}

# ------------------------------------------------------------------ pindahkan
New-Item -ItemType Directory -Path $historyDir -Force | Out-Null

foreach ($e in $archive) {
    $target = Join-Path $historyDir $e.File.Name
    if ($e.File.FullName -ne $target) {
        Move-Item -LiteralPath $e.File.FullName -Destination $target -Force
    }
}

foreach ($e in $keep) {
    $target = Join-Path $migrationsDir $e.File.Name
    if ($e.File.FullName -ne $target) {
        Move-Item -LiteralPath $e.File.FullName -Destination $target -Force
    }
}

# -------------------------------------------------------------- tulis metadata
$nl = [Environment]::NewLine
$lines = New-Object System.Collections.Generic.List[string]

$lines.Add('// <auto-generated />')
$lines.Add('//')
$lines.Add('// Dihasilkan oleh tooling\migrations\Update-MigrationHistory.ps1. Jangan disunting manual.')
$lines.Add('//')
$lines.Add('// Berkas ini memuat atribut [DbContext] dan [Migration] milik migration yang berkas')
$lines.Add('// Designer-nya diarsipkan ke Migrations\History\ dan tidak ikut dikompilasi.')
$lines.Add('//')
$lines.Add('// Atribut inilah yang dipakai EF Core untuk mengenali sebuah migration. Tanpa berkas')
$lines.Add('// ini, migration di bawah menjadi TIDAK TERLIHAT oleh EF: "migrations list" hanya')
$lines.Add('// melihat sebagian, "database update" tidak menerapkan apa pun namun tetap menjawab')
$lines.Add('// "Done.", dan "migrations add" menghasilkan migration berisi seluruh skema dari nol.')
$lines.Add('//')
$lines.Add('// Yang TIDAK dibawa ke sini adalah BuildTargetModel. Method itulah sumber beban')
$lines.Add('// kompilasinya, dan hanya dibutuhkan oleh migration bermuatan data serta oleh')
$lines.Add('// "migrations remove" pada migration terbaru; keduanya tetap dikompilasi utuh.')
$lines.Add('//')
$lines.Add('// Untuk mengompilasi kembali seluruh Designer:')
$lines.Add('//     dotnet build -p:FullMigrationMetadata=true')
$lines.Add('')
$lines.Add('using Microsoft.EntityFrameworkCore.Infrastructure;')
$lines.Add('using Microsoft.EntityFrameworkCore.Migrations;')
$lines.Add('using QuilvianSystemBackend.Repositories;')
$lines.Add('')
$lines.Add('#nullable disable')
$lines.Add('')
$lines.Add('namespace QuilvianSystemBackend.Migrations')
$lines.Add('{')

$first = $true
foreach ($e in $archive) {
    if (-not $first) { $lines.Add('') }
    $first = $false
    $lines.Add('    [DbContext(typeof(ApplicationDbContext))]')
    $lines.Add('    [Migration("' + $e.Id + '")]')
    $lines.Add('    partial class ' + $e.ClassName)
    $lines.Add('    {')
    $lines.Add('    }')
}

$lines.Add('}')

$utf8Bom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($metadataFile, ($lines -join $nl) + $nl, $utf8Bom)

# ------------------------------------------------------------------ tulis props
$propLines = New-Object System.Collections.Generic.List[string]
$propLines.Add('<Project>')
$propLines.Add('')
$propLines.Add('  <!--')
$propLines.Add('    Dihasilkan oleh tooling\migrations\Update-MigrationHistory.ps1. Jangan disunting manual.')
$propLines.Add('')
$propLines.Add('    Dipakai target ValidateMigrationHistoryCoverage di QuilvianSystemBackend.csproj untuk')
$propLines.Add('    memastikan jumlah Designer di Migrations\History\ sama dengan jumlah migration yang')
$propLines.Add('    dideklarasikan di Migrations\MigrationMetadata.g.cs. Bila tidak sama, ada migration')
$propLines.Add('    yang diarsipkan tanpa atribut penggantinya, dan build harus gagal keras di situ')
$propLines.Add('    daripada menghasilkan assembly yang membuat migration hilang tanpa peringatan.')
$propLines.Add('  -->')
$propLines.Add('  <PropertyGroup>')
$propLines.Add('    <MigrationMetadataDeclaredCount>' + $archive.Count + '</MigrationMetadataDeclaredCount>')
$propLines.Add('  </PropertyGroup>')
$propLines.Add('')
$propLines.Add('</Project>')

[System.IO.File]::WriteAllText($propsFile, ($propLines -join $nl) + $nl, $utf8Bom)

Write-Host ''
Write-Host "Ditulis: $metadataFile ($($archive.Count) deklarasi)"
Write-Host "Ditulis: $propsFile"
