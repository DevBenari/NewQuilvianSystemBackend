using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("HrdRemoteAttendance", Schema = "public")]
    public class HrdRemoteAttendance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? UserId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? AttendanceLocationId { get; set; }
        public Guid? AttendancePolicyId { get; set; }

        public DateOnly AttendanceDate { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }

        [MaxLength(500)]
        public string? LocationDescription { get; set; }

        public decimal? CheckInLatitude { get; set; }
        public decimal? CheckInLongitude { get; set; }
        public decimal? CheckInAccuracyMeters { get; set; }
        public decimal? CheckOutLatitude { get; set; }
        public decimal? CheckOutLongitude { get; set; }
        public decimal? CheckOutAccuracyMeters { get; set; }

        [MaxLength(100)]
        public string? CheckInIpAddress { get; set; }

        [MaxLength(100)]
        public string? CheckOutIpAddress { get; set; }

        [MaxLength(500)]
        public string? CheckInUserAgent { get; set; }

        [MaxLength(500)]
        public string? CheckOutUserAgent { get; set; }

        [Required]
        [MaxLength(30)]
        public string ApprovalStatus { get; set; } = "Pending";
        // Pending, Approved, Rejected, AutoApproved, Cancelled.

        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? EvidenceFilePath { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? User { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public MstAttendanceLocation? AttendanceLocation { get; set; }
        public MstAttendancePolicy? AttendancePolicy { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
