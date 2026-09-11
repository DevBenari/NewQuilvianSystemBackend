using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.RadiologyManagement;

/// <summary>
/// Pengelolaan butir keselamatan — <c>BE-RAD-05</c>, turunan acceptance criteria AC-5.
///
/// Butir keselamatan adalah <b>kosakata</b>: menambahnya tidak membuatnya berlaku bagi pasien
/// mana pun. Yang dijaga berkas ini ada dua: kode butir tidak boleh kembar, dan butir yang
/// sedang dipakai aturan keselamatan berlaku tidak boleh hilang — karena pertanyaan yang
/// kehilangan rumusannya tetap terlihat dijawab.
/// </summary>
public sealed class RadSafetyRequirementServiceTests
{
    private static readonly Guid AdminRadiologi = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PenanggungJawabKlinis = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /* ================================================================== *
     * Menambah butir
     * ================================================================== */

    [Fact]
    public async Task ButirBaruDapatDitambahkanDanBelumMengikatSiapaPun()
    {
        await using var db = Konteks();

        var hasil = await Service(db).CreateAsync(Permintaan("pregnancy_screening", "Skrining kehamilan"));

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal("PREGNANCY_SCREENING", hasil.Value!.RequirementCode);
        Assert.Equal(AdminRadiologi, hasil.Value.CreateBy);

        // Kosakata, bukan kebijakan: belum ada satu pun aturan yang memakainya.
        Assert.Equal(0, hasil.Value.ActiveRuleCount);
    }

    [Fact]
    public async Task KodeButirKembar_Ditolak409()
    {
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("METAL_IMPLANT", "Skrining implan logam"));
        var kedua = await service.CreateAsync(Permintaan("METAL_IMPLANT", "Implan logam"));

