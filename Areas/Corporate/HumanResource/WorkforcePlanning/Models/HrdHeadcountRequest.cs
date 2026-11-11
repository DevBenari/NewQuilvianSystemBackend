using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("HrdHeadcountRequest", Schema = "public")]
    public class HrdHeadcountRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid HospitalSiteId { get; set; }

        [Required]
        public Guid OrganizationUnitId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        [Required]
        public Guid SalaryGradeId { get; set; }

        public HeadcountRequestType RequestType { get; set; }

        public HeadcountRequestStatus RequestStatus { get; set; } = HeadcountRequestStatus.Draft;

        public DateOnly RequiredStartDate { get; set; }

        public DateOnly? TemporaryEndDate { get; set; }

        public Guid? TemporaryReasonWorkforceProfileId { get; set; }

        [MaxLength(2000)]
        public string? Justification { get; set; }

        public decimal RequestedQuantity { get; set; } = 0m;

        public int ExternalSeatCount { get; set; } = 0;

        public int InternalSeatCount { get; set; } = 0;

        public Guid? FormationVersionIdAtSubmission { get; set; }

        public decimal? RemainingSeatsAtSubmission { get; set; }

        public decimal? EstimatedMonthlyRate { get; set; }

        public decimal? EstimatedCost { get; set; }

        public CostCalculationStatus CostStatus { get; set; }

        public decimal? FinanceAdjustedCost { get; set; }

        [MaxLength(1000)]
        public string? FinanceAdjustmentReason { get; set; }

        public Guid? FinanceAdjustedByUserId { get; set; }

        public DateTime? FinanceAdjustedAt { get; set; }

        public bool IsOverFormationAfterAddendum { get; set; } = false;

        public bool IsOverdue { get; set; } = false;

        public Guid? CreatedFormationId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? WorkflowInstanceId { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? ReadyForRecruitmentAt { get; set; }

        public DateTime? WithdrawnAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstPosition? Position { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
        public MstWorkforceProfile? TemporaryReasonWorkforceProfile { get; set; }
        public HrdPositionFormation? FormationVersionAtSubmission { get; set; }
        public HrdPositionFormation? CreatedFormation { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public ICollection<HrdHeadcountRequestSeat> Seats { get; set; } = new List<HrdHeadcountRequestSeat>();
    }

    [Table("HrdHeadcountRequestSeat", Schema = "public")]
    public class HrdHeadcountRequestSeat : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid HeadcountRequestId { get; set; }

        public int SeatNumber { get; set; }

        public decimal SeatFte { get; set; } = 1.00m;

        public HeadcountFulfilmentSource FulfilmentSource { get; set; }

        public HeadcountSeatStatus SeatStatus { get; set; } = HeadcountSeatStatus.Open;

        public Guid? ReplacedWorkforceProfileId { get; set; }

        public Guid? LinkedPositionAssignmentId { get; set; }

        public Guid? LinkedPersonnelActionOrderId { get; set; }

        public Guid? LinkedByUserId { get; set; }

        public DateTime? LinkedAt { get; set; }

        public DateOnly? FulfilledAt { get; set; }

        public Guid? CancellationId { get; set; }

        public DateTime? CancelledAt { get; set; }

        public HrdHeadcountRequest? HeadcountRequest { get; set; }
        public MstWorkforceProfile? ReplacedWorkforceProfile { get; set; }
        public WfpPositionAssignment? LinkedPositionAssignment { get; set; }
        public HrdPersonnelActionOrder? LinkedPersonnelActionOrder { get; set; }
        public HrdHeadcountRequestCancellation? Cancellation { get; set; }
    }
}
