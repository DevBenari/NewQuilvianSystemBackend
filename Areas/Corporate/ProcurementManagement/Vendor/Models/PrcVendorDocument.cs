using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models
{
    /// <summary>
    /// Satu dokumen legalitas vendor dan hasil verifikasinya (BE-PBJ-VD-001; PBJ-DEC-045, PBJ-DEC-046).
    /// </summary>
    /// <remarks>
    /// Berkasnya disimpan di <c>PrcAttachment</c> (<c>DocumentType = PBJ_VENDOR_DOCUMENT</c>). <c>CreateBy</c> adalah
    /// pengunggah; pemeriksa (<see cref="VerifiedByUserId"/>) wajib orang lain. Dokumen pengganti adalah baris baru,
    /// dokumen lama tetap tersimpan. Contoh: izin PBF baru 2027 dicatat sebagai baris baru; izin 2026 tetap terbaca.
    /// </remarks>
    [Table("PrcVendorDocument", Schema = "public")]
    public class PrcVendorDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VendorProfileId { get; set; }

        public Guid DocumentTypeId { get; set; }

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public DateOnly? IssuedDate { get; set; }

        /// <summary>Wajib bila jenisnya <c>RequiresExpiryDate</c> (PBJ-VAL-101).</summary>
        public DateOnly? ExpiryDate { get; set; }

        /// <summary><c>PENDING</c>, <c>VERIFIED</c>, <c>REJECTED</c>, <c>CANCELLED</c>.</summary>
        [Required]
        [MaxLength(20)]
        public string VerificationStatus { get; set; } = "PENDING";

        public Guid? VerifiedByUserId { get; set; }

        public DateTimeOffset? VerifiedAt { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public PrcVendorProfile? VendorProfile { get; set; }

        public MstProcurementVendorDocumentType? DocumentType { get; set; }
    }
}
