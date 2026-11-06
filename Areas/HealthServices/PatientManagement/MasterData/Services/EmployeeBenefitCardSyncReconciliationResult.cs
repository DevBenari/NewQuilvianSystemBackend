namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    /// <summary>
    /// Ringkasan hasil pemrosesan satu siklus rekonsiliasi sinkronisasi kartu penjamin internal pasien
    /// (BE-FIN-136; FIN-INTEGRATION-1.11 V.3).
    /// </summary>
    public sealed class EmployeeBenefitCardSyncReconciliationResult
    {
        /// <summary>Jumlah enrollment baru yang ditemukan pada Langkah 1.</summary>
        public int Step1NewDiscovered { get; set; }

        /// <summary>Jumlah enrollment baru yang berhasil dieksekusi sinkronisasinya pada Langkah 1.</summary>
        public int Step1NewProcessed { get; set; }

        /// <summary>Jumlah enrollment baru yang menghasilkan status SYNCED pada Langkah 1.</summary>
        public int Step1NewSuccess { get; set; }

        /// <summary>Jumlah catatan retry (FAILED_RETRYABLE / WAITING_PATIENT) yang jatuh tempo pada Langkah 2.</summary>
        public int Step2RetryDiscovered { get; set; }

        /// <summary>Jumlah catatan retry yang diproses ulang pada Langkah 2.</summary>
        public int Step2RetryProcessed { get; set; }

        /// <summary>Jumlah catatan retry yang berhasil mencapai status SYNCED pada Langkah 2.</summary>
        public int Step2RetrySuccess { get; set; }

        /// <summary>Jumlah kartu aktif yang dicabut oleh staf sehingga catatan diubah ke REVOKED pada Langkah 3.</summary>
        public int Step3RevokedCount { get; set; }

        /// <summary>Jumlah enrollment dibatalkan yang kartunya dinonaktifkan pada Langkah 4.</summary>
        public int Step4CancelledCount { get; set; }

        /// <summary>Menandakan bahwa putaran dilewati karena ActivationCutoffUtc belum ditentukan (FIN-DES-125).</summary>
        public bool WarningCutoffMissing { get; set; }

        /// <summary>Total seluruh item yang diproses pada siklus ini.</summary>
        public int TotalProcessed => Step1NewProcessed + Step2RetryProcessed + Step3RevokedCount + Step4CancelledCount;
    }
}

