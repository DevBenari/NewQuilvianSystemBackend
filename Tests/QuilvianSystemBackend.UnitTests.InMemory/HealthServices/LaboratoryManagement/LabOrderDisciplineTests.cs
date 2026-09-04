using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-01</c> — kolom disiplin pada pesanan laboratorium
/// (<c>FR-10.3</c>, <c>LAB-DEC-025</c>, <c>INV-21</c>).
///
/// Yang dibuktikan di sini:
///   1. Pesanan berdisiplin Mikrobiologi menyimpan disiplinnya dan menampilkannya pada
///      respons detail.
///   2. Pemesanan dari kunjungan Rawat Jalan, Rawat Inap, dan IGD berjalan lewat jalur yang
///      sama persis, dan ketiganya mengisi kolom disiplin (<c>AC-11</c>).
///   3. Disiplin tidak dapat berpindah setelah pesanan dibuat.
///   4. Pemanggil lama yang tidak mengirim disiplin tetap dilayani, karena
///      <c>LAB-API-v1</c> r3 mengunci endpoint pembuatan tetap berlaku apa adanya.
///   5. Angka disiplin di luar ketiganya ditolak.
///   6. Kolom disiplin terpetakan sebagai enum <c>int</c>, ber-index, dan berperilaku
///      tolak-ubah pada model EF.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini dapat dijalankan tanpa database mana pun.
/// Konsekuensinya foreign key dan index fisik tidak ikut diuji di sini; keduanya menjadi
/// bagian verifikasi migration yang merupakan wewenang terpisah dan dicatat pada laporan
/// task.
/// </remarks>
public class LabOrderDisciplineTests
{
    private static readonly Guid ActorUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    // =====================================================================
    // 1. Pesanan berdisiplin Mikrobiologi
    // =====================================================================

    [Fact]
    public async Task MembuatPesananMikrobiologi_MengisiDisiplinPadaResponsDetail()
    {
        await using var context = CreateContext();
        var (encounterId, procedureId) = await SeedAsync(context, EncounterType.Outpatient);
        var service = CreateOrderService(context);

        var dibuat = await service.CreateAsync(new CreateLabOrderRequest
        {
            EncounterId = encounterId,
            ProcedureId = procedureId,
            Discipline = LabDiscipline.Microbiology
        });

        Assert.Equal(nameof(LabDiscipline.Microbiology), dibuat.Discipline);

        // Respons detail yang dibaca ulang dari penyimpanan, bukan hanya objek hasil create.
        var detail = await service.GetDetailAsync(dibuat.Id);

        Assert.NotNull(detail);
        Assert.Equal(nameof(LabDiscipline.Microbiology), detail!.Discipline);

        var tersimpan = await context.LabOrders.AsNoTracking().FirstAsync(x => x.Id == dibuat.Id);

        Assert.Equal(LabDiscipline.Microbiology, tersimpan.Discipline);
    }

    // =====================================================================
    // 2. AC-11 — tiga jenis kunjungan, satu jalur kerja
    // =====================================================================

    [Theory]
    [InlineData(EncounterType.Outpatient, LabDiscipline.ClinicalPathology)]
    [InlineData(EncounterType.Inpatient, LabDiscipline.AnatomicalPathology)]
    [InlineData(EncounterType.Emergency, LabDiscipline.Microbiology)]
    public async Task MembuatPesananDariTigaJenisKunjungan_BerjalanSamaDanMengisiDisiplin(
        EncounterType jenisKunjungan,
        LabDiscipline disiplin)
    {
        await using var context = CreateContext();
        var (encounterId, procedureId) = await SeedAsync(context, jenisKunjungan);
        var service = CreateOrderService(context);

        var dibuat = await service.CreateAsync(new CreateLabOrderRequest
        {
            EncounterId = encounterId,
            ProcedureId = procedureId,
            Discipline = disiplin
        });

        // Hasilnya sama untuk ketiganya: status awal Requested, disiplin terisi, dan tidak ada
        // cabang khusus per jenis kunjungan yang menghasilkan bentuk respons berbeda.
        Assert.Equal(nameof(LabOrderStatus.Requested), dibuat.OrderStatus);
        Assert.Equal(disiplin.ToString(), dibuat.Discipline);
        Assert.Equal(encounterId, dibuat.EncounterId);

        var riwayat = await context.LabTransitionHistories
            .AsNoTracking()
            .Where(x => x.LabOrderId == dibuat.Id)
            .ToListAsync();

        var baris = Assert.Single(riwayat);
        Assert.Equal("Order.Request", baris.Action);
        Assert.Equal(ActorUserId, baris.ActorUserId);
    }

    // =====================================================================
    // 3. INV-21 — disiplin tidak berpindah setelah pesanan dibuat
    // =====================================================================

