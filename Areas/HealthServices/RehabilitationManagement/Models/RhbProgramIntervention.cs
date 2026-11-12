using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Intervensi yang direncanakan program — <c>data-dictionary.md</c> bagian 2.9.
    /// <c>ProcedureId</c> wajib terdaftar pada <c>RhbDisciplineProcedure</c> aktif disiplin itu.
    /// </summary>
    [Table("RhbProgramIntervention", Schema = "public")]
    public class RhbProgramIntervention : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProgramId { get; set; }

        public RhbProgram? Program { get; set; }

        /// <summary>Tindakan master (<c>MstProcedure</c>).</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>Dosis/durasi/intensitas yang direncanakan. SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string ParametersText { get; set; } = string.Empty;
    }
}
