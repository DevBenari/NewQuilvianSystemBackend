using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-14</c> — daftar kerja dan pemantauan keterlambatan cito
/// (<c>FR-04.1</c> .. <c>FR-04.4</c>; <c>LAB-DEC-013</c>).
///
/// Yang dibuktikan di sini:
///   1. <c>AC-10</c> — satu pesanan cito yang masuk belakangan tetap berada di urutan pertama,
///      dan dua pesanan cito di antara mereka sendiri urut menurut waktu masuk;
///   2. <c>AC-39</c> — pada satu pesanan berisi Kalium cito dan Kolesterol biasa, hanya Kalium
///      yang naik ke atas;
///   3. <c>AC-17</c> — keterlambatan dihitung sejak wadah dinyatakan layak, dan pekerjaan yang
///      sudah selesai tidak muncul;
///   4. <c>VAL-39</c> — pemeriksaan cito tanpa batas waktu tetap ditampilkan, tetapi tidak
///      dianggap terlambat;
///   5. <c>FR-04.4</c> — tidak ada tabel daftar kerja yang dibuat.
/// </summary>
/// <remarks>
/// Waktu "sekarang" disuntikkan lewat parameter <c>asOf</c>, sehingga bukti keterlambatan tidak
/// bergantung pada jam mesin yang menjalankan uji.
/// </remarks>
public class LabWorklistTests
{
    private static readonly DateTime Pagi = new(2026, 9, 4, 10, 0, 0, DateTimeKind.Utc);

