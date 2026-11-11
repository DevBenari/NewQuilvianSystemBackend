using System;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    public sealed class RecruitmentQueuePagedQuery
    {
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? PositionId { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Item antrean rekrutmen untuk Recruiter tanpa kolom biaya (BE-HRD-042).
    /// </summary>
    public sealed class RecruitmentQueueItemResponse
    {
        public Guid SeatId { get; set; }
        public Guid HeadcountRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public decimal SeatFte { get; set; }
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionName { get; set; }
        public DateOnly RequiredStartDate { get; set; }
        public HeadcountRequestType RequestType { get; set; }
        public string RequestTypeName => RequestType.ToString();
        public DateTime ReadyForRecruitmentAt { get; set; }
    }

    public sealed class ChangeFulfilmentPlanRequest
    {
        public Guid SeatId { get; set; }
        public HeadcountFulfilmentSource NewSource { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class FulfilmentPlanChangeResponse
    {
        public Guid Id { get; set; }
        public Guid SeatId { get; set; }
        public HeadcountFulfilmentSource FromSource { get; set; }
        public HeadcountFulfilmentSource ToSource { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? ChangedByUserName { get; set; }
    }
}
