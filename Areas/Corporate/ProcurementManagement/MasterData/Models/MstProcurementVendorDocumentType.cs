using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Jenis dokumen legalitas vendor (BE-PBJ-VD-001; PBJ-ASM-001). Contoh: <c>IZIN_PBF</c> Izin Pedagang
    /// Besar Farmasi yang wajib bertanggal berlaku. Daftar awal dari seeder, daftar final dari SOP (PBJ-OQ-019).
    /// </summary>
    [Table("MstProcurementVendorDocumentType", Schema = "public")]
    public class MstProcurementVendorDocumentType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string DocumentTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DocumentTypeName { get; set; } = string.Empty;

        /// <summary>True → dokumen jenis ini wajib punya tanggal berlaku (PBJ-VAL-101).</summary>
        public bool RequiresExpiryDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
