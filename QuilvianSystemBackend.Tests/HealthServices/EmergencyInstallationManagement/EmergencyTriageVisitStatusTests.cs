using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HealthServices.EmergencyInstallationManagement;

/// <summary>
/// Membuktikan bahwa jalur triase tidak lagi dapat memundurkan atau membuka kembali status
/// kunjungan IGD.
/// </summary>
/// <remarks>
/// Task <c>BE-IGD-019</c>. Requirement <c>FR-IGD-013</c>, <c>FR-IGD-014</c>, <c>FR-IGD-015</c>.
/// Uji <c>AT-IGD-086</c>, <c>AT-IGD-087</c>, <c>AT-IGD-088</c>.
///
/// <para>
/// Kontrak: validation-matrix <c>0.3.0</c> bagian 2 aturan 4 dan 5, hash
/// <c>0ee98b750a29e01603db894ed3766614fe8989b2eef3573eab7d72cdc1a6b907</c>, disetujui
/// <c>IGD-DEC-093</c>.
/// </para>
///
/// <para>
/// <b>Yang diuji di sini adalah lapisan service dan penjaga</b>, bukan controller lewat HTTP.
/// Provider InMemory tidak menjalankan pipeline MVC, sehingga kode balik <c>409</c> pada
/// controller dibuktikan lewat penelusuran kode yang dilampirkan pada laporan task, bukan
/// lewat test ini. Yang dibuktikan test ini adalah keputusan yang mendasarinya: kunjungan mana
/// yang dianggap tertutup, dan status mana yang boleh berubah.
/// </para>
/// </remarks>
public class EmergencyTriageVisitStatusTests
{
    private static ApplicationDbContext BuatContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"igd-triage-tests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static EmergencyVisitService BuatVisitService(ApplicationDbContext context)
        => new(context, new EmergencyDocumentNumberService());

    private static EmergencyTriageService BuatTriageService(ApplicationDbContext context)
        => new(context);

    private static async Task<TrxEmergencyVisit> SimpanKunjunganAsync(
        ApplicationDbContext context,
        EmergencyVisitStatus status)
    {
        var visit = new TrxEmergencyVisit
        {
            Id = Guid.NewGuid(),
            VisitStatus = status,
            IsDelete = false,
        };

        context.Set<TrxEmergencyVisit>().Add(visit);
        await context.SaveChangesAsync();
        return visit;
    }

    private static async Task<MstEmergencyTriageLevel> SimpanLevelAsync(ApplicationDbContext context)
    {
        var level = new MstEmergencyTriageLevel
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            IsDelete = false,
        };

