using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpBedPlacement", Schema = "public")]
    public class InpBedPlacement : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        [Required]
        public Guid BedId { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        public Guid PatientClassId { get; set; }

        public int SequenceNumber { get; set; }

        // Untuk jalur datang langsung dan poliklinik: waktu penempatan dibuat.
        // Untuk episode yang lahir dari serah terima IGD: dibaca dari event Tiba pada catatan
        // kepergian IGD dan tidak pernah dikoreksi setelah tersimpan — RWI-DEC-072.
        public DateTime StartDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? EndDateTime { get; set; }

        public InpBedPlacementEndReason? EndReason { get; set; }

        [MaxLength(500)]
        public string? TransferReason { get; set; }

        public DateTime? PhysicallyLeftAt { get; set; }

        public int Version { get; set; } = 1;

        [MaxLength(500)]
        public string? ChangeReason { get; set; }

        /// <summary>
        /// Tetap berarti "penempatan ini diakhiri transfer". <b>Bukan</b> saringan tarif kamar —
        /// yang dikecualikan Billing hanya baris yang <see cref="SupersededByCorrectionId"/>-nya
        /// terisi (kontrak <c>integrasi-billing</c> <c>1.1.0</c>, <c>INV-RWF-07</c>).
        /// </summary>
        public bool IsSuperseded { get; set; } = false;

        public DateTime? SupersededAtUtc { get; set; }

        /// <summary>
        /// Pada baris hasil koreksi salah catat: baris penempatan yang dikoreksinya
        /// (<c>RWI-DEC-157</c>, kamus data <c>integrasi-billing</c> 6.4).
        /// </summary>
        public Guid? CorrectsPlacementId { get; set; }

        /// <summary>
        /// Pada baris yang dikoreksi: baris penggantinya. Billing <b>mengecualikan</b> baris yang
        /// kolom ini terisi dari hitungan tarif kamar (<c>INV-RWF-07</c>), sedangkan barisnya
        /// sendiri tetap tersimpan sebagai jejak.
        /// </summary>
        public Guid? SupersededByCorrectionId { get; set; }

        [Required]
        public Guid PlacedByUserId { get; set; }

        public Guid? EndedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        public InpEpisode? Episode { get; set; }

        public MstBed? Bed { get; set; }

        public MstRoom? Room { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstPatientClass? PatientClass { get; set; }

        public ApplicationUser? PlacedByUser { get; set; }

        public ApplicationUser? EndedByUser { get; set; }

        public InpBedPlacement? CorrectsPlacement { get; set; }

        public InpBedPlacement? SupersededByCorrection { get; set; }
    }
}