    [Fact]
    public async Task MengubahDisiplinSetelahPesananDibuat_Ditolak()
    {
        await using var context = CreateContext();
        var (encounterId, procedureId) = await SeedAsync(context, EncounterType.Outpatient);
        var service = CreateOrderService(context);

        var dibuat = await service.CreateAsync(new CreateLabOrderRequest
        {
            EncounterId = encounterId,
            ProcedureId = procedureId,
            Discipline = LabDiscipline.Microbiology
        });

        // Jalur ubah langsung ke entity — persis yang akan ditempuh seseorang bila kelak
        // menulis endpoint ubah disiplin tanpa membaca INV-21.
        var terlacak = await context.LabOrders.FirstAsync(x => x.Id == dibuat.Id);
        terlacak.Discipline = LabDiscipline.ClinicalPathology;

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());

        // Nilai yang tersimpan tidak ikut berubah.
        context.ChangeTracker.Clear();

        var sesudah = await context.LabOrders.AsNoTracking().FirstAsync(x => x.Id == dibuat.Id);

        Assert.Equal(LabDiscipline.Microbiology, sesudah.Discipline);
    }

    // =====================================================================
    // 4. Pemanggil lama tanpa disiplin tetap dilayani
    // =====================================================================

    [Fact]
    public async Task MembuatPesananTanpaDisiplin_TetapBerhasilDanDisiplinKosong()
    {
        await using var context = CreateContext();
        var (encounterId, procedureId) = await SeedAsync(context, EncounterType.Outpatient);
        var service = CreateOrderService(context);

        var dibuat = await service.CreateAsync(new CreateLabOrderRequest
        {
            EncounterId = encounterId,
            ProcedureId = procedureId
        });

        Assert.Null(dibuat.Discipline);
        Assert.Equal(nameof(LabOrderStatus.Requested), dibuat.OrderStatus);

        var detail = await service.GetDetailAsync(dibuat.Id);

        Assert.NotNull(detail);
        Assert.Null(detail!.Discipline);
    }

    // =====================================================================
    // 5. Disiplin di luar ketiganya ditolak
    // =====================================================================

    [Fact]
    public async Task MembuatPesananDenganDisiplinTidakDikenal_Ditolak()
    {
        await using var context = CreateContext();
        var (encounterId, procedureId) = await SeedAsync(context, EncounterType.Outpatient);
        var service = CreateOrderService(context);

        var galat = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(
            new CreateLabOrderRequest
            {
                EncounterId = encounterId,
                ProcedureId = procedureId,
                Discipline = (LabDiscipline)99
            }));

        Assert.Equal("Disiplin laboratorium tidak dikenal.", galat.Message);

        // Penolakan terjadi sebelum satu baris pun tersimpan.
        Assert.Empty(await context.LabOrders.AsNoTracking().ToListAsync());
    }

    // =====================================================================
    // 6. Pemetaan model
    // =====================================================================

    [Fact]
    public void Discipline_TerpetakanSebagaiEnumIntBerIndexDanTolakUbah()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(LabOrder));

        Assert.NotNull(entityType);

        var property = entityType!.FindProperty(nameof(LabOrder.Discipline));

        Assert.NotNull(property);
        Assert.True(property!.IsNullable);
        Assert.Equal(typeof(int), property.GetProviderClrType());
        Assert.Equal(PropertySaveBehavior.Throw, property.GetAfterSaveBehavior());

        var index = entityType.GetIndexes()
            .SingleOrDefault(x =>
                x.Properties.Count == 1 &&
                x.Properties[0].Name == nameof(LabOrder.Discipline));

        Assert.NotNull(index);
    }

    [Fact]
    public void LabDiscipline_MemuatTepatTigaDisiplinTanpaBankDarah()
    {
        var nilai = Enum.GetValues<LabDiscipline>();

        Assert.Equal(3, nilai.Length);
        Assert.Equal(1, (int)LabDiscipline.ClinicalPathology);
        Assert.Equal(2, (int)LabDiscipline.AnatomicalPathology);
        Assert.Equal(3, (int)LabDiscipline.Microbiology);

        Assert.DoesNotContain(
            Enum.GetNames<LabDiscipline>(),
            nama => nama.Contains("Blood", StringComparison.OrdinalIgnoreCase));
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-order-discipline-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<(Guid EncounterId, Guid ProcedureId)> SeedAsync(
        ApplicationDbContext context,
        EncounterType encounterType)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var encounter = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{suffix}",
            PatientId = Guid.NewGuid(),
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = encounterType,
            EncounterDate = DateTime.UtcNow
        };

        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"LB-{suffix}",
            ProcedureName = "Kultur Darah",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        context.Set<TrxPatientEncounter>().Add(encounter);
        context.Set<MstProcedure>().Add(procedure);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return (encounter.Id, procedure.Id);
    }

    private static LabOrderService CreateOrderService(ApplicationDbContext context)
    {
        var httpContextAccessor = CreateHttpContextAccessor();
        var loggerService = new LoggerService(
            NullLogger<LoggerService>.Instance,
            httpContextAccessor);

        var specimenService = new LabSpecimenService(
            context,
            new ClinicalMilestoneFactProducer(
                context,
                new BillingFolioService(context),
                loggerService),
            httpContextAccessor,
            loggerService);

        return new LabOrderService(context, specimenService, httpContextAccessor, loggerService);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, ActorUserId.ToString()) },
            authenticationType: "LabOrderDisciplineTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
