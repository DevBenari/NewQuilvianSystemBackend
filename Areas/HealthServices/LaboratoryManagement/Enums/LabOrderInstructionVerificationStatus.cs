namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums
{
    /// <summary>
    /// Status verifikasi instruksi dokter atas pesanan laboratorium — <c>BE-RWI-104</c>, migration
    /// <c>R8</c>, kamus data 0.5 bagian 13.11, persetujuan pemilik modul <c>RWI-DEC-153</c>.
    /// </summary>
    /// <remarks>
    /// Enum milik modul Laboratorium sendiri, bukan enum bersama lintas modul: modul pemilik
    /// statusnya berbeda (arsitektur 0.5 bagian 11.10). Bawaan <c>NotRequired</c> membuat seluruh
    /// pesanan lama dan pesanan dari poliklinik atau IGD tidak berubah artinya.
    /// </remarks>
    public enum LabOrderInstructionVerificationStatus
    {
        /// <summary>Pesanan dibuat dokter, atau bukan pesanan rawat inap. Bawaan.</summary>
        NotRequired = 0,

        /// <summary>Pesanan rawat inap dibuat perawat atas instruksi dokter; menunggu verifikasi.</summary>
        Pending = 1,

        /// <summary>Dokter pemberi instruksi sudah memverifikasi.</summary>
        Verified = 2
    }
}
