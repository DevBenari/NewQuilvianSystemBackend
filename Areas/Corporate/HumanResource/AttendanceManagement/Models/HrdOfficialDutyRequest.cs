using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    /// <summary>
    /// Surat perintah tugas dinas luar rumah sakit (ambulans rujukan, kurir lab, pelatihan, home care per HRD-DEC-109, BE-HRD-104, BE-HRD-109).
    /// </summary>
    [Table("HrdOfficialDutyRequest", Schema = "public")]
    public class HrdOfficialDutyRequest : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        /// <summary>
        /// Kategori dinas: AmbulanceReferral, LabCourier, TrainingSeminar, HomeCare, Other
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string DutyCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AssignmentOrderNumber { get; set; } = string.Empty;

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        [Required]
        [MaxLength(255)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public string Purpose { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(11,8)")]
        public decimal? Longitude { get; set; }

        [MaxLength(500)]
        public string? AttachmentUrl { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public Guid? ApprovedByWorkforceId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(WorkforceProfileId))]
        public MstWorkforceProfile? WorkforceProfile { get; set; }

        [ForeignKey(nameof(ApprovedByWorkforceId))]
        public MstWorkforceProfile? ApprovedByWorkforce { get; set; }
    }
}
