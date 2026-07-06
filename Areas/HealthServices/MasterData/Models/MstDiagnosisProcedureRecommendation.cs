using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstDiagnosisProcedureRecommendation", Schema = "public")]
    public class MstDiagnosisProcedureRecommendation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DiagnosisId { get; set; }

        public Guid? ProcedureId { get; set; }
        // Nullable agar bisa menampung rekomendasi monitoring/lab/radiologi sederhana walau belum ada master tindakan terkait.

        [Required]
        [MaxLength(50)]
        public string RecommendationType { get; set; } = "Procedure";
        // Procedure, Lab, Radiology, Monitoring, Referral, FollowUp

        [Required]
        [MaxLength(250)]
        public string RecommendationName { get; set; } = string.Empty;
        // Nama rekomendasi yang tampil di SOAP dokter.

        [MaxLength(1000)]
        public string? InstructionText { get; set; }
        // Instruksi singkat untuk dokter.


        public bool IsActive { get; set; } = true;

        public MstDiagnosis? Diagnosis { get; set; }

        public MstProcedure? Procedure { get; set; }
    }
}
