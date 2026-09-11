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
/// Pengelolaan alat pencitraan — <c>BE-RAD-04</c>, acceptance criteria AC-5.
///
/// Dua hal yang dijaga berkas ini. <b>Pertama</b>, kode alat tidak boleh kembar. <b>Kedua, dan
/// yang benar-benar penting</b>, alat yang masih dipakai aturan keselamatan berlaku tidak boleh
/// dinonaktifkan maupun dihapus — karena aturan yang menggantung pada alat yang sudah
/// dipensiunkan tidak menjaga siapa pun.
/// </summary>
public sealed class RadModalityServiceTests
{
    private static readonly Guid AdminRadiologi = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PenanggungJawabKlinis = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /* ================================================================== *
     * Mendaftarkan alat
     * ================================================================== */

    [Fact]
    public async Task AlatBaruDapatDidaftarkanTanpaMenyentuhDatabaseLangsung()
    {
        await using var db = Konteks();

        var hasil = await Service(db).CreateAsync(Permintaan("ct", "CT-Scan 128 Slice"));

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);

        // Kode disimpan dalam huruf besar supaya "ct" dan "CT" tidak menjadi dua alat berbeda.
        Assert.Equal("CT", hasil.Value!.ModalityCode);
        Assert.Equal(AdminRadiologi, hasil.Value.CreateBy);

