using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.Infrastructure
{
    /// <summary>
    /// Menyiapkan data minimum untuk uji tindakan dokter: satu tindakan master, satu catatan
    /// dokter sebagai induknya, dan satu baris tindakan pasien.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Diperlukan karena <c>TrxPatientProcedure</c> menuntut kunjungan, catatan dokter, pasien,
    /// dokter, dan tindakan master lewat foreign key sungguhan, sedangkan basis data uji
    /// menegakkan foreign key. Menyiapkannya di satu tempat menghindari lima uji menuliskan
    /// rangkaian yang sama.
    /// </para>
    /// <para>
    /// Seluruh nilai di sini adalah data karangan. Tidak ada data pasien sungguhan.
    /// </para>
    /// </remarks>
    public static class TindakanTestData
    {
        /// <summary>
        /// Membuat satu tindakan master siap pakai.
        /// </summary>
        public static MstProcedure BuatTindakanMaster(ApplicationDbContext context)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var tindakan = new MstProcedure
            {
                ProcedureCode = $"TND-{pembeda}",
                ProcedureName = "Perawatan Luka Uji",
                ProcedureType = "DoctorAction",
                IsDoctorAction = true
            };

            context.Set<MstProcedure>().Add(tindakan);
            context.SaveChanges();

            // Tarif rumah sakit wajib ada. Tanpa baris ini, pembuatan tindakan ditolak dengan
            // "Tarif rumah sakit untuk tindakan belum dikonfigurasi", dan penolakan itu
            // menyamarkan hal yang sedang diuji.
            var kategoriTarif = new MstTariffCategory
            {
                TariffCategoryCode = $"KTF-{pembeda}",
                TariffCategoryName = "Tindakan Uji",
                IsProcedure = true
            };
            context.Set<MstTariffCategory>().Add(kategoriTarif);
            context.SaveChanges();

            context.Set<MstTariff>().Add(new MstTariff
            {
                TariffCode = $"TRF-{pembeda}",
                TariffName = "Tarif Perawatan Luka Uji",
                TariffCategoryId = kategoriTarif.Id,
                ProcedureId = tindakan.Id,
                NormalPrice = 150000m,
                IsActive = true
            });
            context.SaveChanges();

            return tindakan;
        }

        /// <summary>
        /// Membuat satu catatan dokter sederhana sebagai induk tindakan.
        /// </summary>
        /// <remarks>
        /// Catatan ini hanya berperan sebagai jangkar; isi klinisnya tidak diuji di sini.
        /// </remarks>
        public static TrxDoctorConsultation BuatCatatanInduk(
            ApplicationDbContext context,
            Guid encounterId,
            Guid patientId,
            Guid doctorMasterId,
            Guid serviceUnitId,
            Guid? inpEpisodeId = null)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var catatan = new TrxDoctorConsultation
            {
                ConsultationNumber = $"CON-{pembeda}",
                EncounterId = encounterId,
                PatientId = patientId,
                DoctorId = doctorMasterId,
                ServiceUnitId = serviceUnitId,
                InpEpisodeId = inpEpisodeId,
                ConsultationDateTime = DateTime.UtcNow,
                ConsultationStatus = DoctorConsultationStatus.InProgress,
                IsActive = true
            };

            context.Set<TrxDoctorConsultation>().Add(catatan);
            context.SaveChanges();

            return catatan;
        }

        /// <summary>
        /// Membuat satu tindakan pasien berstatus <c>Planned</c>, siap dieksekusi.
        /// </summary>
        /// <param name="context">Konteks basis data uji.</param>
        /// <param name="encounterId">Kunjungan yang menaungi tindakan.</param>
        /// <param name="consultationId">Catatan dokter induknya.</param>
        /// <param name="patientId">Pasien yang menerima tindakan.</param>
        /// <param name="doctorMasterId">Dokter yang merencanakan tindakan.</param>
        /// <param name="serviceUnitId">Unit pelayanan tempat tindakan dicatat.</param>
        /// <param name="inpEpisodeId">Perawatan rawat inap, bila ada.</param>
        /// <param name="physicianVisitId">Kejadian visite yang ditautkan, bila ada.</param>
        /// <param name="idempotencyKey">Kunci permintaan, bila diuji.</param>
        public static TrxPatientProcedure BuatTindakan(
            ApplicationDbContext context,
            Guid encounterId,
            Guid consultationId,
            Guid patientId,
            Guid doctorMasterId,
            Guid serviceUnitId,
            Guid? inpEpisodeId = null,
            Guid? physicianVisitId = null,
            string? idempotencyKey = null)
        {
            var master = BuatTindakanMaster(context);

            var tindakan = new TrxPatientProcedure
            {
                EncounterId = encounterId,
                ConsultationId = consultationId,
                PatientId = patientId,
                DoctorId = doctorMasterId,
                ServiceUnitId = serviceUnitId,
                InpEpisodeId = inpEpisodeId,
                PhysicianVisitId = physicianVisitId,
                IdempotencyKey = idempotencyKey,
                ProcedureId = master.Id,
                ProcedureCodeSnapshot = master.ProcedureCode,
                ProcedureNameSnapshot = master.ProcedureName,
                ProcedureSource = PatientProcedureSource.DoctorOrder,
                ProcedureStatus = PatientProcedureStatus.Planned,
                ProcedureDateTime = DateTime.UtcNow,
                IsActive = true
            };

            context.Set<TrxPatientProcedure>().Add(tindakan);
            context.SaveChanges();

            return tindakan;
        }
    }
}
