using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Jenis dokumen paket pengadaan (BE-PBJ-PS-001; PBJ-ASM-007). Contoh: <c>KAK_TOR</c> Kerangka Acuan Kerja pada tahap
    /// persiapan, atau <c>SURAT_PENAWARAN_HARGA</c> pada tahap penawaran.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="DocumentTypeCode"/> dipakai sebagai <c>PrcAttachment.AttachmentCategory</c> untuk lampiran
    /// <c>PBJ_PACKAGE</c>/<c>PBJ_RFQ</c>, sehingga checklist paket terpenuhi bila ada lampiran berkategori kode ini.
    /// </para>
    /// <para>
    /// <see cref="IsPriceSensitive"/> = true menutup berkasnya dari pengguna tanpa izin harga, termasuk evaluator teknis
    /// (PBJ-DEC-079). Contoh: Dasar HPS dan Surat Penawaran Harga.
    /// </para>
    /// <para>
    /// Ini <b>bukan</b> <see cref="MstProcurementVendorDocumentType"/> milik manajemen-vendor, yang berisi dokumen
    /// legalitas vendor (PBJ-CAP-028).
    /// </para>
    /// </remarks>
    [Table("MstProcurementPackageDocumentType", Schema = "public")]
    public class MstProcurementPackageDocumentType : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string DocumentTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DocumentTypeName { get; set; } = string.Empty;

        /// <summary>Tahap paket tempat dokumen ini diunggah: lihat <see cref="ProcurementStatuses.PackageDocumentStage"/>.</summary>
        [Required]
        [MaxLength(20)]
        public string Stage { get; set; } = ProcurementStatuses.PackageDocumentStage.Preparation;

        public bool IsPriceSensitive { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
