namespace QuilvianSystemBackend.Enum
{
    public enum ComplianceAlertType
    {
        Unknown = 0,

        DocumentExpired = 1,
        DocumentWillExpire = 2,

        LicenseExpired = 3,
        LicenseWillExpire = 4,

        HealthRecordExpired = 5,
        HealthRecordWillExpire = 6,

        ContractExpired = 7,
        ContractWillEnd = 8,

        ClinicalPrivilegeExpired = 9,
        ClinicalPrivilegeWillExpire = 10,
        ClinicalPrivilegeReview = 11,

        CertificationExpired = 12,
        CertificationWillExpire = 13,

        ExternalAccessExpired = 14,
        ExternalAccessWillEnd = 15,

        Other = 99
    }
}
