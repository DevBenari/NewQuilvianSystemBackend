namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs
{
    /// <summary>
    /// Ringkasan tagihan satu perawatan rawat inap, <b>baca-saja dan tanpa harga per item</b> —
    /// <c>BE-RWI-126</c>, <c>FR-KEP-082</c>, api-contract <c>keperawatan</c> 0.5.0 bagian 7.14,
    /// <c>RWI-DEC-137</c>, <c>RWI-DEC-154</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yang sengaja tidak ada di sini.</b> Daftar item, harga satuan, harga per item, komponen
    /// tarif, dan satu pun aksi ubah. Perawat di samping tempat tidur perlu tahu gambaran besar —
    /// misalnya untuk mengarahkan keluarga ke kasir — bukan rincian harga.
    /// </para>
    /// <para>
    /// <b>Angka yang belum dapat dihitung tidak pernah ditulis nol.</b> Setiap angka membawa
    /// penanda apakah ia sudah final; bila tidak, pesannya menyebut sebabnya.
    /// </para>
    /// <para>
    /// Contoh: "BPJS — layak — total berjalan Rp 4.250.000 — deposit Rp 1.000.000 — kekurangan
    /// deposit Rp 350.000 — 1 item tidak ditanggung".
    /// </para>
    /// </remarks>
    public class PatientBillingSummaryResponse
    {
        public Guid EpisodeId { get; set; }

        public Guid EncounterId { get; set; }

        /// <summary>Nama penjamin utama, atau "Tunai / Umum".</summary>
        public string GuarantorName { get; set; } = string.Empty;

        /// <summary>Jenis pembayaran penjamin utama: <c>Cash</c>, <c>Insurance</c>, atau jenis lain yang dikenal pendaftaran.</summary>
        public string PaymentType { get; set; } = string.Empty;

        /// <summary>
        /// Keadaan kelayakan keuangan terakhir: <c>Pending</c>, <c>Cleared</c>, <c>Blocked</c>, atau
        /// <c>null</c> bila belum pernah dinilai.
        /// </summary>
        public string? FinancialClearanceStatus { get; set; }

        public string FinancialClearanceStatusLabel { get; set; } = string.Empty;

        /// <summary><c>true</c> bila folio tagihan sudah terbentuk untuk kunjungan ini.</summary>
        public bool HasBillingFolio { get; set; }

        /// <summary>
        /// Total tagihan berjalan dari baris tagihan yang sudah berharga. <c>null</c> bila belum ada
        /// folio — bukan nol.
        /// </summary>
        public decimal? RunningTotalAmount { get; set; }

        /// <summary>Jumlah baris tagihan yang harganya belum dapat dihitung.</summary>
        public int UnpricedChargeCount { get; set; }

        /// <summary>
        /// <c>true</c> bila seluruh baris tagihan sudah berharga, sehingga total berjalan dapat dibaca
        /// apa adanya.
        /// </summary>
        public bool IsRunningTotalComplete { get; set; }

        public bool HasDepositAccount { get; set; }

        /// <summary>Deposit yang sudah diterima.</summary>
        public decimal DepositReceivedAmount { get; set; }

        /// <summary>Sisa deposit yang belum dialokasikan.</summary>
        public decimal DepositRemainingAmount { get; set; }

        /// <summary>Kekurangan terhadap kebijakan deposit minimum; <c>0</c> bila tidak kurang.</summary>
        public decimal DepositShortfallAmount { get; set; }

        /// <summary>
        /// Jumlah item yang tidak ditanggung penjamin. <c>null</c> untuk pasien tunai — pertanyaannya
        /// tidak berlaku.
        /// </summary>
        public int? NotCoveredItemCount { get; set; }

        public string Currency { get; set; } = "IDR";

        /// <summary>Kalimat siap tampil yang menjelaskan keadaan angka di atas.</summary>
        public string Message { get; set; } = string.Empty;

        public DateTime ReadAt { get; set; }
    }
}
