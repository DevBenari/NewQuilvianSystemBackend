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
/// Bukti untuk <c>BE-LAB-10</c> — penanda cito dan duplo per pemeriksaan
/// (<c>FR-01.1</c> .. <c>FR-01.4</c>; <c>LAB-DEC-013</c>, <c>LAB-DEC-026</c>).
///
/// Yang dibuktikan di sini:
///   1. <c>AC-18</c> — dokter pemesan menandai cito, waktunya dan pelakunya tersimpan, dan satu
///      baris riwayat terbentuk; mengembalikannya menjadi biasa menambah satu baris lagi;
///   2. <c>VAL-03</c> — dokter lain ditolak <c>403</c> dan tidak ada data yang berubah;
///   3. <c>VAL-04</c> — pesanan yang sudah selesai ditolak <c>409</c>;
///   4. <c>AC-39</c> — satu pesanan memuat Kalium cito dan Kolesterol biasa sekaligus;
///   5. <c>AC-40</c> — duplo hanya mengenai baris yang ditandai, dan **tidak ada** endpoint
///      kesegeraan pada grup Lab Order;
///   6. riwayat berlingkup <c>LabExamination</c> menunjuk pemeriksaannya, bukan wadahnya.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini berjalan tanpa database mana pun. Yang diuji di
/// sini adalah aturan di dalam service, bukan penjaga fisik database.
/// </remarks>
public class LabExaminationUrgencyTests
{
    private static readonly Guid DokterPemesan = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid DokterLain = Guid.Parse("22222222-2222-2222-2222-222222222222");

