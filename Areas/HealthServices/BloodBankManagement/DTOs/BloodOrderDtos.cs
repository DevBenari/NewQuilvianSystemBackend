using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    // =====================================================================
    // Permintaan
    // =====================================================================

    /// <summary>Satu baris kebutuhan pada permintaan pembuatan order.</summary>
    public class BloodOrderLineRequest
    {
        /// <summary>Komponen darah dari katalog. Ketikan bebas ditolak (<c>VAL-BD-003</c>).</summary>
        [Required]
        public Guid BloodComponentId { get; set; }

        /// <summary>Jumlah kantong yang diminta, wajib lebih dari nol (<c>VAL-BD-002</c>).</summary>
        [Range(1, int.MaxValue)]
        public int RequestedQuantity { get; set; }
    }

    /// <summary>
    /// Membuat order darah <b>elektronik</b> dari unit pelayanan yang berwenang.
    /// </summary>
    /// <remarks>
    /// <b>Tidak memuat pelaku input.</b> Pelaku diturunkan dari pengguna terautentikasi, supaya
    /// tidak ada order yang mengaku dibuat orang lain, dan supaya <c>VAL-BD-011</c> tidak
    /// pernah dapat dilanggar dengan mengosongkan sebuah field.
    /// </remarks>
    public class CreateBloodOrderRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Unit pelayanan pemesan. Wajib berpenanda <c>IsAvailableForBloodOrder = true</c>
        /// (<c>VAL-BD-013</c>).
        /// </summary>
        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        public Guid RequestingDoctorId { get; set; }

        /// <summary>Baris kebutuhan. Minimal satu baris.</summary>
        [Required]
        [MinLength(1)]
        public List<BloodOrderLineRequest> Lines { get; set; } = new();
    }

    /// <summary>
    /// Membuat order darah <b>manual</b>, diinput petugas Bank Darah dari order kertas
    /// (<c>DEC-BD-006</c>).
    /// </summary>
    /// <remarks>
    /// Bentuknya sama dengan order elektronik, dan itu disengaja: yang membedakan keduanya
    /// bukan isian yang berbeda melainkan <b>siapa yang menginput</b> dan penanda
    /// <c>OrderSource</c> yang tersimpan. Kelengkapan seluruh rujukan tetap dijaga
    /// <c>VAL-BD-010</c>.
    /// </remarks>
    public class CreateManualBloodOrderRequest : CreateBloodOrderRequest
    {
    }

    /// <summary>
    /// Melanjutkan pembuatan order yang tertahan deteksi ganda, dengan alasan tertulis
    /// (<c>ASM-BD-001</c>, <c>VAL-BD-001</c>).
    /// </summary>
    /// <remarks>
    /// <b>Bukan tombol "abaikan pemeriksaan".</b> Permintaan ini membuat order yang sama persis
    /// dengan permintaan aslinya, ditambah alasan tertulis yang wajib dan tersimpan permanen
    /// pada ordernya. Tanpa alasan, tidak ada jalan melewati penahanan.
    /// </remarks>
    public class ConfirmDuplicateOrderRequest : CreateBloodOrderRequest
    {
        /// <summary>Alasan tertulis melanjutkan order yang terdeteksi ganda.</summary>
        [Required]
        [MaxLength(500)]
        public string DuplicateOverrideReason { get; set; } = string.Empty;

        /// <summary>
        /// Menandai bahwa order ini diinput Bank Darah, bukan dikirim unit pelayanan.
        /// </summary>
        public bool IsManual { get; set; }
    }

    /// <summary>
    /// Membatalkan order dengan alasan yang dipilih dari daftar terkendali
    /// (<c>DEC-BD-044</c>).
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada field teks bebas untuk alasannya.</b> Yang diterima hanya kode alasan dari
    /// <c>MstBloodBankReason</c>; teksnya disalin backend saat pembatalan (<c>VAL-BD-016</c>).
    /// Kategori kode itulah yang membedakan pembatalan klinis dari pembatalan operasional, dan
    /// kesesuaiannya dengan peran pelaku dijaga <c>VAL-BD-083</c>.
    /// </remarks>
    public class CancelBloodOrderRequest
    {
        /// <summary>Kode alasan terkendali dari <c>MstBloodBankReason</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>
        /// Token konkurensi yang dipegang layar saat membuka order. Bila order sudah berpindah
        /// status di tangan orang lain, pembatalan ditolak <c>409</c>.
        /// </summary>
        public int? Version { get; set; }
    }

    // =====================================================================
    // Balasan
    // =====================================================================

    /// <summary>Satu baris pada daftar kerja order darah (<c>DEC-BD-023</c>).</summary>
    public class BloodOrderListDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public Guid RequestingDoctorId { get; set; }
        public string? RequestingDoctorName { get; set; }

        public BbkOrderSource OrderSource { get; set; }
        public string OrderSourceLabel { get; set; } = string.Empty;
        public BbkBloodOrderStatus OrderStatus { get; set; }
        public string OrderStatusLabel { get; set; } = string.Empty;

        /// <summary>Total kantong yang diminta seluruh baris.</summary>
        public int TotalRequestedQuantity { get; set; }

        /// <summary>
        /// Benar bila order ini dilanjutkan walau terdeteksi ganda. Dihitung dari riwayat
        /// <c>BbkTransitionHistory</c>, bukan kolom order.
        /// </summary>
        public bool IsDuplicateOverridden { get; set; }

        public int Version { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>Baris kebutuhan pada detail order.</summary>
    public class BloodOrderLineDto
    {
        public Guid Id { get; set; }
        public Guid BloodComponentId { get; set; }
        public string? BloodComponentCode { get; set; }
        public string? BloodComponentName { get; set; }
        public int RequestedQuantity { get; set; }
        public int Sequence { get; set; }
    }

    /// <summary>Detail satu order beserta baris, pemenuhan, dan riwayat perpindahan statusnya.</summary>
    public class BloodOrderDetailDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public Guid RequestingDoctorId { get; set; }
        public string? RequestingDoctorName { get; set; }

        public BbkOrderSource OrderSource { get; set; }
        public string OrderSourceLabel { get; set; } = string.Empty;
        public Guid? InputByUserId { get; set; }

        public BbkBloodOrderStatus OrderStatus { get; set; }
        public string OrderStatusLabel { get; set; } = string.Empty;

        /// <summary>
        /// Alasan tertulis ketika order ganda tetap dilanjutkan (<c>ASM-BD-001</c>), beserta
        /// pelaku dan waktunya. Diturunkan dari riwayat <c>CreateConfirmedDuplicate</c>.
        /// </summary>
        public string? DuplicateOverrideReason { get; set; }
        public Guid? DuplicateOverrideByUserId { get; set; }
        public DateTime? DuplicateOverrideAt { get; set; }

        /// <summary>
        /// Sejak kapan order berhenti aktif karena kunjungannya berakhir. Diturunkan dari
        /// riwayat <c>Expire</c>: untuk Rawat Inap nilainya waktu pasien benar-benar pulang,
        /// bukan waktu episode ditutup (<c>AC-BD-017</c>).
        /// </summary>
        public DateTime? ExpiredAt { get; set; }

        public int Version { get; set; }

        public List<BloodOrderLineDto> Lines { get; set; } = new();

        /// <summary>Ringkasan pemenuhan, dihitung dari pemberian nyata (<c>BD-DOM-17</c>).</summary>
        public FulfillmentSummaryDto? Fulfillment { get; set; }

        /// <summary>Riwayat perpindahan status order ini, terlama lebih dulu.</summary>
        public List<BloodOrderTransitionDto> Transitions { get; set; } = new();

        /// <summary>
        /// Aksi yang layak dicoba pada keadaan sekarang. <b>Kelayakan status saja</b> — bukan
        /// kewenangan, yang tetap dijaga butir hak akses pada endpoint masing-masing.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();

        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>Satu perpindahan status pada riwayat order.</summary>
    public class BloodOrderTransitionDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string? ReasonCode { get; set; }
        public string? ReasonNote { get; set; }
        public Guid ActorUserId { get; set; }
        public DateTime OccurredAt { get; set; }
    }

    /// <summary>
    /// Ringkasan pemenuhan satu order: diminta, diberikan, dan sisa (<c>BD-DOM-17</c>).
    /// </summary>
    /// <remarks>
    /// <b>Seluruh angka di sini dihitung saat ditanya</b>, tidak pernah dibaca dari kolom yang
    /// dapat disunting. Perhitungannya wajib menghormati catatan koreksi pemberian, sehingga
    /// pemberian yang pencatatannya dikoreksi tidak terhitung dua kali (<c>BD-DOM-23</c>,
    /// <c>DEC-BD-030</c>).
    /// </remarks>
    public class FulfillmentSummaryDto
    {
        public Guid BloodOrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public BbkBloodOrderStatus OrderStatus { get; set; }
        public string OrderStatusLabel { get; set; } = string.Empty;

        public int TotalRequestedQuantity { get; set; }
        public int TotalIssuedQuantity { get; set; }
        public int TotalOutstandingQuantity { get; set; }

        public List<FulfillmentSummaryLineDto> Lines { get; set; } = new();

        /// <summary>
        /// Keterangan jujur tentang batas perhitungan pada tahap pengiriman saat ini.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>Pemenuhan per komponen yang diminta.</summary>
    public class FulfillmentSummaryLineDto
    {
        public Guid BloodOrderLineId { get; set; }
        public Guid BloodComponentId { get; set; }
        public string? BloodComponentCode { get; set; }
        public string? BloodComponentName { get; set; }
        public int RequestedQuantity { get; set; }
        public int IssuedQuantity { get; set; }
        public int OutstandingQuantity { get; set; }
    }

    /// <summary>Angka ringkasan untuk kartu statistik daftar kerja order darah.</summary>
    public class BloodOrderSummaryResponse
    {
        public int TotalOrder { get; set; }
        public int ActiveOrder { get; set; }
        public int PartiallyFulfilledOrder { get; set; }
        public int FullyFulfilledOrder { get; set; }
        public int CancelledOrder { get; set; }
        public int ExpiredOrder { get; set; }

        /// <summary>Order yang dilanjutkan walau terdeteksi ganda.</summary>
        public int DuplicateOverriddenOrder { get; set; }
    }

    // =====================================================================
    // Metadata penyaring
    // =====================================================================

    public class BloodOrderFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public BloodOrderDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BloodOrderOptionItemResponse> OrderStatusOptions { get; set; } = new();
        public List<BloodOrderOptionItemResponse> OrderSourceOptions { get; set; } = new();
        public List<BloodOrderSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class BloodOrderDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public BbkBloodOrderStatus? OrderStatus { get; set; }
        public BbkOrderSource? OrderSource { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BloodOrderOptionItemResponse
    {
        public int Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class BloodOrderSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
