using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Penyimpan artefak QR tanpa penimpaan, dan pembaca tabel migrasi yang hanya membaca —
/// `AC-09`, `AC-10`, `AC-12`, `AC-13`, `AC-20` sampai `AC-22`, `PAT-OQ-005`, `PAT-OQ-007`.
/// </summary>
public class PenyimpanQrDanSumberMigrasiTests
{
    [Fact]
    public void PathPublik_SamaDenganPolaPembuatanPasien()
    {
        using var h = new RekonsiliasiHarness();

        var path = h.BuatStore().BuildPublicPath("00-79-70-15");

        Assert.Equal("/uploads/patient-qrcodes/00-79-70-15/qrcode.png", path);
    }

    [Fact]
    public void PayloadProduksi_AdalahMrnKanonik()
    {
        Assert.Equal("00-79-70-15", FormatQrPasien.Payload("00-79-70-15"));
    }

    [Fact]
    public void PayloadProduksi_SamaDenganPembentukQrKanonikIntegrasi()
    {
        // Sejak BE-RWI-187, BuildPatientQrPayload mendelegasikan ke PatientQrPayloadBuilder. QR
        // rekonsiliasi wajib tetap memakai pembentuk yang sama dengan pembuatan pasien biasa.
        Assert.Equal(PatientQrPayloadBuilder.Build("00-79-70-15"), FormatQrPasien.Payload("00-79-70-15"));
        Assert.Equal(PatientQrPayloadBuilder.Build("00797015"), FormatQrPasien.Payload("00797015"));
        Assert.Equal("00-79-70-15", FormatQrPasien.Payload("00797015"));
    }

