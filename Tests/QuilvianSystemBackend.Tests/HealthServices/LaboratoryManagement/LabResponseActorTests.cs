using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Dua lubang kontrak: respons Laboratorium membawa penunjuk pelaku tanpa pernah membawa
/// namanya, padahal penunjuk tidak boleh ditampilkan.
///
/// <para>
/// <b>1. Pesanan tidak menyebut pemesannya.</b> <c>LabOrder.RequestedByUserId</c> tersimpan
/// sejak pesanan dibuat, tetapi <c>LabOrderDetailResponse</c> tidak pernah menerbitkannya.
/// Layar karena itu tidak punya cara mengetahui siapa pemesannya, sehingga tombol Tandai Cito
/// tampil kepada setiap dokter dan baru ditolak <c>403</c> sesudah ditekan — <c>VAL-03</c>
/// ditegakkan, tetapi hanya sesudah petugas menekannya.
/// </para>
///
/// <para>
/// <b>2. Riwayat batas nilai tidak menyebut pelakunya.</b> <c>AC-34</c> menuntut riwayat
/// menyebut siapa yang mengubah, tetapi responsnya hanya membawa <c>Guid</c>. Karena penunjuk
/// tidak boleh ditampilkan, kolom pelaku pada layar terpaksa kosong.
/// </para>
/// </summary>
public class LabResponseActorTests
{
    private static readonly Guid Pemesan = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Penyetuju = Guid.Parse("33333333-3333-3333-3333-333333333333");

    // =====================================================================
    // 1. Pesanan menyebut pemesannya
    // =====================================================================

    [Fact]
    public async Task DetailPesanan_MembawaPenunjukDanNamaPemesannya()
    {
        await using var context = CreateContext();

        await SeedUserAsync(context, Pemesan, "dr. Sekar Ayu");
        var procedureId = await SeedProcedureAsync(context);
        var orderId = await SeedOrderAsync(context, procedureId, Pemesan);

        var detail = await CreateOrderService(context).GetDetailAsync(orderId);

        Assert.NotNull(detail);
        Assert.Equal(Pemesan, detail!.RequestedByUserId);

        // Namanya yang dibaca petugas; penunjuknya hanya untuk perbandingan di layar.
        Assert.Equal("dr. Sekar Ayu", detail.RequestedByName);
    }

    [Fact]
    public async Task DetailPesanan_MembuatVal03DapatDitegakkanSebelumTombolDitekan()
    {
        await using var context = CreateContext();

        var dokterLain = Guid.Parse("22222222-2222-2222-2222-222222222222");

        await SeedUserAsync(context, Pemesan, "dr. Sekar Ayu");
        await SeedUserAsync(context, dokterLain, "dr. Bagas Nur");
        var procedureId = await SeedProcedureAsync(context);
        var orderId = await SeedOrderAsync(context, procedureId, Pemesan);

        var detail = await CreateOrderService(context).GetDetailAsync(orderId);

        // Inilah perbandingan yang sebelumnya tidak mungkin dilakukan layar.
        Assert.True(detail!.RequestedByUserId == Pemesan);
        Assert.False(detail.RequestedByUserId == dokterLain);
    }

    [Fact]
    public async Task DetailPesanan_PemesanKosongTidakMenjadiKesalahan()
    {
        await using var context = CreateContext();

        // Pesanan peninggalan sebelum kolom pemesan diisi. Layar menampilkannya apa adanya,
        // bukan menolak memuat halamannya.
        var procedureId = await SeedProcedureAsync(context);
        var orderId = await SeedOrderAsync(context, procedureId, requestedBy: null);

        var detail = await CreateOrderService(context).GetDetailAsync(orderId);

        Assert.NotNull(detail);
        Assert.Null(detail!.RequestedByUserId);
        Assert.Null(detail.RequestedByName);
    }

    // =====================================================================
    // 2. Riwayat batas nilai menyebut pelakunya
    // =====================================================================

