using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
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
        return new OperatingRoomExecutionService(db, Accessor(userId), Logger(userId),
            IntegrationService(userId, db), Relaxation());
    }

    public OperatingRoomRecoveryService RecoveryService(Guid userId, ApplicationDbContext? context = null)
    {
        var db = context ?? NewContext();
        return new OperatingRoomRecoveryService(db, Accessor(userId), Logger(userId), Relaxation());
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
        return new OperatingRoomIntegrationService(db, Accessor(userId), Logger(userId));
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

    public void Dispose()
    {
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
