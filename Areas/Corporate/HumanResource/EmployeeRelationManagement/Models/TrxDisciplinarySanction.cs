using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models
{
    [Table("trx_disciplinary_sanctions", Schema = "public")]
    public class TrxDisciplinarySanction : IdentityModel
    {
        [Key]
        [Column("sanction_id")]
        public Guid SanctionId { get; set; } = Guid.NewGuid();

        [NotMapped]
        public Guid Id
        {
            get => SanctionId;
            set => SanctionId = value;
        }

        [Column("investigation_id")]
        public Guid? InvestigationId { get; set; }

        [Required]
        [Column("employee_id")]
        public Guid EmployeeId { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("sanction_number")]
        public string SanctionNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        [Column("sanction_level")]
        public string SanctionLevel { get; set; } = "SP1"; // VerbalWarning, SP1, SP2, SP3

        [Column("issued_date")]
        public DateTime IssuedDate { get; set; }

        [Column("valid_until")]
        public DateTime ValidUntil { get; set; } // IssuedDate + 6 Months (or + 3 Months for VerbalWarning)

        [Required]
        [Column("violation_description")]
        public string ViolationDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(32)]
        [Column("status")]
        public string Status { get; set; } = "Active"; // Active, Expired, RevokedOnAppeal

        [Column("is_appealed")]
        public bool IsAppealed { get; set; } = false;

        [Column("appeal_statement")]
        public string? AppealStatement { get; set; }

        [Column("appeal_evidence_urls")]
        public string? AppealEvidenceUrls { get; set; }

        [Column("appeal_resolution")]
        public string? AppealResolution { get; set; }

        [Column("appealed_at")]
        public DateTime? AppealedAt { get; set; }

        public TrxDisciplinaryInvestigationRecord? Investigation { get; set; }
    }
}
