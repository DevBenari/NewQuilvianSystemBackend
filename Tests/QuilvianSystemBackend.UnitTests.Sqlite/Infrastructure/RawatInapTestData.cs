using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.Infrastructure
{
    /// <summary>
    /// Menyiapkan data minimum untuk uji sub-modul <c>dokter-rawat-inap</c>: kunjungan beserta
    /// perawatan rawat inap, dokter master, dan penugasan DPJP berperiode.
    /// </summary>
    /// <remarks>
    /// Seluruh nilai di sini adalah data karangan. Tidak ada data pasien sungguhan.
    /// </remarks>
    public static class RawatInapTestData
    {
        /// <summary>
        /// Satu rangkaian data rawat inap siap pakai.
        /// </summary>
        public sealed record Konteks(
            Guid EncounterId,
            Guid PatientId,
            Guid ServiceUnitId,
            Guid EpisodeId,
            Guid DoctorMasterId,
            Guid DokterUserId,
            Guid WorkforceProfileId);

        /// <summary>
        /// Membuat satu dokter master beserta seluruh master pendukungnya.
        /// </summary>
        public static MstDoctor BuatDokterMaster(ApplicationDbContext context)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var profil = new MstWorkforceProfile
            {
                ProfileCode = $"PRF-{pembeda}",
                UserType = QuilvianSystemBackend.Enums.UserType.PermanentDoctor,
                DisplayName = $"Dokter Uji {pembeda}"
            };
            var jenisTenaga = new MstWorkforceType { WorkforceTypeCode = $"WFT-{pembeda}", WorkforceTypeName = "Tenaga Medis" };
            var kategori = new MstEmployeeCategory { EmployeeCategoryCode = $"KAT-{pembeda}", EmployeeCategoryName = "Tetap" };
            var jenisKepegawaian = new MstEmploymentType { EmploymentTypeCode = $"EMT-{pembeda}", EmploymentTypeName = "Purnawaktu" };
            var statusKepegawaian = new MstEmploymentStatus { EmploymentStatusCode = $"EMS-{pembeda}", EmploymentStatusName = "Aktif" };
            var profesi = new MstProfession { ProfessionCode = $"PRO-{pembeda}", ProfessionName = "Dokter Umum", ProfessionGroup = "Medis" };

            context.AddRange(profil, jenisTenaga, kategori, jenisKepegawaian, statusKepegawaian, profesi);
            context.SaveChanges();

            var dokter = new MstDoctor
            {
                WorkforceProfileId = profil.Id,
                DoctorCode = $"DOK-{pembeda}",
                DoctorNumber = $"NO-{pembeda}",
                FullName = $"Dokter Uji {pembeda}",
                WorkforceTypeId = jenisTenaga.Id,
                EmployeeCategoryId = kategori.Id,
                EmploymentTypeId = jenisKepegawaian.Id,
                EmploymentStatusId = statusKepegawaian.Id,
                ProfessionId = profesi.Id
            };
            context.Set<MstDoctor>().Add(dokter);
            context.SaveChanges();
            return dokter;
        }

        /// <summary>
        /// Membuat satu kunjungan beserta perawatan rawat inap dan penugasan DPJP-nya.
        /// </summary>
        /// <param name="context">Konteks basis data uji.</param>
        /// <param name="episodeStatus">Keadaan perawatan yang dibentuk.</param>
        /// <param name="encounterType">Tipe kunjungan yang menaunginya.</param>
        /// <param name="denganPenugasanDpjp">
        /// Salah bila uji memang ingin membuktikan penolakan dokter yang tidak berwenang.
        /// </param>
        public static Konteks SiapkanPerawatan(
            ApplicationDbContext context,
            InpEpisodeStatus episodeStatus = InpEpisodeStatus.Admitted,
            EncounterType encounterType = EncounterType.Inpatient,
            bool denganPenugasanDpjp = true)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var pendaftar = RekamMedisTestData.BuatPengguna(context, "pendaftar");
            var dokterPengguna = RekamMedisTestData.BuatPengguna(context, "dokter");
            var dokterMaster = BuatDokterMaster(context);

            // BE-RWI-045. Akun dokter ditautkan ke profil tenaga kerja miliknya, karena itulah
            // satu-satunya cara backend mengetahui bahwa pengguna yang masuk memang seorang
            // dokter - VAL-DOK-05 diturunkan dari data, bukan dari nama peran. Pengguna yang
            // tidak ditautkan tetap tersedia lewat RekamMedisTestData.BuatPengguna, dan itulah
            // yang dipakai uji penolakan.
            dokterPengguna.WorkforceProfileId = dokterMaster.WorkforceProfileId;
            context.SaveChanges();

            var serviceUnit = new MstServiceUnit
            {
                ServiceUnitCode = $"UNIT-{pembeda}",
                ServiceUnitName = "Rawat Inap Uji",
                ServiceUnitType = ServiceUnitType.Inpatient
            };
            context.Set<MstServiceUnit>().Add(serviceUnit);

            var patientClass = new MstPatientClass
            {
                PatientClassCode = $"KLS-{pembeda}",
                PatientClassName = "Kelas Uji",
                IsForInpatient = true
            };
            context.Set<MstPatientClass>().Add(patientClass);

            var pasien = new MstPatient
            {
                PatientCode = $"PAS-{pembeda}",
                MedicalRecordNumber = $"RM-{pembeda}",
                FullName = "Pasien Uji"
            };
            context.Set<MstPatient>().Add(pasien);
            context.SaveChanges();

            var kunjungan = new TrxPatientEncounter
            {
                EncounterNumber = $"KJG-{pembeda}",
                PatientId = pasien.Id,
                ServiceUnitId = serviceUnit.Id,
                EncounterType = encounterType,
                VisitType = VisitType.NewVisit,
                EncounterStatus = EncounterStatus.Registered,
                RegisteredByUserId = pendaftar.Id
            };
            context.Set<TrxPatientEncounter>().Add(kunjungan);
            context.SaveChanges();

            // Sumber pembayaran wajib ada pada setiap kunjungan - "Satu encounter wajib
            // mempunyai tepat satu sumber pembayaran". Tanpa baris ini, seluruh jalur yang
            // menghitung tarif - resep dan tindakan - ditolak dengan "Sumber pembayaran
            // encounter tidak ditemukan", dan penolakan itu menyamarkan hal yang sedang diuji.
            var sumberPembayaran = new TrxPatientEncounterGuarantor
            {
                PaymentSourceNumber = $"BYR-{pembeda}",
                EncounterId = kunjungan.Id,
                PatientId = pasien.Id,
                PaymentType = EncounterPaymentType.Cash,
                PaymentSourceNameSnapshot = "Tunai",
                IsActive = true
            };
            context.Set<TrxPatientEncounterGuarantor>().Add(sumberPembayaran);
            context.SaveChanges();

            var episode = new InpEpisode
            {
                EpisodeNumber = $"RI-{pembeda}",
                EncounterId = kunjungan.Id,
                PatientId = pasien.Id,
                ServiceUnitId = serviceUnit.Id,
                PatientClassId = patientClass.Id,
                EpisodeStatus = episodeStatus,
                AdmittedAt = DateTime.UtcNow.AddDays(-2),
                CreateBy = pendaftar.Id
            };
            context.Set<InpEpisode>().Add(episode);
            context.SaveChanges();

            if (denganPenugasanDpjp)
            {
                var penugasan = new InpDoctorAssignment
                {
                    EpisodeId = episode.Id,
                    DoctorId = dokterMaster.Id,
                    SequenceNumber = 1,
                    // Berperiode: mulai dua hari lalu dan belum berakhir, sehingga berlaku
                    // pada saat uji berjalan.
                    StartDateTime = DateTime.UtcNow.AddDays(-2),
                    EndDateTime = null,
                    AssignedByUserId = pendaftar.Id,
                    IsActive = true
                };
                context.Set<InpDoctorAssignment>().Add(penugasan);
                context.SaveChanges();
            }

            return new Konteks(
                kunjungan.Id,
                pasien.Id,
                serviceUnit.Id,
                episode.Id,
                dokterMaster.Id,
                dokterPengguna.Id,
                dokterMaster.WorkforceProfileId);
        }
    }
}
