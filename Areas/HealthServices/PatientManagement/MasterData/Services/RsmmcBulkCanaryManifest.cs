using System.Collections.ObjectModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Manifest eksekusi pasien yang disimpan di server. Klien tidak pernah mengirim daftar
    /// <c>legacy_pid</c> (`PAT-DEC-044`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Urutan eksekusi adalah urutan manifest persis; manifest tidak pernah disort ulang (`PAT-DEC-045`).
    /// Pasien manifest yang keadaannya berubah diklasifikasikan, tidak pernah diganti pasien lain
    /// (`PAT-DEC-026`, `PAT-DEC-040`).
    /// </para>
    /// <para>
    /// Sidik jari berurutan = SHA-256 dari <c>legacy_pid</c> dalam urutan manifest, dipisah koma, tanpa
    /// spasi, UTF-8 (kontrak B2.2 <c>manifestSha256</c>).
    /// </para>
    /// </remarks>
    public sealed class RsmmcBulkCanaryManifest
    {
        /// <summary>Sidik jari berurutan <c>RSMMC-CANARY-A</c> v1 yang disetujui (`PAT-DEC-044`, `056`).</summary>
        public const string CanaryAV1OrderedSha256 = "c43207eb73f1f244db13ff930bca1c22e2e4ae779018e5cd48e26f628a5a79ad";

        /// <summary>
        /// Sidik jari himpunan lama — jejak, tidak dipakai respons. SHA-256 dari <c>legacy_pid</c> yang
        /// diurutkan naik; tidak membedakan dua manifest yang isinya sama tetapi urutannya berbeda.
        /// </summary>
        public const string CanaryAV1HistoricalSetFingerprint = "3b5affd9558d24da2fe8164c5c9d16ea230b89b4be0404889a55ad22193434aa";

        public RsmmcBulkCanaryManifest(
            string manifestId,
            int version,
            IEnumerable<long> legacyPids,
            string expectedOrderedSha256,
            IReadOnlyDictionary<string, int> expectedSourceGroupCounts)
        {
            ManifestId = manifestId;
            Version = version;
            LegacyPids = new ReadOnlyCollection<long>(legacyPids.ToArray());
            ExpectedOrderedSha256 = expectedOrderedSha256;
            ExpectedSourceGroupCounts = new ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>(expectedSourceGroupCounts, StringComparer.Ordinal));
        }

        public string ManifestId { get; }

        public int Version { get; }

        /// <summary><c>legacy_pid</c> dalam urutan eksekusi yang disetujui.</summary>
        public IReadOnlyList<long> LegacyPids { get; }

        public string ExpectedOrderedSha256 { get; }

        /// <summary>Sebaran kelompok sumber <c>source_group</c> yang diharapkan, dicocokkan dengan preview saat runtime.</summary>
        public IReadOnlyDictionary<string, int> ExpectedSourceGroupCounts { get; }

        /// <summary>
        /// Manifest eksekusi <c>RSMMC-CANARY-A</c> versi 1: 25 <c>legacy_pid</c> kandidat
        /// (`00-interview-decisions.md` bagian 11), dalam urutan yang sama (`PAT-DEC-056`).
        /// </summary>
        public static RsmmcBulkCanaryManifest CanaryAV1 { get; } = new(
            "RSMMC-CANARY-A",
            1,
            [
                -667992, -667991, -667986, 797423, -667995,
                -667984, 797554, 1, 888, 2907,
                169, 33257, 53055, 420806, 711838,
                797403, 728756, 337098, 490899, 0,
                29, 824, 98, 317, 620
            ],
            CanaryAV1OrderedSha256,
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["NOREG_WITHOUT_RM"] = 7,
                ["NOREG_WITH_RM"] = 6,
                ["REG_WITHOUT_RM"] = 6,
                ["REG_WITH_RM"] = 6
            });

        /// <summary>Teks kanonik sidik jari: <c>legacy_pid</c> dalam urutan manifest, dipisah koma, tanpa spasi.</summary>
        public static string BuildCanonicalText(IEnumerable<long> legacyPids) =>
            string.Join(",", legacyPids.Select(x => x.ToString(CultureInfo.InvariantCulture)));

        public static string ComputeSha256(string canonicalText) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalText))).ToLowerInvariant();

        /// <summary>SHA-256 berurutan dari isi manifest ini, dihitung ulang, tidak disalin dari konstanta.</summary>
        public string ComputeOrderedSha256() => ComputeSha256(BuildCanonicalText(LegacyPids));
    }
}
