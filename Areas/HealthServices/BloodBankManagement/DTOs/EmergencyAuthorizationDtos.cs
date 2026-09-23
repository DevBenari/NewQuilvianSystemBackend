using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>
    /// Permintaan pemberian kantong lewat jalur darurat (DEC-BD-017, DEC-BD-038, DEC-BD-040).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pasien tidak diterima dari client.</b> Pasien tujuan selalu diturunkan dari alokasi
    /// aktif kantong, sama seperti pada jalur normal, sehingga tidak ada jalan mencatat pemberian
    /// atas nama pasien lain hanya dengan mengirim identitas yang berbeda.
    /// </para>
    /// <para>
    /// <b>Pelaku dan waktu juga tidak datang dari body.</b> Penerbit diambil dari akun yang login
    /// dan waktu dari jam server (INV-BD-032).
    /// </para>
    /// </remarks>
    public sealed class EmergencyIssueRequest
    {
        /// <summary>
        /// Kode alasan terkendali berkategori <c>Emergency</c>. Wajib; tidak boleh diketik bebas
        /// (VAL-BD-021, INV-BD-016).
        /// </summary>
        [MaxLength(30)]
        public string? ReasonCode { get; set; }

        /// <summary>
        /// Gerbang yang dilewati: bukti kecocokan, lokasi penyimpanan nonaktif, atau keduanya.
        /// Wajib dan wajib sesuai keadaan kantong (VAL-BD-066, INV-BD-030).
        /// </summary>
        public BbkEmergencyBypassScope? BypassScope { get; set; }

        /// <summary>
        /// Peran yang dipakai penerbit: Dokter Bank Darah atau dokter penanggung jawab pasien.
        /// Wajib dinyatakan (VAL-BD-071, DEC-BD-040).
        /// </summary>
        public BbkEmergencyAuthorizerRole? AuthorizerRole { get; set; }

        /// <summary>
        /// Keterangan keadaan yang membuat pemberian harus dilakukan sekarang. Wajib
        /// (VAL-BD-070). Berbeda dari <see cref="ReasonCode"/>: ini uraian keadaan, bukan kategori.
        /// </summary>
        [MaxLength(500)]
        public string? EmergencyConditionNote { get; set; }

        /// <summary>
        /// Token optimistis kantong. Bila diberikan dan berbeda dengan nilai terkini, tindakan
        /// ditolak sebagai bentrok konkurensi.
        /// </summary>
        public int? Version { get; set; }
    }

    /// <summary>
    /// Satu otorisasi darurat yang melekat pada kantong. Melekat permanen dan tidak dapat
    /// dibatalkan; pemberian bersifat terminal.
    /// </summary>
    public sealed class EmergencyAuthorizationDto
    {
        public Guid Id { get; set; }
        public Guid BloodUnitId { get; set; }
        public Guid PatientId { get; set; }

        public Guid AuthorizedByUserId { get; set; }
        public DateTime AuthorizedAt { get; set; }

        public string ReasonCode { get; set; } = string.Empty;
        public string? ReasonNote { get; set; }

        public BbkEmergencyBypassScope BypassScope { get; set; }
        public string BypassScopeLabel { get; set; } = string.Empty;

        public BbkEmergencyAuthorizerRole AuthorizerRole { get; set; }
        public string AuthorizerRoleLabel { get; set; } = string.Empty;

        public string EmergencyConditionNote { get; set; } = string.Empty;

        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>
    /// Keadaan kantong terhadap kedua gerbang yang dapat dilewati otorisasi darurat.
    /// </summary>
    /// <param name="EvidenceGateClosed">Gerbang bukti kecocokan sedang menahan pemberian normal.</param>
    /// <param name="LocationGateClosed">Lokasi penyimpanan terakhir kantong sedang tidak aktif.</param>
    /// <param name="PatientId">Pasien tujuan dari alokasi aktif.</param>
    /// <param name="ValidCompatibilityEvidenceId">
    /// Bukti kecocokan yang berlaku bila gerbang bukti tidak sedang menahan. Dipakai supaya
    /// pemberian darurat yang hanya melewati gerbang lokasi tetap menunjuk bukti yang sah.
    /// </param>
    public sealed record BloodUnitEmergencyBypassState(
        bool EvidenceGateClosed,
        bool LocationGateClosed,
        Guid? PatientId,
        Guid? ValidCompatibilityEvidenceId);
}