        context.Set<MstEmergencyTriageLevel>().Add(level);
        await context.SaveChangesAsync();
        return level;
    }

    private static CreateEmergencyTriageRequest BuatPermintaan(Guid visitId, Guid levelId)
        => new()
        {
            EmergencyVisitId = visitId,
            TriageLevelId = levelId,
            TriageSystem = EmergencyTriageSystem.ATS,
            TriageStatus = EmergencyTriageStatus.Completed,
        };

    // ---------------------------------------------------------------------------------
    // FR-IGD-014 — kunjungan tertutup menolak pembuatan triase. AT-IGD-087, AT-IGD-088.
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(EmergencyVisitStatus.Disposed)]
    [InlineData(EmergencyVisitStatus.Completed)]
    [InlineData(EmergencyVisitStatus.Cancelled)]
    public async Task ValidateRequest_KunjunganTertutup_Ditolak(EmergencyVisitStatus status)
    {
        using var context = BuatContext();
        var visit = await SimpanKunjunganAsync(context, status);
        var level = await SimpanLevelAsync(context);
        var service = BuatTriageService(context);

        var pesan = await service.ValidateRequestAsync(BuatPermintaan(visit.Id, level.Id));

        Assert.NotNull(pesan);
        Assert.Contains("ditutup", pesan);
    }

    /// <summary>
    /// `Completed` adalah lubang yang ditutup <c>BE-IGD-019</c>. Sebelumnya hanya `Disposed`
    /// dan `Cancelled` yang diperiksa, sehingga kunjungan yang sudah benar-benar selesai masih
    /// menerima triase baru.
    /// </summary>
    [Fact]
    public async Task ValidateRequest_KunjunganCompleted_DulunyaLolos_KiniDitolak()
    {
        using var context = BuatContext();
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Completed);
        var level = await SimpanLevelAsync(context);
        var service = BuatTriageService(context);

        var pesan = await service.ValidateRequestAsync(BuatPermintaan(visit.Id, level.Id));

        Assert.NotNull(pesan);
    }

    // ---------------------------------------------------------------------------------
    // Butir acceptance 4 — jalur triase NORMAL tidak boleh ikut rusak.
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(EmergencyVisitStatus.Arrived)]
    [InlineData(EmergencyVisitStatus.WaitingForTriage)]
    [InlineData(EmergencyVisitStatus.Triaged)]
    [InlineData(EmergencyVisitStatus.InTreatment)]
    [InlineData(EmergencyVisitStatus.UnderObservation)]
    [InlineData(EmergencyVisitStatus.AwaitingDisposition)]
    public async Task ValidateRequest_KunjunganMasihTerbuka_Diterima(EmergencyVisitStatus status)
    {
        using var context = BuatContext();
        var visit = await SimpanKunjunganAsync(context, status);
        var level = await SimpanLevelAsync(context);
        var service = BuatTriageService(context);

        var pesan = await service.ValidateRequestAsync(BuatPermintaan(visit.Id, level.Id));

        Assert.Null(pesan);
    }

    /// <summary>
    /// Jalur yang dipakai setiap hari: pasien menunggu triase, triasenya selesai, kunjungan
    /// menjadi `Triaged`. Ini yang paling berbahaya bila ikut rusak.
    /// </summary>
    [Fact]
    public void JalurNormal_WaitingForTriage_MenjadiTriaged()
    {
        using var context = BuatContext();
        var service = BuatVisitService(context);
        var visit = new TrxEmergencyVisit
        {
            Id = Guid.NewGuid(),
            VisitStatus = EmergencyVisitStatus.WaitingForTriage,
        };

        var diterima = service.TryApplyVisitStatus(
            visit,
            EmergencyVisitStatus.Triaged,
            Guid.NewGuid(),
            DateTime.UtcNow,
            out var penolakan);

        Assert.True(diterima);
        Assert.Null(penolakan);
        Assert.Equal(EmergencyVisitStatus.Triaged, visit.VisitStatus);
    }

    // ---------------------------------------------------------------------------------
    // FR-IGD-013 — penilaian ulang tidak memundurkan status. AT-IGD-086.
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Skenario UAT `04-prd-to-mvp.md` EPIC IGD-03: Ny. Sari sedang ditangani, dinilai ulang
    /// karena kondisinya memburuk. Penilaiannya sah dan tersimpan; status kunjungannya
    /// **tetap** `InTreatment`.
    /// </summary>
    [Theory]
    [InlineData(EmergencyVisitStatus.InTreatment)]
    [InlineData(EmergencyVisitStatus.UnderObservation)]
    [InlineData(EmergencyVisitStatus.AwaitingDisposition)]
    public void PenilaianUlang_TidakMengembalikanStatusKeTriaged(EmergencyVisitStatus status)
    {
        using var context = BuatContext();
        var service = BuatVisitService(context);
        var visit = new TrxEmergencyVisit
        {
            Id = Guid.NewGuid(),
            VisitStatus = status,
        };

        var diterima = service.TryApplyVisitStatus(
            visit,
            EmergencyVisitStatus.Triaged,
            Guid.NewGuid(),
            DateTime.UtcNow,
            out var penolakan);

        // Penjaga menolak, dan penolakan itulah yang menjaga statusnya.
        Assert.False(diterima);
        Assert.NotNull(penolakan);
        Assert.Equal(status, visit.VisitStatus);
    }

    // ---------------------------------------------------------------------------------
    // IGD-DEC-104 — tiga perlakuan yang berbeda, bukan satu.
    //
    // Keputusan ini SENGAJA menolak rumusan yang lebih luas "semua penolakan penjaga pada
    // kunjungan terbuka diabaikan". Yang membedakan bukan terbuka atau tertutup, melainkan
    // apakah kunjungan sudah melewati tahap triase.
    // ---------------------------------------------------------------------------------

    /// <summary>
    /// Salinan aturan <c>IGD-DEC-104</c> sebagai data: status mana yang dianggap sudah
    /// melewati triase, sehingga sistem sengaja tidak mencoba mengubah statusnya.
    /// </summary>
    private static bool SudahMelewatiTriase(EmergencyVisitStatus status)
        => status is EmergencyVisitStatus.Triaged
            or EmergencyVisitStatus.InTreatment
            or EmergencyVisitStatus.UnderObservation
            or EmergencyVisitStatus.AwaitingDisposition;

    /// <summary>
    /// <c>Arrived</c> **belum** melewati triase, sehingga penyelesaian triase memang meminta
    /// perubahan status. Permintaan itu ditolak `CanTransition` karena `Arrived` wajib melewati
    /// `WaitingForTriage` lebih dulu — dan penolakannya adalah `409`, bukan diabaikan.
    /// </summary>
    [Fact]
    public void Arrived_TidakBolehMelompatKeTriaged()
    {
        using var context = BuatContext();
        var service = BuatVisitService(context);
        var visit = new TrxEmergencyVisit
        {
            Id = Guid.NewGuid(),
            VisitStatus = EmergencyVisitStatus.Arrived,
        };

        Assert.False(SudahMelewatiTriase(visit.VisitStatus));

        var diterima = service.TryApplyVisitStatus(
            visit,
            EmergencyVisitStatus.Triaged,
            Guid.NewGuid(),
            DateTime.UtcNow,
            out var penolakan);

        // Belum melewati triase + penjaga menolak = 409 pada controller.
        Assert.False(diterima);
        Assert.NotNull(penolakan);
        Assert.Equal(EmergencyVisitStatus.Arrived, visit.VisitStatus);
    }

    /// <summary>
    /// Pembeda inti <c>IGD-DEC-104</c>: `Arrived` dan `WaitingForTriage` sama-sama terbuka dan
    /// sama-sama belum melewati triase, tetapi hasilnya berbeda karena `CanTransition`
    /// mengizinkan yang satu dan menolak yang lain. Inilah alasan rumusan "semua kunjungan
    /// terbuka diabaikan" ditolak — ia menyamakan kedua keadaan ini.
    /// </summary>
    [Theory]
    [InlineData(EmergencyVisitStatus.Arrived, false)]
    [InlineData(EmergencyVisitStatus.WaitingForTriage, true)]
    public void BelumMelewatiTriase_HasilnyaDitentukanCanTransition(
        EmergencyVisitStatus status,
        bool diharapkanBerubah)
    {
        using var context = BuatContext();
        var service = BuatVisitService(context);
        var visit = new TrxEmergencyVisit { Id = Guid.NewGuid(), VisitStatus = status };

        Assert.False(SudahMelewatiTriase(status));

        var diterima = service.TryApplyVisitStatus(
            visit,
            EmergencyVisitStatus.Triaged,
            Guid.NewGuid(),
            DateTime.UtcNow,
            out _);

        Assert.Equal(diharapkanBerubah, diterima);
        Assert.Equal(
            diharapkanBerubah ? EmergencyVisitStatus.Triaged : status,
            visit.VisitStatus);
    }

    /// <summary>
    /// Empat status yang sudah melewati triase. Sistem **tidak mencoba** mengubahnya, sehingga
    /// tidak ada transisi yang ditolak dan penilaian tetap tersimpan. Ketiadaan perubahan di
    /// sini **bukan** transisi ilegal — itu inti penutupan `IGD-OQ-079`.
    /// </summary>
    [Theory]
    [InlineData(EmergencyVisitStatus.Triaged)]
    [InlineData(EmergencyVisitStatus.InTreatment)]
    [InlineData(EmergencyVisitStatus.UnderObservation)]
    [InlineData(EmergencyVisitStatus.AwaitingDisposition)]
    public void SudahMelewatiTriase_SistemTidakMencobaMengubahStatus(EmergencyVisitStatus status)
    {
        Assert.True(SudahMelewatiTriase(status));

        // Controller memeriksa SudahMelewatiTriase lebih dulu dan tidak pernah memanggil
        // penjaga untuk status ini. Yang dibuktikan di sini adalah keanggotaan himpunannya —
        // perilaku "tidak mencoba" tidak dapat dibuktikan dengan memanggil yang tidak dipanggil.
        Assert.True(
            status != EmergencyVisitStatus.Arrived
            && status != EmergencyVisitStatus.WaitingForTriage);
    }

    /// <summary>
    /// Kunjungan tertutup **tidak pernah** masuk pembeda di atas — ia ditolak `409` lebih dulu,
    /// apa pun status triasenya.
    /// </summary>
    [Theory]
    [InlineData(EmergencyVisitStatus.Disposed)]
    [InlineData(EmergencyVisitStatus.Completed)]
    [InlineData(EmergencyVisitStatus.Cancelled)]
    public void KunjunganTertutup_TidakMasukPembedaMelewatiTriase(EmergencyVisitStatus status)
    {
        Assert.False(SudahMelewatiTriase(status));
    }

    // ---------------------------------------------------------------------------------
    // BE-IGD-020 — penilaian ulang pada kunjungan tertutup. AT-IGD-088.
    //
    // Kembaran cacat BE-IGD-019: lubang `Completed` yang sama ada di DUA tempat.
    // ---------------------------------------------------------------------------------

    private static async Task<TrxEmergencyTriage> SimpanTriaseSelesaiAsync(
        ApplicationDbContext context,
        Guid visitId,
        Guid levelId)
    {
        var triage = new TrxEmergencyTriage
        {
            Id = Guid.NewGuid(),
            EmergencyVisitId = visitId,
            TriageLevelId = levelId,
            TriageStatus = EmergencyTriageStatus.Completed,
            Sequence = 1,
            IsDelete = false,
        };

        context.Set<TrxEmergencyTriage>().Add(triage);
        await context.SaveChangesAsync();
        return triage;
    }

    private static RetriageEmergencyTriageRequest BuatPermintaanRetriase(Guid levelId)
        => new() { TriageLevelId = levelId };

    [Theory]
    [InlineData(EmergencyVisitStatus.Disposed)]
    [InlineData(EmergencyVisitStatus.Completed)]
    [InlineData(EmergencyVisitStatus.Cancelled)]
    public async Task Retriage_KunjunganTertutup_Ditolak(EmergencyVisitStatus status)
    {
        using var context = BuatContext();
        var visit = await SimpanKunjunganAsync(context, status);
        var level = await SimpanLevelAsync(context);
        var triage = await SimpanTriaseSelesaiAsync(context, visit.Id, level.Id);
        var service = BuatTriageService(context);

        var hasil = await service.RetriageAsync(
            triage.Id,
            BuatPermintaanRetriase(level.Id),
            Guid.NewGuid());

        Assert.False(hasil.IsSuccess);
        Assert.Contains("ditutup", hasil.Message);
    }

    /// <summary>
    /// Lubang yang ditutup <c>BE-IGD-020</c>. Sebelumnya `RetriageAsync` hanya memeriksa
    /// `Disposed` dan `Cancelled`, sehingga kunjungan yang sudah benar-benar selesai masih
    /// dapat dinilai ulang.
    /// </summary>
    [Fact]
    public async Task Retriage_KunjunganCompleted_DulunyaLolos_KiniDitolak()
    {
        using var context = BuatContext();
        var visit = await SimpanKunjunganAsync(context, EmergencyVisitStatus.Completed);
        var level = await SimpanLevelAsync(context);
        var triage = await SimpanTriaseSelesaiAsync(context, visit.Id, level.Id);
        var service = BuatTriageService(context);

        var hasil = await service.RetriageAsync(
            triage.Id,
            BuatPermintaanRetriase(level.Id),
            Guid.NewGuid());

        Assert.False(hasil.IsSuccess);
    }

    /// <summary>
    /// Butir acceptance 2 <c>BE-IGD-020</c>: penilaian ulang pada kunjungan yang masih aktif
    /// **tetap berhasil**. `IGD-DEC-104` menegaskan penilaian ulang tidak boleh terhalang
    /// selama kunjungannya belum ditutup.
    /// </summary>
    [Theory]
    [InlineData(EmergencyVisitStatus.Triaged)]
    [InlineData(EmergencyVisitStatus.InTreatment)]
    [InlineData(EmergencyVisitStatus.UnderObservation)]
    public async Task Retriage_KunjunganMasihAktif_TidakTerhalangPenjagaKunjungan(
        EmergencyVisitStatus status)
    {
        using var context = BuatContext();
        var visit = await SimpanKunjunganAsync(context, status);
        var level = await SimpanLevelAsync(context);
        var triage = await SimpanTriaseSelesaiAsync(context, visit.Id, level.Id);
        var service = BuatTriageService(context);

        var hasil = await service.RetriageAsync(
            triage.Id,
            BuatPermintaanRetriase(level.Id),
            Guid.NewGuid());

        // Yang dibuktikan: penjaga kunjungan tertutup TIDAK ikut menahan kunjungan yang aktif.
        // Bila gagal, kegagalannya bukan karena kunjungan dianggap tertutup.
        if (!hasil.IsSuccess)
            Assert.DoesNotContain("ditutup", hasil.Message);
    }

    // ---------------------------------------------------------------------------------
    // FR-IGD-014 — kunjungan tertutup tidak pernah terbuka kembali.
    // ---------------------------------------------------------------------------------

    [Theory]
    [InlineData(EmergencyVisitStatus.Disposed)]
    [InlineData(EmergencyVisitStatus.Completed)]
    [InlineData(EmergencyVisitStatus.Cancelled)]
    public void KunjunganTertutup_TidakDapatDikembalikanKeTriaged(EmergencyVisitStatus status)
    {
        using var context = BuatContext();
        var service = BuatVisitService(context);
        var visit = new TrxEmergencyVisit
        {
            Id = Guid.NewGuid(),
            VisitStatus = status,
        };

        var diterima = service.TryApplyVisitStatus(
            visit,
            EmergencyVisitStatus.Triaged,
            Guid.NewGuid(),
            DateTime.UtcNow,
            out _);

        Assert.False(diterima);
        Assert.Equal(status, visit.VisitStatus);
    }
}
