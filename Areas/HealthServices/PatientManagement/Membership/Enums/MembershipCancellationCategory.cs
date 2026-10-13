namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums
{
    /// <summary>Sebab pembatalan membership (CRM-DEC-013, CRM-DEC-014, CRM-DEC-025).</summary>
    public enum MembershipCancellationCategory
    {
        PatientRequest = 1,
        InputError = 2,
        Unpaid = 3,
        SupersededByUpgrade = 4
    }
}
