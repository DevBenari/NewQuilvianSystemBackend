using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Salinan dokumen wajib paket saat metode dipilih (BE-PBJ-PS-004; PBJ-ASM-007). Terpenuhi bila ada lampiran
    /// <c>PBJ_PACKAGE</c> berkategori <see cref="DocumentTypeCode"/>.
    /// </summary>
    /// <remarks>
    /// Contoh: paket Jasa kalibrasi wajib KAK_TOR dan DASAR_HPS.
    /// </remarks>
    [Table("PrcPackageChecklistItem", Schema = "public")]
    public class PrcPackageChecklistItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PackageId { get; set; }

        public PrcPackage? Package { get; set; }

        public Guid DocumentTypeId { get; set; }

        public MstProcurementPackageDocumentType? DocumentType { get; set; }

        [Required]
        [MaxLength(30)]
        public string DocumentTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DocumentTypeName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;
    }
}
