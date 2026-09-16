using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    // =====================================================================
    // Permintaan
    // =====================================================================

    /// <summary>Membuat permintaan darah ke PMI dari satu order darah yang masih aktif.</summary>
    /// <remarks>
    /// <b>Hanya order asal yang diterima.</b> Pasien disalin dari ordernya (<c>DEC-BD-003</c>), dan
    /// jumlah yang diminta diturunkan dari baris ordernya per komponen — tidak ada isian jumlah
    /// yang dapat berbeda dari kebutuhan yang tercatat. Nomor permintaan diterbitkan backend lewat
    /// number-series, tidak dikirim klien.
    /// </remarks>
    public class CreateProviderRequestRequest
    {
        [Required]
        public Guid BloodOrderId { get; set; }
    }

    /// <summary>Satu kantong fisik pada penerimaan.</summary>
    public class ReceivedBloodUnitRequest
    {
        /// <summary>Nomor kantong dari PMI (<c>ASM-BD-003</c>). Unik. <b>Sensitif.</b></summary>
        [Required]
        [MaxLength(50)]
        public string PmiBagNumber { get; set; } = string.Empty;

        /// <summary>Komponen darah kantong ini, dari katalog.</summary>
        [Required]
        public Guid BloodComponentId { get; set; }
    }

    /// <summary>Mencatat satu kedatangan fisik kantong dari PMI, termasuk yang berlebih.</summary>
    /// <remarks>
    /// <b>Tidak memuat petugas penerima.</b> Petugasnya diturunkan dari pengguna terautentikasi.
    /// Kantong yang melebihi jumlah diminta <b>tidak ditolak</b>: ia tetap dicatat, ditandai
    /// berlebih, dan balasannya membawa peringatan <c>VAL-BD-014</c> dengan status <c>200</c>.
    /// </remarks>
    public class RecordReceiptRequest
    {
        /// <summary>
        /// Waktu kantong diterima fisik. Kosong berarti saat ini. Tidak boleh melewati waktu
        /// sekarang.
        /// </summary>
        public DateTime? ReceivedAt { get; set; }

        /// <summary>Kantong yang datang pada kedatangan ini. Minimal satu.</summary>
        [Required]
        [MinLength(1)]
        public List<ReceivedBloodUnitRequest> Units { get; set; } = new();
    }

    /// <summary>Membatalkan permintaan dengan alasan yang dipilih dari daftar terkendali.</summary>
    /// <remarks>
    /// Hanya kode alasan dari <c>MstBloodBankReason</c> yang diterima; teksnya disalin backend saat
    /// pembatalan (<c>VAL-BD-016</c>).
    /// </remarks>
    public class CancelProviderRequestRequest
    {
        [Required]
        [MaxLength(30)]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>
        /// Token konkurensi yang dipegang layar. Bila permintaan sudah berubah di tangan orang
        /// lain, pembatalan ditolak <c>409</c>.
        /// </summary>
        public int? Version { get; set; }
    }

    // =====================================================================
    // Balasan
    // =====================================================================

    /// <summary>Satu baris pada daftar permintaan darah ke PMI.</summary>
    public class ProviderRequestListDto
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid BloodOrderId { get; set; }
        public string? OrderNumber { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }

        public BbkProviderRequestStatus RequestStatus { get; set; }
        public string RequestStatusLabel { get; set; } = string.Empty;

        /// <summary>Jumlah kantong yang diminta, diturunkan dari baris order asal.</summary>
        public int TotalRequestedQuantity { get; set; }

        /// <summary>Seluruh kantong yang sudah diterima fisik, termasuk yang berlebih.</summary>
        public int TotalReceivedQuantity { get; set; }

        /// <summary>Kantong yang datang melebihi jumlah diminta untuk komponennya.</summary>
        public int TotalExcessQuantity { get; set; }

        /// <summary>Sisa permintaan. Tidak pernah negatif (<c>INV-BD-017</c>).</summary>
        public int TotalOutstandingQuantity { get; set; }

        public int Version { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>Pemenuhan permintaan per komponen darah.</summary>
    public class ProviderRequestComponentDto
    {
        public Guid BloodComponentId { get; set; }
        public string? BloodComponentCode { get; set; }
        public string? BloodComponentName { get; set; }

        /// <summary>Jumlah diminta untuk komponen ini. Nol bila komponennya tidak diminta sama sekali.</summary>
        public int RequestedQuantity { get; set; }

        /// <summary>Kantong yang diterima dan terhitung memenuhi permintaan.</summary>
        public int ReceivedQuantity { get; set; }

        /// <summary>Kantong yang diterima melebihi jumlah diminta.</summary>
        public int ExcessQuantity { get; set; }

        /// <summary>Sisa untuk komponen ini, batas bawah nol.</summary>
        public int OutstandingQuantity { get; set; }
    }

    /// <summary>Satu kantong pada riwayat penerimaan.</summary>
    public class ProviderReceiptUnitDto
    {
        public Guid Id { get; set; }
        public string PmiBagNumber { get; set; } = string.Empty;
        public Guid BloodComponentId { get; set; }
        public string? BloodComponentCode { get; set; }
        public string? BloodComponentName { get; set; }
        public bool IsExcess { get; set; }
        public BbkBloodUnitStatus UnitStatus { get; set; }
        public string UnitStatusLabel { get; set; } = string.Empty;
    }

    /// <summary>Satu kedatangan fisik kantong pada detail permintaan.</summary>
    public class ProviderReceiptDto
    {
        public Guid Id { get; set; }
        public int Sequence { get; set; }
        public int ReceivedQuantity { get; set; }
        public int ExcessQuantity { get; set; }
        public DateTime ReceivedAt { get; set; }
        public Guid ReceivedByUserId { get; set; }
        public List<ProviderReceiptUnitDto> Units { get; set; } = new();
    }

    /// <summary>Detail satu permintaan beserta pemenuhan per komponen, penerimaan, dan riwayatnya.</summary>
    public class ProviderRequestDetailDto
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;

        public Guid BloodOrderId { get; set; }
        public string? OrderNumber { get; set; }
        public BbkBloodOrderStatus? OrderStatus { get; set; }
        public string? OrderStatusLabel { get; set; }
        public Guid? EncounterId { get; set; }
        public string? EncounterNumber { get; set; }

        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }

        public BbkProviderRequestStatus RequestStatus { get; set; }
        public string RequestStatusLabel { get; set; } = string.Empty;

        public int TotalRequestedQuantity { get; set; }
        public int TotalReceivedQuantity { get; set; }
        public int TotalExcessQuantity { get; set; }
        public int TotalOutstandingQuantity { get; set; }

        /// <summary>
        /// Sejak kapan permintaan ditutup karena kunjungannya berakhir. Diturunkan dari riwayat
        /// <c>CloseEncounter</c>.
        /// </summary>
        public DateTime? ClosedEncounterAt { get; set; }

        public int Version { get; set; }

        public List<ProviderRequestComponentDto> Components { get; set; } = new();
        public List<ProviderReceiptDto> Receipts { get; set; } = new();
        public List<BloodBankTransitionDto> Transitions { get; set; } = new();

        /// <summary>
        /// Aksi yang layak dicoba pada keadaan sekarang. <b>Kelayakan status saja</b> — bukan
        /// kewenangan, yang tetap dijaga butir hak akses pada endpoint masing-masing.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();

        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>Angka ringkasan daftar permintaan darah ke PMI.</summary>
    public class ProviderRequestSummaryResponse
    {
        public int TotalRequest { get; set; }
        public int RequestedRequest { get; set; }
        public int PartiallyFulfilledRequest { get; set; }
        public int FulfilledRequest { get; set; }
        public int CancelledRequest { get; set; }
        public int ClosedEncounterRequest { get; set; }

        /// <summary>Permintaan yang pernah menerima kantong berlebih.</summary>
        public int RequestWithExcess { get; set; }
    }

    // =====================================================================
    // Metadata penyaring
    // =====================================================================

    public class ProviderRequestFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public ProviderRequestDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BloodBankOptionItemResponse> RequestStatusOptions { get; set; } = new();
        public List<BloodBankSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ProviderRequestDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? BloodOrderId { get; set; }
        public BbkProviderRequestStatus? RequestStatus { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
