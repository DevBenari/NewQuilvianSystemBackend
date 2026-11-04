using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs
{
    // OPT-API-v1 grup "Health Services / Optical Management / Opt Order" (BE-OPT-032).

    /// <summary>Satu baris barang pada permintaan buat/ubah pesanan.</summary>
    public class OptOrderItemInput
    {
        /// <summary>Barang Farmasi yang sudah punya profil Optik aktif. Kategori diambil dari profil, bukan dari isian.</summary>
        public Guid? DrugId { get; set; }

        /// <summary>Wajib untuk lensa kacamata: stok (dipotong dari depo) atau custom (dipesan ke lab).</summary>
        public OptLensSupply? LensSupply { get; set; }

        public decimal Quantity { get; set; } = 1m;
    }

    /// <summary>
    /// Buat pesanan <c>Draft</c>. Saat ini hanya pesanan berpengerjaan (<c>Fabrication</c>); penjualan langsung datang di
    /// <c>BE-OPT-042</c>. Harga tidak dikirim: tarif dibaca Billing.
    /// </summary>
    public class OptOrderCreateRequest
    {
        public Guid? EncounterId { get; set; }

        public OptOrderKind OrderKind { get; set; } = OptOrderKind.Fabrication;

        /// <summary>Versi resep yang dipakai, wajib untuk pesanan berpengerjaan (<c>OPT-VAL-041</c>). Harus versi terkini dan berlaku.</summary>
        public Guid? PrescriptionVersionId { get; set; }

        public List<OptOrderItemInput> Items { get; set; } = new();
    }

    /// <summary>Ubah pesanan selama <c>Draft</c>: seluruh baris diganti dengan isian ini.</summary>
    public class OptOrderUpdateRequest : OptOrderCreateRequest
    {
        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Simpan &amp; tagihkan.</summary>
    public class OptOrderBillRequest
    {
        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Kirim ke pengerjaan (<c>BE-OPT-034</c>, <c>OPT-DEC-013</c>).</summary>
    public class OptOrderSendToWorkRequest
    {
        /// <summary>Bengkel sendiri (lensa stok) atau lab luar (lensa custom).</summary>
        public OptWorkshopType? WorkshopType { get; set; }

        /// <summary>Wajib bila <c>ExternalLab</c>: pemasok aktif dari data induk.</summary>
        public Guid? ExternalLabSupplierId { get; set; }

        /// <summary>Perkiraan jadi, tanggal WIB, tidak boleh sebelum hari ini.</summary>
        public DateOnly? EstimatedReadyDate { get; set; }

        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Serahkan pesanan (<c>BE-OPT-040</c>, <c>OPT-DEC-048</c>).</summary>
    public class OptOrderHandOverRequest
    {
        /// <summary>Petugas sudah mencocokkan identitas pasien (nama + tanggal lahir atau nomor RM pada nota).</summary>
        public bool HandoverIdentityVerified { get; set; }

        /// <summary>Diisi bila diambil wakil; wajib berpasangan dengan <see cref="ReceiverRelationship"/>.</summary>
        public string? ReceiverName { get; set; }

        /// <summary>Hubungan wakil dengan pasien, mis. "anak".</summary>
        public string? ReceiverRelationship { get; set; }

        /// <summary>Catatan penyesuaian (<i>fitting</i>) saat penyerahan.</summary>
        public string? FittingNote { get; set; }

        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Tanda potong satu lensa stok pada pengajuan batal (<c>OPT-DEC-038</c>).</summary>
    public class OptOrderCancellationItemInput
    {
        public Guid ItemId { get; set; }

        /// <summary>Benar = lensa stok sudah dipotong, sehingga tetap ditagih; salah = kembali ke stok dan barisnya dibatalkan.</summary>
        public bool? IsLensCut { get; set; }
    }

    /// <summary>Ajukan batal sesudah pengerjaan (<c>BE-OPT-041</c>). Wajib menandai setiap lensa stok (<c>OPT-VAL-054</c>).</summary>
    public class OptOrderCancellationCreateRequest
    {
        public string? Reason { get; set; }
        public List<OptOrderCancellationItemInput> Items { get; set; } = new();
        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Keputusan Kepala Optik atas pengajuan batal.</summary>
    public class OptOrderCancelDecisionRequest
    {
        public bool Approve { get; set; }

        /// <summary>Wajib bila menolak.</summary>
        public string? DecisionNote { get; set; }

        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Tutup pesanan sebagai tidak diambil (<c>OPT-DEC-018</c>, <c>049</c>).</summary>
    public class OptOrderNotCollectedRequest
    {
        public string? Reason { get; set; }
        public int? ExpectedVersion { get; set; }
    }

    public class OptOrderCancellationResponse
    {
        public Guid Id { get; set; }
        public OptCancellationStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecisionNote { get; set; }
    }

    /// <summary>Tandai siap diambil.</summary>
    public class OptOrderMarkReadyRequest
    {
        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Batal sebelum pengerjaan.</summary>
    public class OptOrderCancelRequest
    {
        public string? Reason { get; set; }
        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Penyaring daftar pesanan.</summary>
    public class OptOrderListQuery
    {
        public OptOrderStatus? Status { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? PatientId { get; set; }

        /// <summary>Benar = siap diambil dan sudah melewati batas pengingat pengambilan (<c>PickupReminderDays</c>).</summary>
        public bool? NeedsContact { get; set; }

        /// <summary>Benar = masih dikerjakan padahal perkiraan jadi sudah lewat (tanggal WIB).</summary>
        public bool? Overdue { get; set; }

        /// <summary>Mencari nomor pesanan, nomor RM, atau nama pasien.</summary>
        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class OptOrderListItemResponse
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public OptOrderKind OrderKind { get; set; }
        public string OrderKindLabel { get; set; } = string.Empty;
        public OptOrderStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateOnly? EstimatedReadyDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Version { get; set; }
    }

    public class OptOrderItemResponse
    {
        public Guid Id { get; set; }
        public int LineNo { get; set; }
        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public OptItemCategory ItemCategory { get; set; }
        public string ItemCategoryLabel { get; set; } = string.Empty;
        public OptLensSupply? LensSupply { get; set; }
        public decimal Quantity { get; set; }

        /// <summary>Waktu stok baris ini dipotong dari depo Optik; kosong untuk lensa custom atau sebelum pengerjaan.</summary>
        public DateTime? StockIssuedAt { get; set; }

        /// <summary>Waktu stok baris ini kembali ke depo karena pembatalan yang disetujui.</summary>
        public DateTime? StockReturnedAt { get; set; }

        /// <summary>Tanda potong lensa stok dari pengajuan batal; kosong bila tidak pernah diajukan.</summary>
        public bool? IsLensCut { get; set; }

        public OptOrderItemStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
    }

    public class OptOrderStatusHistoryResponse
    {
        public OptOrderStatus? FromStatus { get; set; }
        public OptOrderStatus ToStatus { get; set; }
        public string ToStatusLabel { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string? Reason { get; set; }
    }

    public class OptOrderResponse : OptOrderListItemResponse
    {
        public Guid? PrescriptionId { get; set; }
        public Guid? PrescriptionVersionId { get; set; }
        public string? PrescriptionNumber { get; set; }
        public int? PrescriptionVersionNo { get; set; }

        /// <summary>Persentase minimum uang muka yang dibekukan saat pesanan dibuat (<c>OPT-DEC-009</c>, <c>OPT-AC-006</c>).</summary>
        public decimal DownPaymentMinPercentSnapshot { get; set; }

        public DateTime? BilledAt { get; set; }
        public OptWorkshopType? WorkshopType { get; set; }
        public string? WorkshopTypeLabel { get; set; }
        public Guid? ExternalLabSupplierId { get; set; }
        public string? ExternalLabSupplierName { get; set; }
        public DateTime? SentToWorkAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? NotCollectedAt { get; set; }
        public DateTime? HandedOverAt { get; set; }
        public bool HandoverIdentityVerified { get; set; }
        public string? HandoverReceiverName { get; set; }
        public string? HandoverReceiverRelationship { get; set; }
        public string? FittingNote { get; set; }

        /// <summary>Masa garansi yang dibekukan saat diserahkan (<c>OPT-DEC-041</c>): ukuran (hari), cacat lensa dan frame (bulan).</summary>
        public int? WarrantyFitDaysSnapshot { get; set; }
        public int? WarrantyLensDefectMonthsSnapshot { get; set; }
        public int? WarrantyFrameDefectMonthsSnapshot { get; set; }

        /// <summary>
        /// Hanya pada jawaban serah: benar bila tagihan kunjungan ikut difinalisasi (pesanan terakhir selesai dan lunas), sehingga
        /// kunjungan tertutup (<c>OPT-DEC-043</c>). Kosong pada jawaban lain.
        /// </summary>
        public bool? InvoiceFinalized { get; set; }

        public DateTime? CancelledAt { get; set; }
        public bool IsRemake { get; set; }
        public List<OptOrderItemResponse> Items { get; set; } = new();
        public List<OptOrderStatusHistoryResponse> StatusHistory { get; set; } = new();

        /// <summary>Pengajuan batal sesudah pengerjaan, terlama dulu.</summary>
        public List<OptOrderCancellationResponse> Cancellations { get; set; } = new();

        /// <summary>Diisi bila tagihan belum mengikuti perubahan pesanan (mis. pembatalan baris belum diterima Billing).</summary>
        public string? BillingMessage { get; set; }
    }

    /// <summary>
    /// Status bayar pesanan (<c>INT-OPT-06</c>). <b>Aturan sementara</b> (<c>OPT-DEC-035</c>, menunggu <c>OPT-APR-003</c>):
    /// bagian pasien dihitung per pesanan dari rincian kalkulasi Billing, sedangkan terbayar dibaca per tagihan kunjungan.
    /// </summary>
    public class OptOrderPaymentStatusResponse
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>Bagian pasien untuk baris pesanan ini sesudah diskon item dan tanggungan penjamin.</summary>
        public decimal OrderPatientAmount { get; set; }

        public decimal DownPaymentMinPercent { get; set; }

        /// <summary>Minimum uang muka = persentase × bagian pasien, dibulatkan ke atas ke rupiah penuh.</summary>
        public decimal DownPaymentMinAmount { get; set; }

        /// <summary>Terbayar pada tagihan kunjungan (bukan per baris; lihat <see cref="PaidBasis"/>).</summary>
        public decimal InvoicePaidAmount { get; set; }

        /// <summary>Sisa bagian pasien seluruh tagihan kunjungan.</summary>
        public decimal InvoiceOutstandingAmount { get; set; }

        public bool IsDownPaymentMet { get; set; }

        /// <summary>Benar bila sisa seluruh tagihan kunjungan Rp 0.</summary>
        public bool IsInvoiceSettled { get; set; }

        /// <summary>Nomor pesanan berpengerjaan lain di kunjungan ini yang menahan pesanan ini (<c>OPT-VAL-047</c>); kosong bila tidak ada.</summary>
        public string? BlockingOrderNumber { get; set; }

        public string PaidBasis { get; set; } = Services.OptOrderPaymentGate.PaidBasisPerInvoice;
    }
}
