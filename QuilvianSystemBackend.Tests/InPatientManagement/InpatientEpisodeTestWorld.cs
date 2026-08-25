using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Data master minimal yang dibutuhkan satu admisi supaya dapat dibuka: pasien, dokter, unit
/// layanan rawat inap, kelas perawatan rawat inap, dan baris pengaturan.
/// </summary>
/// <remarks>
/// Dikumpulkan di satu tempat supaya test membaca sebagai cerita, bukan sebagai daftar
/// penyiapan yang diulang-ulang. Setiap test tetap memakai database sendiri.
/// </remarks>
internal sealed class InpatientEpisodeTestWorld
{
    public static readonly Guid ActorUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid SupervisorUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private InpatientEpisodeTestWorld(
        ApplicationDbContext dbContext,
        InpEpisodeService episodeService,
        RecordingLogger<InpSettingService> settingLogger,
        MstPatient patient,
        MstDoctor doctor,
        MstServiceUnit serviceUnit,
        MstPatientClass patientClass)
    {
        DbContext = dbContext;
        EpisodeService = episodeService;
        SettingLogger = settingLogger;
        Patient = patient;
        Doctor = doctor;
        ServiceUnit = serviceUnit;
        PatientClass = patientClass;
    }

    public ApplicationDbContext DbContext { get; }

    public InpEpisodeService EpisodeService { get; }

    public RecordingLogger<InpSettingService> SettingLogger { get; }

    public MstPatient Patient { get; }

    public MstDoctor Doctor { get; }

    public MstServiceUnit ServiceUnit { get; }

    public MstPatientClass PatientClass { get; }

    /// <summary>
    /// Menyiapkan satu dunia uji yang lengkap. <paramref name="draftEpisodeExpiryHours"/>
    /// diturunkan pada test kedaluwarsa supaya batasnya dapat dilewati tanpa menunggu.
    /// </summary>
    public static async Task<InpatientEpisodeTestWorld> CreateAsync(
        int draftEpisodeExpiryHours = 24,
        string episodeNumberPrefix = "RI",
        ApplicationDbContext? dbContext = null)
    {
        var db = dbContext ?? IsolatedInpatientDbContextFactory.Create();

        var patient = new MstPatient
        {
            Id = Guid.NewGuid(),
            PatientCode = "PAT-0001",
            MedicalRecordNumber = "RM-000123",
            FullName = "Ibu Rina",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        var doctor = new MstDoctor
        {
            Id = Guid.NewGuid(),
            WorkforceProfileId = Guid.NewGuid(),
            DoctorCode = "DR-0001",
            DoctorNumber = "0001",
            FullName = "dr. Andi",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        var serviceUnit = new MstServiceUnit
        {
            Id = Guid.NewGuid(),
            ServiceUnitCode = "RANAP-MELATI",
            ServiceUnitName = "Rawat Inap Melati",
            ServiceUnitType = ServiceUnitType.Inpatient,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        var patientClass = new MstPatientClass
        {
            Id = Guid.NewGuid(),
            PatientClassCode = "KELAS-1",
            PatientClassName = "Kelas 1",
            IsForInpatient = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        var setting = new MstInpatientSetting
        {
            Id = Guid.NewGuid(),
            Code = "DEFAULT",
            Name = "Pengaturan Rawat Inap Default",
            BedReservationMinutes = 120,
            DraftEpisodeExpiryHours = draftEpisodeExpiryHours,
            InitialAssessmentTargetHours = 24,
            ProgressNoteVerificationTargetHours = 24,
            PendingClosureThresholdHours = 4,
            EpisodeNumberPrefix = episodeNumberPrefix,
            IsDefault = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        db.Set<MstPatient>().Add(patient);
        db.Set<MstDoctor>().Add(doctor);
        db.Set<MstServiceUnit>().Add(serviceUnit);
        db.Set<MstPatientClass>().Add(patientClass);
        db.Set<MstInpatientSetting>().Add(setting);

        await db.SaveChangesAsync();

        return Build(db, patient, doctor, serviceUnit, patientClass);
    }

    /// <summary>
    /// Menyusun service di atas context lain yang sudah memuat data master yang sama.
    /// Dipakai test yang perlu context yang gagal menyimpan.
    /// </summary>
    public static InpatientEpisodeTestWorld Build(
        ApplicationDbContext dbContext,
        MstPatient patient,
        MstDoctor doctor,
        MstServiceUnit serviceUnit,
        MstPatientClass patientClass)
    {
        var settingLogger = new RecordingLogger<InpSettingService>();
        var settingService = new InpSettingService(dbContext, settingLogger);
        var numberService = new InpEpisodeNumberService(settingService);
        var bedOccupancyService = new InpBedOccupancyService(dbContext, settingService);

        var episodeService = new InpEpisodeService(
            dbContext,
            settingService,
            numberService,
            bedOccupancyService);

        return new InpatientEpisodeTestWorld(
            dbContext,
            episodeService,
            settingLogger,
            patient,
            doctor,
            serviceUnit,
            patientClass);
    }

    public OpenAdmissionRequest BuildOpenAdmissionRequest(Guid? encounterId = null)
        => new()
        {
            PatientId = Patient.Id,
            EncounterId = encounterId,
            ServiceUnitId = ServiceUnit.Id,
            PatientClassId = PatientClass.Id,
            DoctorId = Doctor.Id,
            Notes = "Operasi terencana."
        };

    public UpdateAdmissionRequest BuildUpdateAdmissionRequest(Guid? patientClassId = null)
        => new()
        {
            ServiceUnitId = ServiceUnit.Id,
            PatientClassId = patientClassId ?? PatientClass.Id,
            Notes = "Isian dibetulkan."
        };

    /// <summary>Menambahkan kunjungan yang sudah ada, untuk jalur admisi bukan datang langsung.</summary>
    public async Task<TrxPatientEncounter> AddEncounterAsync(
        EncounterType encounterType = EncounterType.Inpatient,
        Guid? patientId = null)
    {
        var encounter = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-RSMMC-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            PatientId = patientId ?? Patient.Id,
            ServiceUnitId = ServiceUnit.Id,
            PatientClassId = PatientClass.Id,
            EncounterDate = DateTime.UtcNow,
            EncounterType = encounterType,
            EncounterStatus = EncounterStatus.Registered,
            RegisteredAt = DateTime.UtcNow,
            RegisteredByUserId = ActorUserId,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        DbContext.Set<TrxPatientEncounter>().Add(encounter);
        await DbContext.SaveChangesAsync();

        return encounter;
    }
}
