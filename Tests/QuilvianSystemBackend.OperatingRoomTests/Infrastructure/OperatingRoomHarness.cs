using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

/// <summary>
/// Menyusun service modul Operasi persis seperti aplikasi menyusunnya, tetapi di atas basis
/// data uji dan dengan pengguna yang dapat ditentukan tiap pemanggilan.
/// </summary>
/// <remarks>
/// <para>
/// Yang dipalsukan hanya dua hal, dan keduanya bukan aturan: konteks HTTP, supaya identitas
/// pemanggil dapat diatur, dan penampung log, supaya isi jejak audit dapat diperiksa. Seluruh
/// aturan klinis, validasi, dan penyimpanan berjalan memakai kode yang sama dengan produksi.
/// </para>
/// <para>
/// Setiap pemanggilan membuat <see cref="ApplicationDbContext"/> baru di atas basis data yang
/// sama. Itu disengaja: dua perintah yang berjalan lewat konteks berbeda membuktikan penjagaan
/// versi dan idempotensi bekerja di basis data, bukan sekadar di change tracker satu konteks.
/// </para>
/// </remarks>
public sealed class OperatingRoomHarness : IDisposable
{
    private readonly TestDatabase _database;
    private readonly List<ApplicationDbContext> _contexts = [];
    private readonly List<ServiceProvider> _providers = [];

    public OperatingRoomHarness(bool relaxed = false)
    {
        _database = TestDatabase.Create();
        Relaxed = relaxed;
    }

    public bool Relaxed { get; }

    /// <summary>Jejak log yang tertangkap, dipakai uji audit dan privasi.</summary>
    public List<string> LogEntries { get; } = [];

    public ApplicationDbContext NewContext()
    {
        var context = _database.CreateContext();
        _contexts.Add(context);
        return context;
    }

    public OperatingRoomCaseService CaseService(Guid userId, Guid? doctorId = null,
        ApplicationDbContext? context = null)
    {
        var db = context ?? NewContext();
        return new OperatingRoomCaseService(db, Accessor(userId, doctorId), Logger(userId), Relaxation());
    }

    public OperatingRoomSchedulingService SchedulingService(Guid userId, ApplicationDbContext? context = null)
    {
        var db = context ?? NewContext();
        return new OperatingRoomSchedulingService(db, Accessor(userId), Logger(userId),
            new OperatingRoomCredentialResolver(db),
            Options.Create(new OperatingRoomSchedulingOptions()), Relaxation());
    }

    public OperatingRoomPreparationService PreparationService(Guid userId, ApplicationDbContext? context = null)
    {
        var db = context ?? NewContext();
        return new OperatingRoomPreparationService(db, Accessor(userId), Logger(userId), Relaxation());
    }

    public OperatingRoomExecutionService ExecutionService(Guid userId, ApplicationDbContext? context = null)
    {
        var db = context ?? NewContext();
        return Susun<OperatingRoomExecutionService>(userId, db);
    }

    public OperatingRoomRecoveryService RecoveryService(Guid userId, ApplicationDbContext? context = null)
    {
        var db = context ?? NewContext();
        return Susun<OperatingRoomRecoveryService>(userId, db);
    }

    public OperatingRoomMaterialService MaterialService(Guid userId, ApplicationDbContext? context = null)
    {
        var db = context ?? NewContext();
        return new OperatingRoomMaterialService(db, Accessor(userId), Logger(userId),
            IntegrationService(userId, db), Relaxation(), new DrugUnitConversionResolver(db));
    }

    public OperatingRoomIntegrationService IntegrationService(Guid userId, ApplicationDbContext? context = null)
    {
        var db = context ?? NewContext();
        return Susun<OperatingRoomIntegrationService>(userId, db);
    }

