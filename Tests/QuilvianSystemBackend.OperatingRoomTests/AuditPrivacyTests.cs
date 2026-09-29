using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Jejak audit dan privasi jejak itu (`BE-OPR-011`, DoD "privacy log test").
/// </summary>
/// <remarks>
/// <para>
/// Jejak audit ada supaya setiap perubahan kasus operasi dapat ditelusuri ke pelakunya. Jejak
/// itu sendiri tidak boleh berubah menjadi salinan rekam medis: nama pasien, nomor rekam
/// medis, indikasi, dan temuan operasi tidak perlu ada di log untuk menjawab pertanyaan
/// "siapa mengubah apa, kapan", sedangkan log biasanya tersimpan lebih longgar daripada
/// basis data klinis.
/// </para>
/// </remarks>
public class AuditPrivacyTests
{
    [Fact]
    public async Task Setiap_perubahan_status_meninggalkan_riwayat_beserta_pelakunya()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var kasusId = await alur.KasusBerjalanAsync();

        await using var db = harness.NewContext();
        var riwayat = await db.OprStatusHistories.AsNoTracking()
            .Where(x => x.OprCaseId == kasusId && !x.IsDelete).ToListAsync();

        Assert.NotEmpty(riwayat);
        Assert.All(riwayat, x => Assert.NotEqual(Guid.Empty, x.CreateBy));
        Assert.All(riwayat, x => Assert.False(string.IsNullOrWhiteSpace(x.Action)));
        Assert.Contains(riwayat, x => x.Action == "Request");
        Assert.Contains(riwayat, x => x.Action == "Start");
    }

    /// <remarks>
    /// <para>
    /// Yang dituntut di sini hanya tindakannya, bukan pelakunya dan bukan nomor kasusnya.
    /// Alasannya bukan kelonggaran, melainkan keadaan `LoggerService` sekarang: data yang
    /// dikirim service Operasi — nomor kasus, id pelaku, hasil, revisi — tidak ikut tercetak
    /// pada baris lognya, dan kolom `UserId` diisi dari `UserId` lalu `Id` pada data itu,
    /// sehingga yang muncul justru id kasus.
    /// </para>
    /// <para>
    /// Selama itu belum diperbaiki, pertanggungjawaban pelaku dibuktikan lewat
    /// `OprStatusHistory.CreateBy` pada uji di atas. Uji ini sengaja tidak menuntut nama
    /// pelaku pada log, supaya ia tidak diam-diam mengesahkan keadaan yang salah itu.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task Setiap_pembuatan_kasus_meninggalkan_baris_audit()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        await alur.BuatKasusAsync();

        var audit = harness.LogEntries.Where(x => x.Contains("[AUD]", StringComparison.Ordinal)).ToList();

        Assert.NotEmpty(audit);
        Assert.Contains(audit, x => x.Contains("OperatingRoomCase.Create", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Jejak_audit_tidak_memuat_identitas_pasien_maupun_isi_klinis()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        await alur.KasusBerjalanAsync();

        await using var db = harness.NewContext();
        var pasien = await db.MstPatients.AsNoTracking()
            .Where(x => x.Id == seed.PasienId)
            .Select(x => new { Nama = x.FullName, x.PatientCode, x.MedicalRecordNumber })
            .FirstAsync();

        var jejak = string.Join("\n", harness.LogEntries);

        Assert.DoesNotContain(pasien.Nama, jejak, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Indikasi uji otomatis", jejak, StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(pasien.MedicalRecordNumber))
            Assert.DoesNotContain(pasien.MedicalRecordNumber, jejak, StringComparison.OrdinalIgnoreCase);
    }
}
