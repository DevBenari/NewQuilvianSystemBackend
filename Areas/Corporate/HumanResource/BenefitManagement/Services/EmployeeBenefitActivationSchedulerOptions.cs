using System;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Opsi konfigurasi penjadwal aktivasi otomatis enrollment benefit pegawai yang berstatus Approved
    /// saat tanggal mulai berlakunya sudah tiba (BE-FIN-137, FR-FIN-263, FIN-DEC-235, FIN-DES-111, FIN-OQ-127).
    /// Konfigurasi dibaca dari section "HumanResource:EmployeeBenefitActivationScheduler".
    /// DIBANGUN MATI: Enabled = false secara bawaan, sehingga tanpa konfigurasi eksplisit tidak ada aksi otomatis.
    /// </summary>
    public sealed class EmployeeBenefitActivationSchedulerOptions
    {
        /// <summary>
        /// Menandakan apakah penjadwal aktivasi berkala diaktifkan. Bawaan: false (mati).
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Interval siklus pemeriksaan dalam detik. Bawaan: 3600 detik (1 jam), minimum 30 detik.
        /// </summary>
        public int PollIntervalSeconds { get; set; } = 3600;

        /// <summary>
        /// Jumlah maksimum enrollment Approved yang diproses dalam satu siklus (1..500). Bawaan: 100.
        /// </summary>
        public int MaxPerCycle { get; set; } = 100;

        /// <summary>
        /// ID pengguna sistem (akun layanan) yang dicatat sebagai ActivatedByUserId, UpdateBy, dan di jejak audit.
        /// Jika null, sistem menggunakan <see cref="Guid.Empty"/>.
        /// </summary>
        public Guid? SystemActorUserId { get; set; }
    }
}

