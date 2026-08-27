using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Repositories;
using static QuilvianSystemBackend.Tests.HealthServices.EmergencyInstallationManagement.EmergencyControllerTestWorld;

namespace QuilvianSystemBackend.Tests.HealthServices.EmergencyInstallationManagement;

/// <summary>
/// Membuktikan <c>IGD-DEC-106</c>: gerbang penutupan kunjungan membaca keadaan
/// <b>fisik</b> pasien saja, dan penutupan tidak pernah diam soal dokumen serah terima yang
/// masih menggantung.
/// </summary>
/// <remarks>
/// Menutup <c>IGD-OQ-082</c>. Kontrak: validation-matrix bagian 4 aturan 8 dan bagian 6
/// aturan 3, disetujui <c>IGD-DEC-108</c>.
///
/// <para>
/// Alasan yang paling menentukan pada keputusan itu bukan soal dokumen, melainkan
/// <c>BE-IGD-025</c>: kunjungan yang tertahan karena tanda tangan unit lain akan memblokir
/// pendaftaran pasien <b>yang sama</b> ketika ia datang kembali. Test terakhir di kelas ini
/// menguji rantai itu langsung, supaya hubungannya tidak hilang bila seseorang kelak menganggap
/// aturan 3 terlalu longgar dan "memperketatnya".
/// </para>
/// </remarks>
public class EmergencyClosureHandoverTests
{
    private static readonly Guid Pelaku = Guid.NewGuid();

    private static async Task<TrxEmergencyDeparture> SimpanKepergianAsync(
        ApplicationDbContext context,
        Guid emergencyVisitId,
        EmergencyPhysicalStatus physicalStatus,
        EmergencyHandoverStatus handoverStatus)
    {
        var departure = new TrxEmergencyDeparture
        {
            Id = Guid.NewGuid(),
            EmergencyVisitId = emergencyVisitId,
            DepartureNumber = $"DEP{Guid.NewGuid():N}"[..12],
            ToServiceUnitId = Guid.NewGuid(),
            PhysicalStatus = physicalStatus,
            HandoverStatus = handoverStatus,
            RequestedByUserId = Pelaku,
            IsDelete = false,
        };

        context.Set<TrxEmergencyDeparture>().Add(departure);
        await context.SaveChangesAsync();
        return departure;
    }

    // =================================================================================
    // Dokumen belum final TIDAK menahan penutupan
    // =================================================================================

    /// <summary>
    /// Inti <c>IGD-DEC-106</c>. Pasien sudah tiba di unit tujuan; dokumennya belum
    /// ditandatangani. Validation bagian 4 aturan 8 menyebut keadaan ini <b>sah</b>, sehingga
    /// gerbang penutupan tidak boleh menahannya.
    /// </summary>
    [Theory]
    [InlineData(EmergencyHandoverStatus.Pending)]
    [InlineData(EmergencyHandoverStatus.Submitted)]
    [InlineData(EmergencyHandoverStatus.Rejected)]
    public async Task Penutupan_FisikTibaDokumenBelumFinal_TetapBerhasil(
        EmergencyHandoverStatus handoverStatus)
    {
        using var context = BuatContext("igd-closure");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Disposed);
        await SimpanKepergianAsync(context, visit.Id, EmergencyPhysicalStatus.Arrived, handoverStatus);

        var hasil = await BuatVisitController(context, Pelaku)
            .Complete(visit.Id, new CompleteVisitRequest());

        Assert.Equal(200, KodeStatus(hasil));

