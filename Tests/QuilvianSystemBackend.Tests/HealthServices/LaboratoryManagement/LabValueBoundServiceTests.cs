using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Attributes;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-04</c> — endpoint pengelolaan batas nilai
/// (<c>FR-03.1</c> .. <c>FR-03.3</c>, <c>FR-03.5</c>).
///
/// Empat jalur gagal yang diwajibkan roadmap dibuktikan tersendiri:
///   <c>VAL-22</c> batas angka tanpa satuan, <c>VAL-23</c> batas pilihan tanpa satu pun pilihan,
///   <c>VAL-24</c> batas angka disertai daftar pilihan, dan <c>VAL-28</c> upaya mengubah batas
///   kritis lewat <c>PUT</c> biasa.
///
/// Ditambah jalur gagal lain yang melekat pada keenam endpoint ini menurut
/// <c>contracts/validation-matrix.md</c>: <c>VAL-21</c>, <c>VAL-25</c> .. <c>VAL-27</c>,
/// <c>VAL-29</c>, dan <c>VAL-30</c>.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini berjalan tanpa database mana pun. Konsekuensinya
/// index unik fisik tidak ditegakkan di sini, sehingga <c>VAL-21</c> yang diuji adalah
/// pemeriksaan di service; penjaga terakhirnya di database sudah dibuktikan terpisah pada
/// laporan <c>BE-LAB-02</c> bagian 5.1 dan 10.1.
///
/// Pengujian menyentuh service, bukan HTTP. Pemetaan exception menjadi kode status
/// — <c>422</c>, <c>409</c>, <c>404</c>, <c>400</c> — dilakukan controller lewat tipe exception
/// yang diperiksa di sini.
/// </remarks>
public class LabValueBoundServiceTests
{
    private static readonly Guid KepalaInstalasi = Guid.Parse("33333333-3333-3333-3333-333333333333");

    // =====================================================================
    // 1. Jalur berhasil
    // =====================================================================

    [Fact]
    public async Task MembuatBatasAngka_TersimpanBesertaSatuanDanBatasnya()
    {
        await using var context = CreateContext();
        var (procedureId, dewasaId, _) = await SeedAsync(context);
        var service = CreateService(context);

        var hasil = await service.CreateAsync(new CreateLabValueBoundRequest
        {
            ProcedureId = procedureId,
            ResultForm = LabResultForm.Numeric,
            Unit = "mmol/L",
            NormalLow = 3.5m,
            NormalHigh = 5.1m,
            CriticalLow = 2.5m,
            CriticalHigh = 6.0m,
            GenderScope = LabGenderScope.All,
            AgeCategoryId = dewasaId,
            CitoTurnaroundMinutes = 60
        });

        Assert.Equal(nameof(LabResultForm.Numeric), hasil.ResultForm);
        Assert.Equal("mmol/L", hasil.Unit);
        Assert.Equal(3.5m, hasil.NormalLow);
        Assert.Equal(6.0m, hasil.CriticalHigh);
        Assert.Equal(60, hasil.CitoTurnaroundMinutes);
        Assert.True(hasil.IsActive);
        Assert.Empty(hasil.Options);
        Assert.False(hasil.HasPendingCriticalChangeRequest);
    }

    [Fact]
    public async Task MembuatBatasPilihan_TersimpanBesertaDaftarPilihannya()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var hasil = await service.CreateAsync(new CreateLabValueBoundRequest
        {
            ProcedureId = procedureId,
            ResultForm = LabResultForm.Choice,
            GenderScope = LabGenderScope.All,
            Options = ProteinUrin()
        });

