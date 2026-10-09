namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Cara tanda tangan dicatat — backend 13.9 (<c>BE-RWI-192</c>, <c>RWI-DEC-230</c>, <c>237</c>).
    /// </summary>
    public enum InpAdmissionSignatureMethod
    {
        /// <summary>Lembar kertas sudah ditandatangani; petugas mencatat nama, hubungan, dan waktunya.</summary>
        PaperRecorded = 1,

        /// <summary>Atestasi elektronik oleh akun yang benar-benar menandatangani.</summary>
        ElectronicAttestation = 2
    }
}
