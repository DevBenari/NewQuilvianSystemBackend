using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Seeders;

/// <summary>
/// Menyiapkan rantai data minimum agar satu kasus operasi benar-benar dapat dibuat lewat
/// layar, pada database pengembangan yang masih kosong.
/// </summary>
/// <remarks>
/// <para>
/// Seeder ini lahir dari satu kenyataan: <c>OperatingRoomCaseService.CreateAsync</c> menolak
/// permintaan sampai seluruh rantai berikut ada dan saling cocok, sementara penolakannya
/// berupa pesan pendek yang tidak menyebutkan mata rantai mana yang hilang.
/// </para>
/// <code>
/// MstWorkforceType, MstEmployeeCategory, MstEmploymentType, MstEmploymentStatus, MstProfession
///   -&gt; MstWorkforceProfile -&gt; MstDoctor -&gt; AspNetUsers."DoctorId" (klaim doctor_id)
/// MstServiceUnit -&gt; MstPatient -&gt; TrxPatientEncounter -&gt; TrxQueue
///   -&gt; TrxDoctorConsultation -&gt; TrxPatientProcedure (IsSurgeryRelated = true)
///   -&gt; barulah OprCase boleh dibuat
/// </code>
/// <para>Tiga batas yang mengikat seeder ini:</para>
/// <list type="number">
/// <item>
/// MENOLAK berjalan di lingkungan produksi. Pasien, dokter, dan tindakan produksi dibuat
/// petugas lewat layar, bukan oleh baris kode yang ikut terbawa setiap aplikasi dinyalakan.
/// </item>
/// <item>
/// Hanya menambah baris yang belum ada, dikenali lewat kode berawalan <c>DEMO-OPR</c>.
/// Dijalankan berulang kali tidak menggandakan apa pun dan tidak pernah menimpa baris yang
/// sudah tersimpan.
/// </item>
/// <item>
/// TIDAK PERNAH menimpa <c>DoctorId</c> akun yang sudah tertaut dokter. Bila akun sasaran
/// sudah punya dokter, tautan itu dibiarkan apa adanya dan dilaporkan sebagai dilewati.
/// </item>
/// </list>
/// <para>
/// Seeder ini sengaja TIDAK membuat <c>OprCase</c>. Membuat kasus operasi justru itulah yang
/// hendak dibuktikan bisa dilakukan lewat layar.
/// </para>
/// </remarks>
public static class OperatingRoomDemoSeeder
{
    /// <summary>Awalan kode seluruh baris buatan seeder ini, supaya mudah dikenali dan dibersihkan.</summary>
    public const string CodePrefix = "DEMO-OPR";

    /// <summary>Nama lingkungan yang membuat seeder berhenti tanpa menulis apa pun.</summary>
    public const string ProductionEnvironmentName = "Production";

