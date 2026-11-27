using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Dokumen wajib per kategori vendor (BE-PBJ-VD-001; PBJ-DEC-045, PBJ-DEC-061).
    /// </summary>
    /// <remarks>
    /// Dimulai kosong dan diaktifkan bertahap: baris lahir nonaktif, lalu admin mengaktifkannya setelah dokumen
    /// vendor kategori itu diunggah. Contoh: kategori Obat mewajibkan Izin PBF sejak 01-03-2027
    /// (<see cref="ActivatedAt"/>); kategori lain yang belum diaktifkan tidak mengunci vendor.
    /// </remarks>
    [Table("MstProcurementCategoryDocumentRequirement", Schema = "public")]
    public class MstProcurementCategoryDocumentRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ItemCategoryId { get; set; }

        public Guid DocumentTypeId { get; set; }

        public bool IsActive { get; set; }

        /// <summary>Diisi saat kewajiban pertama kali diaktifkan.</summary>
        public DateTimeOffset? ActivatedAt { get; set; }

        public Guid? ActivatedByUserId { get; set; }

        public MstProcurementItemCategory? ItemCategory { get; set; }

        public MstProcurementVendorDocumentType? DocumentType { get; set; }
    }
}