    // =====================================================================
    // 1. Bentuk kontrak
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabWorklistController.GetPending), "pending")]
    [InlineData(nameof(LabWorklistController.GetCitoOverdue), "cito-overdue")]
    public void KeduaEndpoint_MemakaiGetDanPermissionYangDikunciKontrak(string methodName, string template)
    {
        var method = typeof(LabWorklistController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabWorklist", arguments[0]);
        Assert.Equal("Read", arguments[1]);

        var verb = Assert.IsType<HttpGetAttribute>(
            method.GetCustomAttributes().Single(x => x is HttpMethodAttribute));

        Assert.Equal(template, verb.Template);
    }

    /// <summary>
    /// <c>FR-04.4</c>. Daftar kerja diturunkan, tidak disimpan. Godaan terbesarnya adalah
    /// menyimpannya sebagai tabel demi kecepatan, dan uji ini yang menahannya.
    /// </summary>
    [Fact]
    public void TidakAdaTabelDaftarKerja_YangDibuat()
    {
        using var context = CreateContext();

        var entityWorklist = context.Model
            .GetEntityTypes()
            .Where(x => x.ClrType.Name.Contains("Worklist", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(entityWorklist);

        // Grup ini juga tidak punya satu pun jalur tulis.
        var verbs = typeof(LabWorklistController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .ToList();

        Assert.NotEmpty(verbs);
        Assert.All(verbs, x => Assert.IsType<HttpGetAttribute>(x));
    }

    // =====================================================================
    // 2. AC-10 — urutan daftar kerja
    // =====================================================================

    [Fact]
    public async Task AC10_SatuCitoPukul1005_BeradaDiUrutanPertamaDiAtasEmpatBelasPesananBiasa()
    {
        await using var context = CreateContext();
        var kalium = await SeedProcedureAsync(context, "K", "Kalium");

        for (var i = 0; i < 14; i++)
        {
            await SeedPekerjaanAsync(
                context, kalium, Pagi, LabExaminationUrgency.Routine, barcode: $"LSP-BIASA-{i:D2}");
        }

        var cito = await SeedPekerjaanAsync(
            context, kalium, Pagi.AddMinutes(5), LabExaminationUrgency.Cito, barcode: "LSP-CITO-01");

        var hasil = await CreateService(context).GetPendingAsync(new LabWorklistPagedQuery { PageSize = 100 });

        Assert.Equal(15, hasil.TotalData);
        Assert.Equal(cito, hasil.Items[0].ExaminationId);
        Assert.Equal(nameof(LabExaminationUrgency.Cito), hasil.Items[0].Urgency);

        // Sisanya tetap biasa, dan tidak ada cito lain yang menyelinap.
        Assert.All(hasil.Items.Skip(1), x => Assert.Equal(nameof(LabExaminationUrgency.Routine), x.Urgency));
    }

    [Fact]
    public async Task AC10_DuaCitoBerbedaWaktuMasuk_KeduanyaDiAtasYangBiasaDanUrutMenurutWaktuMasuk()
    {
        await using var context = CreateContext();
        var kalium = await SeedProcedureAsync(context, "K", "Kalium");

        await SeedPekerjaanAsync(context, kalium, Pagi, LabExaminationUrgency.Routine, "LSP-BIASA-01");

        var citoKedua = await SeedPekerjaanAsync(
            context, kalium, Pagi.AddMinutes(30), LabExaminationUrgency.Cito, "LSP-CITO-02");

        var citoPertama = await SeedPekerjaanAsync(
            context, kalium, Pagi.AddMinutes(10), LabExaminationUrgency.Cito, "LSP-CITO-01");

        var hasil = await CreateService(context).GetPendingAsync(new LabWorklistPagedQuery());

        Assert.Equal(3, hasil.TotalData);

        // Yang masuk lebih dulu berada di atas, walaupun barisnya dibuat belakangan.
        Assert.Equal(citoPertama, hasil.Items[0].ExaminationId);
        Assert.Equal(citoKedua, hasil.Items[1].ExaminationId);
        Assert.Equal(nameof(LabExaminationUrgency.Routine), hasil.Items[2].Urgency);
    }

    [Fact]
    public async Task AC39_SatuPesananBerisiKaliumCitoDanKolesterolBiasa_HanyaKaliumNaikKeAtas()
    {
        await using var context = CreateContext();

        var kalium = await SeedProcedureAsync(context, "K", "Kalium");
        var kolesterol = await SeedProcedureAsync(context, "CHOL", "Kolesterol");

        // Satu pesanan biasa milik pasien lain, supaya urutan benar-benar diuji.
        await SeedPekerjaanAsync(context, kalium, Pagi.AddMinutes(-30), LabExaminationUrgency.Routine, "LSP-LAIN");

        var (orderId, specimenId) = await SeedPesananDanWadahAsync(context, Pagi, "LSP-CAMPUR");

        var idKalium = await SeedPemeriksaanAsync(
            context, orderId, specimenId, kalium, LabExaminationUrgency.Cito);

        var idKolesterol = await SeedPemeriksaanAsync(
            context, orderId, specimenId, kolesterol, LabExaminationUrgency.Routine);

        var hasil = await CreateService(context).GetPendingAsync(new LabWorklistPagedQuery());

        Assert.Equal(idKalium, hasil.Items[0].ExaminationId);

        // Kolesterol tetap di antrean biasa, di bawah pesanan biasa yang masuk lebih dulu.
        var posisiKolesterol = hasil.Items.FindIndex(x => x.ExaminationId == idKolesterol);

        Assert.Equal(2, posisiKolesterol);
    }

    [Fact]
    public async Task PesananYangSudahSelesai_TidakMunculPadaDaftarKerja()
    {
        await using var context = CreateContext();
        var kalium = await SeedProcedureAsync(context, "K", "Kalium");

        await SeedPekerjaanAsync(context, kalium, Pagi, LabExaminationUrgency.Routine, "LSP-SELESAI");

        var order = await context.LabOrders.FirstAsync();
        order.OrderStatus = LabOrderStatus.Completed;
        order.CompletedAt = Pagi.AddMinutes(20);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var hasil = await CreateService(context).GetPendingAsync(new LabWorklistPagedQuery());

        Assert.Equal(0, hasil.TotalData);
        Assert.Empty(hasil.Items);
    }

    // =====================================================================
    // 3. AC-17 — keterlambatan cito
    // =====================================================================

    [Fact]
    public async Task AC17_KaliumCito60Menit_LayakPukul0900_BelumSelesaiPukul1020_TerlambatDuaPuluhMenit()
    {
        await using var context = CreateContext();

        var kalium = await SeedProcedureAsync(context, "K", "Kalium");
        await SeedBatasWaktuCitoAsync(context, kalium, 60);

        var layakPukul0900 = new DateTime(2026, 9, 4, 9, 0, 0, DateTimeKind.Utc);

        var pemeriksaan = await SeedPekerjaanAsync(
            context, kalium, Pagi.AddHours(-2), LabExaminationUrgency.Cito, "LSP-CITO-K",
            chargeEligibleAt: layakPukul0900);

        var hasil = await CreateService(context).GetCitoOverdueAsync(
            new LabWorklistPagedQuery(),
            asOf: new DateTime(2026, 9, 4, 10, 20, 0, DateTimeKind.Utc));

        var baris = Assert.Single(hasil.Items);

        Assert.Equal(pemeriksaan, baris.ExaminationId);
        Assert.True(baris.HasCitoTurnaround);
        Assert.Equal(60, baris.CitoTurnaroundMinutes);
        Assert.Equal(layakPukul0900, baris.ChargeEligibleAt);
        Assert.Equal(layakPukul0900.AddMinutes(60), baris.DeadlineAt);
        Assert.Equal(20, baris.OverdueMinutes);
        Assert.Null(baris.Note);
    }

    [Fact]
    public async Task AC17_PekerjaanSelesaiPukul0945_TidakMunculPadaDaftarPantau()
    {
        await using var context = CreateContext();

        var kalium = await SeedProcedureAsync(context, "K", "Kalium");
        await SeedBatasWaktuCitoAsync(context, kalium, 60);

        await SeedPekerjaanAsync(
            context, kalium, Pagi.AddHours(-2), LabExaminationUrgency.Cito, "LSP-CITO-K",
            chargeEligibleAt: new DateTime(2026, 9, 4, 9, 0, 0, DateTimeKind.Utc));

        var order = await context.LabOrders.FirstAsync();
        order.OrderStatus = LabOrderStatus.Completed;
        order.CompletedAt = new DateTime(2026, 9, 4, 9, 45, 0, DateTimeKind.Utc);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var hasil = await CreateService(context).GetCitoOverdueAsync(
            new LabWorklistPagedQuery(),
            asOf: new DateTime(2026, 9, 4, 10, 20, 0, DateTimeKind.Utc));

        Assert.Equal(0, hasil.TotalData);
        Assert.Empty(hasil.Items);
    }

    [Fact]
    public async Task CitoYangBelumMelewatiBatasWaktunya_TidakMuncul()
    {
        await using var context = CreateContext();

        var kalium = await SeedProcedureAsync(context, "K", "Kalium");
        await SeedBatasWaktuCitoAsync(context, kalium, 60);

        await SeedPekerjaanAsync(
            context, kalium, Pagi, LabExaminationUrgency.Cito, "LSP-CITO-K",
            chargeEligibleAt: new DateTime(2026, 9, 4, 9, 0, 0, DateTimeKind.Utc));

        var hasil = await CreateService(context).GetCitoOverdueAsync(
            new LabWorklistPagedQuery(),
            asOf: new DateTime(2026, 9, 4, 9, 59, 0, DateTimeKind.Utc));

        Assert.Empty(hasil.Items);
    }

    /// <summary>
    /// <c>FR-04.3</c>. Titik mulainya adalah saat wadah dinyatakan layak, bukan saat pesanan
    /// dibuat. Sebelum bahannya layak, laboratorium belum punya apa pun untuk dikerjakan.
    /// </summary>
    [Fact]
    public async Task CitoYangWadahnyaBelumDinyatakanLayak_TidakDihitungTerlambat()
    {
        await using var context = CreateContext();

        var kalium = await SeedProcedureAsync(context, "K", "Kalium");
        await SeedBatasWaktuCitoAsync(context, kalium, 60);

        // Pesanan masuk lima jam lalu, tetapi wadahnya belum diputuskan.
        await SeedPekerjaanAsync(
            context, kalium, Pagi.AddHours(-5), LabExaminationUrgency.Cito, "LSP-CITO-K",
            chargeEligibleAt: null);

        var hasil = await CreateService(context).GetCitoOverdueAsync(
            new LabWorklistPagedQuery(),
            asOf: Pagi);

        Assert.Empty(hasil.Items);
    }

    [Fact]
    public async Task VAL39_CitoTanpaBatasWaktu_TetapDitampilkanTetapiTidakDianggapTerlambat()
    {
        await using var context = CreateContext();

        var natrium = await SeedProcedureAsync(context, "NA", "Natrium");

        var pemeriksaan = await SeedPekerjaanAsync(
            context, natrium, Pagi.AddHours(-4), LabExaminationUrgency.Cito, "LSP-CITO-NA",
            chargeEligibleAt: new DateTime(2026, 9, 4, 6, 0, 0, DateTimeKind.Utc));

        var hasil = await CreateService(context).GetCitoOverdueAsync(
            new LabWorklistPagedQuery(),
            asOf: Pagi);

        var baris = Assert.Single(hasil.Items);

        Assert.Equal(pemeriksaan, baris.ExaminationId);
        Assert.False(baris.HasCitoTurnaround);
        Assert.Null(baris.CitoTurnaroundMinutes);
        Assert.Null(baris.DeadlineAt);
        Assert.Null(baris.OverdueMinutes);
        Assert.Contains("belum diatur", baris.Note);
    }

    /// <summary>
    /// Keterlambatan yang sesungguhnya berada di atas baris <c>VAL-39</c>, supaya kepala
    /// instalasi membaca yang paling mendesak lebih dulu — bukan data induk yang belum lengkap.
    /// </summary>
    [Fact]
    public async Task BarisTanpaBatasWaktu_BeradaDiBawahKeterlambatanYangSesungguhnya()
    {
        await using var context = CreateContext();

        var kalium = await SeedProcedureAsync(context, "K", "Kalium");
        var natrium = await SeedProcedureAsync(context, "NA", "Natrium");

        await SeedBatasWaktuCitoAsync(context, kalium, 60);

        await SeedPekerjaanAsync(
            context, natrium, Pagi.AddHours(-4), LabExaminationUrgency.Cito, "LSP-NA",
            chargeEligibleAt: new DateTime(2026, 9, 4, 6, 0, 0, DateTimeKind.Utc));

        var terlambat = await SeedPekerjaanAsync(
            context, kalium, Pagi.AddHours(-2), LabExaminationUrgency.Cito, "LSP-K",
            chargeEligibleAt: new DateTime(2026, 9, 4, 9, 0, 0, DateTimeKind.Utc));

        var hasil = await CreateService(context).GetCitoOverdueAsync(
            new LabWorklistPagedQuery(),
            asOf: new DateTime(2026, 9, 4, 10, 20, 0, DateTimeKind.Utc));

        Assert.Equal(2, hasil.TotalData);
        Assert.Equal(terlambat, hasil.Items[0].ExaminationId);
        Assert.True(hasil.Items[0].HasCitoTurnaround);
        Assert.False(hasil.Items[1].HasCitoTurnaround);
    }

    [Fact]
    public async Task DaftarPantau_HanyaMemuatCito()
    {
        await using var context = CreateContext();

        var kalium = await SeedProcedureAsync(context, "K", "Kalium");
        await SeedBatasWaktuCitoAsync(context, kalium, 60);

        await SeedPekerjaanAsync(
            context, kalium, Pagi.AddHours(-5), LabExaminationUrgency.Routine, "LSP-BIASA",
            chargeEligibleAt: new DateTime(2026, 9, 4, 5, 0, 0, DateTimeKind.Utc));

        var hasil = await CreateService(context).GetCitoOverdueAsync(
            new LabWorklistPagedQuery(),
            asOf: Pagi);

        Assert.Empty(hasil.Items);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-worklist-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LabWorklistService CreateService(ApplicationDbContext context) => new(context);

    private static async Task<MstProcedure> SeedProcedureAsync(
        ApplicationDbContext context, string kode, string nama)
    {
        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"{kode}-{Guid.NewGuid().ToString("N")[..6]}",
            ProcedureName = nama,
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        context.Set<MstProcedure>().Add(procedure);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return procedure;
    }

    private static async Task SeedBatasWaktuCitoAsync(
        ApplicationDbContext context, MstProcedure procedure, int menit)
    {
        context.LabValueBounds.Add(new LabValueBound
        {
            Id = Guid.NewGuid(),
            ProcedureId = procedure.Id,
            ResultForm = LabResultForm.Numeric,
            Unit = "mmol/L",
            GenderScope = LabGenderScope.All,
            AgeCategoryId = null,
            CitoTurnaroundMinutes = menit,
            IsActive = true
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private static async Task<(Guid OrderId, Guid SpecimenId)> SeedPesananDanWadahAsync(
        ApplicationDbContext context, DateTime requestedAt, string barcode)
    {
        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = Guid.NewGuid(),
            Discipline = LabDiscipline.ClinicalPathology,
            OrderStatus = LabOrderStatus.Requested,
            RequestedAt = requestedAt
        };

        var specimen = new LabSpecimen
        {
            Id = Guid.NewGuid(),
            LabOrderId = order.Id,
            SpecimenBarcode = barcode,
            SpecimenSequence = 1,
            SpecimenStatus = LabSpecimenStatus.Received
        };

        context.LabOrders.Add(order);
        context.LabSpecimens.Add(specimen);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return (order.Id, specimen.Id);
    }

    private static async Task<Guid> SeedPemeriksaanAsync(
        ApplicationDbContext context,
        Guid orderId,
        Guid specimenId,
        MstProcedure procedure,
        LabExaminationUrgency urgency,
        DateTime? chargeEligibleAt = null)
    {
        var pemeriksaan = new LabExamination
        {
            Id = Guid.NewGuid(),
            LabOrderId = orderId,
            SpecimenId = specimenId,
            ProcedureId = procedure.Id,
            ProcedureCodeSnapshot = procedure.ProcedureCode,
            ProcedureNameSnapshot = procedure.ProcedureName,
            UnitPriceSnapshot = 40_000m,
            ExaminationStatus = chargeEligibleAt == null
                ? LabExaminationStatus.Ordered
                : LabExaminationStatus.ChargeEligible,
            ChargeEligibleAt = chargeEligibleAt,
            Urgency = urgency,
            CreateDateTime = DateTime.UtcNow
        };

        context.LabExaminations.Add(pemeriksaan);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return pemeriksaan.Id;
    }

    /// <summary>Satu pesanan, satu wadah, dan satu pemeriksaan di atasnya.</summary>
    private static async Task<Guid> SeedPekerjaanAsync(
        ApplicationDbContext context,
        MstProcedure procedure,
        DateTime requestedAt,
        LabExaminationUrgency urgency,
        string barcode,
        DateTime? chargeEligibleAt = null)
    {
        var (orderId, specimenId) = await SeedPesananDanWadahAsync(context, requestedAt, barcode);

        return await SeedPemeriksaanAsync(
            context, orderId, specimenId, procedure, urgency, chargeEligibleAt);
    }
}
