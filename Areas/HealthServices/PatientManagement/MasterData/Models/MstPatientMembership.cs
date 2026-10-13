using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstPatientMembership", Schema = "public")]
    public class MstPatientMembership : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid MembershipTierId { get; set; }

        [Required]
        [MaxLength(50)]
        public string MemberNumber { get; set; } = string.Empty;

        public MembershipStatus MembershipStatus { get; set; } = MembershipStatus.PendingPayment;

        public MembershipRegistrationType RegistrationType { get; set; } = MembershipRegistrationType.New;

        /// <summary>Tanggal pendaftaran disimpan (kalender WIB).</summary>
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;

        /// <summary>Tanggal mulai periode (kalender WIB); terisi saat iuran lunas atau saat data lama diakui.</summary>
        public DateTime? PeriodStartDate { get; set; }

        /// <summary>Tanggal akhir periode, inklusif (kalender WIB).</summary>
        public DateTime? ExpiredDate { get; set; }

        /// <summary>Batas pelunasan iuran; lewat dari ini pendaftaran batal otomatis (CRM-DEC-014).</summary>
        public DateTimeOffset? PaymentDueAt { get; set; }

        public DateTimeOffset? ActivatedAt { get; set; }

        /// <summary>Salinan nominal iuran tingkat saat pendaftaran disimpan.</summary>
        public decimal FeeAmountSnapshot { get; set; } = 0;

        /// <summary>Membership yang diperpanjang atau di-upgrade oleh baris ini.</summary>
        public Guid? PreviousPatientMembershipId { get; set; }

        public MembershipCancellationCategory? CancellationCategory { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        /// <summary>Rujukan tagihan iuran di billing; bentuk relasinya menunggu DEC-CRM-001.</summary>
        public Guid? FeeChargeReferenceId { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public bool IsPrimary { get; set; } = true;

        public bool IsAutoCreated { get; set; } = false;

        public bool IsCreatedFromKiosk { get; set; } = false;

        public bool IsCreatedFromAdmission { get; set; } = false;

        public bool IsCreatedByMarketing { get; set; } = false;

        public int PointBalance { get; set; } = 0;

        public decimal TotalSpendAmount { get; set; } = 0;

        public DateTime? LastUpgradeDate { get; set; }

        public DateTime? LastDowngradeDate { get; set; }

        [MaxLength(250)]
        public string? UpgradeDowngradeReason { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPatient? Patient { get; set; }

        public MstMembershipTier? MembershipTier { get; set; }

        public MstPatientMembership? PreviousPatientMembership { get; set; }

        public ICollection<PatMembershipStatusHistory> StatusHistories { get; set; } = new List<PatMembershipStatusHistory>();
    }
}
