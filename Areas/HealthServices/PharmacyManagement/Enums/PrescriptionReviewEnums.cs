namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    public enum PrescriptionReviewStatus
    {
        Pending = 0,
        InReview = 1,
        ClarificationRequired = 2,
        RevisedByDoctor = 3,
        Approved = 4,
        Rejected = 5,
        Cancelled = 6
    }

    public enum PrescriptionReviewCategory
    {
        Administrative = 1,
        Pharmaceutical = 2,
        Clinical = 3,
        CompoundFormula = 4
    }

    public enum PrescriptionReviewResult
    {
        NotReviewed = 0,
        Compliant = 1,
        NotCompliant = 2,
        NotApplicable = 3,
        WarningAccepted = 4
    }

    public enum PrescriptionClarificationStatus
    {
        Open = 0,
        AcknowledgedByDoctor = 1,
        RevisedByDoctor = 2,
        AcceptedByPharmacist = 3,
        Rejected = 4,
        Closed = 5,
        Cancelled = 6
    }

    public enum PrescriptionPreparationStatus
    {
        Pending = 0,
        InPreparation = 1,
        Prepared = 2,
        Cancelled = 3
    }

    public enum PrescriptionFinalCheckStatus
    {
        Pending = 0,
        InReview = 1,
        Passed = 2,
        Failed = 3,
        Cancelled = 4
    }

    public enum PrescriptionSubstitutionApprovalStatus
    {
        Proposed = 0,
        ApprovedByDoctor = 1,
        RejectedByDoctor = 2,
        Cancelled = 3
    }

    public enum PrescriptionIssueSeverity
    {
        Information = 0,
        Warning = 1,
        HardStop = 2
    }
}
