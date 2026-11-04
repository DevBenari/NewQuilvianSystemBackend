namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants
{
    public static class EmploymentLifecycleValueConstants
    {
        // 5 System Item Codes pada template & checklist onboarding
        public const string ItemFirstDayAttendance = "FIRST_DAY_ATTENDANCE";
        public const string ItemContractSigned = "CONTRACT_SIGNED";
        public const string ItemFitnessClearance = "FITNESS_CLEARANCE";
        public const string ItemDirectSupervisorAssigned = "DIRECT_SUPERVISOR_ASSIGNED";
        public const string ItemPreceptorAssigned = "PRECEPTOR_ASSIGNED";

        // 4 Jenis Rujukan Workflow Engine T15 - T18
        public const string WorkflowRefProbationAssessment = "WorkforceProbationAssessment";   // T15
        public const string WorkflowRefContractDecision = "WorkforceContractDecision";           // T16
        public const string WorkflowRefContractCorrection = "WorkforceContractCorrection";       // T17
        public const string WorkflowRefInitialContractBatch = "WorkforceInitialContractBatch";   // T18

        // 6 Kunci Number Series
        public const string NumberSeriesOnboardingCase = "OBC";
        public const string NumberSeriesEmploymentContract = "CTR";
        public const string NumberSeriesProbationAssessment = "PBA";
        public const string NumberSeriesContractDecision = "CDA";
        public const string NumberSeriesContractCorrection = "CCA";
        public const string NumberSeriesInitialContractBatch = "ICB";

        // Nilai Status Kontrak pada WfpContractHistory
        public const string ContractStatusDraft = "Draft";
        public const string ContractStatusActive = "Active";
        public const string ContractStatusCompleted = "Completed";
        public const string ContractStatusEnded = "Ended";
        public const string ContractStatusLapsedWithoutDecision = "LapsedWithoutDecision";
        public const string ContractStatusCancelled = "Cancelled";

        // Nilai HistoryType Baru pada WfpContractHistory
        public const string HistoryTypeInitialData = "InitialData";
        public const string HistoryTypeConversion = "Conversion";

        // Nilai SourceDocumentType pada WfpContractHistory
        public const string SourceDocOffer = "HrdJobOffer";
        public const string SourceDocDecision = "HrdContractDecision";
        public const string SourceDocCorrection = "HrdContractCorrection";
        public const string SourceDocInitialBatch = "HrdInitialContractBatch";

        // Nilai SeparationType Baru untuk Handoff ke Sub-modul 20
        public const string SeparationTypeContractNonRenewal = "ContractNonRenewal";
        public const string SeparationTypeProbationFailed = "ProbationFailed";
    }
}
