using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    /// <summary>
    /// Satu pesanan yang belum selesai ketika pasien pergi dari IGD, beserta sikap yang
    /// ditetapkan klinisi atasnya.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-035</c>. Keputusan <c>IGD-DEC-078</c>, <c>IGD-DEC-100</c>, <c>IGD-DEC-101</c>,
    /// <c>IGD-DEC-102</c>, <c>IGD-DEC-103</c>.
    ///
    /// <para>
    /// <b>Tabel ini milik IGD, dan bukan tabel pesanan tandingan.</b> Ia tidak menyimpan
    /// pesanannya, melainkan <i>sikap IGD atas pesanan itu saat pasien pergi</i> — fakta yang
    /// memang milik IGD dan tidak dimiliki modul mana pun. <c>IGD-DEC-105</c> tetap berlaku:
    /// pemesanan laboratorium tetap lewat <c>LaboratoryManagement</c> apa adanya.
    /// </para>
    ///
    /// <para>
    /// <b>Sikap pesanan laboratorium ditetapkan manual.</b> <c>LabOrder</c> tidak punya kolom
    /// status sama sekali, sehingga sistem tidak dapat membedakan pesanan yang spesimennya
    /// sudah diambil dari yang belum dimulai. <c>IGD-DEC-101</c> karena itu melarang sistem
    /// berpura-pura tahu: klinisi yang menetapkan, dan pelaku serta waktunya disimpan.
    /// </para>
    /// </remarks>
    [Table("EmgHandoverOrderItem", Schema = "public")]
    public class EmgHandoverOrderItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyDepartureId { get; set; }

        [Required]
        public EmergencyOrderKind OrderKind { get; set; }

        [Required]
        public EmergencyOrderSource OrderSource { get; set; } = EmergencyOrderSource.Internal;

        /// <summary>
        /// Baris pesanan di sistem. Wajib untuk <c>Internal</c>, kosong untuk <c>External</c>.
        /// </summary>
        public Guid? OrderReferenceId { get; set; }

        /// <summary>
        /// Identitas pesanan pada sistem luar. Wajib untuk <c>External</c> — <c>IGD-DEC-103</c>.
        /// </summary>
        [MaxLength(150)]
        public string? ExternalReference { get; set; }

        /// <summary>
        /// Uraian pesanan yang dapat diaudit. <b>Wajib selalu</b>: tanpa ini pesanan di luar
        /// sistem tidak dapat ditelusuri sama sekali.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string OrderDescription { get; set; } = string.Empty;

        [Required]
        public EmergencyOrderAction Action { get; set; }

        /// <summary>Wajib bila <see cref="Action"/> bernilai <c>Cancel</c>.</summary>
        [MaxLength(1000)]
        public string? ActionReason { get; set; }

        /// <summary>
        /// Pelaku penetapan sikap. Wajib, termasuk untuk pesanan laboratorium yang sikapnya
        /// ditetapkan manual — <c>IGD-DEC-101</c>.
        /// </summary>
        [Required]
        public Guid ActionByUserId { get; set; }

        [Required]
        public DateTime ActionAt { get; set; } = DateTime.UtcNow;

        /// <summary>Unit penerima. Wajib bila <see cref="Action"/> bernilai <c>Handover</c>.</summary>
        public Guid? ToServiceUnitId { get; set; }

        [Required]
        public EmergencyOrderAcceptanceStatus AcceptanceStatus { get; set; }
            = EmergencyOrderAcceptanceStatus.NotRequired;

        public Guid? AcceptedByUserId { get; set; }

        public DateTime? AcceptedAt { get; set; }

        /// <summary>Wajib bila <see cref="AcceptanceStatus"/> bernilai <c>Rejected</c>.</summary>
        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Salah berarti baris ini sudah digantikan sikap pengganti. Barisnya tidak dihapus —
        /// <c>IGD-DEC-102</c> butir (c), mengikuti pola tambah-saja <c>IGD-DEC-090</c>.
        /// </summary>
        public bool IsEffective { get; set; } = true;

        /// <summary>Baris lama yang digantikan baris ini.</summary>
        public Guid? SupersedesOrderItemId { get; set; }

        public bool IsActive { get; set; } = true;

        public EmgDeparture? EmergencyDeparture { get; set; }

        public MstServiceUnit? ToServiceUnit { get; set; }

        public EmgHandoverOrderItem? SupersedesOrderItem { get; set; }
    }
}