    public static async Task<OperatingRoomDemoSeedResult> SeedAsync(
        ApplicationDbContext db,
        string? environmentName,
        string? targetUserName,
        CancellationToken ct = default)
    {
        var result = new OperatingRoomDemoSeedResult();

        if (string.Equals(environmentName?.Trim(), ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase))
        {
            result.RefusedReason =
                "Seeder demo Operasi menolak berjalan di lingkungan produksi. Data pasien, " +
                "dokter, dan tindakan produksi dibuat petugas lewat layar, bukan lewat seeder.";
            return result;
        }

        var now = DateTime.UtcNow;
        var actor = Guid.Empty;

        // ---------------------------------------------------------------- ketenagakerjaan
        var workforceTypeId = await EnsureAsync(db.MstWorkforceTypes,
            x => x.WorkforceTypeCode == CodePrefix + "-WT",
            () => new MstWorkforceType
            {
                Id = Deterministic("WorkforceType"),
                WorkforceTypeCode = CodePrefix + "-WT",
                WorkforceTypeName = "Tenaga Medis (Demo Operasi)"
            },
            x => x.Id, result, "MstWorkforceType", actor, now, ct);

        var employeeCategoryId = await EnsureAsync(db.MstEmployeeCategories,
            x => x.EmployeeCategoryCode == CodePrefix + "-EC",
            () => new MstEmployeeCategory
            {
                Id = Deterministic("EmployeeCategory"),
                EmployeeCategoryCode = CodePrefix + "-EC",
                EmployeeCategoryName = "Dokter (Demo Operasi)"
            },
            x => x.Id, result, "MstEmployeeCategory", actor, now, ct);

        var employmentTypeId = await EnsureAsync(db.MstEmploymentTypes,
            x => x.EmploymentTypeCode == CodePrefix + "-ET",
            () => new MstEmploymentType
            {
                Id = Deterministic("EmploymentType"),
                EmploymentTypeCode = CodePrefix + "-ET",
                EmploymentTypeName = "Tetap (Demo Operasi)"
            },
            x => x.Id, result, "MstEmploymentType", actor, now, ct);

        var employmentStatusId = await EnsureAsync(db.MstEmploymentStatuses,
            x => x.EmploymentStatusCode == CodePrefix + "-ES",
            () => new MstEmploymentStatus
            {
                Id = Deterministic("EmploymentStatus"),
                EmploymentStatusCode = CodePrefix + "-ES",
                EmploymentStatusName = "Aktif (Demo Operasi)"
            },
            x => x.Id, result, "MstEmploymentStatus", actor, now, ct);

        var professionId = await EnsureAsync(db.MstProfessions,
            x => x.ProfessionCode == CodePrefix + "-PROF",
            () => new MstProfession
            {
                Id = Deterministic("Profession"),
                ProfessionCode = CodePrefix + "-PROF",
                ProfessionName = "Dokter Bedah (Demo Operasi)",
                ProfessionGroup = "Medis"
            },
            x => x.Id, result, "MstProfession", actor, now, ct);

        var profileId = await EnsureAsync(db.MstWorkforceProfiles,
            x => x.ProfileCode == CodePrefix + "-WFP",
            () => new MstWorkforceProfile
            {
                Id = Deterministic("WorkforceProfile"),
                ProfileCode = CodePrefix + "-WFP",
                DisplayName = "dr. Demo Operasi",
                UserType = UserType.PermanentDoctor,
                IsActive = true
            },
            x => x.Id, result, "MstWorkforceProfile", actor, now, ct);

        var doctorId = await EnsureAsync(db.MstDoctors,
            x => x.DoctorCode == CodePrefix + "-DR",
            () => new MstDoctor
            {
                Id = Deterministic("Doctor"),
                DoctorCode = CodePrefix + "-DR",
                DoctorNumber = CodePrefix + "-001",
                FullName = "dr. Demo Operasi, Sp.B",
                WorkforceProfileId = profileId,
                WorkforceTypeId = workforceTypeId,
                EmployeeCategoryId = employeeCategoryId,
                EmploymentTypeId = employmentTypeId,
                EmploymentStatusId = employmentStatusId,
                ProfessionId = professionId,
                IsActive = true
            },
            x => x.Id, result, "MstDoctor", actor, now, ct);

        // ------------------------------------------------------------- pelayanan pasien
        var serviceUnitId = await EnsureAsync(db.MstServiceUnits,
            x => x.ServiceUnitCode == CodePrefix + "-SU",
            () => new MstServiceUnit
            {
                Id = Deterministic("ServiceUnit"),
                ServiceUnitCode = CodePrefix + "-SU",
                ServiceUnitName = "Kamar Operasi (Demo)"
            },
            x => x.Id, result, "MstServiceUnit", actor, now, ct);

        var patientId = await EnsureAsync(db.MstPatients,
            x => x.PatientCode == CodePrefix + "-PT",
            () => new MstPatient
            {
                Id = Deterministic("Patient"),
                PatientCode = CodePrefix + "-PT",
                MedicalRecordNumber = CodePrefix + "-RM-001",
                FullName = "Pasien Demo Operasi"
            },
            x => x.Id, result, "MstPatient", actor, now, ct);

        var encounterId = await EnsureAsync(db.TrxPatientEncounters,
            x => x.EncounterNumber == CodePrefix + "-ENC-001",
            () => new TrxPatientEncounter
            {
                Id = Deterministic("Encounter"),
                EncounterNumber = CodePrefix + "-ENC-001",
                PatientId = patientId,
                ServiceUnitId = serviceUnitId,
                EncounterDate = now
            },
            x => x.Id, result, "TrxPatientEncounter", actor, now, ct);

        var queueId = await EnsureAsync(db.TrxQueues,
            x => x.QueueCode == CodePrefix + "-Q-001",
            () => new TrxQueue
            {
                Id = Deterministic("Queue"),
                QueueCode = CodePrefix + "-Q-001",
                QueueNumber = 1,
                QueueDate = now,
                EncounterId = encounterId,
                PatientId = patientId,
                ServiceUnitId = serviceUnitId
            },
            x => x.Id, result, "TrxQueue", actor, now, ct);

        var consultationId = await EnsureAsync(db.TrxDoctorConsultations,
            x => x.ConsultationNumber == CodePrefix + "-CONS-001",
            () => new TrxDoctorConsultation
            {
                Id = Deterministic("Consultation"),
                ConsultationNumber = CodePrefix + "-CONS-001",
                EncounterId = encounterId,
                QueueId = queueId,
                PatientId = patientId,
                DoctorId = doctorId,
                ServiceUnitId = serviceUnitId,
                ConsultationDateTime = now
            },
            x => x.Id, result, "TrxDoctorConsultation", actor, now, ct);

        // ------------------------------------------------- tindakan yang layak dioperasi
        // IsSurgeryRelated wajib true. Tanpa itu ValidateReferencesAsync menolak dengan
        // "Tindakan tidak ditemukan, tidak aktif, atau bukan tindakan operasi."
        var catalog = new[]
        {
            ("A", CodePrefix + "-PROC-A", "Apendektomi (Demo)"),
            ("B", CodePrefix + "-PROC-B", "Herniotomi (Demo)")
        };

        foreach (var (suffix, code, name) in catalog)
        {
            var procedureId = await EnsureAsync(db.MstProcedures,
                x => x.ProcedureCode == code,
                () => new MstProcedure
                {
                    Id = Deterministic("Procedure" + suffix),
                    ProcedureCode = code,
                    ProcedureName = name,
                    ProcedureType = "Surgery",
                    IsSurgery = true
                },
                x => x.Id, result, "MstProcedure", actor, now, ct);

            var patientProcedureId = await EnsureAsync(db.TrxPatientProcedures,
                x => x.ProcedureCodeSnapshot == code && x.EncounterId == encounterId,
                () => new TrxPatientProcedure
                {
                    Id = Deterministic("PatientProcedure" + suffix),
                    EncounterId = encounterId,
                    ConsultationId = consultationId,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    ProcedureId = procedureId,
                    ServiceUnitId = serviceUnitId,
                    ProcedureCodeSnapshot = code,
                    ProcedureNameSnapshot = name,
                    ProcedureDateTime = now,
                    IsSurgeryRelated = true,
                    IsActive = true
                },
                x => x.Id, result, "TrxPatientProcedure", actor, now, ct);

            result.PatientProcedureIds.Add(patientProcedureId);
        }

        await db.SaveChangesAsync(ct);

        // ------------------------------------------------------ menautkan akun ke dokter
        // Dilakukan setelah SaveChanges supaya baris dokter dipastikan sudah tersimpan.
        var user = string.IsNullOrWhiteSpace(targetUserName)
            ? null
            : await db.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.UserName == targetUserName, ct);

