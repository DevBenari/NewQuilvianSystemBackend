using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Tindakan yang benar-benar diberikan pada satu sesi — <c>data-dictionary.md</c> bagian 2.13.
    /// </summary>
    /// <remarks>
    /// Intervensi di luar rencana (<see cref="ProgramInterventionId"/> kosong) wajib disertai
    /// deviasi <c>Modified</c>. <see cref="PatientProcedureId"/> terisi saat sesi ditandatangani
    /// dan unik agar satu tindakan tidak tertagih dua kali.
    /// </remarks>
    [Table("RhbSessionIntervention", Schema = "public")]
    public class RhbSessionIntervention : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        public RhbSession? Session { get; set; }

        public Guid? ProgramInterventionId { get; set; }

        public RhbProgramIntervention? ProgramIntervention { get; set; }

        /// <summary>Tindakan master (<c>MstProcedure</c>).</summary>
        [Required]
        public Guid ProcedureId { get; set; }

        /// <summary>Tindakan aktual, bukan salinan rencana. SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string ActualParametersText { get; set; } = string.Empty;

        public int? DurationMinutes { get; set; }

        public int Quantity { get; set; } = 1;

        /// <summary>Tindakan pasien bertarif (<c>TrxPatientProcedure</c>) hasil handoff.</summary>
        public Guid? PatientProcedureId { get; set; }
    }
}
