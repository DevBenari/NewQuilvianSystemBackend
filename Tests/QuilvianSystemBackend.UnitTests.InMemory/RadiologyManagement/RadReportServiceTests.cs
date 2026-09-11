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
using System.Text.Json;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Siklus hidup hasil bacaan radiologi — <c>BE-RAD-08</c>, acceptance criteria AC-1 sampai AC-4,
/// seluruh transisi sah dan tidak sah pada <c>RAD-STATE-001</c> bagian 3 dan 4.
///
/// <para>
/// <b>Yang diuji di sini bukan kelengkapan formulir.</b> Yang diuji adalah satu hal: sebuah
/// bacaan tidak pernah menjadi sah dipakai dokter lain tanpa seorang dokter radiolog menyatakan
/// demikian, dan pernyataan itu tidak dapat diberikan sendiri oleh penulis yang bukan radiolog —
/// termasuk ketika ia menjadi radiolog di kemudian hari.
/// </para>
/// </summary>
public sealed class RadReportServiceTests
{
    private static readonly Guid Radiolog = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid RadiologLain = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Residen = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Radiografer = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid BantuanAi = Guid.Parse("55555555-5555-5555-5555-555555555555");

    /* ================================================================== *
     * Kelahiran bacaan — RAD-STATE-001 bagian 3, baris pertama
     * ================================================================== */

