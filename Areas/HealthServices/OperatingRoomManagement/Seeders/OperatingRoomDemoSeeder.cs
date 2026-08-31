using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
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
/// Secara bawaan seeder ini TIDAK membuat <c>OprCase</c>, karena membuat kasus operasi
/// justru itulah yang hendak dibuktikan bisa dilakukan lewat layar. Parameter
/// <paramref name="createCase"/> membukanya sebagai jalan pintas ketika alur SESUDAH
/// pembuatan — penjadwalan, persiapan, pelaksanaan, pemulihan — perlu dicoba lebih dulu
/// sementara form pembuatannya masih bermasalah. Kasus hasil jalan pintas itu bukan bukti
/// bahwa pembuatan lewat layar berfungsi, dan riwayat statusnya ditandai
/// <c>Seeder:OperatingRoomDemo</c> supaya perbedaannya tetap terbaca kemudian.
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
        bool createCase = false,
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

        // Akun sasaran dicari lebih dulu, bukan di akhir, karena TrxPatientEncounter punya
        // kolom RegisteredByUserId yang ber-foreign key ke AspNetUsers dan tidak boleh kosong.
        // Mengisinya Guid.Empty membuat PostgreSQL menolak dengan 23503.
        var user = string.IsNullOrWhiteSpace(targetUserName)
            ? null
            : await db.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.UserName == targetUserName, ct);

        if (user is null)
        {
            result.RefusedReason =
                "Seeder demo Operasi membutuhkan satu akun yang sudah ada untuk dicatat sebagai " +
                "pendaftar kunjungan, karena TrxPatientEncounter.RegisteredByUserId ber-foreign key " +
                "ke AspNetUsers. Akun sasaran " +
                (string.IsNullOrWhiteSpace(targetUserName) ? "belum ditentukan" : "'" + targetUserName + "' tidak ditemukan") +
                ". Pastikan SuperAdminSeeder sudah berjalan, atau isi Seeders:OperatingRoomDemoTargetUserName.";
            return result;
        }

        var actor = user.Id;

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

        // ------------------------------------------------------------------ anggota tim
        // Penjadwalan mewajibkan empat peran terisi: dokter bedah, dokter anestesi,
        // perawat instrumen, dan perawat sirkuler. Dokter bedah memakai profil di atas;
        // tiga sisanya dibuat di sini. Tanpa mereka penjadwalan ditolak dengan OPR004.
        var teamProfileIds = new Dictionary<string, Guid>();

        var teamProfiles = new[]
        {
            ("Anestesi", "-WFP-ANEST", "dr. Demo Anestesi, Sp.An", UserType.PermanentDoctor),
            ("PerawatInstrumen", "-WFP-SCRUB", "Perawat Instrumen Demo", UserType.Employee),
            ("PerawatSirkuler", "-WFP-CIRC", "Perawat Sirkuler Demo", UserType.Employee)
        };

        foreach (var (key, suffix, displayName, userType) in teamProfiles)
        {
            var teamProfileId = await EnsureAsync(db.MstWorkforceProfiles,
                x => x.ProfileCode == CodePrefix + suffix,
                () => new MstWorkforceProfile
                {
                    Id = Deterministic("WorkforceProfile" + key),
                    ProfileCode = CodePrefix + suffix,
                    DisplayName = displayName,
                    UserType = userType,
                    IsActive = true
                },
                x => x.Id, result, "MstWorkforceProfile", actor, now, ct);

            result.TeamWorkforceIds.Add(teamProfileId);
            teamProfileIds[key] = teamProfileId;
        }

        // Profil tenaga saja tidak cukup untuk muncul di layar penjadwalan. Dropdown peran
        // dokter membaca MstDoctor, dan dropdown peran perawat membaca MstEmployee; keduanya
        // tidak pernah membaca MstWorkforceProfile secara langsung. Tanpa baris di bawah ini
        // tenaga demo ada di basis data tetapi tidak dapat dipilih siapa pun.
        var departmentId = await EnsureAsync(db.MstDepartments,
            x => x.DepartmentCode == CodePrefix + "-DEPT",
            () => new MstDepartment
            {
                Id = Deterministic("Department"),
                DepartmentCode = CodePrefix + "-DEPT",
                DepartmentName = "Kamar Operasi (Demo)"
            },
            x => x.Id, result, "MstDepartment", actor, now, ct);

        var positionId = await EnsureAsync(db.MstPositions,
            x => x.PositionCode == CodePrefix + "-POS",
            () => new MstPosition
            {
                Id = Deterministic("Position"),
                PositionCode = CodePrefix + "-POS",
                PositionName = "Perawat Kamar Operasi (Demo)",
                DepartmentId = departmentId
            },
            x => x.Id, result, "MstPosition", actor, now, ct);

        await EnsureAsync(db.MstDoctors,
            x => x.DoctorCode == CodePrefix + "-DR-ANEST",
            () => new MstDoctor
            {
                Id = Deterministic("DoctorAnest"),
                DoctorCode = CodePrefix + "-DR-ANEST",
                DoctorNumber = CodePrefix + "-002",
                FullName = "dr. Demo Anestesi, Sp.An",
                WorkforceProfileId = teamProfileIds["Anestesi"],
                WorkforceTypeId = workforceTypeId,
                EmployeeCategoryId = employeeCategoryId,
                EmploymentTypeId = employmentTypeId,
                EmploymentStatusId = employmentStatusId,
                ProfessionId = professionId,
                IsActive = true
            },
            x => x.Id, result, "MstDoctor", actor, now, ct);

        foreach (var (key, suffix, fullName, identityNumber) in new[]
        {
            ("PerawatInstrumen", "-EMP-SCRUB", "Perawat Instrumen Demo", "3200000000000001"),
            ("PerawatSirkuler", "-EMP-CIRC", "Perawat Sirkuler Demo", "3200000000000002")
        })
        {
            await EnsureAsync(db.MstEmployees,
                x => x.EmployeeCode == CodePrefix + suffix,
                () => new MstEmployee
                {
                    Id = Deterministic("Employee" + key),
                    EmployeeCode = CodePrefix + suffix,
                    EmployeeNumber = CodePrefix + suffix,
                    FullName = fullName,
                    WorkforceProfileId = teamProfileIds[key],
                    BirthDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IdentityType = "KTP",
                    // NIK dibatasi 16 karakter; memakai kode DEMO-OPR di sini membuat
                    // PostgreSQL menolak seluruh penyimpanan dengan 22001.
                    IdentityNumber = identityNumber,
                    Email = key.ToLowerInvariant() + ".demo@contoh.invalid",
                    PrimaryDepartmentId = departmentId,
                    PrimaryPositionId = positionId,
                    WorkforceTypeId = workforceTypeId,
                    EmployeeCategoryId = employeeCategoryId,
                    EmploymentTypeId = employmentTypeId,
                    EmploymentStatusId = employmentStatusId,
                    JoinDate = now,
                    IsActive = true
                },
                x => x.Id, result, "MstEmployee", actor, now, ct);
        }

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

        // Ruang operasi. RoomType WAJIB OperatingRoom; penjadwalan menyaring tepat pada
        // nilai itu, sehingga kamar bertipe lain ditolak dengan "Ruang operasi tidak
        // ditemukan atau tidak aktif" walaupun kamarnya ada dan aktif.
        var roomId = await EnsureAsync(db.MstRooms,
            x => x.RoomCode == CodePrefix + "-OK1",
            () => new MstRoom
            {
                Id = Deterministic("Room"),
                RoomCode = CodePrefix + "-OK1",
                RoomName = "OK 1 (Demo)",
                RoomType = RoomType.OperatingRoom,
                ServiceUnitId = serviceUnitId,
                Capacity = 1,
                IsActive = true
            },
            x => x.Id, result, "MstRoom", actor, now, ct);

        // Unit tujuan untuk serah terima pasca-recovery. Dibuat terpisah dari unit kamar
        // operasi supaya perpindahan pasien benar-benar berpindah unit, bukan ke dirinya.
        var destinationUnitId = await EnsureAsync(db.MstServiceUnits,
            x => x.ServiceUnitCode == CodePrefix + "-SU-RANAP",
            () => new MstServiceUnit
            {
                Id = Deterministic("ServiceUnitDestination"),
                ServiceUnitCode = CodePrefix + "-SU-RANAP",
                ServiceUnitName = "Rawat Inap Bedah (Demo)"
            },
            x => x.Id, result, "MstServiceUnit", actor, now, ct);

        // Bahan dan implan untuk pencatatan pemakaian material. Modul Operasi membacanya
        // dari master farmasi, bukan dari master miliknya sendiri.
        var drugCategoryId = await EnsureAsync(db.MstDrugCategories,
            x => x.DrugCategoryCode == CodePrefix + "-DCAT",
            () => new MstDrugCategory
            {
                Id = Deterministic("DrugCategory"),
                DrugCategoryCode = CodePrefix + "-DCAT",
                DrugCategoryName = "Bahan Habis Pakai Operasi (Demo)"
            },
            x => x.Id, result, "MstDrugCategory", actor, now, ct);

        foreach (var (suffix, code, name) in new[]
        {
            ("Consumable", CodePrefix + "-ITEM-A", "Kasa Steril (Demo)"),
            ("Implant", CodePrefix + "-ITEM-B", "Mesh Hernia (Demo)")
        })
        {
            var drugId = await EnsureAsync(db.MstDrugs,
                x => x.DrugCode == code,
                () => new MstDrug
                {
                    Id = Deterministic("Drug" + suffix),
                    DrugCode = code,
                    DrugName = name,
                    DrugCategoryId = drugCategoryId,
                    IsActive = true
                },
                x => x.Id, result, "MstDrug", actor, now, ct);

            result.MaterialItemIds.Add(drugId);
        }

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
                EncounterDate = now,
                RegisteredByUserId = actor
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

        // ------------------------------------------------------------------- consent
        // Persiapan menuntut dua consent sah — tindakan operasi dan anestesi — sebelum
        // kasus boleh naik ke status Ready. Tanpa keduanya kasus mentok di Scheduled
        // dengan keterangan "Consent tindakan operasi belum sah".
        foreach (var (key, code, type, title) in new[]
        {
            ("Surgery", CodePrefix + "-CONS-OP", PatientConsentType.Surgery,
                "Persetujuan Tindakan Operasi (Demo)"),
            ("Anesthesia", CodePrefix + "-CONS-AN", PatientConsentType.Anesthesia,
                "Persetujuan Tindakan Anestesi (Demo)")
        })
        {
            await EnsureAsync(db.Set<TrxPatientConsent>(),
                x => x.ConsentNumber == code,
                () => new TrxPatientConsent
                {
                    Id = Deterministic("Consent" + key),
                    ConsentNumber = code,
                    PatientId = patientId,
                    EncounterId = encounterId,
                    ConsentType = type,
                    ConsentStatus = PatientConsentStatus.Signed,
                    ConsentTitle = title,
                    SignerName = "Penanggung Jawab Pasien Demo",
                    SignedAt = now,
                    IsDiagnosisExplained = true,
                    IsProcedureExplained = true,
                    IsRiskExplained = true,
                    IsAlternativeExplained = true,
                    IsPatientUnderstood = true
                },
                x => x.Id, result, "TrxPatientConsent", actor, now, ct);
        }

        await db.SaveChangesAsync(ct);

        // ------------------------------------------------------------ satu kasus operasi
        // Hanya dibuat bila diminta. Bacalah catatan pada createCase di ringkasan hasil:
        // kasus buatan seeder membuktikan alur SESUDAH pembuatan, bukan pembuatannya.
        if (createCase)
        {
            await EnsureDemoCaseAsync(db, result, patientId, encounterId, doctorId, actor, now, ct);
            await db.SaveChangesAsync(ct);
        }

        // ------------------------------------------------------ menautkan akun ke dokter
        // Dilakukan setelah SaveChanges supaya baris dokter dipastikan sudah tersimpan.
        // Dua tautan, bukan satu, dan keduanya dipakai untuk hal yang berbeda:
        //
        //   DoctorId          -> klaim doctor_id, dipakai saat MEMBUAT permintaan operasi
        //   WorkforceProfileId -> dipakai setiap tindakan klinis untuk memeriksa apakah
        //                         pengguna benar-benar anggota tim operasi ini
        //
        // Menautkan DoctorId saja membuat pembuatan kasus berhasil tetapi seluruh sign-off,
        // pelaksanaan, material, dan recovery ditolak dengan "Akun pengguna tidak terhubung
        // dengan data tenaga." Penolakan itu berstatus 403 sehingga mudah disangka masalah
        // izin, padahal bukan.
        var changed = new List<string>();

        if (!user.DoctorId.HasValue)
        {
            user.DoctorId = doctorId;
            changed.Add("DoctorId");
        }

        if (!user.WorkforceProfileId.HasValue || user.WorkforceProfileId.Value == Guid.Empty)
        {
            user.WorkforceProfileId = profileId;
            changed.Add("WorkforceProfileId");
        }

        if (changed.Count > 0)
        {
            await db.SaveChangesAsync(ct);
            result.LinkedUserName = user.UserName;
            result.UserLinkNote =
                "Akun '" + user.UserName + "' ditautkan (" + string.Join(" dan ", changed) +
                ") ke tenaga demo. WAJIB logout lalu login lagi supaya klaimnya ikut berubah " +
                "pada token.";
        }
        else
        {
            result.UserLinkNote =
                "Akun '" + user.UserName + "' sudah tertaut dokter dan tenaga; " +
                "tautan dibiarkan apa adanya.";
        }

        result.DoctorId = doctorId;
        result.PatientId = patientId;
        result.RoomId = roomId;
        result.SurgeonWorkforceId = profileId;
        result.DestinationUnitId = destinationUnitId;
        result.EncounterId = encounterId;
        return result;
    }

    /// <summary>
    /// Membuat satu kasus operasi berstatus <c>Requested</c> beserta tindakan dan riwayat
    /// statusnya, meniru bentuk yang dihasilkan
    /// <c>OperatingRoomCaseService.CreateAsync</c>.
    /// </summary>
    /// <remarks>
    /// Kasus ini menembus jalur layar. Ia berguna untuk mencoba alur sesudah pembuatan —
    /// penjadwalan, persiapan, pelaksanaan, pemulihan — tetapi ia TIDAK membuktikan bahwa
    /// pembuatan kasus lewat layar berfungsi. Pembuktian itu tetap harus dilakukan sendiri.
    /// </remarks>
    private static async Task EnsureDemoCaseAsync(
        ApplicationDbContext db,
        OperatingRoomDemoSeedResult result,
        Guid patientId,
        Guid encounterId,
        Guid doctorId,
        Guid actor,
        DateTime now,
        CancellationToken ct)
    {
        var caseId = Deterministic("Case");

        if (await db.OprCases.AnyAsync(x => x.Id == caseId, ct))
        {
            result.Reused.Add("OprCase");
            result.CaseId = caseId;
            return;
        }

        var entity = new OprCase
        {
            Id = caseId,
            CaseNumber = "OPR-" + caseId.ToString("N"),
            PatientId = patientId,
            EncounterId = encounterId,
            RequesterDoctorId = doctorId,
            PrimarySurgeonId = doctorId,
            CaseType = OprCaseType.Elective,
            Priority = OprPriority.Routine,
            Status = OprCaseStatus.Requested,
            Indication = "Kasus contoh buatan seeder untuk mencoba alur Operasi.",
            EstimatedMinutes = 60,
            RequestedAt = now,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actor
        };

        db.OprCases.Add(entity);

        var sequence = 1;
        foreach (var patientProcedureId in result.PatientProcedureIds)
        {
            db.OprCaseProcedures.Add(new OprCaseProcedure
            {
                Id = Deterministic("CaseProcedure" + sequence),
                OprCaseId = caseId,
                PatientProcedureId = patientProcedureId,
                IsPrimary = sequence == 1,
                Sequence = sequence,
                CreateDateTime = now,
                CreateBy = actor
            });

            sequence++;
        }

        // Sumbernya ditandai Seeder, bukan API, supaya baris ini jelas bukan hasil
        // permintaan pengguna ketika riwayat status dibaca orang lain kelak.
        db.OprStatusHistories.Add(new OprStatusHistory
        {
            Id = Deterministic("CaseHistory"),
            OprCaseId = caseId,
            FromStatus = null,
            ToStatus = OprCaseStatus.Requested,
            Action = "Request",
            Reason = "Dibuat OperatingRoomDemoSeeder, bukan lewat layar.",
            ActorUserId = actor,
            OccurredAt = now,
            Source = "Seeder:OperatingRoomDemo",
            CorrelationId = "DEMO-OPR-CASE-001",
            CreateDateTime = now,
            CreateBy = actor
        });

        result.Created.Add("OprCase");
        result.CaseId = caseId;
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
    public List<Guid> TeamWorkforceIds { get; } = [];
    public List<Guid> MaterialItemIds { get; } = [];
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid RoomId { get; set; }
    public Guid SurgeonWorkforceId { get; set; }
    public Guid DestinationUnitId { get; set; }
    public string? UserLinkNote { get; set; }
    public Guid CaseId { get; set; }
    public string? LinkedUserName { get; set; }
}
