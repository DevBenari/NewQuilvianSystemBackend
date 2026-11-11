using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders
{
    /// <summary>
    /// Mengisi sebelas profil dokumen wajib awal (`BE-37`, desain A4.8; `RM-DEC-047`, `053`–`056`,
    /// `060`, `061`).
    /// </summary>
    /// <remarks>
    /// <b>Isi awal ini BUKAN SOP rumah sakit.</b> Disusun dari dokumen yang memang dihasilkan modul
    /// yang ada, supaya tidak ada syarat yang mustahil dipenuhi. Bila SOP diserahkan, daftar
    /// diselaraskan lewat layar master — bukan lewat seeder ini.
    ///
    /// <b>Seeder tidak pernah menimpa.</b> Profil yang kodenya sudah ada — termasuk yang sudah diubah
    /// atau dinonaktifkan petugas rekam medis — dilewati apa adanya. Profil yang kuncinya (jenis
    /// kunjungan + jenis unit + jenis poli + kondisi; jenis poli sejak `BE-42`) sudah dipakai profil
    /// buatan petugas juga dilewati, karena satu kunci hanya boleh punya satu profil.
    ///
    /// <b>`RJ-REHAB` dipasang bersama probe Rehab (`BE-45`).</b> Tanpa probe, kriterianya tidak pernah
    /// terpenuhi dan setiap kunjungan poli rehab menjadi belum lengkap (`RM-CAP-074`); keduanya karena itu
    /// tidak boleh dipisahkan.
    ///
    /// <b>Id deterministik.</b> Id diturunkan dari kode profil, sehingga sama di setiap lingkungan.
    ///
    /// Kunjungan berjenis <c>Unknown</c> sengaja tidak punya profil: penilaiannya menghasilkan
    /// kekurangan <c>RuleNotFound</c>, supaya datanya dibetulkan, bukan dianggap lengkap.
    /// </remarks>
    public static class MedicalRecordCompletenessProfileSeeder
    {
        /// <summary>Pembeda ruang nama hash. Jangan pernah diubah: mengubahnya memindahkan seluruh id.</summary>
        private const string IdNamespace = "MstMedicalRecordCompletenessProfile";

        private sealed record SeedProfile(
            string Code,
            string Name,
            EncounterType? EncounterType,
            ServiceUnitType? ServiceUnitType,
            CompletenessCondition Condition,
            CompletenessCriterion[] Criteria,
            ClinicType? ClinicType = null);

        private static readonly SeedProfile[] Profiles =
        {
            new("RJ", "Rawat Jalan", EncounterType.Outpatient, null, CompletenessCondition.None,
                [CompletenessCriterion.InitialAssessmentAny, CompletenessCriterion.PrimaryDiagnosis, CompletenessCriterion.DoctorNoteOrConsultation]),
            new("RJ-LAB", "Rawat Jalan Laboratorium", EncounterType.Outpatient, ServiceUnitType.Laboratory, CompletenessCondition.None, []),
            new("RJ-RAD", "Rawat Jalan Radiologi", EncounterType.Outpatient, ServiceUnitType.Radiology, CompletenessCondition.None, []),
            new("RJ-HD", "Rawat Jalan Hemodialisa", EncounterType.Outpatient, ServiceUnitType.Hemodialysis, CompletenessCondition.None,
                [CompletenessCriterion.HemodialysisSessionSigned]),
            // RM-DEC-080 (BE-42): unit Optik tanpa dokumen wajib, pola RJ-LAB/RJ-RAD.
            new("RJ-OPT", "Rawat Jalan Optik", EncounterType.Outpatient, ServiceUnitType.Optical, CompletenessCondition.None, []),
            // RM-DEC-081/085/086 (BE-45): poli Rehabilitasi Medik, unit apa pun; dijawab probe Rehab.
            new("RJ-REHAB", "Rawat Jalan Poli Rehabilitasi Medik", EncounterType.Outpatient, null, CompletenessCondition.None,
                [CompletenessCriterion.RehabClinicalDocument], ClinicType.Rehabilitation),
            new("TELE", "Telemedicine", EncounterType.Telemedicine, null, CompletenessCondition.None,
                [CompletenessCriterion.DoctorNoteOrConsultation, CompletenessCriterion.PrimaryDiagnosis]),
            new("IGD", "Instalasi Gawat Darurat", EncounterType.Emergency, null, CompletenessCondition.None,
                [CompletenessCriterion.EmergencyTriage, CompletenessCriterion.InitialAssessmentAny, CompletenessCriterion.PrimaryDiagnosis, CompletenessCriterion.EmergencyDisposition]),
            new("RI", "Rawat Inap", EncounterType.Inpatient, null, CompletenessCondition.None,
                [CompletenessCriterion.InpatientGeneralConsent, CompletenessCriterion.InitialNursingAssessment, CompletenessCriterion.InitialMedicalAssessment,
                 CompletenessCriterion.PrimaryDiagnosis, CompletenessCriterion.ProgressNoteAny, CompletenessCriterion.DischargeSummary]),
            new("MCU", "Medical Check Up", EncounterType.MedicalCheckup, null, CompletenessCondition.None,
                [CompletenessCriterion.McuResume]),
            new("K-OK", "Kondisi: Operasi", null, null, CompletenessCondition.Surgery,
                [CompletenessCriterion.SurgeryReport, CompletenessCriterion.AnesthesiaRecord, CompletenessCriterion.SurgicalSafetyChecklist]),
            new("K-HD", "Kondisi: Sesi Hemodialisa", null, null, CompletenessCondition.HemodialysisSession,
                [CompletenessCriterion.HemodialysisSessionSigned]),
            new("K-TRF", "Kondisi: Transfusi", null, null, CompletenessCondition.Transfusion,
                [CompletenessCriterion.TransfusionMonitoring])
        };

        /// <summary>Jumlah profil awal; dipakai uji.</summary>
        public static int ProfileCount => Profiles.Length;

        public static Guid DeterministicId(string code) =>
            new(SHA256.HashData(Encoding.UTF8.GetBytes($"{IdNamespace}:{code.Trim().ToUpperInvariant()}"))[..16]);

        private static Guid DeterministicItemId(string code, CompletenessCriterion criterion) =>
            new(SHA256.HashData(Encoding.UTF8.GetBytes($"{IdNamespace}Item:{code.Trim().ToUpperInvariant()}:{(int)criterion}"))[..16]);

        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(MedicalRecordCompletenessProfileSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;

            if (!seedEnabled)
            {
                logger.LogInformation("Seeder profil dokumen wajib dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var hasil = await SeedAsync(dbContext, Guid.Empty, cancellationToken);

            logger.LogInformation(
                "Seeder profil dokumen wajib selesai. Ditambahkan {Inserted}, dilewati {Skipped}.",
                hasil.Inserted,
                hasil.Skipped);
        }

        /// <summary>
        /// Menjalankan pengisian terhadap satu <see cref="ApplicationDbContext"/>. Dipisahkan supaya
        /// dapat diuji tanpa membangun service provider.
        /// </summary>
        public static async Task<MedicalRecordCompletenessProfileSeedResult> SeedAsync(
            ApplicationDbContext db,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var hasil = new MedicalRecordCompletenessProfileSeedResult();

            // Baris terhapus ikut dihitung untuk kode, supaya kodenya tidak diisi ulang; kunci hanya
            // dibandingkan dengan baris belum terhapus, sama dengan index uniknya.
            var ada = await db.Set<MstMedicalRecordCompletenessProfile>()
                .AsNoTracking()
                .Select(x => new { x.Id, x.Code, x.EncounterType, x.ServiceUnitType, x.ClinicType, x.Condition, x.IsDelete })
                .ToListAsync(cancellationToken);

            var idAda = ada.Select(x => x.Id).ToHashSet();
            var kodeAda = ada.Select(x => x.Code.ToUpperInvariant()).ToHashSet();
            var kunciAda = ada
                .Where(x => !x.IsDelete)
                .Select(x => (x.EncounterType, x.ServiceUnitType, x.ClinicType, x.Condition))
                .ToHashSet();

            var sekarang = DateTime.UtcNow;

            foreach (var profil in Profiles)
            {
                var id = DeterministicId(profil.Code);
                var kunci = (profil.EncounterType, profil.ServiceUnitType, profil.ClinicType, profil.Condition);

                if (idAda.Contains(id) || kodeAda.Contains(profil.Code) || kunciAda.Contains(kunci))
                {
                    hasil.Skipped++;
                    continue;
                }

                var baris = new MstMedicalRecordCompletenessProfile
                {
                    Id = id,
                    Code = profil.Code,
                    Name = profil.Name,
                    EncounterType = profil.EncounterType,
                    ServiceUnitType = profil.ServiceUnitType,
                    ClinicType = profil.ClinicType,
                    Condition = profil.Condition,
                    IsActive = true,
                    CreateDateTime = sekarang,
                    CreateBy = actorUserId
                };

                foreach (var kriteria in profil.Criteria)
                {
                    baris.Items.Add(new MstMedicalRecordCompletenessProfileItem
                    {
                        Id = DeterministicItemId(profil.Code, kriteria),
                        ProfileId = id,
                        Criterion = kriteria,
                        IsActive = true,
                        CreateDateTime = sekarang,
                        CreateBy = actorUserId
                    });
                }

                db.Set<MstMedicalRecordCompletenessProfile>().Add(baris);
                kunciAda.Add(kunci);
                hasil.Inserted++;
            }

            if (hasil.Inserted > 0)
                await db.SaveChangesAsync(cancellationToken);

            return hasil;
        }
    }

    public sealed class MedicalRecordCompletenessProfileSeedResult
    {
        public int Inserted { get; set; }

        public int Skipped { get; set; }
    }
}
