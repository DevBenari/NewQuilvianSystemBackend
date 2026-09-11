#!/usr/bin/env bash
#
# Authorization Verifier — gerbang regresi otorisasi yang permanen dan BACA-SAJA.
#
# Menggantikan invarian yang dulu dijaga backend test project, tanpa
# menghidupkan kembali folder Tests/. Lihat
# docs/module-blueprints/platform-authorization/evidence/08-*.md untuk 73
# invarian asli beserta alasannya.
#
# YANG TIDAK DILAKUKAN VERIFIER INI
#   - tidak menyalakan aplikasi, tidak memakai host ASP.NET
#   - tidak membuka koneksi database
#   - tidak memanggil AccessMenuSeeder
#   - tidak menyentuh SysAccessPolicy
#   - tidak menulis apa pun ke dalam repository
#
# SATU-SATUNYA OTORITAS PENEMUAN
#   PermissionRegistryDescriptor.BuildFromAssembly(
#       typeof(AccessPermissionService).Assembly)
#
#   Sama persis dengan yang dipakai AccessMenuSeeder. Verifier ini TIDAK
#   memakai grep, tidak mem-parsing source, dan tidak punya algoritma
#   penemuan sendiri.
#
# KENAPA RUNNER-NYA DIBUAT SAAT DIJALANKAN
#   QuilvianSystemBackend.csproj sudah TIDAK lagi memiliki
#   DefaultItemExcludes untuk Tests\**, sehingga berkas .cs mana pun di
#   dalam repository ikut ter-glob dan dikompilasi ke web project utama.
#   Runner karena itu dibuat di direktori temp OS, lalu dihapus.
#
# PEMAKAIAN
#   tools/authorization-verifier/verify-authorization.sh [--configuration Release]
#
# KELUAR DENGAN
#   0  seluruh invarian terpenuhi
#   1  ada invarian yang dilanggar
#   2  kesalahan pemakaian atau assembly tidak ditemukan

set -euo pipefail

CONFIGURATION="Release"
while [ $# -gt 0 ]; do
  case "$1" in
    --configuration) CONFIGURATION="${2:-}"; shift 2 ;;
    -h|--help) sed -n '2,40p' "$0"; exit 0 ;;
    *) echo "Argumen tidak dikenal: $1" >&2; exit 2 ;;
  esac
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Git Bash di Windows memakai jalur POSIX (/c/...), sementara MSBuild dan
# runtime .NET menuntut jalur Windows (C:\...). Di Linux CI cygpath tidak ada
# dan jalurnya diteruskan apa adanya.
to_native() {
  if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else printf '%s' "$1"; fi
}

ASSEMBLY="$REPO_ROOT/bin/$CONFIGURATION/net9.0/QuilvianSystemBackend.dll"
if [ ! -f "$ASSEMBLY" ]; then
  ALT="$REPO_ROOT/bin/Debug/net9.0/QuilvianSystemBackend.dll"
  if [ -f "$ALT" ]; then
    echo "Assembly $CONFIGURATION tidak ada; memakai Debug." >&2
    ASSEMBLY="$ALT"
  else
    echo "GAGAL: QuilvianSystemBackend.dll tidak ditemukan." >&2
    echo "       Bangun dulu project utamanya:" >&2
    echo "       dotnet build ./QuilvianSystemBackend.csproj -c $CONFIGURATION" >&2
    exit 2
  fi
fi
ASSEMBLY_DIR="$(cd "$(dirname "$ASSEMBLY")" && pwd)"

FALLBACK_LIST="$SCRIPT_DIR/approved-compatibility-fallback.txt"
REQUIRED_LIST="$SCRIPT_DIR/required-identities.txt"
for f in "$FALLBACK_LIST" "$REQUIRED_LIST"; do
  [ -f "$f" ] || { echo "GAGAL: berkas governance hilang: $f" >&2; exit 2; }
done

# Runner dibuat di temp, dan dihapus apa pun hasilnya.
RUNNER_DIR="$(mktemp -d 2>/dev/null || mktemp -d -t authzverify)"
RUNNER_NAME="quilvian-authz-verifier"
cleanup() {
  rm -rf "$RUNNER_DIR" 2>/dev/null || true
  rm -f "$ASSEMBLY_DIR/$RUNNER_NAME."* 2>/dev/null || true
}
trap cleanup EXIT INT TERM

