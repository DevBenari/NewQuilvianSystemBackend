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
