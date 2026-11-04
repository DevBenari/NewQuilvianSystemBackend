using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models
{
    [Table("TrxOnCallAssignment", Schema = "public")]
    public class TrxOnCallAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RosterPeriodId { get; set; }

        public Guid? RosterAssignmentId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid OnCallTypeId { get; set; }

        public Guid? UnitId { get; set; }
        public Guid? ShiftId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }

        [Required]
        [MaxLength(30)]
        public string OnCallRole { get; set; } = "Primary";
        // Primary, Backup, Escalation

        [Required]
        [MaxLength(30)]
        public string AssignmentStatus { get; set; } = "Scheduled";
        // Scheduled, Confirmed, Activated, Completed, Cancelled

        public int? ExpectedResponseMinutes { get; set; } = 30;
        public bool IsCalledIn { get; set; } = false;
        public DateTime? CallInTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public int? ResponseMinutes { get; set; }
        public bool? IsSlaMet { get; set; }
        public Guid? CalledByUnitId { get; set; }

        [MaxLength(500)]
        public string? CallNotes { get; set; }

        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxRosterPeriod? RosterPeriod { get; set; }
        public TrxRosterAssignment? RosterAssignment { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstOnCallType? OnCallType { get; set; }
        public MstShift? Shift { get; set; }
        public MstOrganizationUnit? Unit { get; set; }
        public MstOrganizationUnit? CalledByUnit { get; set; }
    }
}
