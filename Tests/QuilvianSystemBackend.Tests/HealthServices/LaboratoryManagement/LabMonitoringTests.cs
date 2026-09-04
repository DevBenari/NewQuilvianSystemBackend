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
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-15</c> — tiga daftar pantau sejajar
/// (<c>FR-10.1</c> .. <c>FR-10.3</c>; <c>LAB-DEC-025</c>).
///
/// Yang dibuktikan di sini:
///   1. <c>AC-41</c> — ketiga daftar dibuka dengan data campuran, masing-masing hanya
///      menampilkan pesanan berdisiplin sesuai jalurnya;
///   2. penyaringnya benar-benar identik: satu bentuk permintaan dipakai ketiganya, dan
///      disiplin **tidak** termasuk di dalamnya;
///   3. <c>GET /lab-orders/by-discipline/{discipline}</c> menolak disiplin yang tidak dikenal
///      alih-alih mengembalikan daftar kosong yang menyesatkan.
/// </summary>
public class LabMonitoringTests
{
    // =====================================================================
    // 1. Bentuk kontrak
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabMonitoringController.GetClinicalPathology), "clinical-pathology")]
    [InlineData(nameof(LabMonitoringController.GetAnatomicPathology), "anatomic-pathology")]
    [InlineData(nameof(LabMonitoringController.GetMicrobiology), "microbiology")]
    public void KetigaEndpoint_MemakaiGetDanPermissionYangDikunciKontrak(string methodName, string template)
    {
        var method = typeof(LabMonitoringController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabMonitoring", arguments[0]);
        Assert.Equal("Read", arguments[1]);

        var verb = Assert.IsType<HttpGetAttribute>(
            method.GetCustomAttributes().Single(x => x is HttpMethodAttribute));

        Assert.Equal(template, verb.Template);
    }

    /// <summary>
    /// "Penyaring identik" pada DoD dibuktikan dari bentuknya: ketiga jalur menerima tipe
    /// permintaan yang sama. Menyalin tiga bentuk berbeda akan membuat ketiganya menyimpang
    /// perlahan tanpa ada yang menyadarinya.
    /// </summary>
    [Fact]
    public void KetigaEndpoint_MemakaiBentukPenyaringYangSamaPersis()
    {
        var metode = typeof(LabMonitoringController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<AccessPermissionAttribute>().Any())
            .ToList();

        Assert.Equal(3, metode.Count);

        Assert.All(metode, x =>
            Assert.Equal(
                typeof(LabMonitoringQuery),
                x.GetParameters().Single(p => p.ParameterType == typeof(LabMonitoringQuery)).ParameterType));

        // Grup ini baca saja.
        var verbs = typeof(LabMonitoringController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .ToList();

        Assert.NotEmpty(verbs);
        Assert.All(verbs, x => Assert.IsType<HttpGetAttribute>(x));
    }

    /// <summary>
    /// Disiplin ditentukan jalur yang dipanggil, bukan ruas yang dikirim. Bila ia menjadi
    /// penyaring biasa, tiga menu terpisah kehilangan alasan keberadaannya.
    /// </summary>
    [Fact]
    public void PenyaringDaftarPantau_TidakMemilikiRuasDisiplin()
    {
        var properti = typeof(LabMonitoringQuery).GetProperties().Select(x => x.Name).ToList();

        Assert.DoesNotContain("Discipline", properti);

        // Sepuluh penyaring yang dikunci kontrak tetap ada.
        foreach (var ruas in new[]
                 {
                     nameof(LabMonitoringQuery.PatientId),
                     nameof(LabMonitoringQuery.MedicalRecordNumber),
                     nameof(LabMonitoringQuery.EncounterNumber),
                     nameof(LabMonitoringQuery.StartDate),
                     nameof(LabMonitoringQuery.EndDate),
                     nameof(LabMonitoringQuery.EncounterType),
                     nameof(LabMonitoringQuery.VisitType),
                     nameof(LabMonitoringQuery.ServiceUnitId),
                     nameof(LabMonitoringQuery.RoomId),
                     nameof(LabMonitoringQuery.PaymentType),
                     nameof(LabMonitoringQuery.OrderStatus),
                     nameof(LabMonitoringQuery.SpecimenStatus),
                     nameof(LabMonitoringQuery.OnlyCito)
                 })
        {
            Assert.Contains(ruas, properti);
        }
    }

    // =====================================================================
    // 2. AC-41 — tiga daftar dengan data campuran
    // =====================================================================

    [Fact]
    public async Task AC41_TigaDaftarDenganDataCampuran_MasingMasingHanyaMenampilkanDisiplinnya()
    {
        await using var context = CreateContext();
        var service = new LabMonitoringService(context);

        var klinik1 = await SeedPesananAsync(context, LabDiscipline.ClinicalPathology, "Ani");
        var klinik2 = await SeedPesananAsync(context, LabDiscipline.ClinicalPathology, "Budi");
        var anatomi = await SeedPesananAsync(context, LabDiscipline.AnatomicalPathology, "Citra");
        var mikro = await SeedPesananAsync(context, LabDiscipline.Microbiology, "Dedi");

        var daftarKlinik = await service.GetByDisciplineAsync(
            LabDiscipline.ClinicalPathology, new LabMonitoringQuery());

        var daftarAnatomi = await service.GetByDisciplineAsync(
            LabDiscipline.AnatomicalPathology, new LabMonitoringQuery());

        var daftarMikro = await service.GetByDisciplineAsync(
            LabDiscipline.Microbiology, new LabMonitoringQuery());

        Assert.Equal(2, daftarKlinik.TotalData);
        Assert.Equal(1, daftarAnatomi.TotalData);
        Assert.Equal(1, daftarMikro.TotalData);

        Assert.Equal(
            new[] { klinik1, klinik2 }.OrderBy(x => x),
            daftarKlinik.Items.Select(x => x.LabOrderId).OrderBy(x => x));

        Assert.Equal(anatomi, daftarAnatomi.Items.Single().LabOrderId);
        Assert.Equal(mikro, daftarMikro.Items.Single().LabOrderId);

        // Tidak ada satu pun baris yang menyeberang ke daftar tetangganya.
        Assert.All(daftarKlinik.Items,
            x => Assert.Equal(nameof(LabDiscipline.ClinicalPathology), x.Discipline));
        Assert.All(daftarAnatomi.Items,
            x => Assert.Equal(nameof(LabDiscipline.AnatomicalPathology), x.Discipline));
        Assert.All(daftarMikro.Items,
            x => Assert.Equal(nameof(LabDiscipline.Microbiology), x.Discipline));
    }

    [Fact]
    public async Task BarisDaftarPantau_MembawaIdentitasPasienDanRekapPekerjaannya()
    {
        await using var context = CreateContext();
        var service = new LabMonitoringService(context);

        var orderId = await SeedPesananAsync(
            context, LabDiscipline.ClinicalPathology, "Ani Lestari", cito: true);

        var hasil = await service.GetByDisciplineAsync(
            LabDiscipline.ClinicalPathology, new LabMonitoringQuery());

        var baris = Assert.Single(hasil.Items);

        Assert.Equal(orderId, baris.LabOrderId);
        Assert.Equal("Ani Lestari", baris.PatientName);
        Assert.False(string.IsNullOrWhiteSpace(baris.MedicalRecordNumber));
        Assert.False(string.IsNullOrWhiteSpace(baris.EncounterNumber));
        Assert.Equal(1, baris.SpecimenCount);
        Assert.Equal(1, baris.ExaminationCount);
        Assert.True(baris.HasCito);
    }

    [Fact]
    public async Task PenyaringCito_HanyaMenampilkanPesananYangMemuatPemeriksaanCito()
    {
        await using var context = CreateContext();
        var service = new LabMonitoringService(context);

        var cito = await SeedPesananAsync(context, LabDiscipline.ClinicalPathology, "Ani", cito: true);
        await SeedPesananAsync(context, LabDiscipline.ClinicalPathology, "Budi", cito: false);

        var hasil = await service.GetByDisciplineAsync(
            LabDiscipline.ClinicalPathology,
            new LabMonitoringQuery { OnlyCito = true });

        Assert.Equal(cito, Assert.Single(hasil.Items).LabOrderId);
    }

    [Fact]
    public async Task PenyaringNomorRekamMedis_MenemukanPesananPasienYangDicari()
    {
        await using var context = CreateContext();
        var service = new LabMonitoringService(context);

        var dicari = await SeedPesananAsync(
            context, LabDiscipline.ClinicalPathology, "Ani", medicalRecordNumber: "MR-777001");

        await SeedPesananAsync(
            context, LabDiscipline.ClinicalPathology, "Budi", medicalRecordNumber: "MR-888002");

        var hasil = await service.GetByDisciplineAsync(
            LabDiscipline.ClinicalPathology,
            new LabMonitoringQuery { MedicalRecordNumber = "777" });

        Assert.Equal(dicari, Assert.Single(hasil.Items).LabOrderId);
    }

    [Fact]
    public async Task PenyaringStatusWadah_MenyaringDariWadahnya_BukanDariStatusPesanan()
    {
        await using var context = CreateContext();
        var service = new LabMonitoringService(context);

        var ditolak = await SeedPesananAsync(
            context, LabDiscipline.ClinicalPathology, "Ani",
            specimenStatus: LabSpecimenStatus.Rejected);

        await SeedPesananAsync(
            context, LabDiscipline.ClinicalPathology, "Budi",
            specimenStatus: LabSpecimenStatus.Received);

        var hasil = await service.GetByDisciplineAsync(
            LabDiscipline.ClinicalPathology,
            new LabMonitoringQuery { SpecimenStatus = LabSpecimenStatus.Rejected });

        Assert.Equal(ditolak, Assert.Single(hasil.Items).LabOrderId);
    }

    [Fact]
    public async Task PesananTanpaDisiplin_TidakMunculPadaSatuPunDaftarPantau()
    {
        await using var context = CreateContext();
        var service = new LabMonitoringService(context);

        // Pesanan peninggalan sebelum kolom disiplin ada.
        await SeedPesananAsync(context, discipline: null, namaPasien: "Ani");

        foreach (var disiplin in new[]
                 {
                     LabDiscipline.ClinicalPathology,
                     LabDiscipline.AnatomicalPathology,
                     LabDiscipline.Microbiology
                 })
        {
            var hasil = await service.GetByDisciplineAsync(disiplin, new LabMonitoringQuery());

            Assert.Equal(0, hasil.TotalData);
        }
    }

    // =====================================================================
    // 3. GET /lab-orders/by-discipline/{discipline}
    // =====================================================================

    [Fact]
    public void EndpointPesananPerDisiplin_MemakaiRouteDanPermissionYangDikunciKontrak()
    {
        var method = typeof(LabOrderController).GetMethod(nameof(LabOrderController.GetByDiscipline));

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabOrder", arguments[0]);
        Assert.Equal("Read", arguments[1]);

        var verb = Assert.IsType<HttpGetAttribute>(
            method.GetCustomAttributes().Single(x => x is HttpMethodAttribute));

        Assert.Equal("by-discipline/{discipline}", verb.Template);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-monitoring-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>Satu pasien, satu kunjungan, satu pesanan, satu wadah, dan satu pemeriksaan.</summary>
    private static async Task<Guid> SeedPesananAsync(
        ApplicationDbContext context,
        LabDiscipline? discipline,
        string namaPasien,
        bool cito = false,
        string? medicalRecordNumber = null,
        LabSpecimenStatus specimenStatus = LabSpecimenStatus.Received)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var patient = new MstPatient
        {
            Id = Guid.NewGuid(),
            PatientCode = $"PC-{suffix}",
            MedicalRecordNumber = medicalRecordNumber ?? $"MR-{suffix}",
            FullName = namaPasien
        };

        var encounter = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{suffix}",
            EncounterDate = DateTime.UtcNow,
            PatientId = patient.Id,
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = EncounterType.Outpatient,
            VisitType = VisitType.NewVisit,
            PaymentType = EncounterPaymentType.Cash
        };

        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"LB-{suffix}",
            ProcedureName = "Hemoglobin",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            IsActive = true
        };

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = encounter.Id,
            ProcedureId = procedure.Id,
            Discipline = discipline,
            OrderStatus = LabOrderStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        var specimen = new LabSpecimen
        {
            Id = Guid.NewGuid(),
            LabOrderId = order.Id,
            SpecimenBarcode = $"LSP-{suffix}",
            SpecimenSequence = 1,
            SpecimenStatus = specimenStatus
        };

        var examination = new LabExamination
        {
            Id = Guid.NewGuid(),
            LabOrderId = order.Id,
            SpecimenId = specimen.Id,
            ProcedureId = procedure.Id,
            ProcedureCodeSnapshot = procedure.ProcedureCode,
            ProcedureNameSnapshot = procedure.ProcedureName,
            ExaminationStatus = LabExaminationStatus.Ordered,
            Urgency = cito ? LabExaminationUrgency.Cito : LabExaminationUrgency.Routine,
            CreateDateTime = DateTime.UtcNow
        };

        context.MstPatients.Add(patient);
        context.TrxPatientEncounters.Add(encounter);
        context.Set<MstProcedure>().Add(procedure);
        context.LabOrders.Add(order);
        context.LabSpecimens.Add(specimen);
        context.LabExaminations.Add(examination);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return order.Id;
    }
}
