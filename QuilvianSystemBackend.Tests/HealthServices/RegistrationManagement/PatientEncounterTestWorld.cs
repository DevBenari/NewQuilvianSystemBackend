using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Tests.HealthServices.RegistrationManagement;

/// <summary>
/// Dunia uji bersama untuk <c>PatientEncounterController</c>: satu
/// <c>ApplicationDbContext</c> InMemory, master minimal yang dibutuhkan satu registrasi
/// rawat jalan, dan controller yang sudah punya <c>ControllerContext</c> sehingga
/// <c>GetCurrentUserId</c> tidak melempar.
/// </summary>
/// <remarks>
/// Dibuat oleh <c>BE-RWI-035</c> mengikuti pola <c>EmergencyControllerTestWorld</c> dan
/// <c>IsolatedInpatientDbContextFactory</c> yang sudah ada.
///
/// <para>
/// Provider InMemory tidak menjalankan pipeline MVC, sehingga <c>[AccessPermission]</c>
/// dan <c>[Authorize]</c> tidak ikut berjalan di sini. Yang dibuktikan test ini adalah
/// <b>isi metode aksinya</b>: baris apa yang tersimpan dan <c>IActionResult</c> apa yang
/// dikembalikan. Bahwa permintaan tanpa hak akses benar-benar dibalas 403 adalah
/// verifikasi runtime terpisah dan dicatat pada laporan task.
/// </para>
///
/// <para>
/// Provider InMemory juga tidak punya transaksi maupun foreign key. Peringatan transaksi
/// diabaikan supaya jalur create yang memang bertransaksi dapat diuji; yang dibuktikan
/// karena itu adalah bahwa encounter dan sumber pembayarannya masuk ke SATU
/// <c>SaveChangesAsync</c>, sehingga kegagalan menyisakan nol baris.
/// </para>
/// </remarks>
internal sealed class PatientEncounterTestWorld
{
    public const string OutpatientPatientClassName = "RAWAT JALAN";

    public static readonly Guid ActorUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private PatientEncounterTestWorld(
        ApplicationDbContext dbContext,
        MstPatient patient,
        MstPatient otherPatient,
        MstServiceUnit serviceUnit,
        MstPatientClass patientClass,
        MstPaymentMethod paymentMethod,
        MstInsuranceProvider insuranceProvider,
        MstPatientInsurance patientInsurance,
        MstCompanyGuarantor companyGuarantor,
        MstPatientCompanyGuarantor patientCompanyGuarantor)
    {
        DbContext = dbContext;
        Patient = patient;
        OtherPatient = otherPatient;
        ServiceUnit = serviceUnit;
        PatientClass = patientClass;
        PaymentMethod = paymentMethod;
        InsuranceProvider = insuranceProvider;
        PatientInsurance = patientInsurance;
        CompanyGuarantor = companyGuarantor;
        PatientCompanyGuarantor = patientCompanyGuarantor;
    }

    public ApplicationDbContext DbContext { get; }

    public MstPatient Patient { get; }

    /// <summary>Pasien kedua, dipakai membuktikan kartu milik pasien lain ditolak.</summary>
    public MstPatient OtherPatient { get; }

    public MstServiceUnit ServiceUnit { get; }

    public MstPatientClass PatientClass { get; }

    public MstPaymentMethod PaymentMethod { get; }

    public MstInsuranceProvider InsuranceProvider { get; }

    public MstPatientInsurance PatientInsurance { get; }

    public MstCompanyGuarantor CompanyGuarantor { get; }

    public MstPatientCompanyGuarantor PatientCompanyGuarantor { get; }

    public PatientEncounterController Controller => BuildController(DbContext);

