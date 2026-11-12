using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Sidang Pleno Kalibrasi Manajemen RS & Guiding Distribution (HRD-DEC-165, PRF-B3, VR-PRF-05..07)
    /// </summary>
    [Table("trx_performance_calibration_sessions", Schema = "public")]
    public class TrxPerformanceCalibrationSession : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PerformanceCycleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SessionNumber { get; set; } = string.Empty;

        public DateTime SessionDate { get; set; }

        /// <summary>
        /// JSON sebaran pedoman fleksibel (10-15% A, 70-80% B, 5-10% C/D)
        /// </summary>
        [Required]
        public string GuidingDistributionJson { get; set; } = "{\"A\": 15, \"B\": 75, \"C\": 8, \"D\": 2}";

        public string? ActualDistributionJson { get; set; }

        public int TotalEmployeesCalibrated { get; set; } = 0;

        public int TotalOverridesApplied { get; set; } = 0;

        [MaxLength(250)]
        public string? MinutesOfMeetingUrl { get; set; }

        /// <summary>
        /// Scheduled, InProgress, Completed, Cancelled
        /// </summary>
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Scheduled";

        public Guid? CompletedBy { get; set; }

        public DateTime? CompletedAt { get; set; }

        public virtual TrxPerformanceCycle? PerformanceCycle { get; set; }
        public virtual MstWorkforceProfile? CompletedByProfile { get; set; }

        [NotMapped] public string SessionCode { get => SessionNumber; set => SessionNumber = value; }
        [NotMapped] public string? MinutesOfMeeting { get => MinutesOfMeetingUrl; set => MinutesOfMeetingUrl = value; }
        [NotMapped] public Guid CommitteeChairpersonId { get => CompletedBy ?? Guid.Empty; set => CompletedBy = value; }

    }
}
