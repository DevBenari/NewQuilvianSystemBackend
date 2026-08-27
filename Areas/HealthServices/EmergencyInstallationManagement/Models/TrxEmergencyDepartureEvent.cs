using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    /// <summary>
    /// Kejadian pada satu catatan kepergian. <b>Tambah-saja</b>: barisnya tidak pernah
    /// ditimpa maupun dihapus.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-033</c> dan <c>BE-IGD-034</c>, keputusan <c>IGD-DEC-090</c> yang menyatukan
    /// <c>IGD-DEC-070</c> (dua kolom status, dibaca cepat) dengan <c>IGD-DEC-065</c>,
    /// <c>IGD-DEC-066</c>, dan <c>IGD-DEC-085</c> (waktu sebenarnya, koreksi, pembalikan).
    ///
    /// <para>
    /// <b>Dua waktu yang berbeda, dan itu disengaja.</b> <see cref="RecordedAt"/> adalah waktu
    /// server ketika barisnya ditulis; <see cref="OccurredAt"/> adalah waktu kejadian yang
    /// sebenarnya. Keduanya berbeda setiap kali petugas mencatat menyusul — misalnya sesudah
    /// jaringan pulih. Menyimpan satu saja berarti memilih antara memalsukan riwayat klinis
    /// atau kehilangan jejak siapa mencatat kapan.
    /// </para>
    ///
    /// <para>
    /// <b>Koreksi tidak menimpa.</b> Baris koreksi ditulis baru dengan
    /// <see cref="SupersedesEventId"/> menunjuk baris lama, dan baris lama ditandai
    /// <see cref="IsEffective"/> bernilai salah. Riwayatnya tetap utuh dan dapat dibaca
    /// siapa pun sesudahnya.
    /// </para>
    /// </remarks>
    [Table("TrxEmergencyDepartureEvent", Schema = "public")]
    public class TrxEmergencyDepartureEvent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyDepartureId { get; set; }

        [Required]
        public EmergencyDepartureEventType EventType { get; set; }

        /// <summary>Waktu kejadian yang sebenarnya. Tidak boleh melampaui waktu sekarang.</summary>
        [Required]
        public DateTime OccurredAt { get; set; }

        /// <summary>Waktu server saat baris ini ditulis.</summary>
        [Required]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid RecordedByUserId { get; set; }

        /// <summary>
        /// Alasan kejadian. Wajib untuk <c>Cancelled</c>, <c>Amended</c>, dan <c>Reversed</c>.
        /// </summary>
        [MaxLength(1000)]
        public string? Reason { get; set; }

        /// <summary>
        /// Rujukan catatan manual ketika kedatangan dicatat menyusul jauh dari waktu server —
        /// validation bagian 4 aturan 9, <c>IGD-DEC-065</c>.
        /// </summary>
        [MaxLength(250)]
        public string? DowntimeReference { get; set; }

        /// <summary>
        /// Salah berarti baris ini sudah digantikan koreksi atau dibalikkan. Barisnya tetap
        /// ada dan tetap terbaca.
        /// </summary>
        public bool IsEffective { get; set; } = true;

        /// <summary>Baris lama yang digantikan baris ini.</summary>
        public Guid? SupersedesEventId { get; set; }

        /// <summary>
        /// Pemberi persetujuan pembalikan. Wajib untuk <c>Reversed</c>, dan wajib berbeda
        /// dari <see cref="RecordedByUserId"/> — <c>IGD-DEC-066</c>.
        /// </summary>
        public Guid? ApprovedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmergencyDeparture? EmergencyDeparture { get; set; }

        public TrxEmergencyDepartureEvent? SupersedesEvent { get; set; }
    }
}
