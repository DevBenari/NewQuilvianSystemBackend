using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models
{
    /// <summary>
    /// Riwayat perpindahan status seluruh alur Bank Darah, hanya dapat ditambah.
    /// <c>BD-DOM-15</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Append-only, dan itu bukan gaya penulisan melainkan syarat audit.</b> Baris di sini
    /// tidak pernah disunting dan tidak pernah dihapus; koreksi dilakukan dengan menambah baris
    /// baru. Aturan umum matriks perpindahan status menuntut <b>setiap</b> perpindahan
    /// meninggalkan satu baris berisi pelaku, waktu, status asal, status tujuan, dan kode
    /// alasan beserta salinan teksnya bila ada.
    /// </para>
    ///
    /// <para>
    /// <b><see cref="ReasonNote"/> adalah salinan, bukan penunjuk.</b> Teks alasan pada
    /// <c>MstBloodBankReason</c> boleh disunting kelak, sedangkan rekam pembatalan wajib tetap
    /// berbunyi persis seperti saat keputusannya diambil (<c>INV-BD-035</c>). Menyimpan
    /// penunjuk saja akan membuat riwayat lama berubah makna diam-diam ketika master disunting.
    /// </para>
    ///
    /// <para>
    /// <b>Satu tabel untuk empat scope</b> — <c>BloodOrder</c>, <c>ProviderRequest</c>,
    /// <c>BloodUnit</c>, dan <c>BloodGroupExam</c> — sesuai kamus data kontrak <c>v4</c>.
    /// Pada slice <c>BE-BD-003</c> baru scope <c>BloodOrder</c> yang menulis ke sini; ketiga
    /// scope lain menyusul bersama task pemiliknya masing-masing.
    /// </para>
    /// </remarks>
    [Table("BbkTransitionHistory", Schema = "public")]
    public class BbkTransitionHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Alur yang berpindah status. Lihat <see cref="BbkTransitionScopes"/>.</summary>
        [Required]
        [MaxLength(30)]
        public string Scope { get; set; } = string.Empty;

        /// <summary>Id entity yang berpindah status.</summary>
        [Required]
        public Guid EntityId { get; set; }

        /// <summary>Nama tindakan yang menyebabkan perpindahan, misalnya <c>Cancel</c>.</summary>
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>Status asal. Kosong ketika barisnya lahir bersama entity-nya.</summary>
        [MaxLength(30)]
        public string? FromStatus { get; set; }

        [Required]
        [MaxLength(30)]
        public string ToStatus { get; set; } = string.Empty;

        /// <summary>Kode alasan terkendali dari <c>MstBloodBankReason.ReasonCode</c>.</summary>
        [MaxLength(30)]
        public string? ReasonCode { get; set; }

        /// <summary>Salinan teks alasan pada saat kejadian.</summary>
        [MaxLength(500)]
        public string? ReasonNote { get; set; }

        [Required]
        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>Korelasi antar-proses, bila satu tindakan memicu beberapa perpindahan.</summary>
        public Guid? CorrelationId { get; set; }
    }

    /// <summary>
    /// Keempat scope sah pada <see cref="BbkTransitionHistory.Scope"/>, sesuai kamus data.
    /// </summary>
    /// <remarks>
    /// Ditulis sebagai konstanta, bukan enum yang dipersistensi, karena kolomnya memang
    /// <c>string(30)</c> — mengikuti pola yang sudah dipakai
    /// <c>BloodBankReasonCategories</c> pada <c>BE-BD-001</c>.
    /// </remarks>
    public static class BbkTransitionScopes
    {
        public const string BloodOrder = "BloodOrder";
        public const string ProviderRequest = "ProviderRequest";
        public const string BloodUnit = "BloodUnit";
        public const string BloodGroupExam = "BloodGroupExam";

        /// <summary>
        /// Scope kelima, tambahan <c>BE-BD-012</c> (11 September 2026). <c>AC-BD-101</c> menuntut
        /// transisi dan audit penyelesaian tindakan tersimpan; kolomnya <c>string(30)</c>, sehingga
        /// tidak ada perubahan schema. Dicatat sebagai delta pada kamus data.
        /// </summary>
        public const string BloodBankProcedure = "BloodBankProcedure";
    }
}
