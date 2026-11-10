using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Tindakan master yang boleh dipakai satu disiplin — <c>data-dictionary.md</c> bagian 2.19.
    /// Tarif tetap milik master tindakan dan Billing.
    /// </summary>
    [Table("RhbDisciplineProcedure", Schema = "public")]
    public class RhbDisciplineProcedure : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DisciplineId { get; set; }

        public RhbDiscipline? Discipline { get; set; }

        /// <summary>Tindakan master (<c>MstProcedure</c>).</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
