using System;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class CreateJobOfferRequest
    {
        [Required(ErrorMessage = "ApplicationId wajib diisi.")]
        public Guid ApplicationId { get; set; }

        [Required(ErrorMessage = "SalaryGradeId wajib diisi.")]
        public Guid SalaryGradeId { get; set; }

        [Required(ErrorMessage = "ContractTypeId wajib diisi (VR-ONB-50).")]
        public Guid ContractTypeId { get; set; }

        [Range(1, 60, ErrorMessage = "ContractDurationMonths harus antara 1 sampai 60 bulan (VR-ONB-51).")]
        public int? ContractDurationMonths { get; set; }

        [Range(1, 1000000000, ErrorMessage = "OfferedBasicSalary harus bernilai positif.")]
        public decimal OfferedBasicSalary { get; set; }

        [Required(ErrorMessage = "OfferedPositionTitle wajib diisi.")]
        [MaxLength(150)]
        public string OfferedPositionTitle { get; set; } = string.Empty;

        public DateOnly ProposedJoinDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public sealed class UpdateJobOfferRequest
    {
        [Required(ErrorMessage = "SalaryGradeId wajib diisi.")]
        public Guid SalaryGradeId { get; set; }

        [Required(ErrorMessage = "ContractTypeId wajib diisi (VR-ONB-50).")]
        public Guid ContractTypeId { get; set; }

        [Range(1, 60, ErrorMessage = "ContractDurationMonths harus antara 1 sampai 60 bulan (VR-ONB-51).")]
        public int? ContractDurationMonths { get; set; }

        [Range(1, 1000000000, ErrorMessage = "OfferedBasicSalary harus bernilai positif.")]
        public decimal OfferedBasicSalary { get; set; }

        [Required(ErrorMessage = "OfferedPositionTitle wajib diisi.")]
        [MaxLength(150)]
        public string OfferedPositionTitle { get; set; } = string.Empty;

        public DateOnly ProposedJoinDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    public sealed class SendConditionalOfferRequest
    {
        public DateTime? ExpiryDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public sealed class CandidateOfferResponseRequest
    {
        [Required(ErrorMessage = "Status respons wajib diisi (Accepted/Rejected).")]
        public JobOfferStatus ResponseStatus { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class JobOfferResponse
    {
        public Guid Id { get; set; }
        public string OfferNumber { get; set; } = string.Empty;
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public string CandidateCode { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public Guid SalaryGradeId { get; set; }
        public string SalaryGradeCode { get; set; } = string.Empty;
        public string SalaryGradeName { get; set; } = string.Empty;
        public Guid? ContractTypeId { get; set; }
        public string? ContractTypeName { get; set; }
        public int? ContractDurationMonths { get; set; }
        public DateOnly? ContractEndDate => (ContractDurationMonths.HasValue && ContractDurationMonths.Value > 0)
            ? ProposedJoinDate.AddMonths(ContractDurationMonths.Value).AddDays(-1)
            : null;
        public decimal OfferedBasicSalary { get; set; }
        public decimal MaximumGradeSalary { get; set; }
        public JobOfferApprovalTier ApprovalTier { get; set; }
        public string ApprovalTierName => ApprovalTier.ToString();
        public string OfferedPositionTitle { get; set; } = string.Empty;
        public DateOnly ProposedJoinDate { get; set; }
        public DateTime OfferDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public JobOfferStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime? CandidateResponseDate { get; set; }
        public string? CandidateResponseNotes { get; set; }
        public string ConditionalNotice => "Surat Penawaran Kerja Bersyarat: Pengangkatan resmi wajib memenuhi kelaikan hasil tes kesehatan pra-kerja (MCU) dan verifikasi dokumen legalitas.";
        public Guid RowVersion { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public sealed class JobOfferListItemResponse
    {
        public Guid Id { get; set; }
        public string OfferNumber { get; set; } = string.Empty;
        public string ApplicationNumber { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string OfferedPositionTitle { get; set; } = string.Empty;
        public DateOnly ProposedJoinDate { get; set; }
        public Guid? ContractTypeId { get; set; }
        public string? ContractTypeName { get; set; }
        public int? ContractDurationMonths { get; set; }
        public DateOnly? ContractEndDate => (ContractDurationMonths.HasValue && ContractDurationMonths.Value > 0)
            ? ProposedJoinDate.AddMonths(ContractDurationMonths.Value).AddDays(-1)
            : null;
        public decimal OfferedBasicSalary { get; set; }
        public JobOfferApprovalTier ApprovalTier { get; set; }
        public string ApprovalTierName => ApprovalTier.ToString();
        public JobOfferStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime OfferDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public sealed class JobOfferQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public JobOfferStatus? Status { get; set; }
        public JobOfferApprovalTier? ApprovalTier { get; set; }
        public string? Search { get; set; }
    }
}