        Assert.Equal(RadOperationResultKind.Conflict, kedua.Kind);
        Assert.Equal(RadErrorCodes.SafetyRequirementCodeAlreadyUsed, kedua.ErrorCode);
    }

    [Fact]
    public async Task KodeButirKembarBerbedaHurufBesarKecil_TetapDitolak()
    {
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("CONTRAST_ALLERGY", "Alergi kontras"));
        var kedua = await service.CreateAsync(Permintaan("contrast_allergy", "Alergi media kontras"));

        Assert.Equal(RadOperationResultKind.Conflict, kedua.Kind);
    }

    [Fact]
    public async Task KodeAtauNamaKosong_Ditolak400()
    {
        await using var db = Konteks();
        var service = Service(db);

        var tanpaKode = await service.CreateAsync(Permintaan("  ", "Skrining kehamilan"));
        var tanpaNama = await service.CreateAsync(Permintaan("PREGNANCY", "   "));

        Assert.Equal(RadOperationResultKind.Validation, tanpaKode.Kind);
        Assert.Contains("Kode butir keselamatan wajib diisi", tanpaKode.ErrorMessage!);

        Assert.Equal(RadOperationResultKind.Validation, tanpaNama.Kind);
        Assert.Contains("Nama butir keselamatan wajib diisi", tanpaNama.ErrorMessage!);
    }

    [Fact]
    public async Task MengubahButir_TidakMenganggapKodenyaSendiriKembar()
    {
        await using var db = Konteks();
        var service = Service(db);

        var butir = await service.CreateAsync(Permintaan("SEDATION_FASTING", "Puasa sebelum sedasi"));

        var hasil = await service.UpdateAsync(butir.Value!.Id, new UpdateRadSafetyRequirementRequest
        {
            RequirementCode = "SEDATION_FASTING",
            RequirementName = "Puasa sebelum tindakan sedasi",
            Category = "Sedation",
            RequiresNote = true,
        });

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal("Puasa sebelum tindakan sedasi", hasil.Value!.RequirementName);
        Assert.True(hasil.Value.RequiresNote);
    }

    [Fact]
    public async Task ButirYangTidakAda_Ditolak404()
    {
        await using var db = Konteks();

        var hasil = await Service(db).GetByIdAsync(Guid.NewGuid());

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
        Assert.Equal(RadErrorCodes.SafetyRequirementNotFound, hasil.ErrorCode);
    }

    /* ================================================================== *
     * Butir yang sedang dipakai tidak boleh hilang
     * ================================================================== */

    [Fact]
    public async Task MenonaktifkanButirYangMasihDipakaiAturanBerlaku_Ditolak409()
    {
        await using var db = Konteks();
        var butir = await Service(db).CreateAsync(Permintaan("PREGNANCY", "Skrining kehamilan"));

        await SahkanAturanAsync(db, butir.Value!.Id);

        var hasil = await Service(db).SetStatusAsync(butir.Value.Id, isActive: false);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.SafetyRequirementStillInUse, hasil.ErrorCode);
        Assert.Contains("Nonaktifkan aturannya lebih dulu", hasil.ErrorMessage!);

        var tersimpan = await db.MstRadSafetyRequirements.AsNoTracking().SingleAsync();
        Assert.True(tersimpan.IsActive);
    }

    [Fact]
    public async Task MenghapusButirYangMasihDipakaiAturanBerlaku_Ditolak409()
    {
        await using var db = Konteks();
        var butir = await Service(db).CreateAsync(Permintaan("PREGNANCY", "Skrining kehamilan"));

        await SahkanAturanAsync(db, butir.Value!.Id);

        var hasil = await Service(db).DeleteAsync(butir.Value.Id);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);

        var tersimpan = await db.MstRadSafetyRequirements.AsNoTracking().SingleAsync();
        Assert.False(tersimpan.IsDelete);
    }

    [Fact]
    public async Task ButirDapatDipensiunkanSetelahAturannyaDihentikan()
    {
        await using var db = Konteks();
        var butir = await Service(db).CreateAsync(Permintaan("PREGNANCY", "Skrining kehamilan"));

        var aturanId = await SahkanAturanAsync(db, butir.Value!.Id);
        await PolicyService(db, PenanggungJawabKlinis).DeactivateAsync(aturanId);

        var hasil = await Service(db).SetStatusAsync(butir.Value.Id, isActive: false);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.False(hasil.Value!.IsActive);
    }

    [Fact]
    public async Task ButirYangHanyaDipakaiDraf_TetapDapatDipensiunkan()
    {
        // Draf belum menanyakan apa pun kepada pasien, sehingga tidak ada alasan menahan.
        await using var db = Konteks();
        var butir = await Service(db).CreateAsync(Permintaan("PRIOR_STUDY", "Perbandingan pemeriksaan sebelumnya"));
        var alat = await AlatAsync(db);

        await PolicyService(db, AdminRadiologi).CreateDraftAsync(new CreateRadSafetyRuleRequest
        {
            ModalityId = alat,
            SafetyRequirementId = butir.Value!.Id,
            IsMandatory = false,
        });

        var hasil = await Service(db).SetStatusAsync(butir.Value.Id, isActive: false);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
    }

    [Fact]
    public async Task ButirYangSudahDihapus_TidakMunculLagiDanKodenyaDapatDipakaiUlang()
    {
        await using var db = Konteks();
        var service = Service(db);

        var butir = await service.CreateAsync(Permintaan("OLD_CODE", "Butir lama"));
        await service.DeleteAsync(butir.Value!.Id);

        var dicari = await service.GetByIdAsync(butir.Value.Id);
        var daftar = await service.GetPagedAsync(new RadSafetyRequirementPagedQuery());
        var baru = await service.CreateAsync(Permintaan("OLD_CODE", "Butir baru"));

        Assert.Equal(RadOperationResultKind.NotFound, dicari.Kind);
        Assert.Equal(0, daftar.TotalData);
        Assert.Equal(RadOperationResultKind.Success, baru.Kind);
    }

    /* ================================================================== *
     * Daftar, pilihan, rekap, metadata
     * ================================================================== */

    [Fact]
    public async Task DaftarMenyaringButirYangSedangDipakai()
    {
        await using var db = Konteks();
        var service = Service(db);

        var dipakai = await service.CreateAsync(Permintaan("PREGNANCY", "Skrining kehamilan"));
        await service.CreateAsync(Permintaan("PRIOR_STUDY", "Perbandingan pemeriksaan sebelumnya"));
        await SahkanAturanAsync(db, dipakai.Value!.Id);

        var terpakai = await Service(db).GetPagedAsync(
            new RadSafetyRequirementPagedQuery { IsUsedByActiveRule = true });

        var belum = await Service(db).GetPagedAsync(
            new RadSafetyRequirementPagedQuery { IsUsedByActiveRule = false });

        Assert.Equal(1, terpakai.TotalData);
        Assert.Equal("PREGNANCY", terpakai.Items.Single().RequirementCode);
        Assert.Equal(1, belum.TotalData);
    }

    [Fact]
    public async Task DaftarMenyaringKelompokButir()
    {
        await using var db = Konteks();
        var service = Service(db);

        var radiasi = Permintaan("PREGNANCY", "Skrining kehamilan");
        radiasi.Category = "Radiation";
        await service.CreateAsync(radiasi);

        var kontras = Permintaan("CONTRAST_ALLERGY", "Alergi kontras");
        kontras.Category = "Contrast";
        await service.CreateAsync(kontras);

        var hasil = await service.GetPagedAsync(
            new RadSafetyRequirementPagedQuery { Category = "Contrast" });

        Assert.Equal(1, hasil.TotalData);
        Assert.Equal("CONTRAST_ALLERGY", hasil.Items.Single().RequirementCode);
    }

    [Fact]
    public async Task MetadataMenurunkanKelompokDariIsinya()
    {
        // Kategori adalah teks bebas. Daftar tetap akan basi begitu ada kelompok baru dipakai.
        await using var db = Konteks();
        var service = Service(db);

        var radiasi = Permintaan("PREGNANCY", "Skrining kehamilan");
        radiasi.Category = "Radiation";
        await service.CreateAsync(radiasi);

        var implan = Permintaan("METAL_IMPLANT", "Skrining implan logam");
        implan.Category = "Implant";
        await service.CreateAsync(implan);

        var metadata = await service.GetFilterMetadataAsync();

        Assert.Equal(new[] { "Implant", "Radiation" }, metadata.Categories);
        Assert.Contains(metadata.QueryParameters, x => x.Name == "isUsedByActiveRule");
        Assert.NotEmpty(metadata.PageSizeOptions);
    }

    [Fact]
    public async Task PilihanDropdownHanyaMemuatButirYangMasihDipakai()
    {
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("PREGNANCY", "Skrining kehamilan"));

        var pensiun = Permintaan("OLD", "Butir lama");
        pensiun.IsActive = false;
        await service.CreateAsync(pensiun);

        var bawaan = await service.GetOptionsAsync();
        var seluruhnya = await service.GetOptionsAsync(onlyActive: false);

        Assert.Single(bawaan);
        Assert.Equal(2, seluruhnya.Count);
    }

    [Fact]
    public async Task RekapMembedakanButirYangDipakaiDariYangTerlupakan()
    {
        await using var db = Konteks();
        var service = Service(db);

        var dipakai = await service.CreateAsync(Permintaan("PREGNANCY", "Skrining kehamilan"));
        await service.CreateAsync(Permintaan("PRIOR_STUDY", "Perbandingan pemeriksaan sebelumnya"));

        var bercatatan = Permintaan("SEDATION", "Puasa sebelum sedasi");
        bercatatan.RequiresNote = true;
        await service.CreateAsync(bercatatan);

        await SahkanAturanAsync(db, dipakai.Value!.Id);

        var rekap = await Service(db).GetSummaryAsync();

        Assert.Equal(3, rekap.TotalButir);
        Assert.Equal(3, rekap.Aktif);
        Assert.Equal(1, rekap.WajibBercatatan);
        Assert.Equal(1, rekap.DipakaiAturanBerlaku);

        // Dua butir aktif belum dipakai satu pun aturan.
        Assert.Equal(2, rekap.BelumDipakai);
    }

    [Fact]
    public async Task DaftarMenormalkanPermintaanHalamanYangTidakMasukAkal()
    {
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("PREGNANCY", "Skrining kehamilan"));

        var nol = await service.GetPagedAsync(
            new RadSafetyRequirementPagedQuery { PageNumber = 0, PageSize = 0 });

        var kebesaran = await service.GetPagedAsync(
            new RadSafetyRequirementPagedQuery { PageSize = 500 });

        Assert.Equal(1, nol.PageNumber);
        Assert.Equal(25, nol.PageSize);
        Assert.Equal(100, kebesaran.PageSize);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-safety-requirement-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }

    private static HttpContextAccessor Accessor(Guid actorUserId)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            "Test"));

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = principal },
        };
    }

    /// <summary>
    /// Service beserta identitas petugasnya. <b>Buat ulang setiap kali ada pemanggilan
    /// bersarang yang juga membuat service</b> — <c>HttpContextAccessor</c> memakai
    /// <c>AsyncLocal</c> bersama, dan setternya mengosongkan context accessor sebelumnya.
    /// </summary>
    private static RadSafetyRequirementService Service(ApplicationDbContext db)
    {
        var accessor = Accessor(AdminRadiologi);

        return new RadSafetyRequirementService(
            db, accessor, new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static RadSafetyPolicyService PolicyService(ApplicationDbContext db, Guid actorUserId)
    {
        var accessor = Accessor(actorUserId);

        return new RadSafetyPolicyService(
            db, accessor, new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static CreateRadSafetyRequirementRequest Permintaan(string kode, string nama) => new()
    {
        RequirementCode = kode,
        RequirementName = nama,
        IsActive = true,
    };

    private static async Task<Guid> AlatAsync(ApplicationDbContext db)
    {
        var alat = new MstRadModality
        {
            Id = Guid.NewGuid(),
            ModalityCode = $"M{Guid.NewGuid():N}"[..8],
            ModalityName = "Alat Uji",
            IsActive = true,
        };

        db.MstRadModalities.Add(alat);
        await db.SaveChangesAsync();

        return alat.Id;
    }

    /// <summary>Aturan keselamatan yang sudah melewati siklus pengesahan sampai berlaku.</summary>
    private static async Task<Guid> SahkanAturanAsync(ApplicationDbContext db, Guid requirementId)
    {
        var alat = await AlatAsync(db);
        var penyusun = PolicyService(db, AdminRadiologi);

        var draf = await penyusun.CreateDraftAsync(new CreateRadSafetyRuleRequest
        {
            ModalityId = alat,
            SafetyRequirementId = requirementId,
            IsMandatory = true,
        });

        await penyusun.SubmitAsync(draf.Value!.Id);
        await PolicyService(db, PenanggungJawabKlinis).ApproveAsync(draf.Value.Id);

        return draf.Value.Id;
    }
}
