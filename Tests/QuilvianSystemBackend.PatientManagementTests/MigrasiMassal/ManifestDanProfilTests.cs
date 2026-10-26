using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;

namespace QuilvianSystemBackend.Tests.PatientManagement.MigrasiMassal;

/// <summary>
/// Manifest eksekusi <c>RSMMC-CANARY-A</c> v1, profil batch kanonik, kunci per pasien, dan penjaga
/// governance Pilot — `PAT-MIG2-AC-02`, `03`, `10`, `11`, `29`, `46`, `48`, `50`; `PAT-DEC-044`, `045`,
/// `048`, `056`, `077`.
/// </summary>
public class ManifestDanProfilTests
{
    // Disalin dari 00-interview-decisions.md bagian 11 dan prompt pemilik, bukan dari source produksi.
    private static readonly long[] UrutanDisetujui =
    [
        -667992, -667991, -667986, 797423, -667995,
        -667984, 797554, 1, 888, 2907,
        169, 33257, 53055, 420806, 711838,
        797403, 728756, 337098, 490899, 0,
        29, 824, 98, 317, 620
    ];

    private const string ShaBerurutanDisetujui = "c43207eb73f1f244db13ff930bca1c22e2e4ae779018e5cd48e26f628a5a79ad";
    private const string SidikJariHimpunanLama = "3b5affd9558d24da2fe8164c5c9d16ea230b89b4be0404889a55ad22193434aa";

    [Fact]
    public void Manifest_IdentitasIsiDanUrutanPersis()
    {
        var manifest = RsmmcBulkCanaryManifest.CanaryAV1;

        Assert.Equal("RSMMC-CANARY-A", manifest.ManifestId);
        Assert.Equal(1, manifest.Version);
        Assert.Equal(UrutanDisetujui, manifest.LegacyPids);
        Assert.Equal(25, manifest.LegacyPids.Count);
        Assert.Equal(25, manifest.LegacyPids.Distinct().Count());
    }