        var tersimpan = await context.Set<TrxEmergencyVisit>().FindAsync(visit.Id);
        Assert.Equal(EmergencyVisitStatus.Completed, tersimpan!.VisitStatus);
    }

    /// <summary>
    /// Syarat (d): penutupan wajib menyebut dokumen mana yang masih menggantung. Menutup
    /// tanpa menyebutnya membuat berkas yang belum tuntas hilang dari perhatian siapa pun.
    /// </summary>
    [Fact]
    public async Task Penutupan_DokumenMenggantung_DisebutPadaBalasannya()
    {
        using var context = BuatContext("igd-closure");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Disposed);
        var departure = await SimpanKepergianAsync(
            context, visit.Id, EmergencyPhysicalStatus.Arrived, EmergencyHandoverStatus.Pending);

        var hasil = await BuatVisitController(context, Pelaku)
            .Complete(visit.Id, new CompleteVisitRequest());

        Assert.Equal(200, KodeStatus(hasil));

        var pesan = Pesan(hasil);
        Assert.Contains(departure.DepartureNumber, pesan);
        Assert.Contains("masih menunggu unit tujuan", pesan);
    }

    /// <summary>
    /// Sebaliknya, penutupan yang bersih tidak boleh menakut-nakuti petugas dengan peringatan
    /// yang tidak ada isinya.
    /// </summary>
    [Fact]
    public async Task Penutupan_DokumenSudahDiterima_PesannyaPolos()
    {
        using var context = BuatContext("igd-closure");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Disposed);
        await SimpanKepergianAsync(
            context, visit.Id, EmergencyPhysicalStatus.Arrived, EmergencyHandoverStatus.Accepted);

        var hasil = await BuatVisitController(context, Pelaku)
            .Complete(visit.Id, new CompleteVisitRequest());

        Assert.Equal(200, KodeStatus(hasil));
        Assert.Equal("Kunjungan IGD berhasil diselesaikan.", Pesan(hasil));
    }

    // =================================================================================
    // Keadaan fisik TETAP menahan penutupan
    // =================================================================================

    [Theory]
    [InlineData(EmergencyPhysicalStatus.Prepared)]
    [InlineData(EmergencyPhysicalStatus.Departed)]
    public async Task Penutupan_FisikBelumTiba_Ditolak409(EmergencyPhysicalStatus physicalStatus)
    {
        using var context = BuatContext("igd-closure");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Disposed);
        await SimpanKepergianAsync(
            context, visit.Id, physicalStatus, EmergencyHandoverStatus.Accepted);

        var hasil = await BuatVisitController(context, Pelaku)
            .Complete(visit.Id, new CompleteVisitRequest());

        Assert.Equal(409, KodeStatus(hasil));
        Assert.Equal("Masih ada proses kepergian pasien yang belum selesai.", Pesan(hasil));
    }

    /// <summary>
    /// Kepergian yang <b>dibatalkan</b> kini dianggap tuntas. Ini perubahan perilaku nyata:
    /// gerbang lama memperlakukan <c>Cancelled</c> sebagai belum tuntas, sehingga satu
    /// perpindahan yang dibatalkan menahan kunjungan selamanya.
    /// </summary>
    [Fact]
    public async Task Penutupan_KepergianDibatalkan_DianggapTuntas()
    {
        using var context = BuatContext("igd-closure");
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Disposed);
        await SimpanKepergianAsync(
            context, visit.Id, EmergencyPhysicalStatus.Cancelled, EmergencyHandoverStatus.Cancelled);

        var hasil = await BuatVisitController(context, Pelaku)
            .Complete(visit.Id, new CompleteVisitRequest());

        Assert.Equal(200, KodeStatus(hasil));
    }

    // =================================================================================
    // Alasan ① IGD-DEC-106 — rantai ke pendaftaran berikutnya
    // =================================================================================

    /// <summary>
    /// Alasan yang paling menentukan pada <c>IGD-DEC-106</c>, diuji langsung: setelah kunjungan
    /// ditutup, pasien yang sama boleh mendaftar lagi <b>tanpa</b> jalan keluar beralasan.
    /// Seandainya dokumen yang masih <c>Pending</c> menahan penutupan, pemeriksaan
    /// <c>CariEpisodeAktifAsync</c> ini akan tetap menemukan episode lama — dan pasien tertahan
    /// di depan pintu IGD karena tanda tangan yang terlupa di unit lain.
    /// </summary>
    [Fact]
    public async Task SetelahDitutup_PasienYangSamaBolehMendaftarLagi()
    {
        using var context = BuatContext("igd-closure");
        var patientId = Guid.NewGuid();

        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Disposed);
        visit.PatientId = patientId;
        await context.SaveChangesAsync();

        await SimpanKepergianAsync(
            context, visit.Id, EmergencyPhysicalStatus.Arrived, EmergencyHandoverStatus.Pending);

        var service = BuatVisitService(context);
        Assert.NotNull(await service.CariEpisodeAktifAsync(patientId));

        var hasil = await BuatVisitController(context, Pelaku)
            .Complete(visit.Id, new CompleteVisitRequest());
        Assert.Equal(200, KodeStatus(hasil));

        // Episode lama sudah tidak menahan apa pun, meski dokumennya masih Pending.
        Assert.Null(await service.CariEpisodeAktifAsync(patientId));
    }
}
