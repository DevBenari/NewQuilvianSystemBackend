using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxClinicalPrivilegeEmergencyOverrideLog", Schema = "public")]
    public class TrxClinicalPrivilegeEmergencyOverrideLog : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrivilegeItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string ProcedureName { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string OverrideReason { get; set; } = string.Empty; // Justifikasi medis darurat (Life-Saving)

        public DateTime OverrideAtUtc { get; set; } = DateTime.UtcNow;

        public Guid? WitnessUserId { get; set; }

        [MaxLength(150)]
        public string? WitnessName { get; set; }

        public bool IsReviewedByKmkp { get; set; } = false; // Komite Mutu & Keselamatan Pasien (KMKP)
        public DateTime? ReviewedAtUtc { get; set; }
        public Guid? ReviewedByUserId { get; set; }

        [MaxLength(2000)]
        public string? KmkpReviewNotes { get; set; }

        [MaxLength(50)]
        public string KmkpVerdict { get; set; } = "PendingReview"; // PendingReview, Justified, Unjustified, DisciplinaryAction

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? WitnessUser { get; set; }
        public ApplicationUser? ReviewedByUser { get; set; }
    }
}
