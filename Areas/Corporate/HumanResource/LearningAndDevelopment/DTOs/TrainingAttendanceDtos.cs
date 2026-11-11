using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class TrainingAttendanceDto
    {
        public Guid Id { get; set; }
        public Guid TrainingSessionId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime? CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public int ScheduledMinutes { get; set; }
        public int AttendedMinutes { get; set; }
        public string AttendanceStatus { get; set; } = "Present";
        public string AttendanceSource { get; set; } = "Manual";
        public string? ProofPath { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RecordAttendanceCheckInRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        public DateTime? AttendanceDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        [MaxLength(40)]
        public string AttendanceSource { get; set; } = "QRCode";

        public int ScheduledMinutes { get; set; } = 480;

        [MaxLength(1000)]
        public string? ProofPath { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class RecordAttendanceCheckOutRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public int? AttendedMinutes { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ParticipantAttendanceEligibilityDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public bool IsClinicalStaff { get; set; }
        public decimal RequiredAttendancePercentage => IsClinicalStaff ? 100.00m : 80.00m;
        public int TotalSessionDays { get; set; }
        public int AttendedDays { get; set; }
        public int TotalScheduledMinutes { get; set; }
        public int TotalAttendedMinutes { get; set; }
        public decimal ActualAttendancePercentage { get; set; }
        public bool IsEligibleForExam { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }
}
