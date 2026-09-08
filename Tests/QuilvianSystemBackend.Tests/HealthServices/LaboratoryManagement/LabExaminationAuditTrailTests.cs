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
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Jejak audit pemeriksaan terpesan — <c>contracts/permission-audit-matrix.md</c> bagian 4.
///
/// Matriks itu menuntut enam kejadian meninggalkan satu baris permanen pada
/// <c>LabTransitionHistory</c> berlingkup <c>LabExamination</c>: <c>Examination.Add</c>,
/// <c>Examination.ChargeEligible</c>, <c>Examination.Void</c>, <c>Examination.Cancel</c>,
/// <c>Examination.SetUrgency</c>, dan <c>Examination.SetDuplo</c>.
///
/// Keempat yang pertama tidak pernah ditulis sampai 2026-09-04 — kolom penunjuknya dan nilai
/// enum lingkupnya baru ada sejak <c>BE-LAB-10</c>. Uji di sini menutup lubang itu sekaligus
/// menjadi penjaganya.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini berjalan tanpa database mana pun.
/// </remarks>
public class LabExaminationAuditTrailTests
{
    private static readonly Guid Pengambil = Guid.Parse("A1111111-1111-1111-1111-111111111111");
    private static readonly Guid Penilai = Guid.Parse("B2222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task MenambahPemeriksaan_MeninggalkanSatuBarisExaminationAdd()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia);
        var service = CreateExaminationService(context, Penilai);

        var hasil = await service.AddAsync(dunia.OrderId, new AddLabExaminationRequest
        {
            SpecimenId = wadah,
            ProcedureId = dunia.Natrium
        });

        var baris = Assert.Single(await BarisAsync(context, hasil.Id));

        Assert.Equal(LabTransitionScope.LabExamination, baris.Scope);
        Assert.Equal("Examination.Add", baris.Action);
        Assert.Null(baris.FromStatus);
        Assert.Equal(nameof(LabExaminationStatus.Ordered), baris.ToStatus);
        Assert.Equal(dunia.OrderId, baris.LabOrderId);
        Assert.Equal(Penilai, baris.ActorUserId);
    }

