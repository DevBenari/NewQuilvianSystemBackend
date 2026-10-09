namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>Jenis baris Permintaan Privasi — backend 13.9 (<c>BE-RWI-192</c>).</summary>
    public enum InpAdmissionPrivacyEntryType
    {
        /// <summary>Kerabat yang diperbolehkan menjenguk; satu nama satu baris.</summary>
        AllowedVisitor = 1,

        /// <summary>Permintaan khusus untuk pelayanan.</summary>
        SpecialServiceRequest = 2
    }
}
