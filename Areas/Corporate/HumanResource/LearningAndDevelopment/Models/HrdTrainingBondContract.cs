using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models
{
    /// <summary>
    /// Tabel Perjanjian Kontrak Ikatan Dinas Retensi Pelatihan Nakes (HRD-DEC-162, VR-TRN-16..18)
    /// </summary>
    [Table("HrdTrainingBondContract", Schema = "public")]
    public class HrdTrainingBondContract : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(60)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? TrainingSessionId { get; set; }

        public Guid? TrainingEnrollmentRequestId { get; set; }

        [Required]
        [MaxLength(250)]
        public string TrainingProgramName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalTrainingCost { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal HospitalFinancedAmount { get; set; } = 0;

        /// <summary>
        /// Durasi retensi kerja wajib (Maksimal 36 bulan, VR-TRN-18)
        /// </summary>
        public int RetentionPeriodMonths { get; set; } = 12;

        public DateTime BondStartDate { get; set; }

        public DateTime BondEndDate { get; set; }

        /// <summary>
        /// ACTIVE, FULFILLED, BREACHED_PENALTY_ISSUED, SETTLED_TERMINATED, CANCELLED
        /// </summary>
        [Required]
        [MaxLength(40)]
        public string BondStatus { get; set; } = "ACTIVE";

        public bool IsBreached { get; set; } = false;

        public DateTime? BreachRecordedAt { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal? CalculatedPenaltyAmount { get; set; }

        [MaxLength(1000)]
        public string? BreachReason { get; set; }

        [MaxLength(50)]
        public string? PenaltyInvoiceNumber { get; set; }

        [MaxLength(256)]
        public string? DigitalSignatureHash { get; set; }

        public virtual MstWorkforceProfile? WorkforceProfile { get; set; }
    }
}
