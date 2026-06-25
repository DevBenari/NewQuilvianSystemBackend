using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpHealthRecord", Schema = "public")]
    public class WfpHealthRecord : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [MaxLength(100)]
        public string? RequirementCode { get; set; }

        [Required]
        public HealthRecordType HealthRecordType { get; set; } = HealthRecordType.Unknown;

        [Required]
        public DateTime RecordDate { get; set; }

        public HealthRecordResultStatus ResultStatus { get; set; } = HealthRecordResultStatus.Unknown;

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool? IsFitToWork { get; set; }

        [MaxLength(250)]
        public string? FitToWorkRestrictionNote { get; set; }

        public bool IsVerified { get; set; } = false;

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(250)]
        public string? VerificationNote { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(100)]
        public string? FileContentType { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        [NotMapped]
        public bool IsExpired =>
            ExpiredDate.HasValue &&
            AppDateTimeHelper.OperationalDate() > ExpiredDate.Value.Date;

        [NotMapped]
        public bool IsCurrentlyValid =>
            IsActive &&
            !IsDelete &&
            IsVerified &&
            (!ExpiredDate.HasValue || AppDateTimeHelper.OperationalDate() <= ExpiredDate.Value.Date);

        [NotMapped]
        public bool IsCompliantForWork =>
            IsCurrentlyValid &&
            (IsFitToWork == null || IsFitToWork == true);

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }
    }
}