        // Alat baru belum punya aturan keselamatan, sehingga belum siap dipakai memeriksa.
        Assert.False(hasil.Value.HasActiveSafetyRule);
        Assert.Equal(0, hasil.Value.ActiveSafetyRuleCount);
    }

    [Fact]
    public async Task KodeAlatKembar_Ditolak409()
    {
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        var kedua = await service.CreateAsync(Permintaan("CT", "CT-Scan Kedua"));

        Assert.Equal(RadOperationResultKind.Conflict, kedua.Kind);
        Assert.Equal(RadErrorCodes.ModalityCodeAlreadyUsed, kedua.ErrorCode);
        Assert.Contains("Gunakan kode lain", kedua.ErrorMessage!);
    }

    [Fact]
    public async Task KodeAlatKembarBerbedaHurufBesarKecil_TetapDitolak()
    {
        // "ct" dan "CT" adalah alat yang sama bagi manusia. Membiarkan keduanya berdampingan
        // membuat aturan keselamatan terpasang pada salah satunya saja.
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        var kedua = await service.CreateAsync(Permintaan("ct", "CT-Scan Lain"));

        Assert.Equal(RadOperationResultKind.Conflict, kedua.Kind);
    }

    [Fact]
    public async Task KodeAtauNamaKosong_Ditolak400()
    {
        await using var db = Konteks();
        var service = Service(db);

        var tanpaKode = await service.CreateAsync(Permintaan("   ", "CT-Scan"));
        var tanpaNama = await service.CreateAsync(Permintaan("CT", "  "));

        Assert.Equal(RadOperationResultKind.Validation, tanpaKode.Kind);
        Assert.Contains("Kode alat wajib diisi", tanpaKode.ErrorMessage!);

        Assert.Equal(RadOperationResultKind.Validation, tanpaNama.Kind);
        Assert.Contains("Nama alat wajib diisi", tanpaNama.ErrorMessage!);
    }

    /* ================================================================== *
     * Mengubah alat
     * ================================================================== */

    [Fact]
    public async Task MengubahAlat_TidakMenganggapKodenyaSendiriKembar()
    {
        await using var db = Konteks();
        var service = Service(db);

        var alat = await service.CreateAsync(Permintaan("CT", "CT-Scan"));

        var hasil = await service.UpdateAsync(alat.Value!.Id, new UpdateRadModalityRequest
        {
            ModalityCode = "CT",
            ModalityName = "CT-Scan 128 Slice",
            SupportsContrast = true,
        });

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.Equal("CT-Scan 128 Slice", hasil.Value!.ModalityName);
        Assert.True(hasil.Value.SupportsContrast);
        Assert.Equal(AdminRadiologi, hasil.Value.UpdateBy);
    }

    [Fact]
    public async Task MengubahAlatMemakaiKodeMilikAlatLain_Ditolak409()
    {
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        var usg = await service.CreateAsync(Permintaan("USG", "Ultrasonografi"));

        var hasil = await service.UpdateAsync(usg.Value!.Id, new UpdateRadModalityRequest
        {
            ModalityCode = "CT",
            ModalityName = "Ultrasonografi",
        });

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.ModalityCodeAlreadyUsed, hasil.ErrorCode);
    }

    [Fact]
    public async Task AlatYangTidakAda_Ditolak404()
    {
        await using var db = Konteks();

        var hasil = await Service(db).GetByIdAsync(Guid.NewGuid());

        Assert.Equal(RadOperationResultKind.NotFound, hasil.Kind);
        Assert.Equal(RadErrorCodes.ModalityNotFound, hasil.ErrorCode);
    }

    /* ================================================================== *
     * AC-5 — alat yang masih dipakai tidak boleh dipensiunkan
     * ================================================================== */

    [Fact]
    public async Task MenonaktifkanAlatYangMasihDipakaiAturanBerlaku_Ditolak409()
    {
        await using var db = Konteks();
        var service = Service(db);

        var alat = await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        await SahkanAturanAsync(db, alat.Value!.Id);

        // Service dibuat ulang di sini. Lihat catatan pada Service(): membuat accessor baru di
        // pemanggilan bersarang mengosongkan context milik accessor sebelumnya.
        var hasil = await Service(db).SetStatusAsync(alat.Value.Id, isActive: false);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);
        Assert.Equal(RadErrorCodes.ModalityStillInUse, hasil.ErrorCode);
        Assert.Contains("Nonaktifkan aturannya lebih dulu", hasil.ErrorMessage!);

        // Penolakannya benar-benar menahan: alat tetap aktif.
        var tersimpan = await db.MstRadModalities.AsNoTracking().SingleAsync();
        Assert.True(tersimpan.IsActive);
    }

    [Fact]
    public async Task MenghapusAlatYangMasihDipakaiAturanBerlaku_Ditolak409()
    {
        await using var db = Konteks();
        var service = Service(db);

        var alat = await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        await SahkanAturanAsync(db, alat.Value!.Id);

        var hasil = await Service(db).DeleteAsync(alat.Value.Id);

        Assert.Equal(RadOperationResultKind.Conflict, hasil.Kind);

        var tersimpan = await db.MstRadModalities.AsNoTracking().SingleAsync();
        Assert.False(tersimpan.IsDelete);
    }

    [Fact]
    public async Task AlatDapatDipensiunkanSetelahAturannyaDihentikan()
    {
        // Urutan yang benar: hentikan aturannya lewat pengesahan, baru pensiunkan alatnya.
        await using var db = Konteks();
        var service = Service(db);

        var alat = await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        var aturanId = await SahkanAturanAsync(db, alat.Value!.Id);

        await PolicyService(db, PenanggungJawabKlinis).DeactivateAsync(aturanId);

        var hasil = await Service(db).SetStatusAsync(alat.Value.Id, isActive: false);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
        Assert.False(hasil.Value!.IsActive);
    }

    [Fact]
    public async Task AturanYangMasihDrafTidakMenahanPenonaktifan()
    {
        // Draf belum menjaga siapa pun, sehingga tidak ada alasan menahan alatnya.
        await using var db = Konteks();
        var service = Service(db);

        var alat = await service.CreateAsync(Permintaan("USG", "Ultrasonografi"));
        var butir = await ButirAsync(db);

        await PolicyService(db, AdminRadiologi).CreateDraftAsync(new CreateRadSafetyRuleRequest
        {
            ModalityId = alat.Value!.Id,
            SafetyRequirementId = butir,
            IsMandatory = true,
        });

        var hasil = await Service(db).SetStatusAsync(alat.Value.Id, isActive: false);

        Assert.Equal(RadOperationResultKind.Success, hasil.Kind);
    }

    [Fact]
    public async Task AlatYangSudahDihapus_TidakMunculLagi()
    {
        await using var db = Konteks();
        var service = Service(db);

        var alat = await service.CreateAsync(Permintaan("RF", "Fluoroskopi"));
        var dihapus = await service.DeleteAsync(alat.Value!.Id);

        Assert.Equal(RadOperationResultKind.Success, dihapus.Kind);
        Assert.True(dihapus.Value);

        var dicari = await service.GetByIdAsync(alat.Value.Id);
        var daftar = await service.GetPagedAsync(new RadModalityPagedQuery());

        Assert.Equal(RadOperationResultKind.NotFound, dicari.Kind);
        Assert.Equal(0, daftar.TotalData);

        // Barisnya tetap ada di database, hanya ditandai terhapus.
        var tersimpan = await db.MstRadModalities.AsNoTracking().SingleAsync();
        Assert.True(tersimpan.IsDelete);
    }

    [Fact]
    public async Task KodeAlatYangSudahDihapus_DapatDipakaiUlang()
    {
        await using var db = Konteks();
        var service = Service(db);

        var alat = await service.CreateAsync(Permintaan("RF", "Fluoroskopi"));
        await service.DeleteAsync(alat.Value!.Id);

        var baru = await service.CreateAsync(Permintaan("RF", "Fluoroskopi Baru"));

        Assert.Equal(RadOperationResultKind.Success, baru.Kind);
    }

    /* ================================================================== *
     * Daftar, pilihan, dan rekap
     * ================================================================== */

    [Fact]
    public async Task DaftarMenyaringKesiapanGerbang()
    {
        await using var db = Konteks();
        var service = Service(db);

        var siap = await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        await service.CreateAsync(Permintaan("USG", "Ultrasonografi"));
        await SahkanAturanAsync(db, siap.Value!.Id);

        var sudahSiap = await service.GetPagedAsync(
            new RadModalityPagedQuery { HasActiveSafetyRule = true });

        var belumSiap = await service.GetPagedAsync(
            new RadModalityPagedQuery { HasActiveSafetyRule = false });

        Assert.Equal(1, sudahSiap.TotalData);
        Assert.Equal("CT", sudahSiap.Items.Single().ModalityCode);

        Assert.Equal(1, belumSiap.TotalData);
        Assert.Equal("USG", belumSiap.Items.Single().ModalityCode);
    }

    [Fact]
    public async Task DaftarMencariPadaKodeNamaDanKeterangan()
    {
        await using var db = Konteks();
        var service = Service(db);

        var permintaan = Permintaan("MR", "MRI 1.5T");
        permintaan.Description = "Alat pencitraan medan magnet";
        await service.CreateAsync(permintaan);
        await service.CreateAsync(Permintaan("CT", "CT-Scan"));

        var perNama = await service.GetPagedAsync(new RadModalityPagedQuery { Search = "MRI" });
        var perKeterangan = await service.GetPagedAsync(
            new RadModalityPagedQuery { Search = "medan magnet" });

        Assert.Equal(1, perNama.TotalData);
        Assert.Equal(1, perKeterangan.TotalData);
    }

    [Fact]
    public async Task DaftarMenormalkanPermintaanHalamanYangTidakMasukAkal()
    {
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        await service.CreateAsync(Permintaan("MR", "MRI"));

        var nol = await service.GetPagedAsync(
            new RadModalityPagedQuery { PageNumber = 0, PageSize = 0 });

        var kebesaran = await service.GetPagedAsync(new RadModalityPagedQuery { PageSize = 500 });

        Assert.Equal(1, nol.PageNumber);
        Assert.Equal(25, nol.PageSize);
        Assert.Equal(100, kebesaran.PageSize);
        Assert.Equal(1, nol.TotalPage);
    }

    [Fact]
    public async Task PilihanDropdownHanyaMemuatAlatYangMasihDipakai()
    {
        await using var db = Konteks();
        var service = Service(db);

        await service.CreateAsync(Permintaan("CT", "CT-Scan"));

        var pensiun = Permintaan("RF", "Fluoroskopi");
        pensiun.IsActive = false;
        await service.CreateAsync(pensiun);

        var bawaan = await service.GetOptionsAsync();
        var seluruhnya = await service.GetOptionsAsync(onlyActive: false);

        Assert.Single(bawaan);
        Assert.Equal("CT", bawaan.Single().ModalityCode);
        Assert.Equal(2, seluruhnya.Count);
    }

    [Fact]
    public async Task RekapMenghitungAlatYangBelumPunyaAturanKeselamatan()
    {
        await using var db = Konteks();
        var service = Service(db);

        var siap = await service.CreateAsync(Permintaan("CT", "CT-Scan"));
        await service.CreateAsync(Permintaan("USG", "Ultrasonografi"));

        var pensiun = Permintaan("RF", "Fluoroskopi");
        pensiun.IsActive = false;
        await service.CreateAsync(pensiun);

        await SahkanAturanAsync(db, siap.Value!.Id);

        var rekap = await service.GetSummaryAsync();

        Assert.Equal(3, rekap.TotalAlat);
        Assert.Equal(2, rekap.Aktif);
        Assert.Equal(1, rekap.Nonaktif);

        // Hanya USG: CT sudah punya aturan, dan alat yang sudah dipensiunkan tidak dihitung.
        Assert.Equal(1, rekap.BelumPunyaAturanKeselamatan);
    }

    [Fact]
    public void MetadataTidakMenjanjikanPenyaringYangTidakDiproses()
    {
        using var db = Konteks();
        var metadata = Service(db).GetFilterMetadata();

        var disebut = metadata.QueryParameters.Select(x => x.Name).ToList();

        Assert.Contains("search", disebut);
        Assert.Contains("isActive", disebut);
        Assert.Contains("hasActiveSafetyRule", disebut);
        Assert.Contains("pageNumber", disebut);
        Assert.Contains("pageSize", disebut);
        Assert.NotEmpty(metadata.PageSizeOptions);
    }

    /* ================================================================== *
     * Perancah
     * ================================================================== */

    private static ApplicationDbContext Konteks()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"rad-modality-{Guid.NewGuid():N}")
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
    /// Service beserta identitas petugasnya.
    ///
    /// <b>Buat ulang setiap kali ada pemanggilan bersarang yang juga membuat service.</b>
    /// <c>HttpContextAccessor</c> menyimpan context pada <c>AsyncLocal</c> yang dipakai
    /// bersama, dan setternya mengosongkan context milik accessor sebelumnya. Menyimpan satu
    /// service lalu memakainya kembali setelah helper lain membuat accessor baru membuat
    /// identitas petugasnya hilang — dan service menolak berjalan tanpa identitas, sebagaimana
    /// seharusnya.
    /// </summary>
    private static RadModalityService Service(ApplicationDbContext db)
    {
        var accessor = Accessor(AdminRadiologi);

        return new RadModalityService(
            db, accessor, new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static RadSafetyPolicyService PolicyService(ApplicationDbContext db, Guid actorUserId)
    {
        var accessor = Accessor(actorUserId);

        return new RadSafetyPolicyService(
            db, accessor, new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static CreateRadModalityRequest Permintaan(string kode, string nama) => new()
    {
        ModalityCode = kode,
        ModalityName = nama,
        UsesIonisingRadiation = true,
        IsActive = true,
    };

    private static async Task<Guid> ButirAsync(ApplicationDbContext db)
    {
        var butir = new MstRadSafetyRequirement
        {
            Id = Guid.NewGuid(),
            RequirementCode = $"REQ{Guid.NewGuid():N}"[..10],
            RequirementName = "Skrining kehamilan",
            IsActive = true,
        };

        db.MstRadSafetyRequirements.Add(butir);
        await db.SaveChangesAsync();

        return butir.Id;
    }

    /// <summary>Aturan keselamatan yang sudah melewati siklus pengesahan sampai berlaku.</summary>
    private static async Task<Guid> SahkanAturanAsync(ApplicationDbContext db, Guid modalityId)
    {
        var butir = await ButirAsync(db);
        var penyusun = PolicyService(db, AdminRadiologi);

        var draf = await penyusun.CreateDraftAsync(new CreateRadSafetyRuleRequest
        {
            ModalityId = modalityId,
            SafetyRequirementId = butir,
            IsMandatory = true,
        });

        await penyusun.SubmitAsync(draf.Value!.Id);
        await PolicyService(db, PenanggungJawabKlinis).ApproveAsync(draf.Value.Id);

        return draf.Value.Id;
    }
}
