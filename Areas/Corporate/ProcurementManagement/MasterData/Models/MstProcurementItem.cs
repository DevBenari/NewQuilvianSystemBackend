using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Katalog barang/jasa Pengadaan (PBJ-DEC-022). Baris rencana dan PR wajib memilih barang dari
    /// sini, sehingga konsolidasi kebutuhan digabung per kode, tidak pernah per nama.
    /// </summary>
    /// <remarks>
    /// Kode <c>KTL-000045</c> dibuat server lewat <c>NumberSeriesAllocator</c> (PBJ-DEC-031).
    /// Katalog tidak menyimpan harga; harga perkiraan ada pada baris dokumen. Barang yang sudah
    /// dipakai dokumen hanya dapat dinonaktifkan (PBJ-VAL-071).
    /// </remarks>
    [Table("MstProcurementItem", Schema = "public")]
    public class MstProcurementItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        public Guid ProcurementTypeId { get; set; }
        public MstProcurementType? ProcurementType { get; set; }

        public Guid ItemCategoryId { get; set; }
        public MstProcurementItemCategory? ItemCategory { get; set; }

        /// <summary>Satuan dari master Health Services (<c>MstMeasurement</c>), tidak disalin.</summary>
        public Guid MeasurementId { get; set; }
        public MstMeasurement? Measurement { get; set; }

        /// <summary>Rujukan opsional ke master obat; hanya untuk jenis barang (PBJ-VAL-072).</summary>
        public Guid? DrugId { get; set; }
        public MstDrug? Drug { get; set; }

        [MaxLength(1000)]
        public string? DefaultSpecification { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
