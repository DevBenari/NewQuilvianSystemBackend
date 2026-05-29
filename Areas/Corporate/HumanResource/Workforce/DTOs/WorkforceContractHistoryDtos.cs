using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceContractHistoryResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string ContractNumber { get; set; } = string.Empty;

        public ContractHistoryType ContractType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ContractHistoryStatus ContractStatus { get; set; }

        public bool IsExpired { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceContractHistoryListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int ActiveContractData { get; set; }

        public int ExpiredContractData { get; set; }

        public int TerminatedContractData { get; set; }

        public int DraftContractData { get; set; }

        public List<WorkforceContractHistoryResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceContractHistoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public ContractHistoryType ContractType { get; set; } = ContractHistoryType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ContractHistoryStatus ContractStatus { get; set; } = ContractHistoryStatus.Draft;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceContractHistoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public ContractHistoryType ContractType { get; set; } = ContractHistoryType.Unknown;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ContractHistoryStatus ContractStatus { get; set; } = ContractHistoryStatus.Draft;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceContractHistoryStatusRequest
    {
        [Required]
        public ContractHistoryStatus ContractStatus { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
