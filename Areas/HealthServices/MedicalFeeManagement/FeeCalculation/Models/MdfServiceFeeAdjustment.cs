using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models
{
    /// <summary>Arah penyesuaian koreksi jasa — data-dictionary.md bagian 7.</summary>
    public static class MdfAdjustmentDirections
    {
        public const string Addition = "Addition";
        public const string Deduction = "Deduction";
    }

    /// <summary>Status pengajuan koreksi jasa medis — data-dictionary.md bagian 7.</summary>
    public static class MdfAdjustmentStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }

    /// <summary>
    /// Koreksi penyesuaian hasil jasa medis berjenjang — MDF-DES-017, MF-DEC-007, MF-DEC-009.
    /// Memungkinkan penambahan atau pengurangan nominal hasil jasa dengan penegakan maker-checker
    /// di mana ApprovedBy wajib berbeda dari RequestedBy (CK_MdfServiceFeeAdjustment_Maker).
    /// </summary>
    [Table("MdfServiceFeeAdjustment", Schema = "public")]
    public class MdfServiceFeeAdjustment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor unik pengajuan koreksi, contoh: 'ADJ-202610-0001'.</summary>
        [Required]
        [MaxLength(50)]
        public string AdjustmentNumber { get; set; } = string.Empty;

        /// <summary>FK ke induk hasil jasa (MdfServiceFee.Id), DeleteBehavior.Restrict.</summary>
        [Required]
        public Guid ServiceFeeId { get; set; }

        /// <summary>Arah penyesuaian: Addition atau Deduction (MdfAdjustmentDirections).</summary>
        [Required]
        [MaxLength(20)]
        public string Direction { get; set; } = MdfAdjustmentDirections.Addition;

        /// <summary>Nominal penyesuaian (selalu bernilai positif, arah ditentukan oleh Direction).</summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>Alasan penyesuaian wajib diisi (10-500 karakter).</summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Status koreksi: Pending, Approved, Rejected (MdfAdjustmentStatuses).</summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = MdfAdjustmentStatuses.Pending;

        /// <summary>User ID pembuat pengajuan koreksi.</summary>
        [Required]
        public Guid RequestedBy { get; set; }

        /// <summary>Waktu pengajuan koreksi dibuat.</summary>
        [Required]
        public DateTime RequestedAt { get; set; }

        /// <summary>User ID penyetujui koreksi (wajib berbeda dari RequestedBy).</summary>
        public Guid? ApprovedBy { get; set; }

        /// <summary>Waktu persetujuan atau penolakan koreksi dicatat.</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>Alasan penolakan jika koreksi berstatus Rejected.</summary>
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>Token konkurensi optimistik (MDF-DES-005).</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MdfServiceFee? ServiceFee { get; set; }
    }
}

