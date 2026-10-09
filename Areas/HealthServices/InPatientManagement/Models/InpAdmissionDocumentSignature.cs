using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Satu slot tanda tangan dokumen admisi — kamus data 20.3 (<c>BE-RWI-192</c>). Tidak pernah
    /// diubah sesudah dicatat. Tanpa kolom gambar atau hash: itu milik <c>EPIC-RWA-13</c>.
    /// </summary>
    /// <remarks>
    /// Contoh: Selisih Biaya Tn. Budi memuat (1) slot pasien/keluarga, kertas, "Rina Santoso",
    /// <c>Spouse</c>, ditandatangani 10.15, diverifikasi Sari; (2) slot Petugas PPRI, atestasi,
    /// "Sari Wulandari", akun Sari. Keduanya sah: verifikasi kertas bukan slot petugas
    /// (<c>RWI-DEC-239</c>).
    /// </remarks>
    public class InpAdmissionDocumentSignature : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        public InpAdmissionSignatureSlot Slot { get; set; }

        public InpAdmissionSignatureMethod Method { get; set; }

        /// <summary>
        /// Kertas: nama yang menandatangani lembar. Atestasi: <c>DisplayName</c> akun saat
        /// menandatangani, dibekukan. SENSITIF.
        /// </summary>
        public string SignerName { get; set; } = string.Empty;

        /// <summary>Atestasi: jabatan utama akun saat menandatangani.</summary>
        public string? SignerPositionName { get; set; }

        /// <summary>Kertas: hubungan penanda tangan dengan pasien.</summary>
        public InpAdmissionPartyRelationship? SignerRelationship { get; set; }

        /// <summary>Kertas: teks hubungan bila <c>Other</c>, misalnya "adik ipar".</summary>
        public string? SignerRelationshipText { get; set; }

        /// <summary>
        /// Kertas: waktu tanda tangan di lembar, diisi petugas. Atestasi: waktu server.
        /// </summary>
        public DateTime SignedAt { get; set; }

        /// <summary>Atestasi: akun penanda tangan (<c>INV-RWA-04</c>).</summary>
        public Guid? SignedByUserId { get; set; }

        /// <summary>Kertas: petugas yang memeriksa lembar dan mencatatnya.</summary>
        public Guid? VerifiedByUserId { get; set; }

        /// <summary>Waktu server saat baris dicatat.</summary>
        public DateTime RecordedAt { get; set; }

        public string? IdempotencyKey { get; set; }

        public InpAdmissionDocument? Document { get; set; }
    }
}
