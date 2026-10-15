using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Globalization;
using System.Linq.Expressions;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Satu sumber aturan "kunjungan Rawat Jalan berklinik" untuk Daftar Pasien Rawat Jalan dan
    /// pemblokir pendaftaran (RJ-DOC-DEC-012, RJ-DOC-DEC-019, RJ-DOC-DEC-021, RJ-DOC-DEC-022).
    ///
    /// <see cref="MedicalRecordManagement.Services.MedicalRecordAccessAuditService.KunjunganMasihBerjalan"/>
    /// sengaja tidak dipakai di sini: definisi itu menjaga hak akses rekam medis dan tetap
    /// menghitung kunjungan status 7-8 maupun kunjungan non-Rawat Jalan sebagai berjalan.
    /// </summary>
    public static class OutpatientEncounterRules
    {
        /// <summary>
        /// Status terakhir yang masih menghalangi pendaftaran. Mulai Konsultasi Selesai (7)
        /// pelayanan klinisnya sudah selesai; penutupan ke Completed milik Registration + Billing.
        /// </summary>
        public const EncounterStatus LastBlockingStatus = EncounterStatus.InConsultation;

        /// <summary>
        /// Kunjungan Rawat Jalan berklinik: bertipe Outpatient, punya klinik, dan bukan kunjungan
        /// IGD lama yang tercatat Outpatient. Pasien penunjang langsung (lab/radiologi walk-in)
        /// juga Outpatient tetapi tanpa klinik, sehingga tidak termasuk.
        /// </summary>
        public static IQueryable<RegPatientEncounter> WhereOutpatientClinicEncounter(
            this IQueryable<RegPatientEncounter> query,
            ApplicationDbContext dbContext) =>
            query.Where(x =>
                !x.IsDelete &&
                x.EncounterType == EncounterType.Outpatient &&
                x.ClinicId != null &&
                !dbContext.Set<EmgVisit>().Any(v => v.EncounterId == x.Id));

        /// <summary>
        /// Kunjungan Rawat Jalan berklinik yang menghalangi pendaftaran poliklinik baru:
        /// belum batal, belum selesai, dan berstatus Draft sampai Sedang Konsultasi.
        /// </summary>
        public static IQueryable<RegPatientEncounter> WhereBlocksRegistration(
            this IQueryable<RegPatientEncounter> query,
            ApplicationDbContext dbContext) =>
            query
                .WhereOutpatientClinicEncounter(dbContext)
                .Where(IsBlockingState);

        /// <summary>
        /// Keadaan kunjungan yang masih menghalangi pendaftaran: belum batal, belum selesai, dan
        /// berstatus Draft sampai Sedang Konsultasi. Dipakai juga sebagai mode "Aktif semua
        /// tanggal" dan kartu Menggantung pada Daftar Pasien Rawat Jalan.
        /// </summary>
        public static readonly Expression<Func<RegPatientEncounter, bool>> IsBlockingState =
            x => !x.IsCancel &&
                 x.CompletedAt == null &&
                 x.EncounterStatus <= LastBlockingStatus;

        /// <summary>
        /// Bagian status dari aturan boleh-batal (RJ-DOC-DEC-016, RJ-DOC-DEC-021): belum batal,
        /// belum selesai, dan berstatus Draft sampai Sedang Konsultasi. Untuk status Sedang
        /// Konsultasi, pemanggil wajib memeriksa tambahan bahwa tidak ada konsultasi aktif.
        /// </summary>
        public static readonly Expression<Func<RegPatientEncounter, bool>> IsCancellableStatus =
            x => !x.IsCancel &&
                 x.CompletedAt == null &&
                 x.EncounterStatus <= LastBlockingStatus;

        /// <summary>
        /// Saklar pemblokir "satu pasien satu kunjungan aktif" (RJ-DOC-REV-BE-007/008).
        /// Ditangguhkan sementara oleh RJ-DOC-DEC-026: bawaannya <c>false</c>, sehingga pendaftaran
        /// tidak ditolak karena kunjungan lama yang belum selesai.
        /// </summary>
        /// <remarks>
        /// Dibaca pendaftaran kunjungan dan pemeriksaan kelayakan admisi rawat inap
        /// (<c>ISSUE-EPS-010</c>). Keduanya wajib memakai saklar, aturan, dan kalimat yang sama:
        /// langkah 1 admisi menolak pasien dengan alasan yang persis sama seperti penolakan
        /// pendaftaran yang akan terjadi di langkah Pilih Bed.
        /// </remarks>
        public const string BlockActiveEncounterConfigKey = "HealthServices:Registration:BlockActiveEncounter";

        public static bool IsActiveEncounterBlockEnabled(IConfiguration? configuration) =>
            configuration?.GetValue(BlockActiveEncounterConfigKey, false) ?? false;

        /// <summary>
        /// Kunjungan Rawat Jalan berklinik terbaru yang masih menghalangi pendaftaran, tanggal
        /// berapa pun (RJ-DOC-REV-BE-007). Kunjungan status 7-8, penunjang tanpa klinik, IGD, dan
        /// Rawat Inap tidak menghalangi (RJ-DOC-DEC-019, RJ-DOC-DEC-022).
        /// </summary>
        public static Task<BlockingEncounterInfo?> FindBlockingEncounterAsync(
            ApplicationDbContext dbContext,
            Guid patientId,
            CancellationToken cancellationToken) =>
            dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .WhereBlocksRegistration(dbContext)
                .Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.EncounterDate)
                .Select(x => new BlockingEncounterInfo(x.EncounterNumber, x.EncounterDate))
                .FirstOrDefaultAsync(cancellationToken);

        public static string BuildActiveEncounterMessage(BlockingEncounterInfo kunjungan) =>
            $"Pasien masih memiliki kunjungan aktif bernomor {kunjungan.EncounterNumber} " +
            $"tanggal {kunjungan.EncounterDate.ToString("dd MMM yyyy", CultureInfo.GetCultureInfo("id-ID"))}. " +
            "Selesaikan atau batalkan kunjungan tersebut sebelum mendaftar ke poliklinik atau layanan lain.";
    }

    /// <summary>Kunjungan yang menghalangi pendaftaran baru: nomor dan tanggalnya.</summary>
    public sealed record BlockingEncounterInfo(string EncounterNumber, DateTime EncounterDate);
}