    [Fact]
    public async Task StudyLayakMelahirkanBacaanBerstatusPending()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Service(db, Radiolog).EnsurePendingReportAsync(study);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadReportStatus.Pending), hasil.Value!.ReportStatus);

        // Nol berarti belum ada yang menuliskannya — bacaan sudah ditunggu, isinya belum ada.
        Assert.Equal(0, hasil.Value.CurrentVersionNumber);
        Assert.Null(hasil.Value.CurrentVersion);
        Assert.NotEmpty(hasil.Value.ReportNumber);
    }

    [Fact]
    public async Task KelahiranBacaanDipanggilDuaKaliTidakMelahirkanBacaanKedua()
    {
        // Kejadian "mutu citra diterima" dapat terkirim ulang. Kiriman kedua tidak boleh
        // melahirkan bacaan kedua atas study yang sama.
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);
        var service = Service(db, Radiolog);

        var pertama = await service.EnsurePendingReportAsync(study);
        var kedua = await service.EnsurePendingReportAsync(study);

        Assert.Equal(RadOperationResultKind.Success, kedua.Kind);
        Assert.Equal(pertama.Value!.Id, kedua.Value!.Id);
        Assert.Equal(1, await db.RadReports.CountAsync());
    }

    [Fact]
    public async Task StudyYangMutunyaBelumDinilaiTidakMelahirkanBacaan()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db, isUsable: null);

        var hasil = await Service(db, Radiolog).EnsurePendingReportAsync(study);

        Assert.Equal(RadOperationResultKind.BusinessRule, hasil.Kind);
        Assert.Equal(RadErrorCodes.StudyQualityNotDecided, hasil.ErrorCode);
        Assert.Empty(await db.RadReports.ToListAsync());
    }

    [Fact]
    public async Task StudyYangDinyatakanTidakLayakTidakMelahirkanBacaan()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db, isUsable: false);

        var hasil = await Service(db, Radiolog).EnsurePendingReportAsync(study);

        Assert.Equal(RadOperationResultKind.BusinessRule, hasil.Kind);
        Assert.Equal(RadErrorCodes.StudyNotUsable, hasil.ErrorCode);
        Assert.Empty(await db.RadReports.ToListAsync());
    }

    /* ================================================================== *
     * Menulis draf — Pending menjadi Drafted
     * ================================================================== */

    [Fact]
    public async Task RadiologMenulisDrafPeranDibekukanSebagaiRadiolog()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Service(db, Radiolog, radiolog: true).CreateDraftAsync(study, Draf());

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadReportStatus.Drafted), hasil.Value!.ReportStatus);
        Assert.Equal(1, hasil.Value.CurrentVersionNumber);

        var versi = hasil.Value.CurrentVersion!;
        Assert.Equal(1, versi.VersionNumber);
        Assert.Null(versi.PreviousVersionId);
        Assert.False(versi.IsAmendment);
        Assert.Equal(nameof(RadReportVersionStatus.Drafted), versi.VersionStatus);
        Assert.Equal(nameof(RadReportAuthorRole.Radiologist), versi.AuthorRoleSnapshot);
        Assert.Equal(Radiolog, versi.AuthorUserId);
        Assert.Null(versi.ValidatorUserId);
    }

    [Fact]
    public async Task ResidenMenulisDrafPeranDibekukanSebagaiResiden()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Service(db, Residen)
            .CreateDraftAsync(study, Draf(RadReportAuthorRole.Resident));

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(
            nameof(RadReportAuthorRole.Resident),
            hasil.Value!.CurrentVersion!.AuthorRoleSnapshot);
    }

    [Fact]
    public async Task DrafTanpaKesimpulanDitolak()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var permintaan = Draf(RadReportAuthorRole.Resident);
        permintaan.Impression = "   ";

        var hasil = await Service(db, Residen).CreateDraftAsync(study, permintaan);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal("Kesimpulan bacaan wajib diisi.", hasil.ErrorMessage);
        Assert.Empty(await db.RadReportVersions.ToListAsync());
    }

    [Fact]
    public async Task KesimpulanYangTerlaluPanjangDitolak()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var permintaan = Draf(RadReportAuthorRole.Resident);
        permintaan.Impression = new string('a', 4001);

        var hasil = await Service(db, Residen).CreateDraftAsync(study, permintaan);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Contains("4.000 huruf", hasil.ErrorMessage!);
    }

    [Fact]
    public async Task UraianTemuanYangTerlaluPanjangDitolak()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var permintaan = Draf(RadReportAuthorRole.Resident);
        permintaan.Findings = new string('a', 8001);

        var hasil = await Service(db, Residen).CreateDraftAsync(study, permintaan);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Contains("8.000 huruf", hasil.ErrorMessage!);
    }

    [Fact]
    public async Task SaranTindakLanjutYangTerlaluPanjangDitolak()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var permintaan = Draf(RadReportAuthorRole.Resident);
        permintaan.Recommendation = new string('a', 2001);

        var hasil = await Service(db, Residen).CreateDraftAsync(study, permintaan);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Contains("2.000 huruf", hasil.ErrorMessage!);
    }

    [Fact]
    public async Task BukanRadiologYangMengakuRadiologDitolak()
    {
        // Ditolak, bukan diturunkan diam-diam menjadi residen. Penurunan diam-diam membuat
        // penulisnya mengira drafnya dapat ia sahkan sendiri.
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Service(db, Residen)
            .CreateDraftAsync(study, Draf(RadReportAuthorRole.Radiologist));

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.AuthorRoleNotPermitted, hasil.ErrorCode);
        Assert.Empty(await db.RadReportVersions.ToListAsync());
    }

    [Fact]
    public async Task PeranPenulisWajibDisebutBilaBukanRadiolog()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Service(db, Residen).CreateDraftAsync(study, Draf());

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal(RadErrorCodes.AuthorRoleRequired, hasil.ErrorCode);
    }

    [Fact]
    public async Task RadiologBolehMenandaiDrafnyaSebagaiHasilBantuanAi()
    {
        // Pernyataan yang lebih rendah dari kewenangan diterima apa adanya: ia hanya membuat
        // aturan pengesahan makin ketat, dan asal-usul draf lebih berharga daripada kerapian.
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);

        var hasil = await Service(db, Radiolog, radiolog: true)
            .CreateDraftAsync(study, Draf(RadReportAuthorRole.AiAssisted));

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(
            nameof(RadReportAuthorRole.AiAssisted),
            hasil.Value!.CurrentVersion!.AuthorRoleSnapshot);
    }

    [Fact]
    public async Task BacaanKeduaAtasStudyYangSamaDitolak()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);
        await Service(db, Radiolog, radiolog: true).CreateDraftAsync(study, Draf());

        var hasil = await Service(db, RadiologLain, radiolog: true).CreateDraftAsync(study, Draf());

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReportAlreadyExists, hasil.ErrorCode);
        Assert.Contains("koreksi", hasil.ErrorMessage!);
        Assert.Equal(1, await db.RadReportVersions.CountAsync());
    }

    [Fact]
    public async Task DrafAtasStudyYangDinyatakanTidakLayakDitolak()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db, isUsable: false);

        var hasil = await Service(db, Radiolog, radiolog: true).CreateDraftAsync(study, Draf());

        Assert.Equal(RadOperationResultKind.BusinessRule, hasil.Kind);
        Assert.Equal(RadErrorCodes.StudyNotUsable, hasil.ErrorCode);
    }

    [Fact]
    public async Task DrafAtasStudyYangMutunyaBelumDinilaiDitolak()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db, isUsable: null);

        var hasil = await Service(db, Radiolog, radiolog: true).CreateDraftAsync(study, Draf());

        Assert.Equal(RadOperationResultKind.BusinessRule, hasil.Kind);
        Assert.Equal(RadErrorCodes.StudyQualityNotDecided, hasil.ErrorCode);
    }

    /* ================================================================== *
     * Mengubah draf
     * ================================================================== */

    [Fact]
    public async Task PenulisMengubahDrafnyaSendiriDiterima()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Service(db, Residen).UpdateDraftAsync(
            laporan,
            new UpdateRadReportDraftRequest { Impression = "Kesimpulan yang sudah diperbaiki." });

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal("Kesimpulan yang sudah diperbaiki.", hasil.Value!.CurrentVersion!.Impression);

        // Koreksi draf tidak melahirkan versi baru; versi baru adalah urusan amandemen.
        Assert.Equal(1, await db.RadReportVersions.CountAsync());
    }

    [Fact]
    public async Task OrangLainMengubahDrafYangBukanMiliknyaDitolak()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Service(db, Radiolog, radiolog: true).UpdateDraftAsync(
            laporan,
            new UpdateRadReportDraftRequest { Impression = "Diubah orang lain." });

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.NotDraftAuthor, hasil.ErrorCode);

        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.NotEqual("Diubah orang lain.", tersimpan.Impression);
    }

    [Fact]
    public async Task DrafYangSudahDisahkanTidakDapatDiubahLagi()
    {
        // Pengesahan adalah pernyataan atas isi yang tertentu. Kalau isinya masih dapat berubah
        // sesudahnya, pernyataan itu tidak mengikat apa pun.
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Radiolog, RadReportAuthorRole.Radiologist, radiolog: true);
        await Service(db, RadiologLain, radiolog: true).ValidateAsync(laporan);

        var hasil = await Service(db, Radiolog, radiolog: true).UpdateDraftAsync(
            laporan,
            new UpdateRadReportDraftRequest { Impression = "Disunting setelah disahkan." });

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.InvalidTransition, hasil.ErrorCode);
    }

    [Fact]
    public async Task IsiVersiYangSudahDirilisTidakDapatDiubah()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Radiolog, RadReportAuthorRole.Radiologist, radiolog: true);
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);
        await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        var hasil = await Service(db, Radiolog, radiolog: true).UpdateDraftAsync(
            laporan,
            new UpdateRadReportDraftRequest { Impression = "Ditimpa setelah dirilis." });

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReportVersionFrozen, hasil.ErrorCode);

        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.Equal(IsiKesimpulan, tersimpan.Impression);
    }

    /* ================================================================== *
     * AC-1 sampai AC-4 — tujuh baris contoh RAD-STATE-001 bagian 3
     * ================================================================== */

    [Fact]
    public async Task AC2_RadiologMengesahkanDrafnyaSendiriDiterima()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Radiolog, RadReportAuthorRole.Radiologist, radiolog: true);

        var hasil = await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadReportStatus.Validated), hasil.Value!.ReportStatus);
        Assert.Equal(Radiolog, hasil.Value.CurrentVersion!.ValidatorUserId);
    }

    [Fact]
    public async Task AC2_RadiologLainMengesahkanDrafRadiologDiterima()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Radiolog, RadReportAuthorRole.Radiologist, radiolog: true);

        var hasil = await Service(db, RadiologLain, radiolog: true).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(RadiologLain, hasil.Value!.CurrentVersion!.ValidatorUserId);
    }

    [Fact]
    public async Task AC1_ResidenMengesahkanDrafnyaSendiriDitolak()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Service(db, Residen).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.SelfValidationNotAllowed, hasil.ErrorCode);

        // Pesannya ditulis apa adanya pada RAD-VAL-001 supaya petugas tahu apa yang harus
        // dilakukan, bukan sekadar tahu bahwa ia ditolak.
        Assert.Equal("Draf yang Anda tulis harus disahkan dokter radiolog.", hasil.ErrorMessage);

        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.Equal(RadReportVersionStatus.Drafted, tersimpan.VersionStatus);
        Assert.Null(tersimpan.ValidatorUserId);
    }

    [Fact]
    public async Task AC1_DrafResidenDisahkanRadiologDiterima()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(Radiolog, hasil.Value!.CurrentVersion!.ValidatorUserId);
    }

    [Fact]
    public async Task AC2_RadiograferMengesahkanDrafnyaSendiriDitolak()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Radiografer, RadReportAuthorRole.Radiographer);

        var hasil = await Service(db, Radiografer).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.SelfValidationNotAllowed, hasil.ErrorCode);
    }

    [Fact]
    public async Task AC2_DrafBantuanAiDisahkanRadiologDiterima()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, BantuanAi, RadReportAuthorRole.AiAssisted);

        var hasil = await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
    }

    [Fact]
    public async Task AC2_DrafBantuanAiTidakDapatDisahkanDirinyaSendiri()
    {
        // Pengesah wajib manusia. Bantuan AI yang mengesahkan hasilnya sendiri berarti tidak ada
        // seorang pun yang pernah menyatakan bacaan itu sah dipakai.
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, BantuanAi, RadReportAuthorRole.AiAssisted);

        var hasil = await Service(db, BantuanAi, radiolog: true).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.SelfValidationNotAllowed, hasil.ErrorCode);
    }

    [Fact]
    public async Task AC3_ResidenYangKemudianMenjadiRadiologTetapDitolakAtasDrafLamanya()
    {
        // Inti RAD-DEC-003, dan satu-satunya uji yang membuktikan pembekuan peran benar-benar
        // berarti: dr. Rian menulis draf pada Januari sebagai residen, lalu pada Juli lulus
        // menjadi Sp.Rad dan memegang RadReport : ActAsRadiologist. Draf Januari tetap wajib
        // disahkan orang lain.
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Service(db, Residen, radiolog: true).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.SelfValidationNotAllowed, hasil.ErrorCode);

        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.Equal(RadReportAuthorRole.Resident, tersimpan.AuthorRoleSnapshot);
        Assert.Equal(RadReportVersionStatus.Drafted, tersimpan.VersionStatus);
    }

    [Fact]
    public async Task AC4_PenulisDanPengesahTerekamSebagaiDuaJejakTerpisah()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Radiolog, RadReportAuthorRole.Radiologist, radiolog: true);
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();

        // Orangnya sama, tetapi jejaknya tetap dua: siapa yang menulis, dan siapa yang
        // menyatakan sah. Meleburnya menjadi satu kolom menghapus pertanyaan kedua.
        Assert.Equal(Radiolog, tersimpan.AuthorUserId);
        Assert.Equal(Radiolog, tersimpan.ValidatorUserId);
        Assert.NotEqual(default(DateTime), tersimpan.DraftedAt);
        Assert.NotNull(tersimpan.ValidatedAt);
    }

    [Fact]
    public async Task PengesahYangBukanDokterRadiologDitolak()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Radiografer, RadReportAuthorRole.Radiographer);

        var hasil = await Service(db, Residen).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.ValidatorNotRadiologist, hasil.ErrorCode);
        Assert.Equal(
            "Hanya dokter radiolog yang boleh mengesahkan hasil bacaan.",
            hasil.ErrorMessage);
    }

    [Fact]
    public async Task PengesahanKeduaAtasBacaanYangSamaDitolak()
    {
        // Dua radiolog yang menekan Sahkan atas bacaan yang sama: satu berhasil, satu ditolak,
        // dan hanya satu baris pengesahan yang tersimpan.
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var pertama = await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);
        var kedua = await Service(db, RadiologLain, radiolog: true).ValidateAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, pertama.Kind);
        Assert.Equal(RadOperationResultKind.Conflict, kedua.Kind);
        Assert.Equal(RadErrorCodes.InvalidTransition, kedua.ErrorCode);

        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.Equal(Radiolog, tersimpan.ValidatorUserId);
    }

    /* ================================================================== *
     * Rilis
     * ================================================================== */

    [Fact]
    public async Task RilisSetelahDisahkanBerhasil()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        var hasil = await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadReportStatus.Released), hasil.Value!.ReportStatus);
        Assert.Equal(
            nameof(RadReportVersionStatus.Released),
            hasil.Value.CurrentVersion!.VersionStatus);
        Assert.NotNull(hasil.Value.CurrentVersion.ReleasedAt);

        // Pertanyaan "sejak kapan hasilnya tersedia" punya satu jawaban yang tidak bergeser.
        Assert.NotNull(hasil.Value.FirstReleasedAt);
        Assert.Equal(hasil.Value.FirstReleasedAt, hasil.Value.LastReleasedAt);
    }

    [Fact]
    public async Task MerilisBacaanYangBelumDisahkanDitolak()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);

        var hasil = await Service(db, Radiolog, radiolog: true).ReleaseAsync(laporan);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal("Bacaan harus disahkan lebih dulu sebelum dirilis.", hasil.ErrorMessage);

        var tersimpan = await db.RadReportVersions.AsNoTracking().SingleAsync();
        Assert.Equal(RadReportVersionStatus.Drafted, tersimpan.VersionStatus);
    }

    [Fact]
    public async Task RilisOlehBukanDokterRadiologDitolak()
    {
        await using var db = Konteks();
        var (_, laporan) = await DrafAsync(db, Residen, RadReportAuthorRole.Resident);
        await Service(db, Radiolog, radiolog: true).ValidateAsync(laporan);

        var hasil = await Service(db, Radiografer).ReleaseAsync(laporan);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.ValidatorNotRadiologist, hasil.ErrorCode);
    }

    [Fact]
    public async Task BacaanYangTidakAdaDitolakSebagaiTidakDitemukan()
    {
        await using var db = Konteks();

        var hasil = await Service(db, Radiolog, radiolog: true).ValidateAsync(Guid.NewGuid());

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReportNotFound, hasil.ErrorCode);
    }

    [Fact]
    public async Task BacaanYangBelumPunyaDrafTidakDapatDisahkan()
    {
        await using var db = Konteks();
        var study = await SeedStudyAsync(db);
        var lahir = await Service(db, Radiolog).EnsurePendingReportAsync(study);

        var hasil = await Service(db, Radiolog, radiolog: true).ValidateAsync(lahir.Value!.Id);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Contains("belum memiliki draf", hasil.ErrorMessage!);
    }

    /* ================================================================== *
     * RAD-PERM-001 bagian 7 — kolom sensitif tidak boleh masuk log
     * ================================================================== */

    [Fact]
    public void MuatanLogTidakMemilikiKolomUntukIsiBacaan()
    {
        var nama = typeof(RadReportLogPayload)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain(nameof(RadReportVersion.Findings), nama);
        Assert.DoesNotContain(nameof(RadReportVersion.Impression), nama);
        Assert.DoesNotContain(nameof(RadReportVersion.Recommendation), nama);
        Assert.DoesNotContain(nameof(RadReportVersion.AmendmentReason), nama);
    }

    [Fact]
    public void MuatanLogTidakMembawaKesimpulanKlinisPasien()
    {
        var report = new RadReport
        {
            RadStudyId = Guid.NewGuid(),
            RadOrderId = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ReportNumber = "RAD-RPT-260911074012-A1B2C3",
            ReportStatus = RadReportStatus.Released,
            CurrentVersionNumber = 1,
        };

        var version = new RadReportVersion
        {
            RadReportId = report.Id,
            VersionNumber = 1,
            VersionStatus = RadReportVersionStatus.Released,
            Findings = "PERDARAHAN-INTRAKRANIAL-KECIL",
            Impression = "KESIMPULAN-KLINIS-RAHASIA",
            Recommendation = "SARAN-TINDAK-LANJUT-RAHASIA",
            AmendmentReason = "ALASAN-KOREKSI-RAHASIA",
            AuthorUserId = Radiolog,
            AuthorRoleSnapshot = RadReportAuthorRole.Radiologist,
            DraftedAt = DateTime.UtcNow,
        };

        var muatan = JsonSerializer.Serialize(
            RadReportLogPayload.For(report, version, Radiolog));

        Assert.DoesNotContain("PERDARAHAN-INTRAKRANIAL-KECIL", muatan);
        Assert.DoesNotContain("KESIMPULAN-KLINIS-RAHASIA", muatan);
        Assert.DoesNotContain("SARAN-TINDAK-LANJUT-RAHASIA", muatan);
        Assert.DoesNotContain("ALASAN-KOREKSI-RAHASIA", muatan);

        // Yang justru wajib terekam audit: peran penulis yang dibekukan dan siapa pelakunya.
        Assert.Contains(nameof(RadReportAuthorRole.Radiologist), muatan);
        Assert.Contains(Radiolog.ToString(), muatan);
    }

    /* ================================================================== *
     * Penomoran bacaan — QBE-CODE-003 dan QBE-CODE-004
     * ================================================================== */

    [Fact]
    public void NomorBacaanTidakPernahKembarWalauDibentukPadaDetikYangSama()
    {
        // Inilah yang membedakan alokator ini dari Count+1: seluruh nomor di bawah dibentuk
        // dari waktu yang sama persis, dan tetap tidak ada yang kembar.
        var service = new RadReportNumberService();
        var waktu = new DateTime(2026, 9, 11, 7, 40, 12, DateTimeKind.Utc);

        var nomor = Enumerable.Range(0, 1000)
            .Select(_ => service.Generate(RadReportNumberService.DefaultPrefix, waktu))
            .ToList();

        Assert.Equal(1000, nomor.Distinct().Count());
    }

    [Fact]
    public void NomorBacaanMuatPadaKolomnya()
    {
        var nomor = new RadReportNumberService().Generate();

        Assert.StartsWith("RAD-RPT-", nomor);
        Assert.True(nomor.Length <= 64, $"Panjang nomor {nomor.Length} melebihi kolom varchar(64).");
    }

    [Fact]
    public async Task DuaBacaanBerbedaMemakaiNomorYangBerbeda()
    {
        await using var db = Konteks();
        var studyPertama = await SeedStudyAsync(db);
        var studyKedua = await SeedStudyAsync(db);
        var service = Service(db, Radiolog, radiolog: true);

        var pertama = await service.CreateDraftAsync(studyPertama, Draf());
        var kedua = await service.CreateDraftAsync(studyKedua, Draf());

        Assert.NotEqual(pertama.Value!.ReportNumber, kedua.Value!.ReportNumber);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private const string IsiKesimpulan = "Tidak tampak perdarahan intrakranial.";

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-report-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Bacaan sungguhan, dengan satu-satunya bagian yang digantikan adalah pemeriksaan penanda
    /// <c>RadReport : ActAsRadiologist</c> — supaya uji tidak perlu menyusun seluruh struktur
    /// RBAC untuk membuktikan aturan pengesahan.
    /// </summary>
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

    /// <summary>Satu study yang citranya sudah dinilai layak dibaca.</summary>
    private static async Task<Guid> SeedStudyAsync(
        ApplicationDbContext db,
        bool? isUsable = true)
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
            StudyStatus = isUsable == true
                ? RadStudyStatus.QualityAccepted
                : RadStudyStatus.Acquired,
            IsUsable = isUsable,
        };

        db.RadOrders.Add(order);
        db.RadStudies.Add(study);
        await db.SaveChangesAsync();

        return study.Id;
    }

    private static CreateRadReportDraftRequest Draf(RadReportAuthorRole? peran = null) => new()
    {
        AuthorRole = peran,
        Findings = "Sulkus dan girus dalam batas normal.",
        Impression = IsiKesimpulan,
        Recommendation = "Tidak diperlukan pemeriksaan lanjutan.",
    };

    /// <summary>Satu study beserta draf pertamanya. Mengembalikan id study dan id bacaan.</summary>
    private static async Task<(Guid StudyId, Guid ReportId)> DrafAsync(
        ApplicationDbContext db,
        Guid penulis,
        RadReportAuthorRole peran,
        bool radiolog = false)
    {
        var study = await SeedStudyAsync(db);

        var draf = await Service(db, penulis, radiolog).CreateDraftAsync(study, Draf(peran));

        Assert.Equal(RadOperationResultKind.Success, draf.Kind);

        return (study, draf.Value!.Id);
    }
}