    /// <param name="databaseRoot">
    /// Diisi ketika dua <c>ApplicationDbContext</c> berbeda harus berbagi satu store —
    /// misalnya pada test atomisitas, yang menyemai master lewat context normal lalu
    /// menjalankan create lewat context yang sengaja gagal menyimpan.
    /// </param>
    public static DbContextOptions<ApplicationDbContext> BuildOptions(
        string? databaseName = null,
        InMemoryDatabaseRoot? databaseRoot = null)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                databaseName ?? $"patient-encounter-tests-{Guid.NewGuid():N}",
                databaseRoot)
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    public static PatientEncounterController BuildController(ApplicationDbContext dbContext)
    {
        var loggerService = new LoggerService(
            NullLogger<LoggerService>.Instance,
            new HttpContextAccessor());

        var queueRealtimeService = new QueueRealtimeService(
            dbContext,
            new FakeQueueHubContext(),
            loggerService);

        var integrityService = new ClinicalDocumentIntegrityService(dbContext);

        var controller = new PatientEncounterController(
            dbContext,
            loggerService,
            new QueueRealtimeService(dbContext, new FakeQueueHubContext(), loggerService),
            new ClinicalDocumentIntegrityService(dbContext));
            queueRealtimeService,
            integrityService);

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    ActorUserId.ToString())
            },
            "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        return controller;
    }

    public static async Task<PatientEncounterTestWorld> CreateAsync(
        ApplicationDbContext? dbContext = null,
        DateTime? companyEffectiveStartDate = null,
        DateTime? companyEffectiveEndDate = null,
        bool companyCardActive = true,
        bool companyCardEligible = true,
        bool companyMasterActive = true)
    {
        var context = dbContext ?? new ApplicationDbContext(BuildOptions());

        // Pelaku disemai sebagai ApplicationUser sungguhan. Tanpa baris ini,
        // Include(x => x.RegisteredByUser) pada jalur baca menjatuhkan encounter-nya:
        // RegisteredByUserId adalah relasi wajib, dan provider InMemory tidak punya
        // foreign key yang menjamin principal-nya ada seperti PostgreSQL.
        var actorUser = new ApplicationUser
        {
            Id = ActorUserId,
            UserCode = "USR-ADMISI",
            DisplayName = "Petugas Admisi",
            UserName = "petugas.admisi",
            IsActive = true
        };

        var patient = new MstPatient
        {
            Id = Guid.NewGuid(),
            MedicalRecordNumber = "RM-0000001",
            FullName = "Budi Santoso",
            IsActive = true,
            IsDelete = false
        };

        var otherPatient = new MstPatient
        {
            Id = Guid.NewGuid(),
            MedicalRecordNumber = "RM-0000002",
            FullName = "Pasien Lain",
            IsActive = true,
            IsDelete = false
        };

        var serviceUnit = new MstServiceUnit
        {
            Id = Guid.NewGuid(),
            ServiceUnitCode = "SU-001",
            ServiceUnitName = "Poliklinik Umum",
            IsActive = true,
            IsDelete = false,
            IsAvailableForRegistration = true,
            // Antrean dimatikan supaya jalur create tidak menyentuh notifikasi realtime.
            IsQueueRequired = false,
            IsScreeningRequired = false,
            IsDoctorRequired = false
        };

        var patientClass = new MstPatientClass
        {
            Id = Guid.NewGuid(),
            PatientClassCode = "PC-RJ",
            PatientClassName = OutpatientPatientClassName,
            IsActive = true,
            IsDelete = false
        };

        var paymentMethod = new MstPaymentMethod
        {
            Id = Guid.NewGuid(),
            PaymentMethodCode = "PM-CASH",
            PaymentMethodName = "Tunai",
            IsActive = true,
            IsDelete = false,
            IsAvailableForRegistration = true
        };

        var insuranceProvider = new MstInsuranceProvider
        {
            Id = Guid.NewGuid(),
            InsuranceProviderCode = "INS-001",
            InsuranceProviderName = "PT Asuransi Sehat",
            IsActive = true,
            IsDelete = false
        };

        var patientInsurance = new MstPatientInsurance
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            InsuranceProviderId = insuranceProvider.Id,
            PolicyNumber = "POL-0001",
            CardNumber = "CARD-0001",
            MemberNumber = "MEM-0001",
            PlanName = "Silver",
            ClassName = "Kelas 2",
            BenefitPlanCode = "BEN-SILVER",
            IsEligible = true,
            IsActive = true,
            IsDelete = false
        };

        var companyGuarantor = new MstCompanyGuarantor
        {
            Id = Guid.NewGuid(),
            CompanyGuarantorCode = "COMP-001",
            CompanyGuarantorName = "PT Sehat Sentosa",
            IsActive = companyMasterActive,
            IsDelete = false
        };

        var patientCompanyGuarantor = new MstPatientCompanyGuarantor
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            CompanyGuarantorId = companyGuarantor.Id,
            EmployeeNumber = "EMP-00125",
            EmployeeName = "Budi Santoso",
            BenefitPlanCode = "BEN-CORP",
            BenefitPlanName = "Corporate Gold",
            ClassName = "Kelas 1",
            EffectiveStartDate = companyEffectiveStartDate,
            EffectiveEndDate = companyEffectiveEndDate,
            IsEligible = companyCardEligible,
            IsActive = companyCardActive,
            IsDelete = false
        };

        context.Set<ApplicationUser>().Add(actorUser);
        context.Set<MstPatient>().AddRange(patient, otherPatient);
        context.Set<MstServiceUnit>().Add(serviceUnit);
        context.Set<MstPatientClass>().Add(patientClass);
        context.Set<MstPaymentMethod>().Add(paymentMethod);
        context.Set<MstInsuranceProvider>().Add(insuranceProvider);
        context.Set<MstPatientInsurance>().Add(patientInsurance);
        context.Set<MstCompanyGuarantor>().Add(companyGuarantor);
        context.Set<MstPatientCompanyGuarantor>().Add(patientCompanyGuarantor);

        await context.SaveChangesAsync();

        return new PatientEncounterTestWorld(
            context,
            patient,
            otherPatient,
            serviceUnit,
            patientClass,
            paymentMethod,
            insuranceProvider,
            patientInsurance,
            companyGuarantor,
            patientCompanyGuarantor);
    }

    /// <summary>Membaca kode status HTTP dari <c>IActionResult</c> aksi controller.</summary>
    public static int KodeStatus(IActionResult result) => result switch
    {
        ObjectResult objectResult => objectResult.StatusCode ?? 0,
        StatusCodeResult statusCodeResult => statusCodeResult.StatusCode,
        _ => 0
    };

    /// <summary>Membaca pesan pada <c>ApiResponse</c> apa pun bentuk generiknya.</summary>
    public static string? Pesan(IActionResult result)
    {
        if (result is not ObjectResult { Value: not null } objectResult)
        {
            return null;
        }

        return objectResult.Value
            .GetType()
            .GetProperty(nameof(ApiResponse<object>.Message))?
            .GetValue(objectResult.Value) as string;
    }

    /// <summary>Membaca payload <c>Data</c> pada <c>ApiResponse&lt;T&gt;</c>.</summary>
    public static T? Data<T>(IActionResult result) where T : class
    {
        if (result is not ObjectResult { Value: not null } objectResult)
        {
            return null;
        }

        return objectResult.Value
            .GetType()
            .GetProperty("Data")?
            .GetValue(objectResult.Value) as T;
    }

    /// <summary>
    /// <c>IHubContext</c> yang tidak mengirim apa pun. Dibutuhkan hanya supaya
    /// <c>QueueRealtimeService</c> dapat dibangun; jalur antrean sendiri dimatikan lewat
    /// <c>IsQueueRequired = false</c> pada unit layanan.
    /// </summary>
    private sealed class FakeQueueHubContext : IHubContext<QueueHub>
    {
        public IHubClients Clients { get; } = new FakeHubClients();

        public IGroupManager Groups { get; } = new FakeGroupManager();
    }

    private sealed class FakeHubClients : IHubClients
    {
        private static readonly IClientProxy Proxy = new FakeClientProxy();

        public IClientProxy All => Proxy;

        public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => Proxy;

        public IClientProxy Client(string connectionId) => Proxy;

        public IClientProxy Clients(IReadOnlyList<string> connectionIds) => Proxy;

        public IClientProxy Group(string groupName) => Proxy;

        public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => Proxy;

        public IClientProxy Groups(IReadOnlyList<string> groupNames) => Proxy;

        public IClientProxy User(string userId) => Proxy;

        public IClientProxy Users(IReadOnlyList<string> userIds) => Proxy;
    }

    private sealed class FakeClientProxy : IClientProxy
    {
        public Task SendCoreAsync(
            string method,
            object?[] args,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeGroupManager : IGroupManager
    {
        public Task AddToGroupAsync(
            string connectionId,
            string groupName,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveFromGroupAsync(
            string connectionId,
            string groupName,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
