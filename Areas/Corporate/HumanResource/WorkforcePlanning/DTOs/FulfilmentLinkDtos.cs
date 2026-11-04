using System;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    public sealed class CreateFulfilmentLinkRequest
    {
        public Guid? LinkedPositionAssignmentId { get; set; }
        public Guid? LinkedPersonnelActionOrderId { get; set; }
    }

    public sealed class FulfilmentLinkResponse
    {
        public Guid SeatId { get; set; }
        public Guid HeadcountRequestId { get; set; }
        public int SeatNumber { get; set; }
        public string SeatStatus { get; set; } = string.Empty;
        public Guid? LinkedPositionAssignmentId { get; set; }
        public Guid? LinkedPersonnelActionOrderId { get; set; }
        public DateTime? LinkedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
