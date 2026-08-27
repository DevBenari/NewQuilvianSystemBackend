using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models
{
    [Table("TrxTravelAttendanceLink", Schema = "public")]
    public class TrxTravelAttendanceLink : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BusinessTravelRequestId { get; set; }

        public Guid? BusinessTravelParticipantId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? BusinessTripAttendanceId { get; set; }
        public Guid? AttendanceDailyId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        [Required, MaxLength(30)]
        public string AttendanceLinkStatus { get; set; } = "Planned";
        // Planned, Synced, Confirmed, Completed, Cancelled, Error.

        public bool IsPayrollEligible { get; set; } = true;
        public bool IsAttendanceGenerated { get; set; } = false;
        public DateTime? SyncedAt { get; set; }
        public Guid? SyncedByUserId { get; set; }

        [MaxLength(1000)]
        public string? SyncMessage { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public TrxBusinessTravelParticipant? BusinessTravelParticipant { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public HrdBusinessTripAttendance? BusinessTripAttendance { get; set; }
        public HrdAttendanceDaily? AttendanceDaily { get; set; }
        public ApplicationUser? SyncedByUser { get; set; }
    }
}
