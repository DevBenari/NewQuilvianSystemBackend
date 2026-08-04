namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants
{
    public static class LeaveValueConstants
    {
        public static class PeriodStatus
        {
            public const string Open = "Open";
            public const string Processing = "Processing";
            public const string Closed = "Closed";
            public const string Reopened = "Reopened";
            public const string Cancelled = "Cancelled";
        }

        public static class BalanceStatus
        {
            public const string Active = "Active";
            public const string Locked = "Locked";
            public const string Closed = "Closed";
            public const string Expired = "Expired";
            public const string Cancelled = "Cancelled";
        }

        public static class EntitlementStatus
        {
            public const string Draft = "Draft";
            public const string Generated = "Generated";
            public const string Posted = "Posted";
            public const string Adjusted = "Adjusted";
            public const string Expired = "Expired";
            public const string Cancelled = "Cancelled";
        }

        public static class AccrualStatus
        {
            public const string Draft = "Draft";
            public const string Calculated = "Calculated";
            public const string Posted = "Posted";
            public const string Reversed = "Reversed";
            public const string Skipped = "Skipped";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
        }

        public static class BatchRunStatus
        {
            public const string Draft = "Draft";
            public const string Queued = "Queued";
            public const string Running = "Running";
            public const string Completed = "Completed";
            public const string CompletedWithErrors = "CompletedWithErrors";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
            public const string Reversed = "Reversed";
        }

        public static class BatchRunMode
        {
            public const string Scheduled = "Scheduled";
            public const string Manual = "Manual";
            public const string Reprocess = "Reprocess";
            public const string Preview = "Preview";
        }

        public static class CarryForwardStatus
        {
            public const string Draft = "Draft";
            public const string Calculated = "Calculated";
            public const string Posted = "Posted";
            public const string Reversed = "Reversed";
            public const string Skipped = "Skipped";
            public const string Failed = "Failed";
            public const string Cancelled = "Cancelled";
        }

        public static class CarryForwardSkipReason
        {
            public const string NoAvailableBalance = "NoAvailableBalance";
            public const string BelowMinimumBalance = "BelowMinimumBalance";
            public const string PolicyDisabled = "PolicyDisabled";
            public const string MaximumPeriodReached = "MaximumPeriodReached";
            public const string MissingDestinationPeriod = "MissingDestinationPeriod";
            public const string MissingDestinationBalance = "MissingDestinationBalance";
            public const string BalanceLocked = "BalanceLocked";
            public const string AlreadyProcessed = "AlreadyProcessed";
            public const string NotEligible = "NotEligible";
        }

        public static class TransactionStatus
        {
            public const string Draft = "Draft";
            public const string Posted = "Posted";
            public const string Reversed = "Reversed";
            public const string Cancelled = "Cancelled";
        }

        public static class TransactionDirection
        {
            public const string Credit = "Credit";
            public const string Debit = "Debit";
        }

        public static class TransactionType
        {
            public const string Opening = "Opening";
            public const string Entitlement = "Entitlement";
            public const string Accrual = "Accrual";
            public const string CarryForward = "CarryForward";
            public const string Reservation = "Reservation";
            public const string ReservationRelease = "ReservationRelease";
            public const string Deduction = "Deduction";
            public const string CancellationRestore = "CancellationRestore";
            public const string RecallAdjustment = "RecallAdjustment";
            public const string Expiry = "Expiry";
            public const string ManualAdjustment = "ManualAdjustment";
            public const string CompensatoryCredit = "CompensatoryCredit";
            public const string Encashment = "Encashment";
            public const string Reversal = "Reversal";
        }

        public static class PostingBatchType
        {
            public const string Entitlement = "Entitlement";
            public const string AccrualRun = "AccrualRun";
            public const string CarryForwardRun = "CarryForwardRun";
            public const string Adjustment = "Adjustment";
            public const string LeaveRequest = "LeaveRequest";
            public const string CompensatoryLeave = "CompensatoryLeave";
            public const string Reconciliation = "Reconciliation";
        }

        public static class EntitlementMethod
        {
            public const string AnnualGrant = "AnnualGrant";
            public const string MonthlyAccrual = "MonthlyAccrual";
            public const string PerServicePeriod = "PerServicePeriod";
            public const string Manual = "Manual";
        }

        public static class PeriodBasis
        {
            public const string CalendarYear = "CalendarYear";
            public const string AnniversaryYear = "AnniversaryYear";
            public const string FiscalYear = "FiscalYear";
            public const string ContractPeriod = "ContractPeriod";
            public const string Custom = "Custom";
        }

        public static class GrantTiming
        {
            public const string StartOfPeriod = "StartOfPeriod";
            public const string AnniversaryDate = "AnniversaryDate";
            public const string EndOfProbation = "EndOfProbation";
            public const string Manual = "Manual";
        }

        public static class AccrualTiming
        {
            public const string StartOfPeriod = "StartOfPeriod";
            public const string EndOfPeriod = "EndOfPeriod";
            public const string SpecificDay = "SpecificDay";
        }

        public static class FirstAccrualRule
        {
            public const string Full = "Full";
            public const string Prorated = "Prorated";
            public const string NextFullPeriod = "NextFullPeriod";
            public const string None = "None";
        }

        public static class FinalAccrualRule
        {
            public const string Full = "Full";
            public const string Prorated = "Prorated";
            public const string PreviousFullPeriod = "PreviousFullPeriod";
            public const string None = "None";
        }

        public static class DayCalculationMethod
        {
            public const string WorkingDays = "WorkingDays";
            public const string ScheduledWorkDays = "ScheduledWorkDays";
            public const string CalendarDays = "CalendarDays";
            public const string Hours = "Hours";
        }

        public static class ReservationTiming
        {
            public const string OnSubmit = "OnSubmit";
            public const string OnApproval = "OnApproval";
            public const string None = "None";
        }

        public static class DeductionTiming
        {
            public const string OnApproval = "OnApproval";
            public const string OnLeaveStart = "OnLeaveStart";
            public const string OnCompletion = "OnCompletion";
        }

        public static class CarryForwardExecutionTiming
        {
            public const string PeriodClose = "PeriodClose";
            public const string NextPeriodOpen = "NextPeriodOpen";
            public const string ScheduledDate = "ScheduledDate";
            public const string Manual = "Manual";
        }

        public static class RoundingMethod
        {
            public const string None = "None";
            public const string Up = "Up";
            public const string Down = "Down";
            public const string NearestHalfDay = "NearestHalfDay";
            public const string NearestDay = "NearestDay";
        }

        public static class ExpiryMethod
        {
            public const string MonthsAfterCarryForward = "MonthsAfterCarryForward";
            public const string FixedDate = "FixedDate";
            public const string EndOfDestinationPeriod = "EndOfDestinationPeriod";
            public const string Never = "Never";
        }

        public static class ExcessBalanceAction
        {
            public const string Forfeit = "Forfeit";
            public const string Payout = "Payout";
            public const string KeepInSource = "KeepInSource";
        }


        public static class AdjustmentType
        {
            public const string OpeningBalance = "OpeningBalance";
            public const string ManualAdjustment = "ManualAdjustment";
            public const string Correction = "Correction";
            public const string Reversal = "Reversal";
        }

        public static class AdjustmentStatus
        {
            public const string Draft = "Draft";
            public const string Submitted = "Submitted";
            public const string UnderReview = "UnderReview";
            public const string NeedRevision = "NeedRevision";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Posted = "Posted";
            public const string Reversed = "Reversed";
            public const string Cancelled = "Cancelled";
        }

        public static class AdjustmentReasonCategory
        {
            public const string OpeningBalance = "OpeningBalance";
            public const string Migration = "Migration";
            public const string ManualAdjustment = "ManualAdjustment";
            public const string DataCorrection = "DataCorrection";
            public const string PolicyCompensation = "PolicyCompensation";
            public const string AuditFinding = "AuditFinding";
            public const string Reversal = "Reversal";
            public const string Other = "Other";
        }

        public static class AdjustmentAllowedDirection
        {
            public const string Credit = "Credit";
            public const string Debit = "Debit";
            public const string Both = "Both";
        }

        public static class AdjustmentSourceType
        {
            public const string HrManual = "HrManual";
            public const string Migration = "Migration";
            public const string System = "System";
            public const string Api = "Api";
            public const string Reconciliation = "Reconciliation";
        }

        public static class WorkflowReferenceType
        {
            public const string LeaveAdjustment = "LEAVE_ADJUSTMENT";
            public const string LeaveRequest = "LEAVE_REQUEST";
        }
    }
}
