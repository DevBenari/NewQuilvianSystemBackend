using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Dokumen wajib paket per metode <b>atau</b> per jenis pengadaan (BE-PBJ-PS-001; PBJ-ASM-007, PBJ-DEC-086).
    /// Tepat satu dari <see cref="MethodId"/> dan <see cref="ProcurementTypeId"/> terisi.
    /// </summary>
    /// <remarks>
    /// Contoh: KAK/TOR wajib untuk jenis Jasa (<see cref="ProcurementTypeId"/> terisi), Dasar HPS wajib untuk metode
    /// Penunjukan Langsung (<see cref="MethodId"/> terisi). Saat metode paket dipilih, kewajiban metode itu dan jenis
    /// pengadaan paket disalin menjadi checklist paket, sehingga perubahan di sini tidak mengubah paket yang berjalan.
    /// </remarks>
    [Table("MstProcurementPackageDocumentRequirement", Schema = "public")]
    public class MstProcurementPackageDocumentRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentTypeId { get; set; }

        public Guid? MethodId { get; set; }

        public Guid? ProcurementTypeId { get; set; }

        public bool IsActive { get; set; } = true;

        public MstProcurementPackageDocumentType? DocumentType { get; set; }

        public MstProcurementMethod? Method { get; set; }

        public MstProcurementType? ProcurementType { get; set; }
    }
}
