using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxBusinessTravelRequest", Schema = "public")]
    public class TrxBusinessTravelRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string TravelRequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? CostCenterId { get; set; }

        [Required]
        public Guid TravelTypeId { get; set; }

        public Guid? TravelPolicyId { get; set; }
        public Guid? DestinationZoneId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        // Optional integration references. Kept as scalar IDs to avoid coupling HR to Finance domain classes.
        public Guid? PayrollPeriodId { get; set; }
        public Guid? FinancePaymentId { get; set; }
        public Guid? GlHeaderId { get; set; }

        [Required, MaxLength(250)]
        public string TravelTitle { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string TravelPurpose { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ActivityDescription { get; set; }

        [Required, MaxLength(250)]
        public string Origin { get; set; } = string.Empty;

        [Required, MaxLength(250)]
        public string Destination { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateTime? PlannedDepartureAt { get; set; }
        public DateTime? PlannedReturnAt { get; set; }

        public bool IsDomestic { get; set; } = true;
        public bool IsInternational { get; set; } = false;
        public bool RequiresVisa { get; set; } = false;
        public bool RequiresPassport { get; set; } = false;
        public bool IsUrgent { get; set; } = false;

        public int ParticipantCount { get; set; } = 1;
        public int TravelDayCount { get; set; } = 1;

        public decimal EstimatedTransportationAmount { get; set; } = 0m;
        public decimal EstimatedAccommodationAmount { get; set; } = 0m;
        public decimal EstimatedAllowanceAmount { get; set; } = 0m;
        public decimal EstimatedOtherAmount { get; set; } = 0m;
        public decimal EstimatedTotalAmount { get; set; } = 0m;
        public decimal ApprovedBudgetAmount { get; set; } = 0m;

        [Required, MaxLength(10)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(100)]
        public string? BudgetSourceCode { get; set; }

        [MaxLength(250)]
        public string? BudgetSourceName { get; set; }

        public bool HasScheduleConflict { get; set; } = false;
        public bool HasLeaveConflict { get; set; } = false;
        public bool HasTrainingConflict { get; set; } = false;
        public bool HasOvertimeConflict { get; set; } = false;
        public bool IsBudgetAvailable { get; set; } = false;
        public bool IsPolicyCompliant { get; set; } = false;

        [Column(TypeName = "jsonb")]
        public string? ValidationResultJson { get; set; }

        [Required, MaxLength(40)]
        public string TravelStatus { get; set; } = "Draft";
        // Draft, Submitted, ManagerApproved, BudgetApproved, HrVerified,
        // FinanceVerified, Approved, AdvancePaid, InTravel, WaitingSettlement,
        // SettlementSubmitted, SettlementVerified, Completed, Rejected, Cancelled.

        public int CurrentApprovalStep { get; set; } = 0;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ManagerApprovedAt { get; set; }
        public DateTime? BudgetApprovedAt { get; set; }
        public DateTime? HrVerifiedAt { get; set; }
        public DateTime? FinanceVerifiedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? TravelStartedAt { get; set; }
        public DateTime? TravelEndedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public Guid? ManagerApprovedByUserId { get; set; }
        public Guid? BudgetApprovedByUserId { get; set; }
        public Guid? HrVerifiedByUserId { get; set; }
        public Guid? FinanceVerifiedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public Guid? CancelledByUserId { get; set; }

        [MaxLength(2000)]
        public string? ApprovalNotes { get; set; }

        [MaxLength(2000)]
        public string? RejectionNotes { get; set; }

        [MaxLength(2000)]
        public string? CancellationReason { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstTravelType? TravelType { get; set; }
        public MstTravelPolicy? TravelPolicy { get; set; }
        public MstTravelDestinationZone? DestinationZone { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ManagerApprovedByUser { get; set; }
        public ApplicationUser? BudgetApprovedByUser { get; set; }
        public ApplicationUser? HrVerifiedByUser { get; set; }
        public ApplicationUser? FinanceVerifiedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<TrxBusinessTravelParticipant> Participants { get; set; } = new List<TrxBusinessTravelParticipant>();
        public ICollection<TrxBusinessTravelApproval> Approvals { get; set; } = new List<TrxBusinessTravelApproval>();
        public ICollection<TrxTravelItinerary> Itineraries { get; set; } = new List<TrxTravelItinerary>();
        public ICollection<TrxTravelTransportation> Transportations { get; set; } = new List<TrxTravelTransportation>();
        public ICollection<TrxTravelAccommodation> Accommodations { get; set; } = new List<TrxTravelAccommodation>();
        public ICollection<TrxTravelAdvanceRequest> AdvanceRequests { get; set; } = new List<TrxTravelAdvanceRequest>();
        public ICollection<TrxTravelExpenseClaim> ExpenseClaims { get; set; } = new List<TrxTravelExpenseClaim>();
        public ICollection<TrxTravelSettlement> Settlements { get; set; } = new List<TrxTravelSettlement>();
        public ICollection<TrxTravelDocument> Documents { get; set; } = new List<TrxTravelDocument>();
        public ICollection<TrxTravelAttendanceLink> AttendanceLinks { get; set; } = new List<TrxTravelAttendanceLink>();
    }
}
