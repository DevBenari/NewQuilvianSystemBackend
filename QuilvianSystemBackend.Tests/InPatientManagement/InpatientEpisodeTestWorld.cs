using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Enums;
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
        InpBedOccupancyService bedOccupancyService,
        InpCensusQueryService censusQueryService,
        InpDischargeService dischargeService,
        RecordingLogger<InpSettingService> settingLogger,
        MstPatient patient,
        MstDoctor doctor,
        MstServiceUnit serviceUnit,
        MstPatientClass patientClass)
    {
        DbContext = dbContext;
        EpisodeService = episodeService;
        BedOccupancyService = bedOccupancyService;
        CensusQueryService = censusQueryService;
        DischargeService = dischargeService;
        SettingLogger = settingLogger;
        Patient = patient;
        Doctor = doctor;
        ServiceUnit = serviceUnit;
        PatientClass = patientClass;
    }

    public ApplicationDbContext DbContext { get; }

    public InpEpisodeService EpisodeService { get; }

    public InpBedOccupancyService BedOccupancyService { get; }

    public InpCensusQueryService CensusQueryService { get; }

    public InpDischargeService DischargeService { get; }

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
            // Jenis kelamin sengaja diisi sejak BE-RWI-013. Pasien yang jenis kelaminnya
            // belum tercatat tunduk pada aturan 5 Kelayakan Penempatan, dan bila pasien
            // bawaan dunia uji dibiarkan kosong, seluruh test penempatan akan gagal karena
            // alasan yang salah.
            Gender = Gender.Female,
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

        var episodeService = new InpEpisodeService(
            dbContext,
            settingService,
            numberService);

        // Sejak BE-RWI-011 arah dependency berbalik: InpBedOccupancyService memakai
        // InpEpisodeService untuk memindahkan status episode lewat satu-satunya pintu.
        var bedOccupancyService = new InpBedOccupancyService(
            dbContext,
            settingService,
            episodeService);

        var censusQueryService = new InpCensusQueryService(dbContext, settingService);

        // Sejak BE-RWI-025, penutupan episode dan pencatatan kepergian melepas tempat tidur
        // lewat InpBedOccupancyService. Arahnya tidak melingkar: tidak ada satu pun service
        // yang menunjuk balik ke InpDischargeService.
        var dischargeService = new InpDischargeService(
            dbContext,
            episodeService,
            bedOccupancyService);

        return new InpatientEpisodeTestWorld(
            dbContext,
            episodeService,
            bedOccupancyService,
            censusQueryService,
            dischargeService,
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

    // =========================================================================
    // Tambahan BE-RWI-009 s.d. BE-RWI-022
    // =========================================================================

    /// <summary>Menambahkan satu kamar rawat inap.</summary>
    public async Task<MstRoom> AddRoomAsync(
        string roomName = "Melati 3",
        Guid? patientClassId = null,
        int capacity = 4)
    {
        var room = new MstRoom
        {
            Id = Guid.NewGuid(),
            ServiceUnitId = ServiceUnit.Id,
            PatientClassId = patientClassId ?? PatientClass.Id,
            RoomCode = $"RM-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            RoomName = roomName,
            Capacity = capacity,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        DbContext.Set<MstRoom>().Add(room);
        await DbContext.SaveChangesAsync();

        return room;
    }

    /// <summary>Menambahkan satu tempat tidur beserta penandanya.</summary>
    public async Task<MstBed> AddBedAsync(
        MstRoom room,
        string bedName = "3A",
        bool isForMale = true,
        bool isForFemale = true,
        bool isForNewborn = false,
        bool isIsolationBed = false,
        bool isReservable = true,
        BedStatus bedStatus = BedStatus.Available,
        bool isActive = true)
    {
        var bed = new MstBed
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BedCode = $"BD-RSMMC-{Guid.NewGuid().ToString("N")[..5].ToUpperInvariant()}",
            BedName = bedName,
            BedStatus = bedStatus,
            IsForMale = isForMale,
            IsForFemale = isForFemale,
            IsForNewborn = isForNewborn,
            IsIsolationBed = isIsolationBed,
            IsReservable = isReservable,
            IsActive = isActive,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        DbContext.Set<MstBed>().Add(bed);
        await DbContext.SaveChangesAsync();

        return bed;
    }

    /// <summary>Menambahkan pasien lain, untuk skenario dua pasien pada satu kamar.</summary>
    public async Task<MstPatient> AddPatientAsync(string fullName, Gender? gender)
    {
        var patient = new MstPatient
        {
            Id = Guid.NewGuid(),
            PatientCode = $"PAT-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            MedicalRecordNumber = $"RM-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            FullName = fullName,
            Gender = gender,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        DbContext.Set<MstPatient>().Add(patient);
        await DbContext.SaveChangesAsync();

        return patient;
    }

    /// <summary>Menambahkan dokter lain, untuk skenario dokter yang bukan DPJP aktif.</summary>
    public async Task<MstDoctor> AddDoctorAsync(string fullName)
    {
        var doctor = new MstDoctor
        {
            Id = Guid.NewGuid(),
            WorkforceProfileId = Guid.NewGuid(),
            DoctorCode = $"DR-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            DoctorNumber = Guid.NewGuid().ToString("N")[..4],
            FullName = fullName,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        DbContext.Set<MstDoctor>().Add(doctor);
        await DbContext.SaveChangesAsync();

        return doctor;
    }

    /// <summary>Menambahkan pegawai, untuk penugasan perawat penanggung jawab.</summary>
    public async Task<MstEmployee> AddEmployeeAsync(string fullName)
    {
        var employee = new MstEmployee
        {
            Id = Guid.NewGuid(),
            WorkforceProfileId = Guid.NewGuid(),
            EmployeeCode = $"EMP-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            EmployeeNumber = Guid.NewGuid().ToString("N")[..4],
            FullName = fullName,
            BirthDate = new DateTime(1990, 1, 1),
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        DbContext.Set<MstEmployee>().Add(employee);
        await DbContext.SaveChangesAsync();

        return employee;
    }

    /// <summary>Membuka satu admisi <c>Draft</c> dan mengembalikan episodenya.</summary>
    public async Task<InpEpisode> OpenDraftEpisodeAsync(
        Guid? patientId = null,
        Guid? doctorId = null)
    {
        var request = new OpenAdmissionRequest
        {
            PatientId = patientId ?? Patient.Id,
            ServiceUnitId = ServiceUnit.Id,
            PatientClassId = PatientClass.Id,
            DoctorId = doctorId ?? Doctor.Id
        };

        var result = await EpisodeService.OpenAdmissionAsync(request, ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, result.Status);

        return result.Episode!;
    }

    /// <summary>Membuka admisi lalu langsung menempatkan pasiennya di satu tempat tidur.</summary>
    public async Task<InpEpisode> OpenAndPlaceAsync(MstBed bed, Guid? patientId = null)
    {
        var episode = await OpenDraftEpisodeAsync(patientId);

        var result = await BedOccupancyService.PlacePatientAsync(
            new PlacePatientRequest { EpisodeId = episode.Id, BedId = bed.Id },
            ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, result.Status);

        return episode;
    }

    // =========================================================================
    // Tambahan BE-RWI-023 s.d. BE-RWI-031
    // =========================================================================

    /// <summary>Menambahkan satu butir daftar periksa administrasi.</summary>
    public async Task<MstInpatientClearanceItem> AddClearanceItemAsync(
        string itemName,
        bool isMandatory = true,
        bool isActive = true,
        int sortOrder = 1)
    {
        var item = new MstInpatientClearanceItem
        {
            Id = Guid.NewGuid(),
            ItemCode = $"CLR-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            ItemName = itemName,
            IsMandatory = isMandatory,
            IsActive = isActive,
            SortOrder = sortOrder,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

        DbContext.Set<MstInpatientClearanceItem>().Add(item);
        await DbContext.SaveChangesAsync();

        return item;
    }

    /// <summary>
    /// Membawa satu episode sampai ke keadaan siap ditutup: ditempatkan, diputuskan pulang,
    /// resume tertandatangani, seluruh butir wajib ditandai, dan kelayakan keuangan
    /// <c>Cleared</c>.
    /// </summary>
    public async Task<InpEpisode> BuildClosableEpisodeAsync(
        MstBed bed,
        Guid? patientId = null,
        bool markFinancialCleared = true,
        bool markMandatoryClearance = true)
    {
        var episode = await OpenAndPlaceAsync(bed, patientId);

        var decide = await DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            ActorUserId,
            Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, decide.Status);

        var upsert = await DischargeService.UpsertSummaryAsync(
            episode.Id,
            new UpsertDischargeSummaryRequest { PrimaryDiagnosisText = "Demam berdarah dengue" },
            ActorUserId,
            Doctor.Id,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Success, upsert.Status);

        var sign = await DischargeService.SignSummaryAsync(
            episode.Id,
            null,
            ActorUserId,
            Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, sign.Status);

        if (markMandatoryClearance)
        {
            await MarkAllMandatoryClearanceItemsAsync(episode.Id);
        }

        if (markFinancialCleared)
        {
            var financial = await DischargeService.MarkFinancialClearanceAsync(
                episode.Id,
                new MarkFinancialClearanceRequest
                {
                    ClearanceStatus = (int)InpFinancialClearanceStatus.Cleared,
                    Note = "Tagihan sudah lunas."
                },
                SupervisorUserId,
                actorIsCashierOrBilling: true);

            Assert.Equal(InpEpisodeOperationStatus.Success, financial.Status);
        }

        return episode;
    }

    /// <summary>Menandai seluruh butir wajib yang masih aktif pada satu episode.</summary>
    public async Task MarkAllMandatoryClearanceItemsAsync(Guid episodeId)
    {
        var mandatoryIds = DbContext.Set<MstInpatientClearanceItem>()
            .Where(x => !x.IsDelete && x.IsActive && x.IsMandatory)
            .Select(x => x.Id)
            .ToList();

        foreach (var itemId in mandatoryIds)
        {
            var result = await DischargeService.MarkClearanceItemAsync(
                episodeId,
                itemId,
                new MarkClearanceItemRequest { Note = "Selesai." },
                ActorUserId);

            Assert.Equal(InpEpisodeOperationStatus.Success, result.Status);
        }
    }
}
