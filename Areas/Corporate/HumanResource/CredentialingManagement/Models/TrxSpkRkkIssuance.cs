using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models
{
    [Table("TrxSpkRkkIssuance", Schema = "public")]
    public class TrxSpkRkkIssuance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? CommitteeId { get; set; }
        public Guid? CredentialingApplicationId { get; set; }

        [Required]
        [MaxLength(80)]
        public string SpkNumber { get; set; } = string.Empty; // Format: SPK/RSQ-DIRUT/[ROMAWI]/[TAHUN]/[NO]

        [Required]
        [MaxLength(80)]
        public string RkkNumber { get; set; } = string.Empty; // Format: RKK/RSQ-DIRUT/[ROMAWI]/[TAHUN]/[NO]

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
        public DateTime ValidUntil { get; set; } // Siklus 3 tahun sesuai CRD-B3

        [MaxLength(50)]
        public string ClinicalPrivilegeType { get; set; } = "Regular"; // Regular, Provisional, Emergency

        public Guid? DirectorGeneralUserId { get; set; }

        [MaxLength(150)]
        public string DirectorGeneralName { get; set; } = string.Empty;

        public bool IsSignedByDirectorGeneral { get; set; } = false;
        public DateTime? SignedAt { get; set; }

        [MaxLength(64)]
        public string? VerificationHash { get; set; }

        [MaxLength(500)]
        public string? QrCodeImagePath { get; set; }

        [MaxLength(500)]
        public string? SpkPdfFilePath { get; set; }

        [MaxLength(500)]
        public string? RkkPdfFilePath { get; set; }

        public long? SpkFileSizeBytes { get; set; }
        public string? SpkFileChecksum { get; set; }

        public Guid? EDossierDocumentId { get; set; } // Tautan arsip ke WfpDocument Sub-02

        [MaxLength(30)]
        public string IssuanceStatus { get; set; } = "Active"; // Active, Suspended, Expired, Revoked

        public bool IsSuspended { get; set; } = false;
        public DateTime? SuspendedAt { get; set; }
        public string? SuspensionReason { get; set; }

        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public string? RevocationBapNumber { get; set; }
        public string? RevocationReason { get; set; }

        public string? ApprovedPrivilegesSnapshotJson { get; set; } // Daftar rincian kewenangan klinis yang disahkan

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstCredentialingCommittee? Committee { get; set; }
        public TrxCredentialingApplication? CredentialingApplication { get; set; }
        public ApplicationUser? DirectorGeneralUser { get; set; }
    }
}