        if (user is null)
        {
            result.UserLinkNote = string.IsNullOrWhiteSpace(targetUserName)
                ? "Tidak ada akun sasaran yang ditentukan; penautan dokter dilewati."
                : "Akun '" + targetUserName + "' tidak ditemukan; penautan dokter dilewati.";
        }
        else if (user.DoctorId.HasValue)
        {
            result.UserLinkNote =
                "Akun '" + user.UserName + "' sudah tertaut dokter " + user.DoctorId +
                "; tautan dibiarkan apa adanya.";
        }
        else
        {
            user.DoctorId = doctorId;
            await db.SaveChangesAsync(ct);
            result.LinkedUserName = user.UserName;
            result.UserLinkNote =
                "Akun '" + user.UserName + "' ditautkan ke dokter demo. WAJIB logout lalu " +
                "login lagi supaya klaim doctor_id ikut berubah pada token.";
        }

        result.DoctorId = doctorId;
        result.PatientId = patientId;
        result.EncounterId = encounterId;
        return result;
    }

    /// <summary>
    /// Mengambil baris yang cocok bila sudah ada, atau menyiapkan baris baru bila belum.
    /// Inilah yang membuat seeder aman dijalankan berulang kali.
    /// </summary>
    private static async Task<Guid> EnsureAsync<TEntity>(
        DbSet<TEntity> set,
        Expression<Func<TEntity, bool>> match,
        Func<TEntity> build,
        Func<TEntity, Guid> selectId,
        OperatingRoomDemoSeedResult result,
        string label,
        Guid actor,
        DateTime now,
        CancellationToken ct) where TEntity : class
    {
        var existing = await set.FirstOrDefaultAsync(match, ct);
        if (existing is not null)
        {
            result.Reused.Add(label);
            return selectId(existing);
        }

        var created = build();
        if (created is IdentityModel audited)
        {
            audited.CreateDateTime = now;
            audited.CreateBy = actor;
        }

        set.Add(created);
        result.Created.Add(label);
        return selectId(created);
    }

    /// <summary>
    /// Membentuk Guid yang selalu sama untuk nama yang sama, supaya seeder yang dijalankan
    /// berulang kali menghasilkan Id tetap dan mudah dilacak di database.
    /// </summary>
    private static Guid Deterministic(string name) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes("OperatingRoomDemoSeeder:" + name))[..16]);
}

/// <summary>Ringkasan apa yang dibuat, dipakai ulang, dan dilewati seeder demo Operasi.</summary>
public sealed class OperatingRoomDemoSeedResult
{
    public string? RefusedReason { get; set; }
    public List<string> Created { get; } = [];
    public List<string> Reused { get; } = [];
    public List<Guid> PatientProcedureIds { get; } = [];
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public Guid EncounterId { get; set; }
    public string? UserLinkNote { get; set; }
    public string? LinkedUserName { get; set; }
}
