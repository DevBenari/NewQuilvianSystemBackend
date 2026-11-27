using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Penanda milik Laboratorium bahwa satu jenis pemeriksaan <b>sensitif</b>: order yang memuatnya
    /// butuh persetujuan kedua sebelum hasil diserahkan kepada pasien (<c>LAB-DC-080</c>,
    /// <c>LAB-DEC-182</c>, <c>LAB-DEC-294</c>).
    /// </summary>
    /// <remarks>
    /// Merujuk <see cref="MstProcedure"/> milik Master Data tanpa menulisnya. Pencabutan = tanda hapus
    /// beralasan (<see cref="RemovalReason"/> + kolom hapus <c>IdentityModel</c>); penandaan ulang =
    /// baris baru.
    /// </remarks>
    [Table("LabSensitiveProcedure", Schema = "public")]
    public class LabSensitiveProcedure : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProcedureId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        /// <summary>Alasan pencabutan; terisi bersama <c>IsDelete = true</c>.</summary>
        [MaxLength(500)]
        public string? RemovalReason { get; set; }

        public MstProcedure? Procedure { get; set; }
    }
}
