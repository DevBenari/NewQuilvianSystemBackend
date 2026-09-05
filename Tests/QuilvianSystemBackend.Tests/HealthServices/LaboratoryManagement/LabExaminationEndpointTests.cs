using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-16</c> — endpoint pemeriksaan terpesan
/// (<c>FR-02.1</c>, <c>FR-02.2</c>; <c>LAB-DEC-024</c>, <c>LAB-DEC-026</c>).
///
/// Yang dibuktikan di sini:
///   1. keempat endpoint punya route, verb, dan hak akses yang dikunci <c>LAB-API-v1</c> r3;
///   2. <c>AC-35</c> — satu wadah menopang dua pemeriksaan, dan keduanya terbaca lewat
///      <c>GET /by-specimen/{specimenId}</c>;
///   3. keempat aturan validasi milik task ini: <c>VAL-17</c>, <c>VAL-18</c>, <c>VAL-19</c>,
///      dan <c>VAL-20</c>;
///   4. keunikan wadah dan jenis pemeriksaan ditegakkan pada jalur tambah;
///   5. <b>batas terpenting</b> — membatalkan satu pemeriksaan tidak mengubah status
///      pemeriksaan lain pada wadah yang sama, dan tidak menyentuh status wadahnya.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini berjalan tanpa database mana pun. Konsekuensinya
/// index unik fisik tidak ditegakkan di sini, sehingga keunikan yang diuji adalah pemeriksaan
/// di service; penjaga terakhirnya di database sudah dibuktikan pada laporan <c>BE-LAB-09</c>.
///
/// Data uji selalu menyimpan <c>MstProcedure</c> dan <c>MstTariff</c> yang sungguhan. Itu bukan
/// kerapian belaka: proyeksi daftar menyentuh navigasi, dan relasi wajib yang principal-nya
/// tidak ada membuat barisnya tidak ikut terbawa sama sekali.
/// </remarks>
public class LabExaminationEndpointTests
{
    private static readonly Guid Petugas = Guid.Parse("88888888-8888-8888-8888-888888888888");

