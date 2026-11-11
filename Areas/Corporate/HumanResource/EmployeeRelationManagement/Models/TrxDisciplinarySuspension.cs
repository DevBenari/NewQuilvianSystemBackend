using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("trx_disciplinary_suspensions", Schema = "public")]
    public class TrxDisciplinarySuspension : IdentityModel
    {
        [Key]
        [Column("suspension_id")]
        public Guid SuspensionId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public Guid Id
        {
            get => SuspensionId;
            set => SuspensionId = value;
        }

        [Required]
        [Column("investigation_id")]
        public Guid InvestigationId { get; set; }

        [Required]
        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("suspension_number")]
        public string SuspensionNumber { get; set; } = string.Empty;

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("expected_end_date")]
        public DateTime ExpectedEndDate { get; set; } // Max 180 days (6 months)

        [Column("actual_end_date")]
        public DateTime? ActualEndDate { get; set; }

        [Required]
        [MaxLength(32)]
        [Column("wage_payment_status")]
        public string WagePaymentStatus { get; set; } = "PaidFull"; // PaidFull (Pasal 157A UU Ketenagakerjaan)

        [Column("is_system_access_revoked")]
        public bool IsSystemAccessRevoked { get; set; } = true; // INT-HR-88 (SSO SIMRS/EMR/PACS)

        [Column("is_physical_access_revoked")]
        public bool IsPhysicalAccessRevoked { get; set; } = true; // Smartcard akses fisik gedung RS

        [Required]
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "Active"; // Active, CompletedReinstated, EscalatedToTermination

        [Column("reinstatement_notes")]
        public string? ReinstatementNotes { get; set; }

        public TrxDisciplinaryInvestigationRecord? Investigation { get; set; }
    }
}
