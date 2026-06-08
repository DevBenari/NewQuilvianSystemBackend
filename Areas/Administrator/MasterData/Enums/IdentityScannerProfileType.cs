namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Enums
{
    public enum IdentityScannerProfileType
    {
        Unknown = 0,
        EktpOcr = 1,
        PatientCardBarcode = 2,
        PatientCardQr = 3,
        MembershipCardQr = 4,
        InsuranceCardOcr = 5,
        ManualInput = 6,
        Other = 99
    }
}