    // =====================================================================
    // 1. Bentuk kontrak
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabExaminationController.GetByOrder), "Read", typeof(HttpGetAttribute), "by-order/{labOrderId:guid}")]
    [InlineData(nameof(LabExaminationController.GetBySpecimen), "Read", typeof(HttpGetAttribute), "by-specimen/{specimenId:guid}")]
    [InlineData(nameof(LabExaminationController.Add), "Create", typeof(HttpPostAttribute), "by-order/{labOrderId:guid}")]
    [InlineData(nameof(LabExaminationController.Cancel), "Update", typeof(HttpPostAttribute), "{id:guid}/cancel")]
    public void KeempatEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak(
        string methodName,
        string action,
        Type verbAttribute,
        string template)
    {
        var method = typeof(LabExaminationController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabExamination", arguments[0]);
        Assert.Equal(action, arguments[1]);

        var verb = method.GetCustomAttributes(verbAttribute, inherit: false).SingleOrDefault();

        Assert.NotNull(verb);
        Assert.Equal(template, ((IRouteTemplateProvider)verb!).Template);
    }

    [Fact]
    public void ControllerPemeriksaan_MemakaiBaseRouteYangDikunciKontrak()
    {
        var route = typeof(LabExaminationController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(route);
        Assert.Equal(
            "api/v1/health-services/laboratory-management/lab-examinations",
            route!.Template);

        var endpoints = typeof(LabExaminationController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<AccessPermissionAttribute>().Any())
            .ToList();

        // Empat endpoint milik BE-LAB-16, ditambah PUT /{id}/urgency dan PUT /{id}/duplo yang
        // dibangun BE-LAB-10. Angka ini adalah kawat pemicu: menambah endpoint pada grup ini
        // tanpa memperbaruinya akan membuat uji ini gagal, dan itu memang yang diinginkan.
        Assert.Equal(6, endpoints.Count);
    }

    /// <summary>
    /// Grup ini tidak punya jalur hapus. Pemeriksaan yang pernah dipesan menempel pada jejak
    /// tagihan; ia dibatalkan, bukan dihapus.
    /// </summary>
    [Fact]
    public void ControllerPemeriksaan_TidakMemilikiJalurHapus()
    {
        var deletes = typeof(LabExaminationController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpDeleteAttribute>().Any())
            .ToList();

        Assert.Empty(deletes);
    }

    // =====================================================================
    // 2. AC-35 — satu wadah menopang dua pemeriksaan
    // =====================================================================

    [Fact]
    public async Task AC35_SatuWadahMenopangDuaPemeriksaan_KeduanyaTerbacaLewatBySpecimen()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context);

        await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Hemoglobin
        });

        await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Leukosit
        });

        var padaWadah = await service.GetBySpecimenAsync(dunia.SpecimenId);
        var padaPesanan = await service.GetByOrderAsync(dunia.OrderId);

        Assert.Equal(2, padaWadah.Count);
        Assert.Equal(2, padaPesanan.Count);

        // Satu wadah, satu barcode, dua pemeriksaan dengan harga masing-masing.
        Assert.All(padaWadah, x => Assert.Equal("BC-0001", x.SpecimenBarcode));
        Assert.Equal(new decimal?[] { 35_000m, 30_000m }, padaWadah.Select(x => x.UnitPrice).ToArray());
        Assert.All(padaWadah, x => Assert.Equal(nameof(LabExaminationStatus.Ordered), x.ExaminationStatus));
        Assert.All(padaWadah, x => Assert.Equal(nameof(LabExaminationUrgency.Routine), x.Urgency));
        Assert.All(padaWadah, x => Assert.False(x.IsDuplo));
    }

    [Fact]
    public async Task MenambahPemeriksaan_MenyalinTarifDariDataInduk()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var hasil = await CreateService(context).AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Hemoglobin
        });

        // Harga tidak pernah dikirim pemanggil; backend menyalinnya dari tarif yang berlaku.
        Assert.Equal(35_000m, hasil.UnitPrice);
        Assert.Equal("TRF-HB", hasil.TariffCode);
        Assert.NotNull(hasil.TariffId);
        Assert.Equal("Hemoglobin", hasil.ProcedureName);
        Assert.Null(hasil.ChargeEligibleAt);
    }

    [Fact]
    public async Task PermintaanTambah_TidakPunyaRuasHargaMaupunKesegeraan()
    {
        var properties = typeof(AddLabExaminationRequest)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        // Harga milik data induk, kesegeraan dan duplo milik BE-LAB-10. Ketiganya tidak boleh
        // dapat diselipkan lewat jalur tambah.
        Assert.Equal(new[] { "SpecimenId", "ProcedureId" }, properties);

        await Task.CompletedTask;
    }

    // =====================================================================
    // 3. VAL-17 sampai VAL-20
    // =====================================================================

    [Fact]
    public async Task VAL17_JenisPemeriksaanBukanLaboratorium_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var galat = await Assert.ThrowsAsync<LabExaminationValidationException>(() =>
            CreateService(context).AddAsync(dunia.OrderId, new AddLabExaminationRequest
            {
                SpecimenId = dunia.SpecimenId,
                ProcedureId = dunia.Radiologi
            }));

        Assert.Equal("Tindakan yang dipilih bukan pemeriksaan laboratorium.", galat.Message);
    }

    [Theory]
    [InlineData(LabSpecimenStatus.Accepted)]
    [InlineData(LabSpecimenStatus.Rejected)]
    public async Task VAL18_WadahYangSudahDiputuskan_TidakDapatBertambahIsinya(LabSpecimenStatus status)
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await context.LabSpecimens.SingleAsync(x => x.Id == dunia.SpecimenId);
        wadah.SpecimenStatus = status;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var galat = await Assert.ThrowsAsync<LabExaminationConflictException>(() =>
            CreateService(context).AddAsync(dunia.OrderId, new AddLabExaminationRequest
            {
                SpecimenId = dunia.SpecimenId,
                ProcedureId = dunia.Hemoglobin
            }));

        Assert.Equal(
            "Wadah ini sudah diputuskan, pemeriksaan baru tidak dapat ditambahkan ke wadah tersebut.",
            galat.Message);
    }

    [Fact]
    public async Task VAL19_PemeriksaanYangSudahGugurBersamaWadah_TidakDapatDibatalkan()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context);

        var pemeriksaan = await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Hemoglobin
        });

        // Wadahnya ditolak, sehingga isinya gugur. Penggugurannya sendiri adalah pekerjaan
        // BE-LAB-12; di sini keadaannya disiapkan langsung.
        var gugur = await context.LabExaminations.SingleAsync(x => x.Id == pemeriksaan.Id);
        gugur.ExaminationStatus = LabExaminationStatus.Voided;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var galat = await Assert.ThrowsAsync<LabExaminationConflictException>(() =>
            CreateService(context).CancelAsync(pemeriksaan.Id, new CancelLabExaminationRequest()));

        Assert.Equal("Pemeriksaan ini sudah gugur karena wadahnya ditolak.", galat.Message);
    }

    [Fact]
    public async Task VAL20_TarifBelumDiatur_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var galat = await Assert.ThrowsAsync<LabExaminationValidationException>(() =>
            CreateService(context).AddAsync(dunia.OrderId, new AddLabExaminationRequest
            {
                SpecimenId = dunia.SpecimenId,
                ProcedureId = dunia.TanpaTarif
            }));

        Assert.Equal(
            "Tarif untuk pemeriksaan ini belum diatur. Hubungi bagian data induk.",
            galat.Message);

        // Tidak ada baris yang terlanjur tersimpan tanpa harga.
        Assert.Equal(0, await context.LabExaminations.CountAsync());
    }

    // =====================================================================
    // 4. Keunikan wadah dan jenis pemeriksaan
    // =====================================================================

    [Fact]
    public async Task JenisPemeriksaanYangSama_TidakBolehDuaKaliPadaSatuWadah()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context);

        await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Hemoglobin
        });

        var galat = await Assert.ThrowsAsync<LabExaminationConflictException>(() =>
            service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
            {
                SpecimenId = dunia.SpecimenId,
                ProcedureId = dunia.Hemoglobin
            }));

        Assert.Equal(
            "Pemeriksaan yang sama tidak boleh dimasukkan dua kali dalam satu wadah.",
            galat.Message);

        Assert.Equal(1, await context.LabExaminations.CountAsync());
    }

    /// <summary>
    /// Jenis pemeriksaan yang sama boleh berdiri pada <b>wadah yang berbeda</b> — misalnya
    /// pemeriksaan ulang dari tabung kedua. Keunikannya per wadah, bukan per pesanan.
    /// </summary>
    [Fact]
    public async Task JenisPemeriksaanYangSama_BolehPadaWadahYangBerbeda()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context);

        await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Hemoglobin
        });

        var hasil = await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenKedua,
            ProcedureId = dunia.Hemoglobin
        });

        Assert.Equal(dunia.SpecimenKedua, hasil.SpecimenId);
        Assert.Equal(2, await context.LabExaminations.CountAsync());
    }

    [Fact]
    public async Task WadahMilikPesananLain_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var galat = await Assert.ThrowsAsync<LabExaminationValidationException>(() =>
            CreateService(context).AddAsync(dunia.OrderKeduaId, new AddLabExaminationRequest
            {
                SpecimenId = dunia.SpecimenId,
                ProcedureId = dunia.Hemoglobin
            }));

        Assert.Equal("Wadah yang dipilih bukan milik pesanan ini.", galat.Message);
    }

    // =====================================================================
    // 5. Pembatalan satu pemeriksaan — batas terpenting task ini
    // =====================================================================

    /// <summary>
    /// Inti butir DoD: membatalkan satu pemeriksaan <b>tidak</b> mengubah pemeriksaan lain pada
    /// wadah yang sama, dan <b>tidak</b> menyentuh status wadahnya. Menggugurkan seluruh isi
    /// wadah adalah akibat penolakan wadah, dan itu pekerjaan <c>BE-LAB-12</c> — mencampur
    /// keduanya melanggar <c>VAL-13</c>.
    /// </summary>
    [Fact]
    public async Task MembatalkanSatuPemeriksaan_TidakMengubahPemeriksaanLainMaupunWadahnya()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context);

        var dibatalkan = await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Hemoglobin
        });

        var tetangga = await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Leukosit
        });

        var hasil = await service.CancelAsync(
            dibatalkan.Id,
            new CancelLabExaminationRequest { Reason = "Dokter mencabut permintaan hemoglobin." });

        Assert.Equal(nameof(LabExaminationStatus.Cancelled), hasil.ExaminationStatus);

        context.ChangeTracker.Clear();

        // Pemeriksaan tetangga tidak bergeser sedikit pun.
        var sesudah = await context.LabExaminations
            .AsNoTracking()
            .SingleAsync(x => x.Id == tetangga.Id);

        Assert.Equal(LabExaminationStatus.Ordered, sesudah.ExaminationStatus);
        Assert.False(sesudah.IsCancel);

        // Wadahnya juga tidak berubah statusnya.
        var wadah = await context.LabSpecimens
            .AsNoTracking()
            .SingleAsync(x => x.Id == dunia.SpecimenId);

        Assert.Equal(LabSpecimenStatus.Planned, wadah.SpecimenStatus);
    }

    [Fact]
    public async Task MembatalkanPemeriksaan_MencatatJejakPembatalanDanMenaikkanTokenKonkurensi()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context);

        var pemeriksaan = await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Hemoglobin
        });

        Assert.Equal(0, pemeriksaan.Version);

        var hasil = await service.CancelAsync(pemeriksaan.Id, new CancelLabExaminationRequest());

        Assert.Equal(1, hasil.Version);

        context.ChangeTracker.Clear();

        var tersimpan = await context.LabExaminations
            .AsNoTracking()
            .SingleAsync(x => x.Id == pemeriksaan.Id);

        Assert.True(tersimpan.IsCancel);
        Assert.NotNull(tersimpan.CancelDateTime);
        Assert.Equal(Petugas, tersimpan.CancelBy);
        Assert.Equal(Petugas, tersimpan.UpdateBy);
    }

    [Fact]
    public async Task MembatalkanPemeriksaanDuaKali_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context);

        var pemeriksaan = await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = dunia.SpecimenId,
            ProcedureId = dunia.Hemoglobin
        });

        await service.CancelAsync(pemeriksaan.Id, new CancelLabExaminationRequest());

        var galat = await Assert.ThrowsAsync<LabExaminationConflictException>(() =>
            service.CancelAsync(pemeriksaan.Id, new CancelLabExaminationRequest()));

        Assert.Equal("Pemeriksaan ini sudah dibatalkan.", galat.Message);
    }

    // =====================================================================
    // 6. Jalur tidak ditemukan
    // =====================================================================

    [Fact]
    public async Task PesananWadahDanPemeriksaanYangTidakAda_Ditolak()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByOrderAsync(Guid.NewGuid()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetBySpecimenAsync(Guid.NewGuid()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CancelAsync(Guid.NewGuid(), new CancelLabExaminationRequest()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.AddAsync(Guid.NewGuid(), new AddLabExaminationRequest
            {
                SpecimenId = dunia.SpecimenId,
                ProcedureId = dunia.Hemoglobin
            }));
    }

    [Fact]
    public async Task PesananYangSudahDibatalkan_TidakDapatMenerimaPemeriksaanBaru()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var pesanan = await context.LabOrders.SingleAsync(x => x.Id == dunia.OrderId);
        pesanan.OrderStatus = LabOrderStatus.Cancelled;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<LabExaminationConflictException>(() =>
            CreateService(context).AddAsync(dunia.OrderId, new AddLabExaminationRequest
            {
                SpecimenId = dunia.SpecimenId,
                ProcedureId = dunia.Hemoglobin
            }));
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private sealed record Dunia(
        Guid OrderId,
        Guid OrderKeduaId,
        Guid SpecimenId,
        Guid SpecimenKedua,
        Guid Hemoglobin,
        Guid Leukosit,
        Guid Radiologi,
        Guid TanpaTarif);

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-examination-endpoint-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LabExaminationService CreateService(ApplicationDbContext context)
    {
        var accessor = CreateHttpContextAccessor();

        return new LabExaminationService(
            context,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static IHttpContextAccessor CreateHttpContextAccessor()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, Petugas.ToString()) },
            authenticationType: "LabExaminationEndpointTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    /// <summary>
    /// Dua pesanan, dua wadah pada pesanan pertama, dan empat jenis pemeriksaan: dua
    /// laboratorium bertarif, satu radiologi, dan satu laboratorium yang tarifnya belum diatur.
    /// </summary>
    private static async Task<Dunia> SeedAsync(ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var hemoglobin = Procedure($"HB-{suffix}", "Hemoglobin", isLab: true);
        var leukosit = Procedure($"WBC-{suffix}", "Leukosit", isLab: true);
        var radiologi = Procedure($"RAD-{suffix}", "Foto Toraks", isLab: false);
        var tanpaTarif = Procedure($"NOP-{suffix}", "Pemeriksaan Tanpa Tarif", isLab: true);

        context.Set<MstProcedure>().AddRange(hemoglobin, leukosit, radiologi, tanpaTarif);

        context.Set<MstTariff>().AddRange(
            Tarif(hemoglobin.Id, "TRF-HB", 35_000m),
            Tarif(leukosit.Id, "TRF-WBC", 30_000m),
            Tarif(radiologi.Id, "TRF-RAD", 150_000m));

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = hemoglobin.Id,
            Discipline = LabDiscipline.ClinicalPathology,
            OrderStatus = LabOrderStatus.Requested
        };

        var orderKedua = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = hemoglobin.Id,
            Discipline = LabDiscipline.ClinicalPathology,
            OrderStatus = LabOrderStatus.Requested
        };

        var wadah = Wadah(order.Id, "BC-0001", 1);
        var wadahKedua = Wadah(order.Id, "BC-0002", 2);

        context.LabOrders.AddRange(order, orderKedua);
        context.LabSpecimens.AddRange(wadah, wadahKedua);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return new Dunia(
            order.Id, orderKedua.Id, wadah.Id, wadahKedua.Id,
            hemoglobin.Id, leukosit.Id, radiologi.Id, tanpaTarif.Id);
    }

    private static MstProcedure Procedure(string kode, string nama, bool isLab) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProcedureCode = kode,
            ProcedureName = nama,
            ProcedureType = isLab ? "Laboratory" : "Radiology",
            IsLaboratory = isLab,
            IsRadiology = !isLab,
            IsActive = true
        };

    private static MstTariff Tarif(Guid procedureId, string kode, decimal harga) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProcedureId = procedureId,
            TariffCode = kode,
            NormalPrice = harga
        };

    private static LabSpecimen Wadah(Guid labOrderId, string barcode, int urutan) =>
        new()
        {
            Id = Guid.NewGuid(),
            LabOrderId = labOrderId,
            SpecimenBarcode = barcode,
            SpecimenSequence = urutan,
            SpecimenStatus = LabSpecimenStatus.Planned
        };
}
