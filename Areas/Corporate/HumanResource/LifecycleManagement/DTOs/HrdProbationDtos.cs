using System;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs
{
    public sealed class HrdProbationResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Guid ContractHistoryId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly ReminderFromDate { get; set; }
        public ProbationStatus ProbationStatus { get; set; }
        public string ProbationStatusName => ProbationStatus.ToString();
        public DateTime? ClosedAt { get; set; }
        public Guid RowVersion { get; set; }
    }

    public sealed class HrdProbationQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ProbationStatus? Status { get; set; }
        public DateOnly? EndDateFrom { get; set; }
        public DateOnly? EndDateTo { get; set; }
        public string? Search { get; set; }
    }
}
