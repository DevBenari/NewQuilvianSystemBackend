using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class GenerateBondContractRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        public Guid? TrainingSessionId { get; set; }

        public Guid? TrainingEnrollmentRequestId { get; set; }

        [Required(ErrorMessage = "Nama program pelatihan wajib diisi.")]
        [MaxLength(250)]
        public string TrainingProgramName { get; set; } = string.Empty;

        [Range(0, 1000000000, ErrorMessage = "Biaya pelatihan tidak boleh negatif.")]
        public decimal TotalTrainingCost { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Jumlah pembiayaan RS tidak boleh negatif.")]
        public decimal HospitalFinancedAmount { get; set; }

        [Range(12, 36, ErrorMessage = "Durasi retensi ikatan dinas berada di antara 12 sampai 36 bulan (VR-TRN-18).")]
        public int RetentionPeriodMonths { get; set; } = 12;

        public DateTime? BondStartDate { get; set; }

        [MaxLength(256)]
        public string? DigitalSignatureHash { get; set; }
    }

    public class ProcessBreachPenaltyRequestDto
    {
        [Required(ErrorMessage = "Tanggal pengunduran diri / pemutusan ikatan wajib diisi.")]
        public DateTime ResignationEffectiveDate { get; set; }

        [Required(ErrorMessage = "Alasan pelanggaran kontrak / resign wajib diisi.")]
        [MaxLength(1000)]
        public string BreachReason { get; set; } = string.Empty;

        public decimal MultiplierFactor { get; set; } = 1.0m;
    }

    public class TrainingBondContractDto
    {
        public Guid Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public Guid? TrainingSessionId { get; set; }
        public string TrainingProgramName { get; set; } = string.Empty;
        public decimal TotalTrainingCost { get; set; }
        public decimal HospitalFinancedAmount { get; set; }
        public int RetentionPeriodMonths { get; set; }
        public DateTime BondStartDate { get; set; }
        public DateTime BondEndDate { get; set; }
        public string BondStatus { get; set; } = "ACTIVE";
        public bool IsBreached { get; set; }
        public DateTime? BreachRecordedAt { get; set; }
        public decimal? CalculatedPenaltyAmount { get; set; }
        public string? BreachReason { get; set; }
        public string? PenaltyInvoiceNumber { get; set; }
        public string? DigitalSignatureHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