    // =====================================================================
    // 1. Bentuk kontrak
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabExaminationController.SetUrgency), "{id:guid}/urgency")]
    [InlineData(nameof(LabExaminationController.SetDuplo), "{id:guid}/duplo")]
    public void KeduaEndpoint_MemakaiPutDanPermissionYangDikunciKontrak(string methodName, string template)
    {
        var method = typeof(LabExaminationController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabExamination", arguments[0]);
        Assert.Equal("Update", arguments[1]);

        var verb = Assert.IsType<HttpPutAttribute>(
            method.GetCustomAttributes().Single(x => x is HttpMethodAttribute));

        Assert.Equal(template, verb.Template);
    }

    /// <summary>
    /// <c>AC-40</c> pada sisi yang paling mudah terlanggar tanpa disadari: kesegeraan pada
    /// tingkat pesanan. <c>LAB-DEC-026</c> membatalkan endpoint itu, dan yang membatalkannya
    /// mudah dipasang kembali oleh siapa pun yang mengira ia hilang karena kelupaan.
    /// </summary>
    [Fact]
    public void GrupLabOrder_TidakMemilikiEndpointKesegeraanSamaSekali()
    {
        var routes = typeof(LabOrderController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .Select(x => x.Template ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(routes, x => x.Contains("urgency", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(routes, x => x.Contains("cito", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(routes, x => x.Contains("duplo", StringComparison.OrdinalIgnoreCase));
    }

    // =====================================================================
    // 2. AC-18 — menandai cito dan mengembalikannya
    // =====================================================================

    [Fact]
    public async Task AC18_DokterPemesanMenandaiCito_MenyimpanWaktuPelakuDanSatuBarisRiwayat()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, DokterPemesan);

        var sebelum = DateTime.UtcNow;

        var hasil = await service.SetUrgencyAsync(
            dunia.Kalium,
            new SetLabExaminationUrgencyRequest { IsCito = true });

        Assert.Equal(nameof(LabExaminationUrgency.Cito), hasil.Urgency);
        Assert.NotNull(hasil.UrgencyMarkedAt);
        Assert.True(hasil.UrgencyMarkedAt >= sebelum);
        Assert.Equal(DokterPemesan, hasil.UrgencyMarkedByUserId);

        var riwayat = await context.LabTransitionHistories
            .Where(x => x.LabExaminationId == dunia.Kalium)
            .ToListAsync();

        var baris = Assert.Single(riwayat);

        Assert.Equal(LabTransitionScope.LabExamination, baris.Scope);
        Assert.Equal("Examination.SetUrgency", baris.Action);
        Assert.Equal(nameof(LabExaminationUrgency.Routine), baris.FromStatus);
        Assert.Equal(nameof(LabExaminationUrgency.Cito), baris.ToStatus);
        Assert.Equal(DokterPemesan, baris.ActorUserId);
        Assert.Equal(dunia.OrderId, baris.LabOrderId);

        // Yang berpindah adalah pemeriksaan, bukan wadahnya.
        Assert.Null(baris.LabSpecimenId);
    }

    [Fact]
    public async Task AC18_MengembalikanCitoMenjadiBiasa_MenambahSatuBarisRiwayatLagi()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, DokterPemesan);

        await service.SetUrgencyAsync(dunia.Kalium, new SetLabExaminationUrgencyRequest { IsCito = true });

        var hasil = await service.SetUrgencyAsync(
            dunia.Kalium,
            new SetLabExaminationUrgencyRequest { IsCito = false });

        Assert.Equal(nameof(LabExaminationUrgency.Routine), hasil.Urgency);

        var riwayat = await context.LabTransitionHistories
            .Where(x => x.LabExaminationId == dunia.Kalium)
            .OrderBy(x => x.OccurredAt)
            .ToListAsync();

        Assert.Equal(2, riwayat.Count);
        Assert.Equal(nameof(LabExaminationUrgency.Cito), riwayat[1].FromStatus);
        Assert.Equal(nameof(LabExaminationUrgency.Routine), riwayat[1].ToStatus);
    }

    /// <summary>
    /// Menekan tombol yang sama dua kali bukan perpindahan. Riwayat mencatat perubahan keadaan,
    /// bukan jumlah kali tombol ditekan.
    /// </summary>
    [Fact]
    public async Task MenyetelKesegeraanYangSudahBerlaku_TidakMenambahRiwayat()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, DokterPemesan);

        await service.SetUrgencyAsync(dunia.Kalium, new SetLabExaminationUrgencyRequest { IsCito = true });
        await service.SetUrgencyAsync(dunia.Kalium, new SetLabExaminationUrgencyRequest { IsCito = true });

        var jumlah = await context.LabTransitionHistories
            .CountAsync(x => x.LabExaminationId == dunia.Kalium);

        Assert.Equal(1, jumlah);
    }

    // =====================================================================
    // 3. VAL-03 dan VAL-04 — jalur gagal
    // =====================================================================

    [Fact]
    public async Task VAL03_DokterLainMenandaiCito_Ditolak403DanTidakMengubahApaPun()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, DokterLain);

        var galat = await Assert.ThrowsAsync<LabExaminationForbiddenException>(
            () => service.SetUrgencyAsync(dunia.Kalium, new SetLabExaminationUrgencyRequest { IsCito = true }));

        Assert.Contains("dokter yang membuat pesanan ini", galat.Message);

        context.ChangeTracker.Clear();

        var pemeriksaan = await context.LabExaminations.AsNoTracking().FirstAsync(x => x.Id == dunia.Kalium);

        Assert.Equal(LabExaminationUrgency.Routine, pemeriksaan.Urgency);
        Assert.Null(pemeriksaan.UrgencyMarkedAt);
        Assert.Null(pemeriksaan.UrgencyMarkedByUserId);

        Assert.Empty(await context.LabTransitionHistories
            .Where(x => x.LabExaminationId == dunia.Kalium)
            .ToListAsync());
    }

    [Fact]
    public async Task VAL04_PesananSudahSelesai_Ditolak409()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var order = await context.LabOrders.FirstAsync(x => x.Id == dunia.OrderId);
        order.OrderStatus = LabOrderStatus.Completed;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = CreateService(context, DokterPemesan);

        var galat = await Assert.ThrowsAsync<LabExaminationConflictException>(
            () => service.SetUrgencyAsync(dunia.Kalium, new SetLabExaminationUrgencyRequest { IsCito = true }));

        Assert.Contains("sudah selesai atau dibatalkan", galat.Message);
    }

    /// <summary>
    /// <c>VAL-04</c> didahulukan atas <c>VAL-03</c>. Pesanan yang sudah selesai tidak dapat
    /// diubah oleh siapa pun, sehingga menjawab <c>409</c> kepada dokter pemesannya lebih benar
    /// daripada menjawab <c>403</c> kepada orang yang sebenarnya berwenang.
    /// </summary>
    [Fact]
    public async Task PesananSelesaiDanPelakuBukanPemesan_Menjawab409BukanNya403()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var order = await context.LabOrders.FirstAsync(x => x.Id == dunia.OrderId);
        order.OrderStatus = LabOrderStatus.Cancelled;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = CreateService(context, DokterLain);

        await Assert.ThrowsAsync<LabExaminationConflictException>(
            () => service.SetUrgencyAsync(dunia.Kalium, new SetLabExaminationUrgencyRequest { IsCito = true }));
    }

    // =====================================================================
    // 4. AC-39 — cito dan biasa berdampingan pada satu pesanan
    // =====================================================================

    [Fact]
    public async Task AC39_SatuPesananMemuatKaliumCitoDanKolesterolBiasaSekaligus()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, DokterPemesan);

        await service.SetUrgencyAsync(dunia.Kalium, new SetLabExaminationUrgencyRequest { IsCito = true });

        var daftar = await service.GetByOrderAsync(dunia.OrderId);

        var kalium = daftar.Single(x => x.Id == dunia.Kalium);
        var kolesterol = daftar.Single(x => x.Id == dunia.Kolesterol);

        Assert.Equal(nameof(LabExaminationUrgency.Cito), kalium.Urgency);
        Assert.Equal(nameof(LabExaminationUrgency.Routine), kolesterol.Urgency);

        // Kolesterol tidak ikut tertandai, dan tidak ada jejak penandaan atasnya.
        Assert.Null(kolesterol.UrgencyMarkedAt);
        Assert.Null(kolesterol.UrgencyMarkedByUserId);
    }

    // =====================================================================
    // 5. AC-40 — duplo
    // =====================================================================

    [Fact]
    public async Task AC40_MenandaiDuplo_HanyaMengenaiBarisYangDitandai()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, DokterPemesan);

        var hasil = await service.SetDuploAsync(
            dunia.Kalium,
            new SetLabExaminationDuploRequest { IsDuplo = true });

        Assert.True(hasil.IsDuplo);

        var kolesterol = await context.LabExaminations
            .AsNoTracking()
            .FirstAsync(x => x.Id == dunia.Kolesterol);

        Assert.False(kolesterol.IsDuplo);

        var baris = Assert.Single(await context.LabTransitionHistories
            .Where(x => x.LabExaminationId == dunia.Kalium)
            .ToListAsync());

        Assert.Equal("Examination.SetDuplo", baris.Action);
        Assert.Equal("Single", baris.FromStatus);
        Assert.Equal("Duplo", baris.ToStatus);
    }

    /// <summary>
    /// Duplo bukan penilaian klinis dokter pemesan melainkan keputusan pelaksanaan
    /// laboratorium, sehingga <c>VAL-03</c> sengaja tidak berlaku di sini.
    /// </summary>
    [Fact]
    public async Task PenandaanDuplo_TidakMenuntutPelakunyaDokterPemesan()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);
        var service = CreateService(context, DokterLain);

        var hasil = await service.SetDuploAsync(
            dunia.Kalium,
            new SetLabExaminationDuploRequest { IsDuplo = true });

        Assert.True(hasil.IsDuplo);
    }

    [Fact]
    public async Task WadahSudahDitolak_PenandaDuploDitolak409()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var wadah = await context.LabSpecimens.FirstAsync(x => x.Id == dunia.WadahId);
        wadah.SpecimenStatus = LabSpecimenStatus.Rejected;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = CreateService(context, DokterPemesan);

        var galat = await Assert.ThrowsAsync<LabExaminationConflictException>(
            () => service.SetDuploAsync(dunia.Kalium, new SetLabExaminationDuploRequest { IsDuplo = true }));

        Assert.Contains("sudah ditolak", galat.Message);
    }

    [Fact]
    public async Task PemeriksaanYangSudahDibatalkan_TidakDapatDitandaiCitoMaupunDuplo()
    {
        using var context = CreateContext();
        var dunia = await SeedAsync(context);

        var pemeriksaan = await context.LabExaminations.FirstAsync(x => x.Id == dunia.Kalium);
        pemeriksaan.ExaminationStatus = LabExaminationStatus.Cancelled;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = CreateService(context, DokterPemesan);

        await Assert.ThrowsAsync<LabExaminationConflictException>(
            () => service.SetUrgencyAsync(dunia.Kalium, new SetLabExaminationUrgencyRequest { IsCito = true }));

        await Assert.ThrowsAsync<LabExaminationConflictException>(
            () => service.SetDuploAsync(dunia.Kalium, new SetLabExaminationDuploRequest { IsDuplo = true }));
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private sealed record Dunia(Guid OrderId, Guid WadahId, Guid Kalium, Guid Kolesterol);

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-examination-urgency-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LabExaminationService CreateService(ApplicationDbContext context, Guid actorUserId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            authenticationType: "LabExaminationUrgencyTest");

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return new LabExaminationService(
            context,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    /// <summary>
    /// Satu pesanan milik <see cref="DokterPemesan"/>, satu wadah, dan dua pemeriksaan di
    /// atasnya: Kalium dan Kolesterol. Keduanya berangkat dari <c>Routine</c>.
    /// </summary>
    private static async Task<Dunia> SeedAsync(ApplicationDbContext context)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var kalium = Procedure($"K-{suffix}", "Kalium");
        var kolesterol = Procedure($"CHOL-{suffix}", "Kolesterol");

        context.Set<MstProcedure>().AddRange(kalium, kolesterol);

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = kalium.Id,
            Discipline = LabDiscipline.ClinicalPathology,
            OrderStatus = LabOrderStatus.Requested,
            RequestedByUserId = DokterPemesan
        };

        var wadah = new LabSpecimen
        {
            Id = Guid.NewGuid(),
            LabOrderId = order.Id,
            SpecimenBarcode = $"LSP-{suffix}",
            SpecimenSequence = 1,
            SpecimenStatus = LabSpecimenStatus.Received
        };

        var pemeriksaanKalium = Pemeriksaan(order.Id, wadah.Id, kalium.Id, "K", 45_000m);
        var pemeriksaanKolesterol = Pemeriksaan(order.Id, wadah.Id, kolesterol.Id, "CHOL", 55_000m);

        context.LabOrders.Add(order);
        context.LabSpecimens.Add(wadah);
        context.LabExaminations.AddRange(pemeriksaanKalium, pemeriksaanKolesterol);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return new Dunia(order.Id, wadah.Id, pemeriksaanKalium.Id, pemeriksaanKolesterol.Id);
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

    private static LabExamination Pemeriksaan(
        Guid orderId,
        Guid specimenId,
        Guid procedureId,
        string kode,
        decimal harga) =>
        new()
        {
            Id = Guid.NewGuid(),
            LabOrderId = orderId,
            SpecimenId = specimenId,
            ProcedureId = procedureId,
            ProcedureCodeSnapshot = kode,
            ProcedureNameSnapshot = kode,
            TariffCodeSnapshot = $"TRF-{kode}",
            UnitPriceSnapshot = harga,
            ExaminationStatus = LabExaminationStatus.Ordered,
            Urgency = LabExaminationUrgency.Routine,
            CreateDateTime = DateTime.UtcNow
        };
}
