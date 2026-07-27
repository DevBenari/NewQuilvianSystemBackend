using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpContractHistorySummaryResponse
    {
        public int TotalContractHistory { get; set; }
        public int ActiveContractHistory { get; set; }
        public int InactiveContractHistory { get; set; }
        public int CurrentContract { get; set; }
        public int DraftContract { get; set; }
        public int ActiveContract { get; set; }
        public int EndedContract { get; set; }
        public int ExpiredContract { get; set; }
        public int TerminatedContract { get; set; }
    }

    public class WfpContractHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? PreviousContractHistoryId { get; set; }
        public string? PreviousContractNumber { get; set; }
        public Guid? ContractTypeId { get; set; }
        public string? ContractTypeCode { get; set; }
        public string? ContractTypeName { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public string? EmploymentTypeCode { get; set; }
        public string? EmploymentTypeName { get; set; }
        public Guid? WorkerSourceId { get; set; }
        public string? WorkerSourceCode { get; set; }
        public string? WorkerSourceName { get; set; }
        public Guid? TerminationReasonId { get; set; }
        public string? TerminationReasonCode { get; set; }
        public string? TerminationReasonName { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string HistoryType { get; set; } = string.Empty;
        public string ContractStatus { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? TerminatedAt { get; set; }
        public int RenewalSequence { get; set; }
        public bool IsCurrent { get; set; }
        public string? DocumentPath { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpContractHistoryDetailResponse : WfpContractHistoryResponse
    {
        public int RenewalCount { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpContractHistoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpContractHistoryDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpContractHistoryStringOptionResponse> HistoryTypeOptions { get; set; } = new();
        public List<WfpContractHistoryStringOptionResponse> ContractStatusOptions { get; set; } = new();
        public List<WfpMasterOptionResponse> ContractTypeOptions { get; set; } = new();
        public List<WfpMasterOptionResponse> EmploymentTypeOptions { get; set; } = new();
        public List<WfpMasterOptionResponse> WorkerSourceOptions { get; set; } = new();
        public List<WfpMasterOptionResponse> TerminationReasonOptions { get; set; } = new();
        public List<WfpPreviousContractOptionResponse> PreviousContractOptions { get; set; } = new();
        public List<WfpContractHistorySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpContractHistoryDefaultFilterResponse
    {
        public string? HistoryType { get; set; }
        public string? ContractStatus { get; set; }
        public Guid? ContractTypeId { get; set; }
        public bool? IsCurrent { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "startDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpContractHistoryStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpContractHistorySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpMasterOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpPreviousContractOptionResponse
    {
        public Guid Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ContractStatus { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpContractHistoryRequest
    {
        public Guid? PreviousContractHistoryId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? WorkerSourceId { get; set; }
        public Guid? TerminationReasonId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string HistoryType { get; set; } = "Initial";

        [Required]
        [MaxLength(50)]
        public string ContractStatus { get; set; } = "Draft";

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? TerminatedAt { get; set; }

        [Range(0, int.MaxValue)]
        public int RenewalSequence { get; set; }

        public bool IsCurrent { get; set; }

        [MaxLength(500)]
        public string? DocumentPath { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpContractHistoryRequest : CreateWfpContractHistoryRequest
    {
    }

    public class UpdateWfpContractHistoryStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string ContractStatus { get; set; } = "Draft";

        public bool IsActive { get; set; } = true;
        public DateTime? EndDate { get; set; }
        public DateTime? TerminatedAt { get; set; }
        public Guid? TerminationReasonId { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class SetWfpContractHistoryCurrentRequest
    {
        public bool IsCurrent { get; set; } = true;
    }
}