    [Fact]
    public async Task MembatalkanPemeriksaan_MeninggalkanSatuBarisExaminationCancelBesertaAlasannya()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia);
        var pemeriksaan = await PemeriksaanPertamaAsync(context, wadah);

        await CreateExaminationService(context, Penilai).CancelAsync(
            pemeriksaan,
            new CancelLabExaminationRequest { Reason = "Dokter menarik permintaannya." });

        var barisCancel = Assert.Single(
            await BarisAsync(context, pemeriksaan),
            x => x.Action == "Examination.Cancel");

        Assert.Equal(LabTransitionScope.LabExamination, barisCancel.Scope);
        Assert.Equal(nameof(LabExaminationStatus.Ordered), barisCancel.FromStatus);
        Assert.Equal(nameof(LabExaminationStatus.Cancelled), barisCancel.ToStatus);

        // Matriks audit menuntut alasan ikut tersimpan pada barisnya, bukan hanya pada log.
        Assert.Equal("Dokter menarik permintaannya.", barisCancel.ReasonNote);
    }

    [Fact]
    public async Task MenyatakanWadahLayak_MeninggalkanSatuBarisChargeEligiblePerPemeriksaan()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia);

        await CreateSpecimenService(context, Penilai).AcceptAsync(wadah, new AcceptLabSpecimenRequest());

        context.ChangeTracker.Clear();

        var pemeriksaan = await context.LabExaminations.AsNoTracking()
            .Where(x => x.SpecimenId == wadah)
            .Select(x => x.Id)
            .ToListAsync();

        Assert.Equal(2, pemeriksaan.Count);

        var baris = await context.LabTransitionHistories.AsNoTracking()
            .Where(x => x.Action == "Examination.ChargeEligible")
            .ToListAsync();

        Assert.Equal(2, baris.Count);
        Assert.All(baris, x => Assert.Equal(LabTransitionScope.LabExamination, x.Scope));
        Assert.All(baris, x => Assert.Equal(nameof(LabExaminationStatus.Ordered), x.FromStatus));
        Assert.All(baris, x => Assert.Equal(nameof(LabExaminationStatus.ChargeEligible), x.ToStatus));

        // Satu baris untuk setiap pemeriksaan, bukan dua baris untuk pemeriksaan yang sama.
        Assert.Equal(
            pemeriksaan.OrderBy(x => x),
            baris.Select(x => x.LabExaminationId!.Value).OrderBy(x => x));
    }

    [Fact]
    public async Task MenyatakanLayakDuaKali_TidakMenggandakanBarisRiwayat()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia);
        var service = CreateSpecimenService(context, Penilai);

        await service.AcceptAsync(wadah, new AcceptLabSpecimenRequest());
        await service.AcceptAsync(wadah, new AcceptLabSpecimenRequest());

        context.ChangeTracker.Clear();

        var jumlah = await context.LabTransitionHistories.AsNoTracking()
            .CountAsync(x => x.Action == "Examination.ChargeEligible");

        Assert.Equal(2, jumlah);
    }

    [Fact]
    public async Task MenolakWadah_MeninggalkanSatuBarisVoidPerPemeriksaanBesertaCatatannya()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia);

        await CreateSpecimenService(context, Penilai).RejectAsync(wadah, new RejectLabSpecimenRequest
        {
            ReasonCode = "OTHER",
            Note = "Tabung bocor saat pengiriman."
        });

        context.ChangeTracker.Clear();

        var baris = await context.LabTransitionHistories.AsNoTracking()
            .Where(x => x.Action == "Examination.Void")
            .ToListAsync();

        Assert.Equal(2, baris.Count);
        Assert.All(baris, x => Assert.Equal(LabTransitionScope.LabExamination, x.Scope));
        Assert.All(baris, x => Assert.Equal(nameof(LabExaminationStatus.Voided), x.ToStatus));
        Assert.All(baris, x => Assert.Equal("Tabung bocor saat pengiriman.", x.ReasonNote));
        Assert.All(baris, x => Assert.NotNull(x.LabExaminationId));
    }

    /// <summary>
    /// Baris berlingkup pemeriksaan menunjuk pemeriksaannya, dan baris berlingkup wadah
    /// menunjuk wadahnya. Keduanya tidak saling menggantikan — itulah gunanya kolom terpisah.
    /// </summary>
    [Fact]
    public async Task BarisBerlingkupPemeriksaan_TidakMenunjukWadahDanSebaliknya()
    {
        await using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await SiapkanWadahDiterimaAsync(context, dunia);

        await CreateSpecimenService(context, Penilai).AcceptAsync(wadah, new AcceptLabSpecimenRequest());

        context.ChangeTracker.Clear();

        var semua = await context.LabTransitionHistories.AsNoTracking().ToListAsync();

        var lingkupPemeriksaan = semua.Where(x => x.Scope == LabTransitionScope.LabExamination).ToList();
        var lingkupWadah = semua.Where(x => x.Scope == LabTransitionScope.LabSpecimen).ToList();

        Assert.NotEmpty(lingkupPemeriksaan);
        Assert.NotEmpty(lingkupWadah);

        Assert.All(lingkupPemeriksaan, x => Assert.NotNull(x.LabExaminationId));
        Assert.All(lingkupPemeriksaan, x => Assert.Null(x.LabSpecimenId));

        Assert.All(lingkupWadah, x => Assert.NotNull(x.LabSpecimenId));
        Assert.All(lingkupWadah, x => Assert.Null(x.LabExaminationId));
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private sealed record Dunia(Guid OrderId, Guid Hemoglobin, Guid Leukosit, Guid Natrium);

    private static Task<List<LabTransitionHistory>> BarisAsync(ApplicationDbContext context, Guid examinationId)
    {
        context.ChangeTracker.Clear();

        return context.LabTransitionHistories
            .AsNoTracking()
            .Where(x => x.LabExaminationId == examinationId)
            .OrderBy(x => x.OccurredAt)
            .ToListAsync();
    }

    private static async Task<Guid> PemeriksaanPertamaAsync(ApplicationDbContext context, Guid wadah)
    {
        context.ChangeTracker.Clear();

        return await context.LabExaminations
            .AsNoTracking()
            .Where(x => x.SpecimenId == wadah)
            .OrderBy(x => x.ProcedureCodeSnapshot)
            .Select(x => x.Id)
            .FirstAsync();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-examination-audit-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LabSpecimenService CreateSpecimenService(ApplicationDbContext context, Guid actorUserId)
    {
        var accessor = CreateHttpContextAccessor(actorUserId);
        var loggerService = new LoggerService(NullLogger<LoggerService>.Instance, accessor);

        return new LabSpecimenService(
            context,
            new ClinicalMilestoneFactProducer(context, new BillingFolioService(context), loggerService),
            accessor,
            loggerService);
    }

    private static LabExaminationService CreateExaminationService(ApplicationDbContext context, Guid actorUserId)
    {
        var accessor = CreateHttpContextAccessor(actorUserId);

        return new LabExaminationService(
            context,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(Guid actorUserId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            authenticationType: "LabExaminationAuditTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    private static async Task<Dunia> SeedAsync(ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var hemoglobin = Procedure($"HB-{suffix}", "Hemoglobin");
        var leukosit = Procedure($"WBC-{suffix}", "Leukosit");
        var natrium = Procedure($"NA-{suffix}", "Natrium");

        context.Set<MstProcedure>().AddRange(hemoglobin, leukosit, natrium);

        context.Set<MstTariff>().AddRange(
            Tarif(hemoglobin.Id, "TRF-HB", 35_000m),
            Tarif(leukosit.Id, "TRF-WBC", 30_000m),
            Tarif(natrium.Id, "TRF-NA", 40_000m));

        context.MstLabRejectionReasons.Add(new MstLabRejectionReason
        {
            Id = Guid.NewGuid(),
            ReasonCode = "OTHER",
            ReasonName = "Lainnya",
            IsActive = true,
            RequiresNote = true
        });

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = hemoglobin.Id,
            Discipline = LabDiscipline.ClinicalPathology,
            OrderStatus = LabOrderStatus.Requested
        };

        context.LabOrders.Add(order);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return new Dunia(order.Id, hemoglobin.Id, leukosit.Id, natrium.Id);
    }

    /// <summary>Wadah berisi dua pemeriksaan, sudah diambil dan tercatat tiba di laboratorium.</summary>
    private static async Task<Guid> SiapkanWadahDiterimaAsync(ApplicationDbContext context, Dunia dunia)
    {
        var hasil = await CreateSpecimenService(context, Penilai).PlanAsync(
            dunia.OrderId,
            new PlanLabSpecimenRequest
            {
                Examinations = new List<Guid> { dunia.Hemoglobin, dunia.Leukosit }
            });

        context.ChangeTracker.Clear();

        var wadah = await context.LabSpecimens.SingleAsync(x => x.Id == hasil.Specimen.Id);
        wadah.SpecimenStatus = LabSpecimenStatus.Received;
        wadah.CollectedByUserId = Pengambil;
        wadah.CollectedAt = DateTime.UtcNow;
        wadah.ReceivedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return wadah.Id;
    }

    private static MstProcedure Procedure(string kode, string nama) =>
        new()
        {
            Id = Guid.NewGuid(),
            ProcedureCode = kode,
            ProcedureName = nama,
            ProcedureType = "Laboratory",
            IsLaboratory = true,
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
}
