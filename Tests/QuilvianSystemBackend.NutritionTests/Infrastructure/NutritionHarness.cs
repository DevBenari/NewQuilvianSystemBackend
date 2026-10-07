using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
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

        // Pasien, kunjungan, dan dokter pemohon. `GIZ001` memeriksa ketiganya ke basis data, dan
        // kunjungan harus benar-benar milik pasien itu — bukan sekadar ada.
        konteks.Set<MstPatient>().Add(new MstPatient
        {
            Id = PasienId,
            FullName = "Pasien Uji Gizi",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<RegPatientEncounter>().Add(new RegPatientEncounter
        {
            Id = KunjunganRawatId,
            EncounterNumber = "UJI-GZI-ENC-1",
            PatientId = PasienId,
            CreateDateTime = DateTime.UtcNow
        });

        // `MstDoctor.WorkforceProfileId` berindeks unik, jadi kedua dokter uji harus menunjuk
        // profil yang berbeda — dibiarkan kosong, keduanya bentrok pada nilai default yang sama.
        konteks.MstDoctors.Add(new MstDoctor
        {
            Id = DokterId,
            DoctorCode = "UJI-GZI-DR-1",
            WorkforceProfileId = AhliGiziId,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        // Dokter tidak aktif, ada dengan sengaja: tanpa baris ini `GIZ001` tidak terbukti
        // memeriksa keaktifan, hanya keberadaan.
        konteks.MstDoctors.Add(new MstDoctor
        {
            Id = DokterNonaktifId,
            DoctorCode = "UJI-GZI-DR-2",
            WorkforceProfileId = AhliGiziNonaktifId,
            IsActive = false,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = AhliGiziNonaktifId,
            ProfileCode = "UJI-GZI-WFP-OFF",
            IsActive = false,
            CreateDateTime = DateTime.UtcNow
        });

        // Episode rawat inap aktif. `PrescribeAsync` menuntutnya — diet pasien hanya bermakna
        // selama pasien benar-benar dirawat, dan `GIZ001` memeriksanya ke `InpEpisode`.
        konteks.Set<InpEpisode>().Add(new InpEpisode
        {
            Id = Guid.NewGuid(),
            EpisodeNumber = "UJI-GZI-EP-1",
            EncounterId = KunjunganRawatId,
            PatientId = PasienId,
            ServiceUnitId = Guid.NewGuid(),
            PatientClassId = Guid.NewGuid(),
            EpisodeStatus = InpEpisodeStatus.Admitted,
            AdmittedAt = DateTime.UtcNow,
            CreateDateTime = DateTime.UtcNow
        });

        // Episode yang sudah selesai, pada kunjungan tersendiri: tanpa baris ini aturan "sudah
        // tidak aktif" tidak terbukti, hanya aturan "tidak ditemukan". Status aktif menurut
        // service adalah `Admitted` dan `DischargePending`, jadi `Closed` yang dipakai di sini.
        konteks.Set<InpEpisode>().Add(new InpEpisode
        {
            Id = Guid.NewGuid(),
            EpisodeNumber = "UJI-GZI-EP-2",
            EncounterId = KunjunganPulangId,
            PatientId = PasienId,
            ServiceUnitId = Guid.NewGuid(),
            PatientClassId = Guid.NewGuid(),
            EpisodeStatus = InpEpisodeStatus.Closed,
            AdmittedAt = DateTime.UtcNow.AddDays(-5),
            CreateDateTime = DateTime.UtcNow
        });

        konteks.GziDietTypes.Add(new GziDietType
        {
            Id = JenisDietId,
            DietTypeCode = "UJI-DIET-1",
            DietTypeName = "Diet Biasa (Uji)",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.GziDietTypes.Add(new GziDietType
        {
            Id = JenisDietNonaktifId,
            DietTypeCode = "UJI-DIET-OFF",
            DietTypeName = "Diet Nonaktif (Uji)",
            IsActive = false,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.GziFoodForms.Add(new GziFoodForm
        {
            Id = BentukMakananId,
            FoodFormCode = "UJI-FORM-1",
            FoodFormName = "Makanan Biasa (Uji)",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.GziFoodForms.Add(new GziFoodForm
        {
            Id = BentukMakananNonaktifId,
            FoodFormCode = "UJI-FORM-OFF",
            FoodFormName = "Bentuk Nonaktif (Uji)",
            IsActive = false,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.GziMealSchedules.Add(new GziMealSchedule
        {
            Id = JadwalMakanId,
            MealScheduleCode = "UJI-JADWAL-1",
            MealScheduleName = "Makan Siang (Uji)",
            ServingTime = new TimeOnly(12, 0),
            IsMainMeal = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        // Jadwal makan kedua, dipakai menguji bahwa larangan batch ganda terikat pada PASANGAN
        // tanggal dan jadwal — bukan pada tanggalnya saja.
        konteks.GziMealSchedules.Add(new GziMealSchedule
        {
            Id = JadwalMakanLainId,
            MealScheduleCode = "UJI-JADWAL-2",
            MealScheduleName = "Makan Malam (Uji)",
            ServingTime = new TimeOnly(18, 0),
            IsMainMeal = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.GziMealSchedules.Add(new GziMealSchedule
        {
            Id = JadwalMakanNonaktifId,
            MealScheduleCode = "UJI-JADWAL-OFF",
            MealScheduleName = "Jadwal Nonaktif (Uji)",
            ServingTime = new TimeOnly(21, 0),
            IsMainMeal = false,
            IsActive = false,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<RegPatientEncounter>().Add(new RegPatientEncounter
        {
            Id = KunjunganPulangId,
            EncounterNumber = "UJI-GZI-ENC-2",
            PatientId = PasienId,
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
    public Guid AhliGiziNonaktifId { get; } = Guid.NewGuid();
    public Guid PasienId { get; } = Guid.NewGuid();
    public Guid KunjunganRawatId { get; } = Guid.NewGuid();
    public Guid DokterId { get; } = Guid.NewGuid();
    public Guid DokterNonaktifId { get; } = Guid.NewGuid();
    public Guid KunjunganPulangId { get; } = Guid.NewGuid();
    public Guid JenisDietId { get; } = Guid.NewGuid();
    public Guid JenisDietNonaktifId { get; } = Guid.NewGuid();
    public Guid BentukMakananId { get; } = Guid.NewGuid();
    public Guid BentukMakananNonaktifId { get; } = Guid.NewGuid();
    public Guid JadwalMakanId { get; } = Guid.NewGuid();
    public Guid JadwalMakanLainId { get; } = Guid.NewGuid();
    public Guid JadwalMakanNonaktifId { get; } = Guid.NewGuid();
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

    public NutritionReportService ReportService(ApplicationDbContext konteks) => new(konteks);

    public NutritionDietService DietService(ApplicationDbContext konteks) =>
        new(konteks,
            Accessor(PenggunaId),
            new LoggerService(new CapturingLogger(LogEntries), Accessor(PenggunaId)));

    /// <remarks>
    /// <c>InpatientClinicalContextService</c> menyusul sebagai dependensi wajib dari integration;
    /// ia menentukan profesi penulis dan unit perawatan pasien (<c>GUARD-INP-08</c>). Dibangun di
    /// atas konteks yang sama supaya ia melihat keadaan yang sama dengan service ordernya.
    /// </remarks>
    public NutritionOrderService OrderService(ApplicationDbContext konteks) =>
        new(konteks,
            Accessor(PenggunaId),
            new LoggerService(new CapturingLogger(LogEntries), Accessor(PenggunaId)),
            new InpatientClinicalContextService(konteks));

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
