using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class TrainingSessionDto
    {
        public Guid Id { get; set; }
        public Guid TrainingPlanId { get; set; }
        public string? TrainingPlanName { get; set; }
        public string SessionCode { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public int Sequence { get; set; } = 1;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime? RegistrationOpenAt { get; set; }
        public DateTime? RegistrationCloseAt { get; set; }
        public string DeliveryMode { get; set; } = "Classroom";
        public string? Venue { get; set; }
        public string? MeetingUrl { get; set; }
        public string? InstructorName { get; set; }
        public Guid? InstructorUserId { get; set; }
        public int Capacity { get; set; }
        public int MinimumParticipant { get; set; }
        public int RegisteredParticipantCount { get; set; }
        public int RemainingQuota => Math.Max(0, Capacity - RegisteredParticipantCount);
        public string SessionStatus { get; set; } = "Draft";
        public bool RequiresAttendance { get; set; } = true;
        public bool RequiresPreTest { get; set; }
        public bool RequiresPostTest { get; set; }
        public bool GeneratesCertificate { get; set; }
        public decimal MinimumPassingScore { get; set; }
        public string? AgendaJson { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateTrainingSessionRequestDto
    {
        [Required(ErrorMessage = "TrainingPlanId wajib diisi.")]
        public Guid TrainingPlanId { get; set; }

        [Required(ErrorMessage = "Nama sesi kelas pelatihan wajib diisi.")]
        [MaxLength(250)]
        public string SessionName { get; set; } = string.Empty;

        [MaxLength(60)]
        public string? SessionCode { get; set; }

        public int Sequence { get; set; } = 1;

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public DateTime? RegistrationOpenAt { get; set; }

        public DateTime? RegistrationCloseAt { get; set; }

        [MaxLength(50)]
        public string DeliveryMode { get; set; } = "Classroom";

        [MaxLength(500)]
        public string? Venue { get; set; }

        [MaxLength(1000)]
        public string? MeetingUrl { get; set; }

        [MaxLength(250)]
        public string? InstructorName { get; set; }

        public Guid? InstructorUserId { get; set; }

        [Range(1, 1000, ErrorMessage = "Kapasitas kuota peserta minimal 1 orang (VR-TRN-07).")]
        public int Capacity { get; set; } = 30;

        public int MinimumParticipant { get; set; } = 5;

        public bool RequiresAttendance { get; set; } = true;

        public bool RequiresPreTest { get; set; } = false;

        public bool RequiresPostTest { get; set; } = true;

        public bool GeneratesCertificate { get; set; } = true;

        public decimal MinimumPassingScore { get; set; } = 75.0m;

        public string? AgendaJson { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class UpdateTrainingSessionRequestDto
    {
        [Required(ErrorMessage = "Nama sesi kelas pelatihan wajib diisi.")]
        [MaxLength(250)]
        public string SessionName { get; set; } = string.Empty;

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public DateTime? RegistrationOpenAt { get; set; }

        public DateTime? RegistrationCloseAt { get; set; }

        [MaxLength(50)]
        public string DeliveryMode { get; set; } = "Classroom";

        [MaxLength(500)]
        public string? Venue { get; set; }

        [MaxLength(1000)]
        public string? MeetingUrl { get; set; }

        [MaxLength(250)]
        public string? InstructorName { get; set; }

        public Guid? InstructorUserId { get; set; }

        [Range(1, 1000)]
        public int Capacity { get; set; }

        public int MinimumParticipant { get; set; }

        public bool RequiresAttendance { get; set; }

        public bool RequiresPreTest { get; set; }

        public bool RequiresPostTest { get; set; }

        public bool GeneratesCertificate { get; set; }

        public decimal MinimumPassingScore { get; set; }

        public string? AgendaJson { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
