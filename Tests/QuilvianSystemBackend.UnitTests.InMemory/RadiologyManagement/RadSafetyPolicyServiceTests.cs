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
/// Siklus pengesahan aturan keselamatan radiologi — <c>BE-RAD-02</c>, acceptance criteria
/// AC-13, AC-14, dan AC-15 pada <c>RAD-DEC-005</c>.
///
/// Aturan keselamatan menentukan pertanyaan apa yang wajib dijawab sebelum seorang pasien
/// disinari. Yang diuji di sini bukan kelengkapan formulir, melainkan satu hal: pengesahan
/// benar-benar berpindah tangan, dan tidak ada jalan memutar untuk melewatinya.
/// </summary>
public sealed class RadSafetyPolicyServiceTests
{
    private static readonly Guid AdminRadiologi = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PenanggungJawabKlinis = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AdminLain = Guid.Parse("33333333-3333-3333-3333-333333333333");

    /* ================================================================== *
     * Menyusun draf
     * ================================================================== */

    [Fact]
    public async Task DrafLahirBelumBerlaku()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);

        var hasil = await Service(db, AdminRadiologi)
            .CreateDraftAsync(Permintaan(alat, butir));

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadSafetyRuleStatus.Draft), hasil.Value!.RuleStatus);
        Assert.Equal(1, hasil.Value.RuleVersion);

        // Yang menentukan gerbang adalah RuleStatus. Kolom lama IsActive ikut diisi supaya
        // keduanya tidak pernah berselisih pada baris yang lahir dari jalur ini.
        var tersimpan = await db.MstRadModalitySafetyRules.SingleAsync();
        Assert.False(tersimpan.IsActive);
    }

    [Fact]
    public async Task DrafPadaAlatYangSudahDipensiunkan_Ditolak()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db, alatAktif: false);

        var hasil = await Service(db, AdminRadiologi)
            .CreateDraftAsync(Permintaan(alat, butir));

        Assert.Equal(RadOperationResultKind.BusinessRule, hasil.Kind);
        Assert.Equal(RadErrorCodes.MasterDataInactive, hasil.ErrorCode);
        Assert.Contains("tidak aktif", hasil.ErrorMessage!);
    }

    [Fact]
    public async Task MasaBerlakuTerbalik_Ditolak()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);

        var permintaan = Permintaan(alat, butir);
        permintaan.EffectiveFrom = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        permintaan.EffectiveTo = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

        var hasil = await Service(db, AdminRadiologi).CreateDraftAsync(permintaan);

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Contains("lebih awal", hasil.ErrorMessage!);
    }

    /* ================================================================== *
     * AC-13 — yang menyusun tidak boleh mengesahkan
     * ================================================================== */

    [Fact]
    public async Task PenyusunMengesahkanAturannyaSendiri_Ditolak()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        var hasil = await Service(db, AdminRadiologi).ApproveAsync(aturanId);

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.SelfApprovalNotAllowed, hasil.ErrorCode);

        // Yang paling penting: penolakannya benar-benar menahan. Aturan tetap menunggu
        // keputusan, tidak diam-diam berlaku.
        var tersimpan = await db.MstRadModalitySafetyRules.AsNoTracking().SingleAsync();
        Assert.Equal(RadSafetyRuleStatus.PendingApproval, tersimpan.RuleStatus);
        Assert.Equal(1, tersimpan.RuleVersion);
        Assert.Null(tersimpan.ApprovedAt);
    }

    [Fact]
    public async Task PengajuMengesahkanWalauBukanPenyusun_Ditolak()
    {
        // Jalan memutar yang paling mudah: A menyusun draf, lalu meminta B sekadar menekan
        // tombol Ajukan supaya B dapat mengesahkannya. Memeriksa pengaju sekaligus penyusun
        // menutup jalan itu.
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);

        var draf = await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(alat, butir));
        await Service(db, AdminLain).SubmitAsync(draf.Value!.Id);

        var olehPengaju = await Service(db, AdminLain).ApproveAsync(draf.Value.Id);
        var olehPenyusun = await Service(db, AdminRadiologi).ApproveAsync(draf.Value.Id);

        Assert.Equal(RadOperationResultKind.Forbidden, olehPengaju.Kind);
        Assert.Equal(RadOperationResultKind.Forbidden, olehPenyusun.Kind);
    }

    /* ================================================================== *
     * AC-14 — pengesahan menaikkan versi tepat satu kali
     * ================================================================== */

    [Fact]
    public async Task Pengesahan_MenaikkanVersiTepatSatuKali()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        var hasil = await Service(db, PenanggungJawabKlinis).ApproveAsync(aturanId);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadSafetyRuleStatus.Active), hasil.Value!.RuleStatus);

        // Naik dari 1 ke 2, bukan ke 3. Nomor inilah yang dibekukan pada study yang lolos,
        // sehingga melompatinya membuat jejak lama menunjuk versi yang tidak pernah ada.
        Assert.Equal(2, hasil.Value.RuleVersion);
        Assert.Equal(PenanggungJawabKlinis, hasil.Value.ApprovedByUserId);
        Assert.NotNull(hasil.Value.ApprovedAt);
    }

    [Fact]
    public async Task PengesahanBerulang_TidakMenaikkanVersiLagi()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        await Service(db, PenanggungJawabKlinis).ApproveAsync(aturanId);
        var kedua = await Service(db, PenanggungJawabKlinis).ApproveAsync(aturanId);

        Assert.Equal(RadOperationResultKind.Conflict, kedua.Kind);
        Assert.Equal(RadErrorCodes.InvalidTransition, kedua.ErrorCode);

        var tersimpan = await db.MstRadModalitySafetyRules.AsNoTracking().SingleAsync();
        Assert.Equal(2, tersimpan.RuleVersion);
    }

    [Fact]
    public async Task PenolakanDiTengahJalan_TidakMenaikkanVersi()
    {
        // Satu aturan yang bolak-balik ditolak lalu diperbaiki tetap menjadi versi 2 ketika
        // akhirnya disahkan. Versi menghitung pengesahan, bukan percobaan.
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        await Service(db, PenanggungJawabKlinis).RejectAsync(
            aturanId, new RadSafetyRuleRejectRequest { RejectionReason = "Butir belum sesuai SOP." });

        await Service(db, AdminRadiologi).SubmitAsync(aturanId);
        var hasil = await Service(db, PenanggungJawabKlinis).ApproveAsync(aturanId);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(2, hasil.Value!.RuleVersion);
    }

    /* ================================================================== *
     * AC-15 — penolakan wajib beralasan
     * ================================================================== */

    [Fact]
    public async Task PenolakanTanpaAlasan_Ditolak()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        var hasil = await Service(db, PenanggungJawabKlinis).RejectAsync(
            aturanId, new RadSafetyRuleRejectRequest { RejectionReason = "   " });

        Assert.Equal(RadOperationResultKind.Validation, hasil.Kind);
        Assert.Equal(RadErrorCodes.ReasonRequired, hasil.ErrorCode);

        var tersimpan = await db.MstRadModalitySafetyRules.AsNoTracking().SingleAsync();
        Assert.Equal(RadSafetyRuleStatus.PendingApproval, tersimpan.RuleStatus);
    }

    [Fact]
    public async Task PenolakanBeralasan_MengembalikanAturanKeDraf()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        var hasil = await Service(db, PenanggungJawabKlinis).RejectAsync(
            aturanId,
            new RadSafetyRuleRejectRequest { RejectionReason = "Butir ini sudah dicabut SOP 2026." });

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadSafetyRuleStatus.Draft), hasil.Value!.RuleStatus);
        Assert.Equal("Butir ini sudah dicabut SOP 2026.", hasil.Value.RejectionReason);
        Assert.Equal(PenanggungJawabKlinis, hasil.Value.RejectedByUserId);
        Assert.Equal(1, hasil.Value.RuleVersion);
    }

    [Fact]
    public async Task JejakPenolakanTidakTerhapusSaatDiajukanUlang()
    {
        // Audit penolakan termasuk yang tidak boleh diubah. Menghapusnya saat pengajuan ulang
        // akan menghilangkan satu-satunya catatan bahwa aturan itu pernah dinilai tidak layak.
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        await Service(db, PenanggungJawabKlinis).RejectAsync(
            aturanId, new RadSafetyRuleRejectRequest { RejectionReason = "Alat salah." });

        var hasil = await Service(db, AdminRadiologi).SubmitAsync(aturanId);

        Assert.Equal("Alat salah.", hasil.Value!.RejectionReason);
        Assert.NotNull(hasil.Value.RejectedAt);
    }

    /* ================================================================== *
     * Tabrakan aturan yang sama-sama berlaku
     * ================================================================== */

    [Fact]
    public async Task PengesahanKeduaUntukKombinasiSama_Ditolak()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);

        var pertama = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);
        await Service(db, PenanggungJawabKlinis).ApproveAsync(pertama);

        var kedua = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);
        var hasil = await Service(db, PenanggungJawabKlinis).ApproveAsync(kedua);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.ActiveSafetyRuleExists, hasil.ErrorCode);
        Assert.Contains("Nonaktifkan aturan lama", hasil.ErrorMessage!);
    }

    [Fact]
    public async Task PenggantiDapatDisahkanSetelahAturanLamaDihentikan()
    {
        // Inilah jalan yang benar untuk mengganti aturan: hentikan yang lama, sahkan yang baru.
        // Keduanya tidak pernah berlaku bersamaan, sehingga tidak ada dua aturan bertentangan
        // yang sama-sama menahan pasien.
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);

        var lama = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);
        await Service(db, PenanggungJawabKlinis).ApproveAsync(lama);
        await Service(db, PenanggungJawabKlinis).DeactivateAsync(lama);

        var baru = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);
        var hasil = await Service(db, PenanggungJawabKlinis).ApproveAsync(baru);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal(nameof(RadSafetyRuleStatus.Active), hasil.Value!.RuleStatus);
    }

    /* ================================================================== *
     * Transisi yang tidak sah
     * ================================================================== */

    [Fact]
    public async Task AturanYangSedangBerlaku_TidakDapatDiubah()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);
        await Service(db, PenanggungJawabKlinis).ApproveAsync(aturanId);

        var hasil = await Service(db, AdminRadiologi).UpdateDraftAsync(
            aturanId,
            new UpdateRadSafetyRuleRequest
            {
                ModalityId = alat,
                SafetyRequirementId = butir,
                IsMandatory = false,
            });

        Assert.Equal(RadOperationResultKind.Forbidden, hasil.Kind);
        Assert.Equal(RadErrorCodes.SafetyRuleNotEditable, hasil.ErrorCode);
        Assert.Contains("Susun draf baru", hasil.ErrorMessage!);

        var tersimpan = await db.MstRadModalitySafetyRules.AsNoTracking().SingleAsync();
        Assert.True(tersimpan.IsMandatory);
    }

    [Fact]
    public async Task DrafBelumDiajukan_TidakDapatDisahkan()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var draf = await Service(db, AdminRadiologi).CreateDraftAsync(Permintaan(alat, butir));

        var hasil = await Service(db, PenanggungJawabKlinis).ApproveAsync(draf.Value!.Id);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.InvalidTransition, hasil.ErrorCode);
    }

    [Fact]
    public async Task AturanYangBelumBerlaku_TidakDapatDinonaktifkan()
    {
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        var hasil = await Service(db, PenanggungJawabKlinis).DeactivateAsync(aturanId);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.InvalidTransition, hasil.ErrorCode);
    }

    [Fact]
    public async Task AturanYangTidakAda_Ditolak404()
    {
        await using var db = Konteks();

        var hasil = await Service(db, PenanggungJawabKlinis).ApproveAsync(Guid.NewGuid());

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
        Assert.Equal(RadErrorCodes.SafetyRuleNotFound, hasil.ErrorCode);
    }

    /* ================================================================== *
     * Sambungan ke gerbang keselamatan
     * ================================================================== */

    [Fact]
    public async Task HanyaAturanYangSudahDisahkanYangMenahanPasien()
    {
        // Menyambung BE-RAD-02 dengan BE-RAD-06. Sepanjang aturan belum disahkan, gerbang
        // membacanya sebagai kebijakan yang belum ditetapkan; sesudah disahkan, barulah ia
        // menahan dan dapat dituntaskan.
        await using var db = Konteks();
        var (alat, butir) = await SeedAsync(db);
        var aturanId = await DrafDiajukanAsync(db, alat, butir, AdminRadiologi);

        var sebelum = await db.MstRadModalitySafetyRules
            .AsNoTracking().Include(x => x.SafetyRequirement).ToListAsync();

        Assert.False(RadSafetyGateEvaluator.Evaluate(
            sebelum, Array.Empty<RadStudySafetyCheck>()).PolicyConfigured);

        await Service(db, PenanggungJawabKlinis).ApproveAsync(aturanId);

        var sesudah = await db.MstRadModalitySafetyRules
            .AsNoTracking().Include(x => x.SafetyRequirement).ToListAsync();

        var hasil = RadSafetyGateEvaluator.Evaluate(
            sesudah,
            new[]
            {
                new RadStudySafetyCheck
                {
                    SafetyRequirementId = butir,
                    CheckState = RadSafetyCheckState.Passed,
                },
            });

        Assert.True(hasil.PolicyConfigured);
        Assert.True(hasil.Cleared);
        Assert.Equal(2, hasil.RuleVersion);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-safety-policy-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static RadSafetyPolicyService Service(ApplicationDbContext db, Guid actorUserId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };

        return new RadSafetyPolicyService(
            db,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static async Task<(Guid AlatId, Guid ButirId)> SeedAsync(
        ApplicationDbContext db,
        bool alatAktif = true,
        bool butirAktif = true)
    {
        var alat = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = "CT",
            ModalityName = "CT-Scan",
            UsesIonisingRadiation = true,
            IsActive = alatAktif,
        };

        var butir = new MstRadSafetyRequirement
        {
            Id = Guid.NewGuid(),
            RequirementCode = "PREGNANCY_SCREENING",
            RequirementName = "Skrining kehamilan",
            IsActive = butirAktif,
        };

        db.MstRadModalities.Add(alat);
        db.MstRadSafetyRequirements.Add(butir);
        await db.SaveChangesAsync();

        return (alat.Id, butir.Id);
    }

    private static CreateRadSafetyRuleRequest Permintaan(Guid alatId, Guid butirId) => new()
    {
        ModalityId = alatId,
        SafetyRequirementId = butirId,
        IsMandatory = true,
    };

    /// <summary>Draf yang sudah disusun dan diajukan oleh orang yang sama.</summary>
    private static async Task<Guid> DrafDiajukanAsync(
        ApplicationDbContext db,
        Guid alatId,
        Guid butirId,
        Guid penyusun)
    {
        var service = Service(db, penyusun);
        var draf = await service.CreateDraftAsync(Permintaan(alatId, butirId));
        await service.SubmitAsync(draf.Value!.Id);

        return draf.Value.Id;
    }
}
