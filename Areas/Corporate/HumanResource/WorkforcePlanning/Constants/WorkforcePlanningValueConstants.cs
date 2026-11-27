namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants
{
    public static class WorkforcePlanningValueConstants
    {
        public static class WorkflowReferenceTypes
        {
            public const string WorkforceStaffingStandard = "WorkforceStaffingStandard";
            public const string WorkforceInitialFormationBatch = "WorkforceInitialFormationBatch";
            public const string WorkforceManpowerPlan = "WorkforceManpowerPlan";
            public const string WorkforceHeadcountRequest = "WorkforceHeadcountRequest";
            public const string WorkforceHeadcountCancellation = "WorkforceHeadcountCancellation";
        }

        /// <summary>
        /// Kode definisi workflow resmi S-D1 Perencanaan Tenaga Kerja pada MstWorkflowDefinition (BE-HRD-FIN-WFP-008).
        /// Batch Formasi Awal dan Rencana Tahunan sama-sama memakai definisi T10 existing.
        /// </summary>
        public static class WorkflowDefinitionCodes
        {
            public const string StaffingStandard = "T9";
            public const string ManpowerPlanAndInitialFormation = "T10";
            public const string HeadcountWithinFormation = "T11";
            public const string HeadcountBeyondFormation = "T12";
            public const string HeadcountCancellation = "T13";
        }

        public static class NumberSeriesKeys
        {
            public const string StaffingStandard = "HRD_STAFFING_STANDARD";
            public const string InitialFormationBatch = "HRD_INITIAL_FORMATION_BATCH";
            public const string ManpowerPlan = "HRD_MANPOWER_PLAN";
            public const string HeadcountRequest = "HRD_HEADCOUNT_REQUEST";
            public const string HeadcountCancellation = "HRD_HEADCOUNT_CANCELLATION";
        }

        public static class AdvisoryLockPatterns
        {
            public const string FormationLockPattern = "HRD_WFP_FORMATION_{0}_{1}";
        }
    }
}