cat > "$RUNNER_DIR/$RUNNER_NAME.csproj" <<PROJ
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>$RUNNER_NAME</AssemblyName>
    <RootNamespace>QuilvianAuthzVerifier</RootNamespace>
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include="QuilvianSystemBackend">
      <HintPath>$(to_native "$ASSEMBLY")</HintPath>
    </Reference>
  </ItemGroup>
</Project>
PROJ

cat > "$RUNNER_DIR/Program.cs" <<'PROG'
using QuilvianSystemBackend.Services.Security;

// Satu-satunya otoritas penemuan. Murni refleksi atas atribut: tidak
// menyalakan host, tidak menyentuh database, tidak memanggil seeder.
var snapshot = PermissionRegistryDescriptor.BuildFromAssembly(
    typeof(AccessPermissionService).Assembly);

static HashSet<string> ReadList(string path) =>
    File.ReadAllLines(path)
        .Select(x => x.Trim())
        .Where(x => x.Length > 0 && !x.StartsWith("#"))
        .ToHashSet(StringComparer.Ordinal);

var approvedFallback = ReadList(args[0]);
var requiredIdentities = ReadList(args[1]);

var failures = new List<string>();

// ---- Diagnostik. BUKAN kriteria lulus/gagal: jumlah ini memang tumbuh
// ---- seiring modul baru ditambahkan.
Console.WriteLine("Registry diagnostics (informational only):");
Console.WriteLine($"  Modules       = {snapshot.Modules.Count}");
Console.WriteLine($"  Resources     = {snapshot.Resources.Count}");
Console.WriteLine($"  Actions       = {snapshot.Actions.Count}");
Console.WriteLine($"  Fallback      = {snapshot.UnenforcedActions.Count}");
Console.WriteLine($"  Metadata gaps = {snapshot.MetadataGaps.Count}");
Console.WriteLine();

// ---- Invarian 1: nol metadata gap.
// Endpoint terproteksi yang kuncinya tidak terdaftar tidak dapat diberikan
// admin, sehingga menolak semua orang kecuali SuperAdmin — selamanya.
if (snapshot.MetadataGaps.Count > 0)
{
    failures.Add($"[1] {snapshot.MetadataGaps.Count} endpoint terproteksi tanpa kunci registry:");
    foreach (var g in snapshot.MetadataGaps.OrderBy(x => x.ResourceName).ThenBy(x => x.ActionName))
        failures.Add($"      {g.ResourceName}.{g.ActionName}  ({g.DeclaringController}.{g.MethodName})");
}

// ---- Invarian 2: himpunan fallback tidak berubah tanpa keputusan governance.
// Dibandingkan sebagai himpunan, bukan jumlah: pertukaran diam-diam
// (satu diperbaiki, satu baru masuk) menjaga jumlah tetap sama.
var actualFallback = snapshot.UnenforcedActions
    .Select(x => $"{x.DeclaringController}.{x.MethodName}")
    .ToHashSet(StringComparer.Ordinal);

var newlyFallback = actualFallback.Except(approvedFallback, StringComparer.Ordinal)
    .OrderBy(x => x, StringComparer.Ordinal).ToList();
var noLongerFallback = approvedFallback.Except(actualFallback, StringComparer.Ordinal)
    .OrderBy(x => x, StringComparer.Ordinal).ToList();

if (newlyFallback.Count > 0)
{
    failures.Add("[2] Endpoint memakai fallback kompatibilitas tanpa persetujuan.");
    failures.Add("    Endpoint terproteksi yang baru wajib memakai [AccessPermission]:");
    foreach (var x in newlyFallback) failures.Add($"      + {x}");
}
if (noLongerFallback.Count > 0)
{
    failures.Add("[2] Endpoint tidak lagi memakai fallback. Ini kabar baik, tetapi");
    failures.Add("    allowlist wajib diperbarui secara sadar supaya tetap menggambarkan keadaan:");
    foreach (var x in noLongerFallback) failures.Add($"      - {x}");
}

// ---- Invarian 3: identitas kanonik tidak ganda dan tidak bertentangan.
var duplicateKeys = snapshot.Actions
    .GroupBy(x => PermissionRegistryDescriptor.RegistrySnapshot.Key(x.ResourceName, x.ActionName),
             StringComparer.Ordinal)
    .Where(g => g.Count() > 1)
    .Select(g => g.Key)
    .OrderBy(x => x, StringComparer.Ordinal)
    .ToList();
