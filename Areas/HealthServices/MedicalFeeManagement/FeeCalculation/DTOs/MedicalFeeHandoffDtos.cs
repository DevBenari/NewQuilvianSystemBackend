using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs
{
    /// <summary>
    /// Parameter filter dan penomoran halaman untuk daftar penyerahan jasa ke Finance (MDF-API-1.0 §7).
    /// </summary>
    public class MedicalFeeHandoffListQuery
    {
        /// <summary>Filter berdasarkan kode periode (contoh: '2026-10').</summary>
        public string? PeriodCode { get; set; }

        /// <summary>Filter berdasarkan status: Created, Acknowledged, Failed.</summary>
        public string? Status { get; set; }

        /// <summary>Filter jenis penerima: Doctor atau Workforce.</summary>
        public string? PayeeType { get; set; }

        /// <summary>Kata kunci pencarian (PayeeReferenceId, PeriodCode, atau HandoffKey).</summary>
        public string? Search { get; set; }

        /// <summary>Batas awal tanggal penyerahan dibuat.</summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>Batas akhir tanggal penyerahan dibuat.</summary>
        public DateTime? DateTo { get; set; }

        /// <summary>Nomor halaman (1-based, default: 1).</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Ukuran per halaman (default: 20, max: 100).</summary>
        public int PageSize { get; set; } = 20;

        /// <summary>Kolom pengurutan (default: CreateDateTime).</summary>
        public string? SortBy { get; set; } = "CreateDateTime";

        /// <summary>Arah pengurutan: 'asc' atau 'desc' (default: desc).</summary>
        public string? SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// Ringkasan baris penyerahan hasil jasa medis untuk tampilan tabel (MDF-INTEGRATION-1.0 §7.1).
    /// </summary>
    public class MedicalFeeHandoffItemDto
    {
        public Guid Id { get; set; }
        public Guid ServiceFeeId { get; set; }
        public string FeeNumber { get; set; } = string.Empty;
        public Guid HandoffKey { get; set; }
        public string PayeeType { get; set; } = string.Empty;
        public Guid PayeeReferenceId { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public string PeriodCode { get; set; } = string.Empty;
        public decimal GrossAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid CorrelationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? FailureReason { get; set; }
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Detail lengkap satu baris penyerahan ke Finance beserta jejak audit dan rujukan periode.
    /// </summary>
    public class MedicalFeeHandoffDetailDto
    {
        public Guid Id { get; set; }
        public Guid ServiceFeeId { get; set; }
        public string FeeNumber { get; set; } = string.Empty;
        public Guid HandoffKey { get; set; }
        public string PayeeType { get; set; } = string.Empty;
        public Guid PayeeReferenceId { get; set; }
        public string PayeeName { get; set; } = string.Empty;
        public Guid PeriodId { get; set; }
        public string PeriodCode { get; set; } = string.Empty;
        public decimal GrossAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid CorrelationId { get; set; }
        public Guid? CausationId { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? FailureReason { get; set; }
        public Guid RowVersion { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime? LastUpdateDateTime { get; set; }
        public Guid? LastUpdateBy { get; set; }

        // Ringkasan hasil jasa terkait
        public decimal ServiceFeeFinalAmount { get; set; }
        public string ServiceFeeStatus { get; set; } = string.Empty;
        public int DetailCount { get; set; }
    }

    /// <summary>
    /// Permintaan konfirmasi penerimaan dari Finance AP (MDF-VAL-1.0 §7.8, T-75).
    /// HandoffKey wajib disertakan dan cocok dengan data tersimpan.
    /// </summary>
    public class AcknowledgeHandoffRequest
    {
        [Required(ErrorMessage = "Kunci penyerahan (HandoffKey) wajib diisi.")]
        public Guid HandoffKey { get; set; }

        /// <summary>Token kontrol konkurensi optimistik.</summary>
        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Permintaan penolakan dari Finance AP (MDF-VAL-1.0 §7.9, T-77).
    /// Wajib menyertakan alasan penolakan minimal 10 karakter.
    /// </summary>
    public class FailHandoffRequest
    {
        [Required(ErrorMessage = "Alasan penolakan (FailureReason) wajib diisi.")]
        [MinLength(10, ErrorMessage = "Alasan penolakan (FailureReason) wajib diisi minimal 10 karakter (MDF-VAL-1.0 §7.9, T-77).")]
        [MaxLength(500, ErrorMessage = "Alasan penolakan tidak boleh melebihi 500 karakter.")]
        public string FailureReason { get; set; } = string.Empty;

        /// <summary>Token kontrol konkurensi optimistik.</summary>
        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Permintaan pengiriman ulang penyerahan yang berstatus Failed (MDF-STATE-1.0 §5, T-76).
    /// </summary>
    public class ResendHandoffRequest
    {
        /// <summary>Token kontrol konkurensi optimistik.</summary>
        public Guid? ExpectedRowVersion { get; set; }
    }

    /// <summary>
    /// Opsi metadata filter untuk tampilan frontend penyerahan ke Finance.
    /// </summary>
    public class HandoffFilterOptionItem
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class MedicalFeeHandoffMetadataResponse
    {
        public List<HandoffFilterOptionItem> Statuses { get; set; } = new();
        public List<HandoffFilterOptionItem> PayeeTypes { get; set; } = new();
        public List<string> PeriodCodes { get; set; } = new();
    }
}
