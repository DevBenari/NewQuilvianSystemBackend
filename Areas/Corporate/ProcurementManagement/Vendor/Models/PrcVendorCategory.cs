using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models
{
    /// <summary>
    /// Kategori barang/jasa yang dilayani vendor (BE-PBJ-VD-001; PBJ-DEC-052). Kategori menentukan dokumen
    /// wajib, kriteria penilaian, dan kelayakan vendor di RFQ. Dilepas = hapus lunak, tercatat di riwayat.
    /// </summary>
    [Table("PrcVendorCategory", Schema = "public")]
    public class PrcVendorCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid VendorProfileId { get; set; }

        public Guid ItemCategoryId { get; set; }

        public PrcVendorProfile? VendorProfile { get; set; }

        public MstProcurementItemCategory? ItemCategory { get; set; }
    }
}