        Assert.Equal(nameof(LabResultForm.Choice), hasil.ResultForm);
        Assert.Equal(5, hasil.Options.Count);
        Assert.Equal(new[] { "Negatif", "+1", "+2", "+3", "+4" }, hasil.Options.Select(x => x.OptionName));
        Assert.Equal(new[] { "P3", "P4" }, hasil.Options.Where(x => x.IsCritical).Select(x => x.OptionCode));
    }

    [Fact]
    public async Task AC24_TigaBarisBatasHemoglobin_DapatDibuatUntukSatuPemeriksaan()
    {
        await using var context = CreateContext();
        var (procedureId, dewasaId, anakId) = await SeedAsync(context);
        var service = CreateService(context);

        await service.CreateAsync(Angka(procedureId, LabGenderScope.Male, dewasaId, 13.0m, 17.0m));
        await service.CreateAsync(Angka(procedureId, LabGenderScope.Female, dewasaId, 12.0m, 15.0m));
        await service.CreateAsync(Angka(procedureId, LabGenderScope.All, anakId, 11.0m, 14.0m));

        var daftar = await service.GetListAsync(new LabValueBoundPagedQuery { ProcedureId = procedureId });

        Assert.Equal(3, daftar.TotalData);
        Assert.Equal(3, daftar.Items.Count);
        Assert.Equal(1, daftar.PageNumber);
    }

    [Fact]
    public async Task MengubahBatasNormal_LangsungBerlakuDanMenerbitkanRiwayat()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var dibuat = await service.CreateAsync(Angka(procedureId, LabGenderScope.All, null, 3.5m, 5.1m));

        var diubah = await service.UpdateAsync(dibuat.Id, new UpdateLabValueBoundRequest
        {
            Unit = "mmol/L",
            NormalLow = 3.5m,
            NormalHigh = 5.3m,
            CriticalLow = 2.5m,
            CriticalHigh = 6.1m,
            ChangeReason = "Penyesuaian setelah penggantian alat."
        });

        // AC-33 bagian pertama: batas normal langsung berlaku, tanpa menunggu siapa pun.
        Assert.Equal(5.3m, diubah.NormalHigh);

        // AC-34: satu baris riwayat, lengkap dengan nilai lama, nilai baru, pelaku, dan alasan.
        var riwayat = await service.GetHistoryAsync(dibuat.Id);

        var barisNormalHigh = Assert.Single(riwayat, x => x.ChangedField == nameof(LabValueBound.NormalHigh));

        Assert.Equal("5.1", barisNormalHigh.OldValue);
        Assert.Equal("5.3", barisNormalHigh.NewValue);
        Assert.Equal(KepalaInstalasi, barisNormalHigh.ActorUserId);
        Assert.Equal("Penyesuaian setelah penggantian alat.", barisNormalHigh.ChangeReason);

        // Perubahan batas normal tidak menempuh persetujuan, jadi penyetujunya kosong.
        Assert.Null(barisNormalHigh.ApprovedByUserId);
    }

    [Fact]
    public async Task MenonaktifkanBatas_BerhasilBilaMasihAdaBatasLainYangAktif()
    {
        await using var context = CreateContext();
        var (procedureId, dewasaId, anakId) = await SeedAsync(context);
        var service = CreateService(context);

        var pertama = await service.CreateAsync(Angka(procedureId, LabGenderScope.All, dewasaId, 3.5m, 5.1m));
        await service.CreateAsync(Angka(procedureId, LabGenderScope.All, anakId, 3.0m, 4.8m));

        var hasil = await service.DeactivateAsync(pertama.Id);

        Assert.False(hasil.IsActive);

        var riwayat = await service.GetHistoryAsync(pertama.Id);

        Assert.Contains(riwayat, x =>
            x.ChangedField == nameof(LabValueBound.IsActive) &&
            x.OldValue == "true" &&
            x.NewValue == "false");
    }

    // =====================================================================
    // 2. Empat jalur gagal yang diwajibkan roadmap
    // =====================================================================

    [Fact]
    public async Task VAL22_BatasAngkaTanpaSatuan_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.CreateAsync(new CreateLabValueBoundRequest
            {
                ProcedureId = procedureId,
                ResultForm = LabResultForm.Numeric,
                Unit = null,
                GenderScope = LabGenderScope.All
            }));

        Assert.Equal("Pemeriksaan berhasil angka wajib punya satuan, misalnya g/dL.", galat.Message);
        Assert.Empty(await context.LabValueBounds.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task VAL23_BatasPilihanTanpaSatuPunPilihan_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.CreateAsync(new CreateLabValueBoundRequest
            {
                ProcedureId = procedureId,
                ResultForm = LabResultForm.Choice,
                GenderScope = LabGenderScope.All
            }));

        Assert.Equal("Pemeriksaan berhasil pilihan wajib punya sekurang-kurangnya satu pilihan.", galat.Message);
        Assert.Empty(await context.LabValueBounds.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task VAL24_BatasAngkaDisertaiDaftarPilihan_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.CreateAsync(new CreateLabValueBoundRequest
            {
                ProcedureId = procedureId,
                ResultForm = LabResultForm.Numeric,
                Unit = "mmol/L",
                GenderScope = LabGenderScope.All,
                Options = ProteinUrin()
            }));

        Assert.Equal("Pemeriksaan berhasil angka tidak boleh punya daftar pilihan.", galat.Message);
        Assert.Empty(await context.LabValueBounds.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task VAL28_MengubahBatasKritisLewatPutBiasa_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var dibuat = await service.CreateAsync(Angka(procedureId, LabGenderScope.All, null, 3.5m, 5.1m));

        // Kepala instalasi mencoba menaikkan batas kritis atas dari 6,0 menjadi 8,0 lewat jalur
        // ubah biasa. Inilah skenario yang LAB-DEC-023 ada untuk mencegahnya.
        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.UpdateAsync(dibuat.Id, new UpdateLabValueBoundRequest
            {
                Unit = "mmol/L",
                NormalLow = 3.5m,
                NormalHigh = 5.1m,
                CriticalLow = 2.5m,
                CriticalHigh = 8.0m
            }));

        Assert.Equal("Perubahan batas kritis harus lewat pengajuan yang disetujui pihak klinis.", galat.Message);

        // Ditolak seluruhnya: batas kritis lama tetap berlaku dan tidak ada riwayat yang terbit.
        var sesudah = await service.GetDetailAsync(dibuat.Id);

        Assert.Equal(6.1m, sesudah!.CriticalHigh);
        Assert.Empty(await service.GetHistoryAsync(dibuat.Id));
    }

    [Fact]
    public async Task VAL28_MengubahPenandaPilihanKritisLewatPutBiasa_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var dibuat = await service.CreateAsync(new CreateLabValueBoundRequest
        {
            ProcedureId = procedureId,
            ResultForm = LabResultForm.Choice,
            GenderScope = LabGenderScope.All,
            Options = ProteinUrin()
        });

        // Menurunkan "+3" dari kritis menjadi tidak kritis juga merupakan perubahan batas
        // kritis, walaupun tidak menyentuh satu pun angka.
        var pilihan = ProteinUrin();
        pilihan.Single(x => x.OptionCode == "P3").IsCritical = false;

        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.UpdateAsync(dibuat.Id, new UpdateLabValueBoundRequest { Options = pilihan }));

        Assert.Equal("Perubahan batas kritis harus lewat pengajuan yang disetujui pihak klinis.", galat.Message);

        var sesudah = await service.GetDetailAsync(dibuat.Id);

        Assert.True(sesudah!.Options.Single(x => x.OptionCode == "P3").IsCritical);
    }

    [Fact]
    public async Task VAL28_MenghapusPilihanKritisLewatPutBiasa_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var dibuat = await service.CreateAsync(new CreateLabValueBoundRequest
        {
            ProcedureId = procedureId,
            ResultForm = LabResultForm.Choice,
            GenderScope = LabGenderScope.All,
            Options = ProteinUrin()
        });

        // Membuang "+4" dari daftar sama dengan mencabut salah satu batas kritis.
        var pilihan = ProteinUrin().Where(x => x.OptionCode != "P4").ToList();

        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.UpdateAsync(dibuat.Id, new UpdateLabValueBoundRequest { Options = pilihan }));

        Assert.Equal("Perubahan batas kritis harus lewat pengajuan yang disetujui pihak klinis.", galat.Message);
        Assert.Equal(5, (await service.GetDetailAsync(dibuat.Id))!.Options.Count);
    }

    [Fact]
    public async Task MengubahDaftarPilihanTanpaMenyentuhPenandaKritis_Diterima()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var dibuat = await service.CreateAsync(new CreateLabValueBoundRequest
        {
            ProcedureId = procedureId,
            ResultForm = LabResultForm.Choice,
            GenderScope = LabGenderScope.All,
            Options = ProteinUrin()
        });

        // Mengganti nama tampil satu pilihan bukan perubahan batas kritis, jadi harus diterima.
        var pilihan = ProteinUrin();
        pilihan.Single(x => x.OptionCode == "NEG").OptionName = "Negatif (-)";

        var hasil = await service.UpdateAsync(dibuat.Id, new UpdateLabValueBoundRequest
        {
            Options = pilihan,
            ChangeReason = "Penyeragaman penulisan."
        });

        Assert.Equal("Negatif (-)", hasil.Options.Single(x => x.OptionCode == "NEG").OptionName);
        Assert.Equal(5, hasil.Options.Count);
    }

    // =====================================================================
    // 3. Jalur gagal lain pada keenam endpoint ini
    // =====================================================================

    [Fact]
    public async Task VAL21_KombinasiKelompokPasienYangSudahAda_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, dewasaId, _) = await SeedAsync(context);
        var service = CreateService(context);

        await service.CreateAsync(Angka(procedureId, LabGenderScope.Male, dewasaId, 13.0m, 17.0m));

        var galat = await Assert.ThrowsAsync<LabValueBoundConflictException>(() =>
            service.CreateAsync(Angka(procedureId, LabGenderScope.Male, dewasaId, 13.5m, 17.5m)));

        Assert.Equal(
            "Batas nilai untuk kelompok pasien ini sudah ada. Ubah yang sudah ada, jangan membuat baru.",
            galat.Message);

        Assert.Single(await context.LabValueBounds.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task VAL21_KelompokUmurKosong_JugaDijagaSebagaiSatuKelompok()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        await service.CreateAsync(Angka(procedureId, LabGenderScope.All, null, 3.5m, 5.1m));

        // "Semua umur" adalah satu kelompok pasien yang nyata, bukan ketiadaan kelompok.
        await Assert.ThrowsAsync<LabValueBoundConflictException>(() =>
            service.CreateAsync(Angka(procedureId, LabGenderScope.All, null, 3.6m, 5.2m)));
    }

    [Theory]
    [InlineData(6.0, 5.0, 2.5, 9.0, "Batas normal bawah tidak boleh lebih besar daripada batas atas.")]
    [InlineData(3.5, 5.1, 4.0, 9.0, "Batas kritis bawah harus lebih rendah daripada batas normal bawah.")]
    [InlineData(3.5, 5.1, 2.5, 4.0, "Batas kritis atas harus lebih tinggi daripada batas normal atas.")]
    public async Task VAL25SampaiVAL27_BatasYangTidakMasukAkal_Ditolak(
        double normalLow,
        double normalHigh,
        double criticalLow,
        double criticalHigh,
        string pesan)
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.CreateAsync(new CreateLabValueBoundRequest
            {
                ProcedureId = procedureId,
                ResultForm = LabResultForm.Numeric,
                Unit = "mmol/L",
                GenderScope = LabGenderScope.All,
                NormalLow = (decimal)normalLow,
                NormalHigh = (decimal)normalHigh,
                CriticalLow = (decimal)criticalLow,
                CriticalHigh = (decimal)criticalHigh
            }));

        Assert.Equal(pesan, galat.Message);
    }

    [Fact]
    public async Task VAL29_BatasWaktuCitoNolAtauNegatif_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.CreateAsync(new CreateLabValueBoundRequest
            {
                ProcedureId = procedureId,
                ResultForm = LabResultForm.Numeric,
                Unit = "mmol/L",
                GenderScope = LabGenderScope.All,
                CitoTurnaroundMinutes = 0
            }));

        Assert.Equal("Batas waktu cito harus lebih dari nol menit.", galat.Message);
    }

    [Fact]
    public async Task VAL30_MenonaktifkanBatasAktifTerakhir_Ditolak()
    {
        await using var context = CreateContext();
        var (procedureId, _, _) = await SeedAsync(context);
        var service = CreateService(context);

        var satusatunya = await service.CreateAsync(Angka(procedureId, LabGenderScope.All, null, 3.5m, 5.1m));

        var galat = await Assert.ThrowsAsync<LabValueBoundValidationException>(() =>
            service.DeactivateAsync(satusatunya.Id));

        Assert.Equal(
            "Ini satu-satunya batas nilai untuk pemeriksaan tersebut. Menonaktifkannya membuat hasil tidak dapat dinilai.",
            galat.Message);

        Assert.True((await service.GetDetailAsync(satusatunya.Id))!.IsActive);
    }

    [Fact]
    public async Task MembuatBatasUntukProcedureBukanLaboratorium_Ditolak()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context);

        var bukanLab = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = "OP-001",
            ProcedureName = "Operasi",
            ProcedureType = "Surgery",
            IsLaboratory = false,
            IsActive = true
        };

        context.Set<MstProcedure>().Add(bukanLab);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var galat = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(Angka(bukanLab.Id, LabGenderScope.All, null, 3.5m, 5.1m)));

        Assert.Equal(
            "Procedure tidak ditemukan, tidak aktif, atau bukan procedure laboratorium.",
            galat.Message);
    }

    [Fact]
    public async Task MembacaBatasNilaiYangTidakAda_MenghasilkanKosongDanRiwayatnyaDitolak()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context);

        Assert.Null(await service.GetDetailAsync(Guid.NewGuid()));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetHistoryAsync(Guid.NewGuid()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new UpdateLabValueBoundRequest()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeactivateAsync(Guid.NewGuid()));
    }

    // =====================================================================
    // 4. Kontrak endpoint — DoD BE-LAB-04
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabValueBoundController.GetList), "LabValueBound", "Read", typeof(HttpGetAttribute), null)]
    [InlineData(nameof(LabValueBoundController.GetById), "LabValueBound", "Read", typeof(HttpGetAttribute), "{id:guid}")]
    [InlineData(nameof(LabValueBoundController.Create), "LabValueBound", "Create", typeof(HttpPostAttribute), null)]
    [InlineData(nameof(LabValueBoundController.Update), "LabValueBound", "Update", typeof(HttpPutAttribute), "{id:guid}")]
    [InlineData(nameof(LabValueBoundController.Deactivate), "LabValueBound", "Update", typeof(HttpPutAttribute), "{id:guid}/deactivate")]
    [InlineData(nameof(LabValueBoundController.GetHistory), "LabValueBound", "Read", typeof(HttpGetAttribute), "{id:guid}/history")]
    public void KeenamEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak(
        string methodName,
        string resource,
        string action,
        Type verbAttribute,
        string? template)
    {
        var method = typeof(LabValueBoundController).GetMethod(methodName);

        Assert.NotNull(method);

        // [AccessPermission] inilah yang membuat permissionnya terdaftar sendiri (CAP-14);
        // tanpa atribut ini endpoint akan berjalan tanpa hak akses yang terdaftar.
        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal(resource, arguments[0]);
        Assert.Equal(action, arguments[1]);

        var verb = method.GetCustomAttributes(verbAttribute, inherit: false).SingleOrDefault();

        Assert.NotNull(verb);
        Assert.Equal(template, ((IRouteTemplateProvider)verb!).Template);
    }

    [Fact]
    public void ControllerBatasNilai_MemakaiBaseRouteYangDikunciKontrak()
    {
        var route = typeof(LabValueBoundController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(route);
        Assert.Equal(
            "api/v1/health-services/laboratory-management/lab-value-bounds",
            route!.Template);

        // Enam endpoint, tidak lebih. Grup ini tidak menyediakan satu pun jalur hapus.
        var endpoints = typeof(LabValueBoundController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<AccessPermissionAttribute>().Any())
            .ToList();

        // Enam endpoint pengelolaan dari LAB-API-v1 r3, ditambah filters/metadata dan summary
        // dari amandemen r4 (BE-LAB-17).
        Assert.Equal(8, endpoints.Count);
        Assert.Empty(endpoints.Where(x => x.GetCustomAttributes<HttpDeleteAttribute>().Any()));
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    /// <summary>
    /// Batas berbentuk angka yang isinya masuk akal: batas kritis selalu berada di luar batas
    /// normal, sesuai <c>VAL-26</c> dan <c>VAL-27</c>. Diturunkan dari rentang normalnya supaya
    /// data uji tidak diam-diam melanggar aturan yang justru sedang diuji di tempat lain.
    /// </summary>
    private static CreateLabValueBoundRequest Angka(
        Guid procedureId,
        LabGenderScope genderScope,
        Guid? ageCategoryId,
        decimal normalLow,
        decimal normalHigh) =>
        new()
        {
            ProcedureId = procedureId,
            ResultForm = LabResultForm.Numeric,
            Unit = "mmol/L",
            GenderScope = genderScope,
            AgeCategoryId = ageCategoryId,
            NormalLow = normalLow,
            NormalHigh = normalHigh,
            CriticalLow = normalLow - 1.0m,
            CriticalHigh = normalHigh + 1.0m
        };

    private static List<LabValueOptionRequest> ProteinUrin() =>
        new()
        {
            new LabValueOptionRequest { OptionCode = "NEG", OptionName = "Negatif", SortOrder = 0 },
            new LabValueOptionRequest { OptionCode = "P1", OptionName = "+1", IsOutOfReference = true, SortOrder = 1 },
            new LabValueOptionRequest { OptionCode = "P2", OptionName = "+2", IsOutOfReference = true, SortOrder = 2 },
            new LabValueOptionRequest { OptionCode = "P3", OptionName = "+3", IsOutOfReference = true, IsCritical = true, SortOrder = 3 },
            new LabValueOptionRequest { OptionCode = "P4", OptionName = "+4", IsOutOfReference = true, IsCritical = true, SortOrder = 4 }
        };

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-value-bound-service-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LabValueBoundService CreateService(ApplicationDbContext context)
    {
        var httpContextAccessor = CreateHttpContextAccessor();

        return new LabValueBoundService(
            context,
            httpContextAccessor,
            new LoggerService(NullLogger<LoggerService>.Instance, httpContextAccessor));
    }

    private static IHttpContextAccessor CreateHttpContextAccessor()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, KepalaInstalasi.ToString()) },
            authenticationType: "LabValueBoundServiceTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static async Task<(Guid ProcedureId, Guid DewasaId, Guid AnakId)> SeedAsync(
        ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"LB-{suffix}",
            ProcedureName = "Kalium",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        var dewasa = new MstAgeCategory
        {
            Id = Guid.NewGuid(),
            AgeCategoryCode = $"ADT-{suffix}",
            AgeCategoryName = "Dewasa",
            MinAgeDays = 6570,
            IsActive = true
        };

        var anak = new MstAgeCategory
        {
            Id = Guid.NewGuid(),
            AgeCategoryCode = $"CHD-{suffix}",
            AgeCategoryName = "Anak",
            MinAgeDays = 0,
            MaxAgeDays = 6569,
            IsActive = true
        };

        context.Set<MstProcedure>().Add(procedure);
        context.Set<MstAgeCategory>().AddRange(dewasa, anak);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return (procedure.Id, dewasa.Id, anak.Id);
    }
}