    [Fact]
    public void PembuatPngProduksi_MenghasilkanPngUntukMrnKanonik()
    {
        var png = FormatQrPasien.RenderAsli(FormatQrPasien.Payload("00-79-70-15"));

        Assert.True(png.Length > 8);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, png[..8]);
    }

    [Fact]
    public void CreateNew_ArtefakSudahAda_KonflikDanIsiTidakBerubah()
    {
        using var h = new RekonsiliasiHarness();
        RekonsiliasiHarness.TulisFile(h.PathQrFisik("00-79-70-15"), "ISI-LAMA");

        var hasil = h.BuatStore().CreateNew("00-79-70-15", "uji");

        Assert.True(hasil.IsConflict);
        Assert.False(hasil.IsRaceConflict);
        Assert.Null(hasil.Created);
        Assert.Equal("ISI-LAMA", File.ReadAllText(h.PathQrFisik("00-79-70-15")));
        Assert.Empty(h.PayloadDirender);
    }

    [Fact]
    public void CreateNew_ArtefakMunculSaatPembentukan_KonflikRaceDanIsiTidakBerubah()
    {
        using var h = new RekonsiliasiHarness();
        h.SaatRender = _ => RekonsiliasiHarness.TulisFile(h.PathQrFisik("00-79-70-15"), "ISI-PROSES-LAIN");

        var hasil = h.BuatStore().CreateNew("00-79-70-15", "uji");

        Assert.True(hasil.IsConflict);
        Assert.True(hasil.IsRaceConflict);
        Assert.Equal("ISI-PROSES-LAIN", File.ReadAllText(h.PathQrFisik("00-79-70-15")));
    }

    [Fact]
    public void CreateNew_MencatatBuktiKepemilikan_PenandaPanjangDanSha256()
    {
        using var h = new RekonsiliasiHarness();

        var dibuat = h.BuatStore().CreateNew("00-79-70-15", "uji").Created!;

        var isi = File.ReadAllBytes(h.PathQrFisik("00-79-70-15"));
        Assert.True(dibuat.CreatedByThisInvocation);
        Assert.Equal(isi.LongLength, dibuat.Length);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(isi)).ToLowerInvariant(), dibuat.Sha256);
        Assert.Equal(64, dibuat.Sha256.Length);
    }

    [Fact]
    public void TryDeleteCreated_ArtefakPersisMiliknya_Dihapus_BerkasLainDiFolderTetap()
    {
        using var h = new RekonsiliasiHarness();
        var store = h.BuatStore();
        var folder = Path.GetDirectoryName(h.PathQrFisik("00-79-70-15"))!;
        var berkasLain = Path.Combine(folder, "catatan.txt");
        RekonsiliasiHarness.TulisFile(berkasLain, "JANGAN-DIHAPUS");

        var hasil = store.CreateNew("00-79-70-15", "uji");
        Assert.NotNull(hasil.Created);
        Assert.False(hasil.Created!.CreatedFolder);

        var bersih = store.TryDeleteCreated(hasil.Created);

        Assert.Equal(RsmmcPilotQrCleanupOutcome.Deleted, bersih.Outcome);
        Assert.Null(bersih.Error);
        Assert.False(File.Exists(h.PathQrFisik("00-79-70-15")));
        Assert.Equal("JANGAN-DIHAPUS", File.ReadAllText(berkasLain));
        Assert.True(Directory.Exists(folder));
    }

    [Fact]
    public void TryDeleteCreated_PanjangSamaIsiBerbeda_TidakDihapus()
    {
        using var h = new RekonsiliasiHarness();
        var store = h.BuatStore();
        var dibuat = store.CreateNew("00-79-70-15", "uji").Created!;
        var path = h.PathQrFisik("00-79-70-15");

        // Isi diganti dengan panjang yang persis sama. Ukuran saja akan salah menyimpulkan "milik".
        var pengganti = new string('X', (int)dibuat.Length);
        File.WriteAllText(path, pengganti);
        Assert.Equal(dibuat.Length, new FileInfo(path).Length);

        var bersih = store.TryDeleteCreated(dibuat);

        Assert.Equal(RsmmcPilotQrCleanupOutcome.OwnershipUnproven, bersih.Outcome);
        Assert.Equal(pengganti, File.ReadAllText(path));
    }

    [Fact]
    public void TryDeleteCreated_PanjangBerbeda_TidakDihapus()
    {
        using var h = new RekonsiliasiHarness();
        var store = h.BuatStore();
        var dibuat = store.CreateNew("00-79-70-15", "uji").Created!;
        File.WriteAllText(h.PathQrFisik("00-79-70-15"), "BERKAS-PENGGANTI-YANG-LEBIH-PANJANG");

        var bersih = store.TryDeleteCreated(dibuat);

        Assert.Equal(RsmmcPilotQrCleanupOutcome.OwnershipUnproven, bersih.Outcome);
        Assert.Equal("BERKAS-PENGGANTI-YANG-LEBIH-PANJANG", File.ReadAllText(h.PathQrFisik("00-79-70-15")));
    }

    [Fact]
    public void TryDeleteCreated_BukanBuatanPemanggilanIni_TidakDihapusWalauIsinyaCocok()
    {
        using var h = new RekonsiliasiHarness();
        var store = h.BuatStore();
        var dibuat = store.CreateNew("00-79-70-15", "uji").Created!;

        var bersih = store.TryDeleteCreated(dibuat with { CreatedByThisInvocation = false });

        Assert.Equal(RsmmcPilotQrCleanupOutcome.NotCreatedByThisInvocation, bersih.Outcome);
        Assert.True(File.Exists(h.PathQrFisik("00-79-70-15")));
    }

    [Fact]
    public void TryDeleteCreated_TidakPernahMenyentuhQrLama()
    {
        using var h = new RekonsiliasiHarness();
        h.TambahPilot("1001", "00-00-07-01", "00-79-70-15");
        var store = h.BuatStore();
        var dibuat = store.CreateNew("00-79-70-15", "uji").Created!;

        // Artefak yang diarahkan ke path QR lama: penandanya benar, tetapi digest-nya bukan milik
        // QR lama, sehingga QR lama tetap tidak terhapus.
        var bersih = store.TryDeleteCreated(dibuat with { PhysicalPath = h.PathQrFisik("00-00-07-01") });

        Assert.NotEqual(RsmmcPilotQrCleanupOutcome.Deleted, bersih.Outcome);
        Assert.Equal("QR-LAMA:00-00-07-01", File.ReadAllText(h.PathQrFisik("00-00-07-01")));
    }

    [Fact]
    public void SqlPembacaMigrasi_HanyaMembaca()
    {
        var terlarang = new Regex(
            @"\b(INSERT|UPDATE|DELETE|TRUNCATE|ALTER|DROP|CREATE|MERGE|COPY|GRANT|REVOKE|CALL|DO)\b",
            RegexOptions.IgnoreCase);

        foreach (var (nama, sql) in SemuaSql())
        {
            Assert.StartsWith("SELECT", sql.TrimStart(), StringComparison.OrdinalIgnoreCase);
            Assert.False(terlarang.IsMatch(sql), $"{nama} memuat perintah yang mengubah data atau skema.");
            Assert.DoesNotContain(";", sql);
        }
    }

    [Fact]
    public void SqlPembacaMigrasi_TidakMemakaiNormalizedMrn_DanNilaiMasukanBerparameter()
    {
        foreach (var (nama, sql) in SemuaSql())
        {
            Assert.DoesNotContain("normalized_mrn", sql, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("{0}", sql);
        }

        var mappedSql = SemuaSql().Single(x => x.Nama == "MappedFinalizedRowsSql").Sql;
        Assert.Contains("final_medical_record_number", mappedSql);
        Assert.Contains("p.plan_status = 'FINALIZED'", mappedSql);
        Assert.Contains("c.quilvian_patient_id IS NOT NULL", mappedSql);
        Assert.Contains("ORDER BY p.legacy_pid", mappedSql);
    }

    [Fact]
    public void AntarmukaSumberMigrasi_TidakPunyaOperasiTulis()
    {
        var operasi = typeof(IRsmmcPilotMigrationSource).GetMethods().Select(x => x.Name).ToList();

        Assert.All(operasi, nama => Assert.True(
            nama.StartsWith("Get", StringComparison.Ordinal) || nama.StartsWith("Count", StringComparison.Ordinal),
            $"{nama} bukan operasi baca."));
    }

    private static List<(string Nama, string Sql)> SemuaSql() =>
        typeof(RsmmcPilotMigrationSource)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(x => x.IsLiteral && x.FieldType == typeof(string) && x.Name.EndsWith("Sql", StringComparison.Ordinal))
            .Select(x => (x.Name, (string)x.GetRawConstantValue()!))
            .ToList();
}