    private OperatingRoomRuleRelaxation Relaxation()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OperatingRoom:RelaxClinicalRules"] = Relaxed ? "true" : "false"
            })
            .Build();
        return new OperatingRoomRuleRelaxation(configuration, new TestHostEnvironment());
    }

    private LoggerService Logger(Guid userId) =>
        new(new CapturingLogger(LogEntries), Accessor(userId));

    private static IHttpContextAccessor Accessor(Guid userId, Guid? doctorId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        if (doctorId.HasValue) claims.Add(new Claim("doctor_id", doctorId.Value.ToString()));

        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Uji"))
        };

        // Bukan HttpContextAccessor bawaan. Yang bawaan menyimpan konteksnya pada AsyncLocal
        // statis, sehingga accessor yang dibuat belakangan — misalnya milik logger — menimpa
        // konteks milik service yang dibuat lebih dulu, dan klaim dokternya ikut hilang.
        return new AccessorTetap(context);
    }

    private sealed class AccessorTetap(HttpContext context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }

    /// <summary>
    /// Menyusun satu service beserta seluruh rantai dependensinya lewat container DI, di atas
    /// konteks yang diberikan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dipakai untuk service yang rantai dependensinya dalam dan melintasi modul — pelaksanaan,
    /// pemulihan, dan integrasi Operasi kini menarik <c>ClinicalMilestoneFactProducer</c>,
    /// <c>PatientProcedureExecutionService</c>, <c>InpAdmissionReferralService</c>, dan
    /// seterusnya. Merangkainya dengan tangan berarti harness ini pecah setiap kali salah satu
    /// konstruktor di modul lain berubah, dan itu sudah terjadi.
    /// </para>
    /// <para>
    /// Konteksnya didaftarkan sebagai <b>instance</b>, bukan pabrik, supaya seluruh rantai
    /// memakai konteks yang sama persis dengan yang dipegang uji. Kalau tidak, uji memeriksa
    /// keadaan pada konteks yang berbeda dari yang dilihat service.
    /// </para>
    /// </remarks>
    private T Susun<T>(Guid userId, ApplicationDbContext db) where T : notnull
    {
        var layanan = new ServiceCollection();

        layanan.AddLogging();
        layanan.AddSingleton<IHttpContextAccessor>(_ => Accessor(userId));
        layanan.AddSingleton(db);
        layanan.AddSingleton(_ => Logger(userId));
        layanan.AddSingleton(_ => Relaxation());

        // Seluruh service yang ikut tertarik rantai dependensi Operasi. Didaftarkan dengan
        // tipenya sendiri supaya DI yang menentukan urutan pembangunannya, bukan harness.
        foreach (var tipe in new[]
        {
            typeof(OperatingRoomExecutionService),
            typeof(OperatingRoomRecoveryService),
            typeof(OperatingRoomIntegrationService),
            typeof(OperatingRoomCompletionEffects),
            typeof(InpPatientLocationQuery),
            typeof(InpAdmissionReferralService),
            typeof(InpSettingService),
            typeof(PatientProcedureExecutionService),
            typeof(PatientProcedureOrderService),
            typeof(ClinicalDocumentIntegrityService),
            typeof(ClinicalMilestoneFactProducer),
            typeof(InpatientClinicalContextService),
            typeof(InsuranceCoverageService),
            typeof(EncounterInsuranceService),
            typeof(NursingActorService),
            typeof(BillingFolioService),
            typeof(BillingClinicalChargeBridgeService)
        })
        {
            layanan.AddScoped(tipe);
        }

        var penyedia = layanan.BuildServiceProvider();
        _providers.Add(penyedia);

        return penyedia.GetRequiredService<T>();
    }

    public void Dispose()
    {
        foreach (var penyedia in _providers) penyedia.Dispose();
        foreach (var context in _contexts) context.Dispose();
        _database.Dispose();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "QuilvianSystemBackend.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    private sealed class CapturingLogger(List<string> sink) : ILogger<LoggerService>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => sink.Add(formatter(state, exception));
    }
}
