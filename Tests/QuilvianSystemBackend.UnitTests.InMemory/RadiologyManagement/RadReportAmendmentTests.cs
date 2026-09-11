using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Koreksi hasil bacaan berversi — <c>BE-RAD-10</c>, acceptance criteria AC-18,
/// <c>FR-RAD-020</c> sampai <c>FR-RAD-023</c>, dan <c>RAD-STATE-001</c> bagian 4.
///
/// <para>
/// <b>Satu kalimat menjadi alasan keberadaan seluruh berkas ini: tidak ada satu pun jalur yang
/// boleh menimpa versi yang sudah dirilis.</b>
/// </para>
///
/// <para>
/// Bacaan yang sudah dirilis mungkin sudah dipakai dokter lain untuk memberi obat, menjadwalkan
/// operasi, atau memulangkan pasien. Ketika bacaan itu kemudian dikoreksi, pertanyaan yang harus
/// tetap dapat dijawab bukan hanya "apa yang benar sekarang", melainkan juga <b>"apa yang dibaca
/// dokter itu waktu itu"</b> — dan itulah pertanyaan pertama ketika keputusan klinisnya ditinjau
/// ulang. Versi yang tertimpa menghapus jawaban kedua, dan tidak ada cara mengembalikannya.
/// </para>
/// </summary>
public sealed class RadReportAmendmentTests
{
    private static readonly Guid Radiolog = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid RadiologLain = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Residen = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private const string IsiVersiSatu = "Tidak tampak perdarahan intrakranial.";
    private const string TemuanVersiSatu = "Sulkus dan girus dalam batas normal.";
    private const string SaranVersiSatu = "Tidak diperlukan pemeriksaan lanjutan.";

    private const string IsiVersiDua = "Tampak perdarahan kecil pada lobus temporalis kanan.";
    private const string AlasanKoreksi = "Peninjauan ulang menemukan perdarahan yang terlewat.";

    /* ================================================================== *
     * AC-18 — inti: versi lama tidak berubah satu huruf pun
     * ================================================================== */

