namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums
{
    /// <summary>Asal sebuah baris membership — <c>crm-membership/erd/data-dictionary.md</c>.</summary>
    public enum MembershipRegistrationType
    {
        New = 1,
        Renewal = 2,
        Upgrade = 3,

        /// <summary>Data sebelum aturan siklus hidup berlaku (CRM-DEC-021).</summary>
        LegacyImport = 4
    }
}
