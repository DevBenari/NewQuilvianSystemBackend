using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxEmployeeBenefitDependent", Schema = "public")]
    public class TrxEmployeeBenefitDependent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeBenefitEnrollmentId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? FamilyMemberId { get; set; }

        public Guid? DependentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string DependentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RelationshipType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(30)]
        public string? GenderSnapshot { get; set; }

        [Required]
        [MaxLength(30)]
        public string DependentStatus { get; set; } = "Draft";

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public decimal CoverageLimitAmount { get; set; } = 0m;

        public decimal UsedAmount { get; set; } = 0m;

        public decimal RemainingAmount { get; set; } = 0m;

        public bool IsEligible { get; set; } = false;

        public bool IsCovered { get; set; } = false;

        public bool IsPrimaryDependent { get; set; } = false;

        [MaxLength(1000)]
        public string? EligibilityReason { get; set; }

        public string? SupportingDocumentJson { get; set; }

        public DateTime? RequestedAt { get; set; }

        public Guid? RequestedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }

        [MaxLength(16)]
        public string? NationalIdNumber { get; set; } // NIK 16 digit (VR-BEN-03)

        [MaxLength(16)]
        public string? FamilyCardNumber { get; set; } // Nomor KK 16 digit (VR-BEN-04)

        public bool IsActiveStudent { get; set; } = false; // Mahasiswa aktif (VR-BEN-05..06)

        [MaxLength(255)]
        public string? StudentVerificationLetterPath { get; set; } // Surat dekanat (VR-BEN-07)

        public DateTime? StudentVerificationExpiryDate { get; set; } // Batas akhir surat 31 Agustus (VR-BEN-08)

        [MaxLength(50)]
        public string? VerificationStatus { get; set; } = "Draft"; // Draft, Submitted, Verified, Rejected, ExpiredAgeLimit

        [MaxLength(1000)]
        public string? RejectionReason { get; set; } // Alasan penolakan minimal 15 char (VR-BEN-25)

        public bool IsActive { get; set; } = true;

        /// <summary>BE-FIN-103, FIN-API-1.10: token konkurensi optimistik, diganti service pada setiap perubahan.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public TrxEmployeeBenefitEnrollment? EmployeeBenefitEnrollment { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public WfpFamilyMember? FamilyMember { get; set; }
        public WfpDependent? Dependent { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }

    }
}