    [Fact]
    public void Manifest_SidikJariBerurutanPersis_DihitungUlangBukanDisalin()
    {
        var manifest = RsmmcBulkCanaryManifest.CanaryAV1;
        var teks = string.Join(",", UrutanDisetujui.Select(x => x.ToString(CultureInfo.InvariantCulture)));
        var shaMandiri = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(teks))).ToLowerInvariant();

        Assert.Equal(ShaBerurutanDisetujui, shaMandiri);
        Assert.Equal(ShaBerurutanDisetujui, manifest.ComputeOrderedSha256());
        Assert.Equal(ShaBerurutanDisetujui, manifest.ExpectedOrderedSha256);
        Assert.Equal(ShaBerurutanDisetujui, RsmmcBulkCanaryManifest.CanaryAV1OrderedSha256);
        Assert.Equal(teks, RsmmcBulkCanaryManifest.BuildCanonicalText(manifest.LegacyPids));
    }

    [Fact]
    public void Manifest_SidikJariHimpunanLama_HanyaJejakDanBerbedaDariBerurutan()
    {
        var naik = UrutanDisetujui.OrderBy(x => x).ToArray();

        Assert.Equal(SidikJariHimpunanLama, RsmmcBulkCanaryManifest.CanaryAV1HistoricalSetFingerprint);
        Assert.Equal(SidikJariHimpunanLama, RsmmcBulkCanaryManifest.ComputeSha256(RsmmcBulkCanaryManifest.BuildCanonicalText(naik)));
        Assert.NotEqual(SidikJariHimpunanLama, RsmmcBulkCanaryManifest.CanaryAV1.ComputeOrderedSha256());
    }

    [Fact]
    public void Manifest_IsiSamaUrutanBerbeda_SidikJariBerbeda()
    {
        var naik = new RsmmcBulkCanaryManifest("X", 1, UrutanDisetujui.OrderBy(x => x), ShaBerurutanDisetujui, new Dictionary<string, int>());
        var tertukar = UrutanDisetujui.ToArray();
        (tertukar[0], tertukar[1]) = (tertukar[1], tertukar[0]);
        var dua = new RsmmcBulkCanaryManifest("X", 1, tertukar, ShaBerurutanDisetujui, new Dictionary<string, int>());

        Assert.NotEqual(ShaBerurutanDisetujui, naik.ComputeOrderedSha256());
        Assert.NotEqual(ShaBerurutanDisetujui, dua.ComputeOrderedSha256());
    }

    [Fact]
    public void Manifest_SebaranKelompokSumber_7_6_6_6()
    {
        var sebaran = RsmmcBulkCanaryManifest.CanaryAV1.ExpectedSourceGroupCounts;

        Assert.Equal(4, sebaran.Count);
        Assert.Equal(7, sebaran["NOREG_WITHOUT_RM"]);
        Assert.Equal(6, sebaran["NOREG_WITH_RM"]);
        Assert.Equal(6, sebaran["REG_WITHOUT_RM"]);
        Assert.Equal(6, sebaran["REG_WITH_RM"]);
        Assert.Equal(25, sebaran.Values.Sum());
    }

    [Fact]
    public void Manifest_TidakBerisiLegacyPidTertunda()
    {
        Assert.DoesNotContain(280143L, RsmmcBulkCanaryManifest.CanaryAV1.LegacyPids);
        Assert.DoesNotContain(805596L, RsmmcBulkCanaryManifest.CanaryAV1.LegacyPids);
    }

    [Fact]
    public void Manifest_TidakDapatDiubahSesudahDibentuk()
    {
        var sumber = new long[] { 3, 1, 2 };
        var manifest = new RsmmcBulkCanaryManifest("X", 1, sumber, "sha", new Dictionary<string, int> { ["A"] = 3 });

        sumber[0] = 99;

        Assert.Equal(new long[] { 3, 1, 2 }, manifest.LegacyPids);
        Assert.Throws<NotSupportedException>(() => ((IList<long>)manifest.LegacyPids)[0] = 7);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, int>)manifest.ExpectedSourceGroupCounts)["A"] = 1);
    }

    [Fact]
    public void Profil_NilaiKanonikPersis()
    {
        var profil = RsmmcBulkApprovedBatchProfile.Canonical;

        Assert.Equal(Guid.Parse("45eeefba-dd39-6831-1bd2-d986851f7c1f"), profil.BatchId);
        Assert.Equal(Guid.Parse("2ce3929b-b5af-430b-a2b4-bdb3aaf6908f"), profil.PilotBatchId);
        Assert.Equal(704506, profil.ExpectedPreviewRows);
        Assert.Equal(705223, profil.ExpectedFinalizedPlannerRows);
        Assert.Equal(new long[] { 280143, 805596 }, profil.DeferredLegacyPids);
        Assert.Equal(new[] { "PAT-RSMMC-101416", "PAT-RSMMC-619077" }, profil.DeferredReservedPatientCodes);
        Assert.Equal(741, profil.ApprovedPatientBaseline);
        Assert.Equal(715, profil.ExpectedPilotTargets);
        Assert.Equal(710, profil.ExpectedPilotAutoMigrated);
        Assert.Equal(5, profil.ExpectedPilotManualMigrated);
        Assert.Equal(715, profil.ExpectedPilotPreviewRows);
        Assert.Equal(715, profil.ExpectedBackupRows);
    }

    [Fact]
    public void Profil_StatusDanNamespaceQrPersis()
    {
        Assert.Equal("TARGET_CREATED", RsmmcBulkApprovedBatchProfile.MatchStatusTargetCreated);
        Assert.Equal("MIGRATED_BULK_WORKER", RsmmcBulkApprovedBatchProfile.MigrationStatusBulkWorker);
        Assert.Equal("MIGRATED_API_AUTO", RsmmcBulkApprovedBatchProfile.MigrationStatusPilotAuto);
        Assert.Equal("MIGRATED_API_PILOT", RsmmcBulkApprovedBatchProfile.MigrationStatusPilotManual);
        Assert.Equal("PENDING", RsmmcBulkApprovedBatchProfile.PreviewPendingStatus);
        Assert.Equal("FINALIZED", RsmmcBulkApprovedBatchProfile.PlannerFinalizedStatus);
        Assert.Equal("rsmmc-bulk", RsmmcBulkApprovedBatchProfile.QrNamespaceFolderName);

        // Panjang nilai muat varchar(30) tanpa perubahan skema (PAT-FACT-080).
        Assert.True(RsmmcBulkApprovedBatchProfile.MatchStatusTargetCreated.Length <= 30);
        Assert.True(RsmmcBulkApprovedBatchProfile.MigrationStatusBulkWorker.Length <= 30);
    }

    [Fact]
    public void KunciWorker_32BitPertamaBatchKanonik_TerpisahDariAllocatorNormal()
    {
        var hexAwal = RsmmcBulkApprovedBatchProfile.Canonical.BatchId.ToString("N")[..8];

        Assert.Equal(1173286842, RsmmcBulkApprovedBatchProfile.WorkerLockNamespace);
        Assert.Equal(0x45EEEFBA, RsmmcBulkApprovedBatchProfile.WorkerLockNamespace);
        Assert.Equal(RsmmcBulkApprovedBatchProfile.WorkerLockNamespace, int.Parse(hexAwal, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
        Assert.NotEqual(RsmmcIdentifierReservationProfile.AllocationLockNamespace, RsmmcBulkApprovedBatchProfile.WorkerLockNamespace);
    }

    [Theory]
    [InlineData(-667992L, true, -667992)]
    [InlineData(0L, true, 0)]
    [InlineData(797554L, true, 797554)]
    [InlineData(2147483647L, true, int.MaxValue)]
    [InlineData(-2147483648L, true, int.MinValue)]
    [InlineData(2147483648L, false, 0)]
    [InlineData(-2147483649L, false, 0)]
    [InlineData(long.MaxValue, false, 0)]
    [InlineData(long.MinValue, false, 0)]
    public void KonversiKunci_PemeriksaanRentangInt32Eksplisit(long legacyPid, bool sah, int kunci)
    {
        var hasil = RsmmcBulkApprovedBatchProfile.TryConvertLockKey(legacyPid, out var kunciTerbentuk);

        Assert.Equal(sah, hasil);

        if (sah)
        {
            Assert.Equal(kunci, kunciTerbentuk);
        }
    }

    [Fact]
    public void KonversiKunci_SeluruhManifestCanaryADalamRentangTanpaPerubahanNilai()
    {
        foreach (var legacyPid in RsmmcBulkCanaryManifest.CanaryAV1.LegacyPids)
        {
            Assert.True(RsmmcBulkApprovedBatchProfile.TryConvertLockKey(legacyPid, out var kunci));
            Assert.Equal(legacyPid, kunci);
        }
    }

    [Fact]
    public void Respons_LegacyPidBertipeTeks()
    {
        Assert.Equal(typeof(string), typeof(RsmmcBulkPatientMigrationItemResponse).GetProperty(nameof(RsmmcBulkPatientMigrationItemResponse.LegacyPid))!.PropertyType);
    }

    /// <summary>
    /// `BE-PAT-MIG-001` wajib tidak berubah byte demi byte (prompt pemilik "REUSE / DO NOT MODIFY"). Hash
    /// SHA-256 isi berkas — dinormalkan ke LF tanpa BOM — dihitung dari commit induk <c>c83bcdaa</c>.
    /// </summary>
    [Fact]
    public void Governance_SourcePilotTidakBerubahDariCommitInduk()
    {
        var akar = AkarRepository();
        var services = Path.Combine(akar, "Areas", "HealthServices", "PatientManagement", "MasterData", "Services");

        var pin = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Path.Combine(services, "RsmmcPilotApprovedBatchProfile.cs")] = "5410f5d5b3fc8e648e73088bd6cd7ec1ad99f3b941bb8268fad430898ddf7372",
            [Path.Combine(services, "RsmmcPilotEnvironmentGate.cs")] = "2c01b5a999c40b404982cf2df6a9ad89e607d6a44c5c750310c4d7b78f3c2bc3",
            [Path.Combine(services, "RsmmcPilotMigrationSource.cs")] = "6199d8c2473dd2833de76742945992150feac0e7badb3090b59ecdcea5f9d2ee",
            [Path.Combine(services, "RsmmcPilotMrnReconciliationService.cs")] = "f18a733032414a5d687d964739b18c263b09a3f382c61804a88abc071de9d43e",
            [Path.Combine(services, "RsmmcPilotQrArtifactStore.cs")] = "a42306dfd77efc341eecc905f0a2c29403acbc29290c3e2b70a9d3d347cb8a37",
            [Path.Combine(akar, "Areas", "HealthServices", "PatientManagement", "MasterData", "DTOs", "RsmmcPilotMrnReconciliationDtos.cs")] =
                "68bfe123f02dec08a656a0fc655c0b1c25842ed9f51fbfd3b92600376c58363d"
        };

        Assert.Equal(
            pin.Keys.Where(x => x.StartsWith(services, StringComparison.Ordinal)).Select(Path.GetFileName).OrderBy(x => x, StringComparer.Ordinal),
            Directory.GetFiles(services, "RsmmcPilot*.cs").Select(Path.GetFileName).OrderBy(x => x, StringComparer.Ordinal));

        foreach (var (berkas, hash) in pin)
        {
            var teks = File.ReadAllText(berkas).Replace("\r", string.Empty, StringComparison.Ordinal);
            var aktual = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(teks))).ToLowerInvariant();

            Assert.True(hash == aktual, $"{Path.GetFileName(berkas)} berubah dari commit induk c83bcdaa.");
        }
    }

    private static string AkarRepository()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);

        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "QuilvianSystemBackend.csproj")))
        {
            folder = folder.Parent;
        }

        return folder?.FullName
            ?? throw new InvalidOperationException("Akar repository tidak ditemukan; uji governance membutuhkan source checkout.");
    }
}
