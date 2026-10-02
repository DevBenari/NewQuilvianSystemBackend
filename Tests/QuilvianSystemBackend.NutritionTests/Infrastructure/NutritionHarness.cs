using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.Nutrition.Infrastructure;

/// <summary>
/// Satu basis data Gizi berisi data acuan paling kecil yang masih sah, beserta service yang
/// dibangun di atasnya.
/// </summary>
/// <remarks>
/// <para>
/// Aturan yang diuji di sini seluruhnya menolak di dalam service, bukan di controller, sehingga
/// pengujiannya tidak perlu HTTP. Yang tetap dibutuhkan: baris acuan yang nyata, karena hampir
/// setiap aturan Gizi memeriksanya ke basis data — parameter aktif, rumus terdaftar, ahli gizi
/// yang ada, order yang masih berjalan.
/// </para>
/// <para>
/// Nilai batas parameter mengikuti keputusan <c>GIZ-DEC-012</c>: energi kkal/hari, protein,
/// lemak, dan karbohidrat gram/hari, cairan ml/hari. Angka batasnya sengaja dipilih lebar dan
/// jelas sebagai data uji, bukan sebagai anjuran klinis — modul ini tidak menetapkan rumus
/// maupun rentang gizi, dan <c>GIZ-OQ-007</c> masih ditunda pemilik proses.
/// </para>
/// </remarks>
public sealed class NutritionHarness : IDisposable
{
    private readonly TestDatabase _database;
    private readonly List<ApplicationDbContext> _contexts = [];

    public NutritionHarness()
    {
        _database = TestDatabase.Create();

        using var konteks = CreateContext();

        // Lima parameter dan tiga domain diagnosis sudah ditanam konfigurasi EF lewat `HasData`,
        // karena `GIZ-DEC-011` dan `GIZ-DEC-012` menyebut keduanya langsung. Harness ini MEMBACA
        // baris itu, tidak membuat versinya sendiri: parameter buatan sendiri akan membuat
        // `GIZ015` diuji terhadap daftar yang tidak pernah dipakai produksi, dan domainnya pun
        // bentrok pada indeks unik `DomainCode`.
        Parameter = konteks.GziNutritionParameters.OrderBy(x => x.SortOrder).ToList();
        Energi = Parameter.Single(x => x.ParameterCode == "ENERGY");
        Protein = Parameter.Single(x => x.ParameterCode == "PROTEIN");
        Cairan = Parameter.Single(x => x.ParameterCode == "FLUID");
        DomainNiId = konteks.GziNutritionDiagnosisDomains
            .Single(x => x.DomainCode == "NI").Id;

        konteks.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = AhliGiziId,
            ProfileCode = "UJI-GZI-WFP",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.GziNutritionFormulas.Add(new GziNutritionFormula
        {
            Id = RumusId,
            FormulaCode = "UJI-GZI-RUMUS",
            FormulaName = "Rumus uji",
            FormulaVersion = "1.0",
            ImplementationKey = "uji",
            SourceReference = "Data uji; bukan rumus klinis",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        // Master diagnosisnya sendiri sengaja dibiarkan kosong oleh konfigurasi — isi IDNT
        // adalah keputusan instalasi gizi, bukan milik kode. Tiga baris di bawah data uji.
        konteks.GziNutritionDiagnoses.AddRange(
            Diagnosis(DiagnosisAId, "NI-1.1", "Asupan energi tidak adekuat"),
            Diagnosis(DiagnosisBId, "NI-1.2", "Asupan energi berlebih"),
            // Tidak dapat dipilih. `GIZ005` harus menolaknya walaupun barisnya aktif.
            Diagnosis(DiagnosisTerlarangId, "NI-1", "Keseimbangan energi", dapatDipilih: false));

        konteks.GziNutritionOrders.Add(OrderBaru(OrderId, "UJI-GZI-ORD-001"));
        konteks.GziNutritionOrders.Add(OrderBaru(OrderTertutupId, "UJI-GZI-ORD-002",
            GziOrderStatus.Closed));

        konteks.GziNutritionCareRecords.Add(new GziNutritionCareRecord
        {
            Id = KunjunganId,
            NutritionOrderId = OrderId,
            RecordType = GziCareRecordType.Initial,
            RecordedByWorkforceId = AhliGiziId,
            VisitAt = DateTime.UtcNow,
            VisitSequence = 1,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.SaveChanges();
    }

    public Guid AhliGiziId { get; } = Guid.NewGuid();
    public Guid RumusId { get; } = Guid.NewGuid();
    public Guid OrderId { get; } = Guid.NewGuid();
    public Guid OrderTertutupId { get; } = Guid.NewGuid();
    public Guid KunjunganId { get; } = Guid.NewGuid();
    public Guid DomainNiId { get; }
    public Guid DiagnosisAId { get; } = Guid.NewGuid();
    public Guid DiagnosisBId { get; } = Guid.NewGuid();
    public Guid DiagnosisTerlarangId { get; } = Guid.NewGuid();
    public Guid PenggunaId { get; } = Guid.NewGuid();

    public List<GziNutritionParameter> Parameter { get; }
    public GziNutritionParameter Energi { get; }
    public GziNutritionParameter Protein { get; }
    public GziNutritionParameter Cairan { get; }

    public List<string> LogEntries { get; } = [];

    public ApplicationDbContext CreateContext()
    {
        var konteks = _database.CreateContext();
        _contexts.Add(konteks);
        return konteks;
    }

    public NutritionRequirementService RequirementService(ApplicationDbContext konteks)
    {
        var accessor = Accessor(PenggunaId);
        return new NutritionRequirementService(
            konteks,
            accessor,
            new LoggerService(new CapturingLogger(LogEntries), Accessor(PenggunaId)),
            new NutritionRequirementCalculator([]));
    }

    private GziNutritionDiagnosis Diagnosis(Guid id, string kode, string nama,
        bool dapatDipilih = true) => new()
    {
        Id = id,
        DiagnosisDomainId = DomainNiId,
        DiagnosisCode = kode,
        DiagnosisName = nama,
        Standard = "IDNT",
        IsSelectable = dapatDipilih,
        IsActive = true,
        CreateDateTime = DateTime.UtcNow
    };

    private GziNutritionOrder OrderBaru(Guid id, string nomor,
        GziOrderStatus status = GziOrderStatus.Requested) => new()
    {
        Id = id,
        OrderNumber = nomor,
        PatientId = Guid.NewGuid(),
        EncounterId = Guid.NewGuid(),
        RequesterDoctorId = Guid.NewGuid(),
        AssignedWorkforceId = AhliGiziId,
        Status = status,
        Priority = GziOrderPriority.Routine,
        ReasonForReferral = "Data uji",
        RequestedAt = DateTime.UtcNow,
        ClosedAt = status == GziOrderStatus.Closed ? DateTime.UtcNow : null,
        CreateDateTime = DateTime.UtcNow
    };

    private static IHttpContextAccessor Accessor(Guid userId)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Uji"))
        };

        // Bukan HttpContextAccessor bawaan: yang bawaan menyimpan konteksnya pada AsyncLocal
        // statis, sehingga accessor yang dibuat belakangan menimpa konteks milik service yang
        // dibuat lebih dulu dan klaim penggunanya hilang.
        return new AccessorTetap(context);
    }

    private sealed class AccessorTetap(HttpContext context) : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; } = context;
    }

    private sealed class CapturingLogger(List<string> sink) : ILogger<LoggerService>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter) =>
            sink.Add(formatter(state, exception));
    }

    public void Dispose()
    {
        foreach (var konteks in _contexts) konteks.Dispose();
        _database.Dispose();
    }
}
