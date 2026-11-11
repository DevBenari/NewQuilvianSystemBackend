using System;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    public sealed class CreateCancellationRequest
    {
        public Guid SeatId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public sealed class CancellationResponse
    {
        public Guid Id { get; set; }
        public string CancellationNumber { get; set; } = string.Empty;
        public Guid HeadcountRequestId { get; set; }
        public Guid SeatId { get; set; }
        public HeadcountCancellationStatus CancellationStatus { get; set; }
        public string CancellationStatusName => CancellationStatus.ToString();
        public string Reason { get; set; } = string.Empty;
        public Guid RequestedByUserId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
