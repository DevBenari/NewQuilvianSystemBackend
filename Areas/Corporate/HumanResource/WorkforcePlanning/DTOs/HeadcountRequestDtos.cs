using System;
using System.Collections.Generic;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    public sealed class HeadcountSeatRequest
    {
        public int SeatNumber { get; set; }
        public decimal SeatFte { get; set; } = 1.00m;
        public HeadcountFulfilmentSource FulfilmentSource { get; set; }
        public Guid? ReplacedWorkforceProfileId { get; set; }
    }

    public sealed class CreateHeadcountRequest
    {
        public Guid HospitalSiteId { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public Guid PositionId { get; set; }
        public Guid SalaryGradeId { get; set; }
        public HeadcountRequestType RequestType { get; set; }
        public DateOnly RequiredStartDate { get; set; }
        public DateOnly? TemporaryEndDate { get; set; }
        public Guid? TemporaryReasonWorkforceProfileId { get; set; }
        public string? Justification { get; set; }
        public int ExternalSeatCount { get; set; }
        public int InternalSeatCount { get; set; }
        public List<HeadcountSeatRequest> Seats { get; set; } = new();
    }

    public sealed class UpdateHeadcountRequest
    {
        public Guid SalaryGradeId { get; set; }
        public HeadcountRequestType RequestType { get; set; }
        public DateOnly RequiredStartDate { get; set; }
        public DateOnly? TemporaryEndDate { get; set; }
        public Guid? TemporaryReasonWorkforceProfileId { get; set; }
        public string? Justification { get; set; }
        public int ExternalSeatCount { get; set; }
        public int InternalSeatCount { get; set; }
        public List<HeadcountSeatRequest> Seats { get; set; } = new();
    }

    public sealed class SubmitHeadcountRequestDto
    {
        public string? Note { get; set; }
    }

    public sealed class WithdrawHeadcountRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class FinanceAdjustmentRequest
    {
        public decimal AdjustedCost { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class RemainingSeatsResponse
    {
        public Guid HospitalSiteId { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public Guid PositionId { get; set; }
        public decimal FormationQuantity { get; set; }
        public decimal FilledFte { get; set; }
        public decimal WillBeVacantFte { get; set; }
        public decimal ClaimedSeatsFte { get; set; }
        public decimal RemainingSeats { get; set; }
        public bool HasActiveFormation { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public sealed class HeadcountSeatResponse
    {
        public Guid Id { get; set; }
        public int SeatNumber { get; set; }
        public decimal SeatFte { get; set; }
        public HeadcountFulfilmentSource FulfilmentSource { get; set; }
        public string FulfilmentSourceName => FulfilmentSource.ToString();
        public HeadcountSeatStatus SeatStatus { get; set; }
        public string SeatStatusName => SeatStatus.ToString();
        public Guid? ReplacedWorkforceProfileId { get; set; }
        public string? ReplacedEmployeeName { get; set; }
        public Guid? LinkedPositionAssignmentId { get; set; }
        public Guid? LinkedPersonnelActionOrderId { get; set; }
        public DateOnly? FulfilledAt { get; set; }
    }

    public sealed class HeadcountRequestListItemResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionName { get; set; }
        public HeadcountRequestType RequestType { get; set; }
        public string RequestTypeName => RequestType.ToString();
        public HeadcountRequestStatus RequestStatus { get; set; }
        public string RequestStatusName => RequestStatus.ToString();
        public DateOnly RequiredStartDate { get; set; }
        public decimal RequestedQuantity { get; set; }
        public int TotalSeats { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? FinanceAdjustedCost { get; set; }
        public bool CostVisible { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public Guid RowVersion { get; set; }
    }

    public sealed class HeadcountRequestResponse
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid SalaryGradeId { get; set; }
        public string? SalaryGradeName { get; set; }
        public HeadcountRequestType RequestType { get; set; }
        public string RequestTypeName => RequestType.ToString();
        public HeadcountRequestStatus RequestStatus { get; set; }
        public string RequestStatusName => RequestStatus.ToString();
        public DateOnly RequiredStartDate { get; set; }
        public DateOnly? TemporaryEndDate { get; set; }
        public Guid? TemporaryReasonWorkforceProfileId { get; set; }
        public string? TemporaryReasonEmployeeName { get; set; }
        public string? Justification { get; set; }
        public decimal RequestedQuantity { get; set; }
        public int ExternalSeatCount { get; set; }
        public int InternalSeatCount { get; set; }
        public decimal? RemainingSeatsAtSubmission { get; set; }
        public decimal? EstimatedMonthlyRate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public CostCalculationStatus CostStatus { get; set; }
        public string CostStatusName => CostStatus.ToString();
        public decimal? FinanceAdjustedCost { get; set; }
        public string? FinanceAdjustmentReason { get; set; }
        public DateTime? FinanceAdjustedAt { get; set; }
        public bool CostVisible { get; set; } = true;
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReadyForRecruitmentAt { get; set; }
        public DateTime? WithdrawnAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public List<HeadcountSeatResponse> Seats { get; set; } = new();
        public Guid RowVersion { get; set; }
    }

    public sealed class HeadcountRequestPagedQuery
    {
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? PositionId { get; set; }
        public HeadcountRequestStatus? Status { get; set; }
        public HeadcountRequestType? RequestType { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public sealed class HeadcountRequestFilterMetadataResponse
    {
        public List<string> Statuses { get; set; } = new();
        public List<string> RequestTypes { get; set; } = new();
        public List<string> FulfilmentSources { get; set; } = new();
    }
}
