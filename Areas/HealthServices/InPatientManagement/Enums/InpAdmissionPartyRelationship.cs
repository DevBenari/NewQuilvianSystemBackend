namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>Hubungan penanda tangan dengan pasien — backend 13.9 (<c>BE-RWI-192</c>).</summary>
    public enum InpAdmissionPartyRelationship
    {
        Self = 1,
        Spouse = 2,
        Child = 3,
        Parent = 4,
        Sibling = 5,
        Guardian = 6,

        /// <summary>Lainnya; teks hubungannya dicatat terpisah.</summary>
        Other = 7
    }
}
