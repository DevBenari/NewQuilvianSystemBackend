using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class InitialContractBatchListItemResponse
    {
        public Guid Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string HospitalSiteName { get; set; } = string.Empty;
        public InitialContractBatchStatus BatchStatus { get; set; }
        public string BatchStatusName => BatchStatus.ToString();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public string? SubmittedByUserName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
    }

    public sealed class InitialContractBatchDetailResponse
    {
        public Guid Id { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string HospitalSiteName { get; set; } = string.Empty;
        public InitialContractBatchStatus BatchStatus { get; set; }
        public string BatchStatusName => BatchStatus.ToString();
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public string? SubmittedByUserName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? DecidedByUserId { get; set; }
        public string? DecidedByUserName { get; set; }
        public DateTime? DecidedAt { get; set; }
        public DateTime? AppliedAt { get; set; }
        public List<InitialContractBatchRowResponse> Rows { get; set; } = new();
        public Guid RowVersion { get; set; }
    }

    public sealed class InitialContractBatchRowResponse
    {
        public Guid Id { get; set; }
        public Guid BatchId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid ContractTypeId { get; set; }
        public string ContractTypeName { get; set; } = string.Empty;
        public string ContractNumber { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateOnly? FixedTermSeriesStartDate { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
        public Guid? CreatedContractHistoryId { get; set; }
    }

    public sealed class CreateInitialContractBatchRequest
    {
        [Required]
        public Guid HospitalSiteId { get; set; }
    }

    public sealed class AddInitialContractBatchRowRequest
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid ContractTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ContractNumber { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public DateOnly? FixedTermSeriesStartDate { get; set; }
    }

    public sealed class InitialContractBatchQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? HospitalSiteId { get; set; }
        public InitialContractBatchStatus? BatchStatus { get; set; }
        public string? Search { get; set; }
    }
}