    [Fact]
    public async Task AC18_SetelahKoreksiDirilis_IsiVersiSatuTidakBerubahSatuHurufPun()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);
        await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        var versiSatu = await db.RadReportVersions
            .AsNoTracking()
            .SingleAsync(x => x.VersionNumber == 1);

        // Tiga kolom isi, dibandingkan persis. Inilah keseluruhan janji RJ-BIL-GATE-DEC-004.
        Assert.Equal(IsiVersiSatu, versiSatu.Impression);
        Assert.Equal(TemuanVersiSatu, versiSatu.Findings);
        Assert.Equal(SaranVersiSatu, versiSatu.Recommendation);

        // Jejak penulis dan pengesahnya juga tidak boleh bergeser.
        Assert.Equal(Residen, versiSatu.AuthorUserId);
        Assert.Equal(RadReportAuthorRole.Resident, versiSatu.AuthorRoleSnapshot);
        Assert.Equal(Radiolog, versiSatu.ValidatorUserId);
        Assert.NotNull(versiSatu.ReleasedAt);
    }

    [Fact]
    public async Task AC18_SetelahKoreksiDirilis_VersiSatuMenjadiSuperseded()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);
        await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        var versi = await db.RadReportVersions.AsNoTracking()
            .OrderBy(x => x.VersionNumber).ToListAsync();

        Assert.Equal(RadReportVersionStatus.Superseded, versi[0].VersionStatus);
        Assert.Equal(RadReportVersionStatus.Released, versi[1].VersionStatus);
    }

    [Fact]
    public async Task AC18_SetelahKoreksiDirilis_CurrentVersionNumberMenjadiDua()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);
        var hasil = await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(2, hasil.Value!.CurrentVersionNumber);
        Assert.Equal(nameof(RadReportStatus.AmendmentReleased), hasil.Value.ReportStatus);
        Assert.Equal(IsiVersiDua, hasil.Value.CurrentVersion!.Impression);
    }

    [Fact]
    public async Task AC18_VersiDuaMenunjukVersiSatuLewatPreviousVersionId()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());

        var versi = await db.RadReportVersions.AsNoTracking()
            .OrderBy(x => x.VersionNumber).ToListAsync();

        Assert.Null(versi[0].PreviousVersionId);
        Assert.Equal(versi[0].Id, versi[1].PreviousVersionId);
        Assert.True(versi[1].IsAmendment);
        Assert.False(versi[0].IsAmendment);
    }

    [Fact]
    public async Task AC18_RiwayatDapatDitelusuriMundurSampaiBacaanAslinya()
    {
        // Definition of Done BE-RAD-10, dibuktikan dengan benar-benar menelusurinya.
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await RilisKoreksiAsync(db, laporan, "Koreksi pertama.");
        await RilisKoreksiAsync(db, laporan, "Koreksi kedua.");

        var versi = await db.RadReportVersions.AsNoTracking().ToListAsync();
        var berlaku = versi.Single(x => x.VersionNumber == 3);

        var rantai = new List<int>();
        RadReportVersion? jejak = berlaku;

        while (jejak != null)
        {
            rantai.Add(jejak.VersionNumber);
            jejak = jejak.PreviousVersionId == null
                ? null
                : versi.Single(x => x.Id == jejak.PreviousVersionId.Value);
        }

        Assert.Equal(new[] { 3, 2, 1 }, rantai);
    }

    /* ================================================================== *
     * Selama koreksi disusun, versi rilis tetap yang berlaku
     * ================================================================== */

    [Fact]
    public async Task SelamaKoreksiDisusun_VersiRilisTetapYangBerlaku()
    {
        // Jendela waktu paling berbahaya pada seluruh alur ini. Antara "koreksi mulai ditulis"
        // dan "koreksi dirilis", satu-satunya bacaan yang sah tetap versi 1. Dokter jaga yang
        // membuka hasil pada saat itu harus membaca versi 1 — bukan draf yang belum diperiksa
        // siapa pun, dan bukan halaman kosong.
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        var hasil = await Service(db, Radiolog, radiolog: true)
            .CreateAmendmentAsync(laporan, Koreksi());

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);

        var isi = hasil.Value!;
        Assert.Equal(nameof(RadReportStatus.AmendmentDrafted), isi.ReportStatus);

        // Yang berlaku tetap versi 1, dengan isi versi 1.
        Assert.Equal(1, isi.CurrentVersionNumber);
        Assert.Equal(IsiVersiSatu, isi.CurrentVersion!.Impression);
        Assert.Equal(nameof(RadReportVersionStatus.Released), isi.CurrentVersion.VersionStatus);

        // Yang sedang dikerjakan adalah versi 2 — dan perbedaan angka inilah yang memberi tahu
        // layar bahwa ada koreksi yang belum sah.
        Assert.Equal(2, isi.WorkingVersionNumber);
        Assert.Equal(2, isi.Versions.Count);
    }

    [Fact]
    public async Task SelamaKoreksiDisusun_VersiSatuBelumMenjadiSuperseded()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        // Bahkan setelah koreksinya disahkan, sebelum dirilis versi 1 masih yang berlaku.
        var versiSatu = await db.RadReportVersions.AsNoTracking()
            .SingleAsync(x => x.VersionNumber == 1);
        var induk = await db.RadReports.AsNoTracking().SingleAsync();

        Assert.Equal(RadReportVersionStatus.Released, versiSatu.VersionStatus);
        Assert.Equal(1, induk.CurrentVersionNumber);
    }

    [Fact]
    public async Task FirstReleasedAtTidakBergeserOlehKoreksi()
    {
        // "Sejak kapan hasilnya tersedia bagi dokter pengirim" harus punya satu jawaban yang
        // tidak berubah setiap kali bacaan dikoreksi.
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        var sebelum = await db.RadReports.AsNoTracking().SingleAsync();
        var pertamaKali = sebelum.FirstReleasedAt;

        await RilisKoreksiAsync(db, laporan, AlasanKoreksi);

        var sesudah = await db.RadReports.AsNoTracking().SingleAsync();

        Assert.Equal(pertamaKali, sesudah.FirstReleasedAt);
        Assert.NotEqual(sesudah.FirstReleasedAt, sesudah.LastReleasedAt);
    }

    /* ================================================================== *
     * Koreksi atas koreksi
     * ================================================================== */

    [Fact]
    public async Task KoreksiAtasKoreksiMenghasilkanVersiTiga()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await RilisKoreksiAsync(db, laporan, "Koreksi pertama.");
        var hasil = await RilisKoreksiAsync(db, laporan, "Koreksi kedua.");

        Assert.Equal(3, hasil.Value!.CurrentVersionNumber);

        var versi = await db.RadReportVersions.AsNoTracking()
            .OrderBy(x => x.VersionNumber).ToListAsync();

        Assert.Equal(3, versi.Count);
        Assert.Equal(RadReportVersionStatus.Superseded, versi[0].VersionStatus);
        Assert.Equal(RadReportVersionStatus.Superseded, versi[1].VersionStatus);
        Assert.Equal(RadReportVersionStatus.Released, versi[2].VersionStatus);

        // Versi 1 tetap Superseded, tidak dihidupkan kembali oleh koreksi berikutnya.
        Assert.Equal(IsiVersiSatu, versi[0].Impression);
    }

    [Fact]
    public async Task TidakAdaVersiYangPernahHilang()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await RilisKoreksiAsync(db, laporan, "Koreksi pertama.");
        await RilisKoreksiAsync(db, laporan, "Koreksi kedua.");
        await RilisKoreksiAsync(db, laporan, "Koreksi ketiga.");

        var versi = await db.RadReportVersions.AsNoTracking().ToListAsync();

        Assert.Equal(4, versi.Count);
        Assert.DoesNotContain(versi, x => x.IsDelete);
        Assert.Equal(new[] { 1, 2, 3, 4 }, versi.Select(x => x.VersionNumber).Order());
    }

    /* ================================================================== *
     * FR-RAD-021 — alasan koreksi wajib
     * ================================================================== */

    [Fact]
    public async Task FR021_KoreksiTanpaAlasanDitolak()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        var permintaan = Koreksi();
        permintaan.AmendmentReason = "   ";

        var hasil = await Service(db, Radiolog, radiolog: true)
            .CreateAmendmentAsync(laporan, permintaan);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal(RadErrorCodes.AmendmentReasonRequired, hasil.ErrorCode);
        Assert.Equal("Alasan koreksi wajib diisi.", hasil.ErrorMessage);

        // Penolakannya menahan: tidak ada versi kedua yang terlanjur dibuat.
        Assert.Equal(1, await db.RadReportVersions.CountAsync());
    }

    [Fact]
    public async Task AlasanKoreksiYangTerlaluPanjangDitolak()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        var permintaan = Koreksi();
        permintaan.AmendmentReason = new string('a', 1001);

        var hasil = await Service(db, Radiolog, radiolog: true)
            .CreateAmendmentAsync(laporan, permintaan);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Contains("1.000 huruf", hasil.ErrorMessage!);
    }

    [Fact]
    public async Task AlasanKoreksiTersimpanPadaVersinya()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());

        var versiDua = await db.RadReportVersions.AsNoTracking()
            .SingleAsync(x => x.VersionNumber == 2);

        Assert.Equal(AlasanKoreksi, versiDua.AmendmentReason);

        // Versi pertama tidak pernah punya alasan koreksi — ia bukan koreksi.
        var versiSatu = await db.RadReportVersions.AsNoTracking()
            .SingleAsync(x => x.VersionNumber == 1);
        Assert.Null(versiSatu.AmendmentReason);
    }

    [Fact]
    public async Task KoreksiTanpaKesimpulanDitolak()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        var permintaan = Koreksi();
        permintaan.Impression = "  ";

        var hasil = await Service(db, Radiolog, radiolog: true)
            .CreateAmendmentAsync(laporan, permintaan);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal("Kesimpulan bacaan wajib diisi.", hasil.ErrorMessage);
    }

    /* ================================================================== *
     * Koreksi hanya atas bacaan yang sudah dirilis
     * ================================================================== */

    [Fact]
    public async Task KoreksiAtasBacaanYangBelumPernahDirilisDitolak()
    {
        await using var db = Konteks();
        var laporan = await DrafAsync(db);

        var hasil = await Service(db, Radiolog, radiolog: true)
            .CreateAmendmentAsync(laporan, Koreksi());

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReportNeverReleased, hasil.ErrorCode);
        Assert.Equal(
            "Bacaan ini belum pernah dirilis, sehingga belum ada yang perlu dikoreksi.",
            hasil.ErrorMessage);
    }

    [Fact]
    public async Task KoreksiKeduaSaatKoreksiPertamaMasihDisusunDitolak()
    {
        // Dibedakan dari penolakan di atas karena tindakan yang dituntutnya berlawanan: yang
        // ini menuntut koreksi yang sedang berjalan diselesaikan lebih dulu.
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);
        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());

        var hasil = await Service(db, RadiologLain, radiolog: true)
            .CreateAmendmentAsync(laporan, Koreksi());

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.AmendmentAlreadyInProgress, hasil.ErrorCode);
        Assert.Equal(2, await db.RadReportVersions.CountAsync());
    }

    [Fact]
    public async Task KoreksiAtasBacaanYangTidakAdaDitolak()
    {
        await using var db = Konteks();

        var hasil = await Service(db, Radiolog, radiolog: true)
            .CreateAmendmentAsync(Guid.NewGuid(), Koreksi());

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReportNotFound, hasil.ErrorCode);
    }

    /* ================================================================== *
     * FR-RAD-022 — versi rilis tidak dapat diubah lewat jalur mana pun
     * ================================================================== */

    [Fact]
    public async Task FR022_MengubahIsiVersiRilisDitolak()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        var hasil = await Service(db, Residen).UpdateDraftAsync(
            laporan,
            new UpdateRadReportDraftRequest { Impression = "Ditimpa diam-diam." });

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReportVersionFrozen, hasil.ErrorCode);
        Assert.Equal(
            "Bacaan yang sudah dirilis tidak dapat diubah. Buat koreksi bila ada yang perlu " +
            "diperbaiki.",
            hasil.ErrorMessage);

        var versiSatu = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.Equal(IsiVersiSatu, versiSatu.Impression);
    }

    [Fact]
    public async Task FR022_MengubahIsiVersiSupersededDitolak()
    {
        // Setelah koreksi dirilis, jalur ubah draf menunjuk versi tertinggi. Versi lama yang
        // Superseded tidak lagi dapat dicapai jalur mana pun — dan itulah yang dibuktikan:
        // isinya tetap utuh berapa kali pun jalur ubah dicoba.
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);
        await RilisKoreksiAsync(db, laporan, AlasanKoreksi);

        var hasil = await Service(db, Radiolog, radiolog: true).UpdateDraftAsync(
            laporan,
            new UpdateRadReportDraftRequest { Impression = "Ditimpa diam-diam." });

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReportVersionFrozen, hasil.ErrorCode);

        var versi = await db.RadReportVersions.AsNoTracking()
            .OrderBy(x => x.VersionNumber).ToListAsync();
        Assert.Equal(IsiVersiSatu, versi[0].Impression);
        Assert.Equal(IsiVersiDua, versi[1].Impression);
    }

    [Fact]
    public void TidakAdaMethodHapusVersiPadaService()
    {
        // Risiko utama BE-RAD-10 ditulis apa adanya pada roadmap: "Tidak ada endpoint hapus
        // versi". Diuji pada service, karena controller tanpa endpoint hapus tetap tidak aman
        // bila servicenya menyediakan jalurnya.
        var mencurigakan = typeof(RadReportService)
            .GetMethods()
            .Select(x => x.Name)
            .Where(x =>
                x.Contains("Delete", StringComparison.OrdinalIgnoreCase) ||
                x.Contains("Remove", StringComparison.OrdinalIgnoreCase) ||
                x.Contains("Hapus", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(mencurigakan);
    }

    /* ================================================================== *
     * FR-RAD-023 — aturan pengesahan berlaku sama pada koreksi
     * ================================================================== */

    [Fact]
    public async Task FR023_ResidenMengesahkanKoreksinyaSendiriDitolak()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Residen).CreateAmendmentAsync(
            laporan,
            Koreksi(RadReportAuthorRole.Resident));

        var hasil = await Service(db, Residen).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.SelfValidationNotAllowed, hasil.ErrorCode);
        Assert.Equal("Draf yang Anda tulis harus disahkan dokter radiolog.", hasil.ErrorMessage);
    }

    [Fact]
    public async Task FR023_RadiologMengesahkanKoreksinyaSendiriDiterima()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());

        var hasil = await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadReportStatus.AmendmentValidated), hasil.Value!.ReportStatus);
    }

    [Fact]
    public async Task FR023_BukanRadiologMengakuRadiologSaatMengoreksiDitolak()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        var hasil = await Service(db, Residen)
            .CreateAmendmentAsync(laporan, Koreksi(RadReportAuthorRole.Radiologist));

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.AuthorRoleNotPermitted, hasil.ErrorCode);
    }

    [Fact]
    public async Task FR023_PeranPenulisKoreksiDibekukanTerpisahDariVersiSebelumnya()
    {
        // Versi 1 ditulis residen, versi 2 ditulis radiolog. Keduanya menyimpan perannya
        // sendiri, dan peran versi 1 tidak ikut berubah.
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);

        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());

        var versi = await db.RadReportVersions.AsNoTracking()
            .OrderBy(x => x.VersionNumber).ToListAsync();

        Assert.Equal(RadReportAuthorRole.Resident, versi[0].AuthorRoleSnapshot);
        Assert.Equal(RadReportAuthorRole.Radiologist, versi[1].AuthorRoleSnapshot);
    }

    /* ================================================================== *
     * Riwayat versi dapat dibaca
     * ================================================================== */

    [Fact]
    public async Task RiwayatVersiDikembalikanTerbaruLebihDulu()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);
        await RilisKoreksiAsync(db, laporan, "Koreksi pertama.");

        var hasil = await Service(db, Radiolog, radiolog: true).GetVersionsAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);

        var riwayat = hasil.Value!;
        Assert.Equal(new[] { 2, 1 }, riwayat.Select(x => x.VersionNumber));

        // Alasan koreksi ikut terbaca, supaya pembaca riwayat tahu mengapa versi lama diganti.
        Assert.Equal("Koreksi pertama.", riwayat[0].AmendmentReason);
        Assert.Null(riwayat[1].AmendmentReason);
    }

    [Fact]
    public async Task RiwayatVersiBacaanYangTidakAdaDitolak()
    {
        await using var db = Konteks();

        var hasil = await Service(db, Radiolog, radiolog: true).GetVersionsAsync(Guid.NewGuid());

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReportNotFound, hasil.ErrorCode);
    }

    [Fact]
    public async Task DaftarBacaanMenampilkanVersiYangBerlaku_BukanKoreksiYangBelumSah()
    {
        await using var db = Konteks();
        var laporan = await BacaanDirilisAsync(db);
        await Service(db, Radiolog, radiolog: true).CreateAmendmentAsync(laporan, Koreksi());

        var daftar = await Service(db, Radiolog, radiolog: true)
            .GetPagedAsync(new RadReportPagedQuery());

        var baris = Assert.Single(daftar.Items);

        // Nomor versi yang berlaku tetap 1, dan penulis yang tercatat adalah penulis versi 1.
        Assert.Equal(1, baris.CurrentVersionNumber);
        Assert.Equal(Residen, baris.AuthorUserId);
        Assert.Equal(nameof(RadReportStatus.AmendmentDrafted), baris.ReportStatus);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-report-amend-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed class BacaanDenganKewenangan(
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService,
        RadReportNumberService reportNumberService,
        bool radiolog)
        : RadReportService(dbContext, httpContextAccessor, null!, loggerService, reportNumberService)
    {
        protected override Task<bool> HasRadiologistAuthorityAsync(CancellationToken cancellationToken) =>
            Task.FromResult(radiolog);
    }

    private static RadReportService Service(
        ApplicationDbContext db,
        Guid actorUserId,
        bool radiolog = false)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        return new BacaanDenganKewenangan(
            db,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor),
            new RadReportNumberService(),
            radiolog);
    }

    private static async Task<Guid> SeedStudyAsync(ApplicationDbContext db)
    {
        var order = new RadOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = Guid.NewGuid(),
            ModalityId = Guid.NewGuid(),
            OrderStatus = RadOrderStatus.InProgress,
        };

        var study = new RadStudy
        {
            Id = Guid.NewGuid(),
            RadOrderId = order.Id,
            EncounterId = order.EncounterId,
            ProcedureId = order.ProcedureId,
            ModalityId = order.ModalityId,
            StudySequence = 1,
            StudyNumber = $"RAD-{order.Id:N}-001".ToUpperInvariant(),
            StudyStatus = RadStudyStatus.QualityAccepted,
            IsUsable = true,
        };

        db.RadOrders.Add(order);
        db.RadStudies.Add(study);
        await db.SaveChangesAsync();

        return study.Id;
    }

    /// <summary>Bacaan dengan draf versi 1 yang ditulis residen, belum disahkan.</summary>
    private static async Task<Guid> DrafAsync(ApplicationDbContext db)
    {
        var study = await SeedStudyAsync(db);

        var draf = await Service(db, Residen).CreateDraftAsync(
            study,
            new CreateRadReportDraftRequest
            {
                AuthorRole = RadReportAuthorRole.Resident,
                Findings = TemuanVersiSatu,
                Impression = IsiVersiSatu,
                Recommendation = SaranVersiSatu,
            });

        Assert.Equal(RadOperationResultKind.Success, draf.Kind);

        return draf.Value!.Id;
    }

    /// <summary>Bacaan versi 1 yang sudah disahkan dan dirilis dokter radiolog.</summary>
    private static async Task<Guid> BacaanDirilisAsync(ApplicationDbContext db)
    {
        var laporan = await DrafAsync(db);

        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);
        await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        return laporan;
    }

    private static CreateRadReportAmendmentRequest Koreksi(
        RadReportAuthorRole? peran = null,
        string alasan = AlasanKoreksi) => new()
        {
            AmendmentReason = alasan,
            AuthorRole = peran,
            Findings = "Tampak lesi hiperdens kecil pada lobus temporalis kanan.",
            Impression = IsiVersiDua,
            Recommendation = "Disarankan CT ulang dalam 24 jam.",
        };

    /// <summary>Satu siklus koreksi penuh: tulis, sahkan, rilis.</summary>
    private static async Task<RadOperationResult<RadReportDetailResponse>> RilisKoreksiAsync(
        ApplicationDbContext db,
        Guid reportId,
        string alasan)
    {
        var service = Service(db, Radiolog, radiolog: true);

        var draf = await service.CreateAmendmentAsync(reportId, Koreksi(alasan: alasan));
        Assert.Equal(RadOperationResultKind.Success, draf.Kind);

        await service.ValidateAsync(reportId);

        return await service.ReleaseAsync(reportId);
    }
}
