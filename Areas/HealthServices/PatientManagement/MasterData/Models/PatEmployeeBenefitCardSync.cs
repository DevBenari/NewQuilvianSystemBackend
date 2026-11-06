using System;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    /// <summary>
    /// Catatan hasil terakhir sinkronisasi pemasangan kartu "Benefit Karyawan RS" ke pasien yang
    /// terbukti sama dengan pemilik benefit (FIN-DEC-237, FIN-DEC-239, FIN-DEC-250, FIN-DEC-251, FIN-DEC-253; kamus data bagian 12).
    /// </summary>
    public sealed class PatEmployeeBenefitCardSync : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// ID enrollment benefit terkait. Unik untuk baris yang belum dihapus (IsDelete = false).
        /// </summary>
        public Guid EmployeeBenefitEnrollmentId { get; set; }

        /// <summary>
        /// ID profil workforce pemilik benefit. Disalin dari enrollment saat catatan dibuat.
        /// </summary>
        public Guid WorkforceProfileId { get; set; }

        /// <summary>
        /// Status sinkronisasi: SYNCED, WAITING_PATIENT, ON_HOLD, FAILED_RETRYABLE, REVOKED.
        /// </summary>
        public string SyncStatus { get; set; } = string.Empty;

        /// <summary>
        /// Kode alasan status selain SYNCED. Wajib diisi bila status != SYNCED, null bila SYNCED.
        /// </summary>
        public string? ReasonCode { get; set; }

        /// <summary>
        /// Jumlah percobaan sinkronisasi (>= 0).
        /// </summary>
        public int AttemptCount { get; set; } = 0;

        /// <summary>
        /// Waktu percobaan sinkronisasi terakhir (UTC).
        /// </summary>
        public DateTime? LastAttemptAt { get; set; }

        /// <summary>
        /// Waktu jadwal percobaan sinkronisasi berikutnya (UTC). Hanya diisi untuk WAITING_PATIENT dan FAILED_RETRYABLE.
        /// </summary>
        public DateTime? NextAttemptAt { get; set; }

        /// <summary>
        /// ID pasien yang berhasil dipasangkan kartu. Wajib terisi untuk status SYNCED dan REVOKED.
        /// Bersama WorkforceProfileId menyatakan bahwa seorang pegawai adalah pasien RS (data sensitif).
        /// </summary>
        public Guid? PatientId { get; set; }

        /// <summary>
        /// ID kartu penjamin perusahaan (MstPatientCompanyGuarantor) yang terpasang atau dipakai ulang.
        /// Wajib terisi untuk status SYNCED dan REVOKED.
        /// </summary>
        public Guid? PatientCompanyGuarantorId { get; set; }

        /// <summary>
        /// Waktu pertama kali atau terakhir kali sinkronisasi berhasil terpasang (UTC).
        /// </summary>
        public DateTime? SyncedAt { get; set; }

        /// <summary>
        /// Waktu saat pencabutan kartu terdeteksi (UTC).
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Sumber pemicu terakhir: ACTIVATION, SCHEDULER, MANUAL_RESYNC, CANCEL.
        /// </summary>
        public string LastTriggerSource { get; set; } = string.Empty;

        /// <summary>
        /// ID user yang memicu sinkronisasi manual atau akun layanan untuk penjadwal.
        /// </summary>
        public Guid? LastTriggeredByUserId { get; set; }

        /// <summary>
        /// Token konkurensi optimistik untuk mencegah konflik saat dua proses memperbarui catatan secara bersamaan.
        /// </summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        // Navigasi
        public TrxEmployeeBenefitEnrollment? EmployeeBenefitEnrollment { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstPatient? Patient { get; set; }
        public MstPatientCompanyGuarantor? PatientCompanyGuarantor { get; set; }
    }
}
