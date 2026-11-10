using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Disiplin terapi yang dilayani unit rehab beserta profesinya — konfigurasi
    /// (<c>data-dictionary.md</c> bagian 2.18, RHB-DEC-004).
    /// </summary>
    /// <remarks>
    /// Berprefix <c>Rhb</c>, bukan <c>Mst</c>, karena hanya bermakna di unit rehab (preseden
    /// <c>HmdSetting</c>, <c>OptSetting</c>). Sengaja tidak di sub-folder <c>MasterData</c> agar
    /// tidak berbenturan dengan alias registry <c>Mst</c>. Kewenangan profesi diperiksa lewat
    /// <see cref="ProfessionId"/>, bukan lewat nama profesi.
    /// </remarks>
    [Table("RhbDiscipline", Schema = "public")]
    public class RhbDiscipline : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Contoh: <c>PHYSIOTHERAPY</c>. Tetap setelah dibuat.</summary>
        [Required]
        [MaxLength(40)]
        public string DisciplineCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string DisciplineName { get; set; } = string.Empty;

        /// <summary>Profesi klinis (<c>MstProfession</c>). Wajib sebelum diaktifkan.</summary>
        public Guid? ProfessionId { get; set; }

        public bool IsActive { get; set; }

        public ICollection<RhbDisciplineProcedure> Procedures { get; set; } = new List<RhbDisciplineProcedure>();

        public ICollection<RhbDisciplineInstrument> Instruments { get; set; } = new List<RhbDisciplineInstrument>();
    }
}
