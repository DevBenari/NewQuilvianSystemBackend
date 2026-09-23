using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    /// <summary>
    /// Dugaan reaksi obat dari satu dosis MAR — <c>BE-RWI-117</c>, api-contract <c>keperawatan</c> 0.5.0 bagian 7.10.
    /// </summary>
    public class CreateSuspectedAdverseDrugReactionRequest
    {
        public Guid MedicationAdministrationId { get; set; }

        /// <summary>Wajib. "Ruam gatal di dada 20 menit setelah infus ceftriaxone".</summary>
        public string? ReactionDescription { get; set; }

        public PatientAllergySeverity Severity { get; set; } = PatientAllergySeverity.Unknown;

        public DateTime? ReactionOnsetAt { get; set; }

        public string? ClinicalNote { get; set; }

        public string? IdempotencyKey { get; set; }
    }
}