if (duplicateKeys.Count > 0)
{
    failures.Add("[3] Identitas kanonik ganda:");
    foreach (var k in duplicateKeys) failures.Add($"      {k}");
}

var resourceInManyModules = snapshot.Resources
    .GroupBy(x => x.ResourceName, StringComparer.Ordinal)
    .Where(g => g.Select(x => x.ModuleCode).Distinct(StringComparer.Ordinal).Count() > 1)
    .OrderBy(g => g.Key, StringComparer.Ordinal)
    .ToList();
if (resourceInManyModules.Count > 0)
{
    // Identitas SysControllerAccess adalah (ModuleId, ResourceName). Resource
    // yang terdaftar di dua modul meyatimkan policy saat seeder menutup baris lama.
    failures.Add("[3] Resource terdaftar pada lebih dari satu modul:");
    foreach (var g in resourceInManyModules)
        failures.Add($"      {g.Key}  ->  {string.Join(", ", g.Select(x => x.ModuleCode).Distinct().OrderBy(x => x))}");
}

var fallbackKeys = snapshot.UnenforcedActions
    .Select(x => PermissionRegistryDescriptor.RegistrySnapshot.Key(x.ResourceName, x.ActionName))
    .ToHashSet(StringComparer.Ordinal);
var notFromPermission = snapshot.Actions
    .Select(x => PermissionRegistryDescriptor.RegistrySnapshot.Key(x.ResourceName, x.ActionName))
    .Where(k => !fallbackKeys.Contains(k) && !snapshot.DeclaredKeys.Contains(k))
    .Distinct(StringComparer.Ordinal)
    .OrderBy(x => x, StringComparer.Ordinal)
    .ToList();
if (notFromPermission.Count > 0)
{
    failures.Add("[3] Identitas terdaftar yang tidak berasal dari [AccessPermission]:");
    foreach (var k in notFromPermission) failures.Add($"      {k}");
}

// ---- Invarian 4: identitas target BE-SEC-003 ada di source.
var declared = snapshot.Actions
    .Select(x => $"{x.ResourceName}.{x.ActionName}")
    .ToHashSet(StringComparer.Ordinal);
var missing = requiredIdentities.Except(declared, StringComparer.Ordinal)
    .OrderBy(x => x, StringComparer.Ordinal).ToList();

Console.WriteLine($"Required identities: {requiredIdentities.Count - missing.Count} / {requiredIdentities.Count} present.");
if (missing.Count > 0)
{
    failures.Add($"[4] {missing.Count} identitas wajib hilang dari source:");
    foreach (var k in missing) failures.Add($"      {k}");
}
Console.WriteLine();

if (failures.Count > 0)
{
    Console.WriteLine("AUTHORIZATION VERIFIER: FAIL");
    Console.WriteLine();
    foreach (var line in failures) Console.WriteLine(line);
    return 1;
}

Console.WriteLine("AUTHORIZATION VERIFIER: PASS");
Console.WriteLine("  [1] metadata gap                 : 0");
Console.WriteLine($"  [2] fallback himpunan disetujui  : {actualFallback.Count} cocok persis");
Console.WriteLine("  [3] identitas kanonik            : tidak ganda, satu resource satu modul");
Console.WriteLine($"  [4] identitas wajib BE-SEC-003   : {requiredIdentities.Count} / {requiredIdentities.Count}");
return 0;
PROG

echo "Membangun runner sementara di $RUNNER_DIR"
if ! dotnet build "$(to_native "$RUNNER_DIR/$RUNNER_NAME.csproj")" \
      -c Release -o "$(to_native "$ASSEMBLY_DIR")" --nologo -v quiet > "$RUNNER_DIR/build.log" 2>&1; then
  echo "GAGAL membangun runner verifier:" >&2
  tail -30 "$RUNNER_DIR/build.log" >&2
  exit 2
fi

echo "Menjalankan verifier terhadap $(basename "$ASSEMBLY") ($CONFIGURATION)"
echo "-----------------------------------------------------------------------"
set +e
dotnet "$(to_native "$ASSEMBLY_DIR/$RUNNER_NAME.dll")" \
       "$(to_native "$FALLBACK_LIST")" \
       "$(to_native "$REQUIRED_LIST")"
STATUS=$?
set -e
echo "-----------------------------------------------------------------------"
exit $STATUS
