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
    /// <b>Rupiah tersedia hanya di endpoint amounts.</b> Ringkasan Read tidak memuat nominal.
    /// </para>
    /// <para>
    /// Contoh: "BPJS — layak — deposit kurang — 1 item tidak ditanggung".
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

        /// <summary>Jumlah baris tagihan yang harganya belum dapat dihitung.</summary>
        public int UnpricedChargeCount { get; set; }

        /// <summary>
        /// <c>true</c> bila seluruh baris tagihan sudah berharga, sehingga total berjalan dapat dibaca
        /// apa adanya.
        /// </summary>
        public bool IsRunningTotalComplete { get; set; }

        public bool HasDepositAccount { get; set; }

        /// <summary>Deposit belum memenuhi kebijakan minimum, tanpa memuat nominal.</summary>
        public bool HasDepositShortfall { get; set; }

        /// <summary>
        /// Jumlah item yang tidak ditanggung penjamin. <c>null</c> untuk pasien tunai — pertanyaannya
        /// tidak berlaku.
        /// </summary>
        public int? NotCoveredItemCount { get; set; }


        /// <summary>Kalimat siap tampil yang menjelaskan keadaan angka di atas.</summary>
        public string Message { get; set; } = string.Empty;

        public DateTime ReadAt { get; set; }
    }

    public sealed class PatientBillingAmountResponse
    {
        public Guid EpisodeId { get; set; }
        public Guid EncounterId { get; set; }
        public string InvoiceState { get; set; } = "NOT_FORMED";
        public DateTimeOffset? CalculatedAt { get; set; }
        public decimal? RunningTotalAmount { get; set; }
        public decimal DepositReceivedAmount { get; set; }
        public decimal DepositRemainingAmount { get; set; }
        public decimal DepositShortfallAmount { get; set; }
        public string Currency { get; set; } = "IDR";
    }

    public sealed class PatientBillingBreakdownResponse
    {
        public Guid EpisodeId { get; set; }
        public Guid EncounterId { get; set; }
        public string InvoiceState { get; set; } = "NOT_FORMED";
        public DateTimeOffset? CalculatedAt { get; set; }
        public List<PatientBillingGroupResponse> Groups { get; set; } = [];
        public List<LinkedBillingEncounterResponse> LinkedEncounters { get; set; } = [];
    }

    public sealed class PatientBillingGroupResponse
    {
        public string GroupCode { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public List<PatientBillingLineResponse> Lines { get; set; } = [];
    }

    public sealed class PatientBillingLineResponse
    {
        public string Label { get; set; } = string.Empty;
        public string? PeriodLabel { get; set; }
        public DateTimeOffset? ServiceDate { get; set; }
        public decimal Quantity { get; set; }
        public string UnitLabel { get; set; } = "unit";
        public string LineStatus { get; set; } = "ACTIVE";
        public LinkedBillingEncounterResponse? LinkedEncounter { get; set; }
    }

    public sealed class LinkedBillingEncounterResponse
    {
        public Guid EncounterId { get; set; }
        public string EncounterTypeName { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
    }

    public sealed class PatientBillingBreakdownAmountResponse
    {
        public Guid EpisodeId { get; set; }
        public string InvoiceState { get; set; } = "NOT_FORMED";
        public DateTimeOffset? CalculatedAt { get; set; }
        public List<PatientBillingGroupAmountResponse> Groups { get; set; } = [];
        public decimal? RunningTotalAmount { get; set; }
    }

    public sealed class PatientBillingGroupAmountResponse
    {
        public string GroupCode { get; set; } = string.Empty;
        public decimal? SubtotalAmount { get; set; }
        public bool IncludesLinkedEncounter { get; set; }
    }
}