    [Fact]
    public async Task RiwayatBatasNilai_MembawaNamaPelakuDanPenyetujunya()
    {
        await using var context = CreateContext();

        await SeedUserAsync(context, Pemesan, "Analis Rina");
        await SeedUserAsync(context, Penyetuju, "dr. Hadi Santoso, Sp.PK");

        var boundId = await SeedValueBoundAsync(context);

        context.LabValueBoundHistories.AddRange(
            Riwayat(boundId, "NormalHigh", "15", "16", Pemesan, approvedBy: null),
            Riwayat(boundId, "CriticalHigh", "20", "22", Pemesan, approvedBy: Penyetuju));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var riwayat = await CreateValueBoundService(context).GetHistoryAsync(boundId);

        Assert.Equal(2, riwayat.Count);
        Assert.All(riwayat, x => Assert.Equal("Analis Rina", x.ActorUserName));

        var batasKritis = riwayat.Single(x => x.ChangedField == "CriticalHigh");
        Assert.Equal("dr. Hadi Santoso, Sp.PK", batasKritis.ApprovedByUserName);

        // Perubahan batas normal tidak melewati persetujuan, jadi penyetujunya memang kosong.
        var batasNormal = riwayat.Single(x => x.ChangedField == "NormalHigh");
        Assert.Null(batasNormal.ApprovedByUserId);
        Assert.Null(batasNormal.ApprovedByUserName);
    }

    [Fact]
    public async Task RiwayatBatasNilai_PelakuYangSudahTidakAdaTidakMenggagalkanPembacaan()
    {
        await using var context = CreateContext();

        var boundId = await SeedValueBoundAsync(context);

        // Penggunanya tidak pernah di-seed: meniru akun yang sudah dihapus. Riwayatnya tetap
        // wajib terbaca — jejak audit tidak boleh hilang hanya karena pelakunya sudah pergi.
        context.LabValueBoundHistories.Add(
            Riwayat(boundId, "NormalLow", "4", "5", Pemesan, approvedBy: null));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var riwayat = await CreateValueBoundService(context).GetHistoryAsync(boundId);

        var baris = Assert.Single(riwayat);
        Assert.Equal(Pemesan, baris.ActorUserId);
        Assert.Null(baris.ActorUserName);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static LabValueBoundHistory Riwayat(
        Guid boundId,
        string ruas,
        string lama,
        string baru,
        Guid pelaku,
        Guid? approvedBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            ValueBoundId = boundId,
            ChangedField = ruas,
            OldValue = lama,
            NewValue = baru,
            ActorUserId = pelaku,
            ApprovedByUserId = approvedBy,
            OccurredAt = DateTime.UtcNow,
        };

    private static async Task SeedUserAsync(ApplicationDbContext context, Guid id, string nama)
    {
        context.Users.Add(new ApplicationUser
        {
            Id = id,
            UserName = $"user-{id:N}",
            NormalizedUserName = $"USER-{id:N}",
            UserCode = $"U-{id.ToString()[..4]}",
            DisplayName = nama,
            IsActive = true,
            SecurityStamp = Guid.NewGuid().ToString("N"),
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private static async Task<Guid> SeedProcedureAsync(ApplicationDbContext context)
    {
        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = "LAB-HB",
            ProcedureName = "Hemoglobin",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true,
        };

        context.Set<MstProcedure>().Add(procedure);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return procedure.Id;
    }

    private static async Task<Guid> SeedOrderAsync(
        ApplicationDbContext context,
        Guid procedureId,
        Guid? requestedBy)
    {
        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = procedureId,
            OrderStatus = LabOrderStatus.Requested,
            RequestedAt = DateTime.UtcNow,
            RequestedByUserId = requestedBy,
            CreateDateTime = DateTime.UtcNow,
        };

        context.LabOrders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return order.Id;
    }

    private static async Task<Guid> SeedValueBoundAsync(ApplicationDbContext context)
    {
        var bound = new LabValueBound
        {
            Id = Guid.NewGuid(),
            ProcedureId = Guid.NewGuid(),
            ResultForm = LabResultForm.Numeric,
            Unit = "g/dL",
            IsActive = true,
        };

        context.LabValueBounds.Add(bound);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return bound.Id;
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-response-actor-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor()
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, Pemesan.ToString()) },
            authenticationType: "LabResponseActorTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static LoggerService CreateLoggerService(IHttpContextAccessor accessor) =>
        new(NullLogger<LoggerService>.Instance, accessor);

    private static LabValueBoundService CreateValueBoundService(ApplicationDbContext context)
    {
        var accessor = CreateHttpContextAccessor();

        return new LabValueBoundService(context, accessor, CreateLoggerService(accessor));
    }

    private static LabOrderService CreateOrderService(ApplicationDbContext context)
    {
        var accessor = CreateHttpContextAccessor();
        var loggerService = CreateLoggerService(accessor);

        var specimenService = new LabSpecimenService(
            context,
            new ClinicalMilestoneFactProducer(
                context,
                new BillingFolioService(context),
                loggerService),
            accessor,
            loggerService);

        return new LabOrderService(context, specimenService, accessor, loggerService);
    }
}
