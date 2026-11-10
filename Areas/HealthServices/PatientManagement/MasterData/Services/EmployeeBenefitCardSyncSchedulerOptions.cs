using System;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Pengaturan penjadwal rekonsiliasi otomatis sinkronisasi kartu penjamin internal pasien (Benefit Karyawan RS).
    /// Dibaca dari konfigurasi <c>PatientManagement:EmployeeBenefitCardSyncScheduler</c>.
    /// Dibangun MATI secara bawaan (<see cref="Enabled"/> = false).
    /// (BE-FIN-136; FR-FIN-265, FR-FIN-268; FIN-DEC-253; FIN-DES-113, 117, 125; FIN-INTEGRATION-1.11 V.3).
    /// </summary>
    public sealed class EmployeeBenefitCardSyncSchedulerOptions
    {
        /// <summary>Bila false, penjadwal berhenti setelah mencatat log informasi. Nilai bawaan: <c>false</c>.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>Jeda antar siklus dalam detik. Minimum 30 detik. Nilai bawaan: 900 (15 menit).</summary>
        public int PollIntervalSeconds { get; set; } = 900;

        /// <summary>Batas enrollment yang diproses per siklus rekonsiliasi (1..500). Nilai bawaan: 50.</summary>
        public int MaxPerCycle { get; set; } = 50;

        /// <summary>
        /// Batas waktu aktivasi enrollment yang diproses oleh penjadwal massal (FIN-DES-125).
        /// WAJIB diisi bila <see cref="Enabled"/> = true.
        /// Enrollment yang aktif sebelum batas ini tidak diproses otomatis dan hanya disinkronkan manual per enrollment.
        /// </summary>
        public DateTimeOffset? ActivationCutoffUtc { get; set; }

        /// <summary>Jeda percobaan ulang dalam menit untuk status FAILED_RETRYABLE. Nilai bawaan: [15, 60, 360, 1440].</summary>
        public int[] RetryDelaysMinutes { get; set; } = { 15, 60, 360, 1440 };

        /// <summary>Jeda waktu pemeriksaan ulang dalam jam untuk status WAITING_PATIENT. Nilai bawaan: 24 jam.</summary>
        public int WaitingPatientRecheckHours { get; set; } = 24;

        /// <summary>
        /// ID akun pengguna sistem untuk pencatatan jejak audit penjadwal.
        /// Bila null, menggunakan <see cref="Guid.Empty"/>.
        /// </summary>
        public Guid? SystemActorUserId { get; set; }
    }
}

