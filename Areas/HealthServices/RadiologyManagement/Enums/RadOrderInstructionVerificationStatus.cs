namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums
{
    /// <summary>
    /// Status verifikasi instruksi dokter atas pesanan radiologi — <c>BE-RWI-104</c>, migration
    /// <c>R8</c>, kamus data 0.5 bagian 13.11, persetujuan pemilik modul <c>RWI-DEC-153</c>.
    /// </summary>
    /// <remarks>
    /// Enum milik modul Radiologi sendiri, bukan enum bersama lintas modul (arsitektur 0.5 bagian
    /// 11.10). Bawaan <c>NotRequired</c> membuat seluruh pesanan lama dan pesanan dari poliklinik
    /// atau IGD tidak berubah artinya.
    /// </remarks>
    public enum RadOrderInstructionVerificationStatus
    {
        /// <summary>Pesanan dibuat dokter, atau bukan pesanan rawat inap. Bawaan.</summary>
        NotRequired = 0,

        /// <summary>Pesanan rawat inap dibuat perawat atas instruksi dokter; menunggu verifikasi.</summary>
        Pending = 1,

        /// <summary>Dokter pemberi instruksi sudah memverifikasi.</summary>
        Verified = 2
    }
}
