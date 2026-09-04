using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.Infrastructure
{
    /// <summary>
    /// Menyiapkan data minimum yang dibutuhkan uji modul Rekam Medis.
    ///
    /// Baris keutuhan dokumen menunjuk pasien dan kunjungan lewat foreign key sungguhan, jadi
    /// keduanya harus benar-benar ada sebelum baris keutuhan dapat disimpan. Penyiapan ini
    /// dikumpulkan di satu tempat supaya setiap uji tidak menuliskannya sendiri-sendiri.
    ///
    /// Seluruh nilai di sini adalah data karangan. Tidak ada data pasien sungguhan.
    /// </summary>
    public static class RekamMedisTestData
    {
        /// <summary>
        /// Satu rangkaian data siap pakai: pengguna, unit pelayanan, pasien, dan kunjungannya.
        /// </summary>
        public sealed record Konteks(
            Guid UserId,
            Guid ServiceUnitId,
            Guid PatientId,
            Guid EncounterId);

        /// <summary>
        /// Membuat satu akun pengguna uji.
        ///
        /// Diperlukan karena kunjungan menyimpan siapa yang mendaftarkannya, dan kolom itu
        /// memiliki foreign key ke tabel pengguna.
        /// </summary>
        public static ApplicationUser BuatPengguna(ApplicationDbContext context, string nama)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var pengguna = new ApplicationUser
            {
                // UserCode wajib unik pada tabel pengguna, jadi pembeda ikut disertakan.
                UserCode = $"UJI-{nama.ToUpperInvariant()}-{pembeda}",
                DisplayName = $"{nama} uji {pembeda}",
                UserName = $"{nama}.{pembeda}",
                NormalizedUserName = $"{nama}.{pembeda}".ToUpperInvariant(),
                Email = $"{nama}.{pembeda}@contoh.uji",
                NormalizedEmail = $"{nama}.{pembeda}@contoh.uji".ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString("N")
            };

            context.Set<ApplicationUser>().Add(pengguna);
            context.SaveChanges();

            return pengguna;
        }

        /// <summary>
        /// Membuat satu pasien beserta satu kunjungan yang menaunginya.
        /// </summary>
        /// <param name="encounterStatus">
        /// Keadaan kunjungan. Dipakai uji yang membedakan kunjungan berjalan dari kunjungan
        /// yang sudah ditutup.
        /// </param>
        public static Konteks SiapkanPasienDanKunjungan(
            ApplicationDbContext context,
            EncounterStatus encounterStatus = EncounterStatus.Registered)
        {
            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var pendaftar = BuatPengguna(context, "pendaftar");

            var serviceUnit = new MstServiceUnit
            {
                ServiceUnitCode = $"UNIT-{pembeda}",
                ServiceUnitName = "Poliklinik Uji",
                ServiceUnitType = ServiceUnitType.Outpatient
            };
            context.Set<MstServiceUnit>().Add(serviceUnit);

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
                EncounterType = EncounterType.Outpatient,
                VisitType = VisitType.NewVisit,
                EncounterStatus = encounterStatus,
                RegisteredByUserId = pendaftar.Id
            };
            context.Set<TrxPatientEncounter>().Add(kunjungan);

            context.SaveChanges();

            return new Konteks(pendaftar.Id, serviceUnit.Id, pasien.Id, kunjungan.Id);
        }
    }
}
