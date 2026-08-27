using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    [Table("HrdBusinessTripAttendance", Schema = "public")]
    public class HrdBusinessTripAttendance : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? UserId { get; set; }
        public Guid? AttendanceDailyId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? AttendanceLocationId { get; set; }

        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        public Guid? ReferenceId { get; set; }

        [MaxLength(100)]
        public string? ReferenceNumber { get; set; }

        public DateOnly AttendanceDate { get; set; }
        public DateTime? PlannedStartAt { get; set; }
        public DateTime? PlannedEndAt { get; set; }
        public DateTime? ActualStartAt { get; set; }
        public DateTime? ActualEndAt { get; set; }

        [MaxLength(250)]
        public string? Origin { get; set; }

        [MaxLength(250)]
        public string? Destination { get; set; }

        [MaxLength(500)]
        public string? ActivityDescription { get; set; }

        [Required]
        [MaxLength(30)]
        public string AttendanceStatus { get; set; } = "Planned";
        // Planned, Confirmed, Completed, Cancelled, Rejected.

        [MaxLength(500)]
        public string? EvidenceFilePath { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsPayrollEligible { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public ApplicationUser? User { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstAttendanceLocation? AttendanceLocation { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}
