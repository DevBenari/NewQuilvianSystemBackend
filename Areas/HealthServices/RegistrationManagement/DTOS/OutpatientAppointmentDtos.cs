using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Saringan Tracking Jadwal Booking (RJ-REG-REV-BE-005). Booking = kunjungan Rawat Jalan
    /// berklinik dengan <c>IsAppointment</c>.
    /// </summary>
    public class OutpatientAppointmentListQuery
    {
        /// <summary>Tanggal kunjungan awal (<c>yyyy-MM-dd</c>); bawaan hari ini.</summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>Tanggal kunjungan akhir (<c>yyyy-MM-dd</c>); bawaan hari ini + 30. Rentang maks 92 hari.</summary>
        public DateTime? DateTo { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        /// <summary><c>Upcoming</c> (belum datang), <c>Arrived</c> (sudah diproses), <c>Cancelled</c>; kosong = semua.</summary>
        [MaxLength(20)]
        public string? Status { get; set; }

        [MaxLength(100)]
        public string? Search { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;
    }

    public static class OutpatientAppointmentStatuses
    {
        public const string Upcoming = "Upcoming";
        public const string Arrived = "Arrived";
        public const string Cancelled = "Cancelled";
    }

    public class OutpatientAppointmentListItemResponse
    {
        public Guid Id { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public DateTime EncounterDate { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public Guid ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? ScheduleLabel { get; set; }
        public string? QueueCode { get; set; }
        public int EncounterStatus { get; set; }
        public string EncounterStatusName { get; set; } = string.Empty;

        /// <summary><c>Upcoming</c>, <c>Arrived</c>, atau <c>Cancelled</c>.</summary>
        public string BookingStatus { get; set; } = OutpatientAppointmentStatuses.Upcoming;
        public string BookingStatusName { get; set; } = string.Empty;
        public int RescheduleCount { get; set; }
        public bool CanReschedule { get; set; }
        public string? RescheduleBlockedReason { get; set; }
    }

    public class OutpatientAppointmentRescheduleHistoryResponse
    {
        public Guid Id { get; set; }
        public DateTime OldEncounterDate { get; set; }
        public DateTime NewEncounterDate { get; set; }
        public string? OldDoctorName { get; set; }
        public string? NewDoctorName { get; set; }
        public string? OldQueueCode { get; set; }
        public string? NewQueueCode { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime RescheduledAt { get; set; }
        public string? RescheduledByName { get; set; }
    }

    public class OutpatientAppointmentDetailResponse : OutpatientAppointmentListItemResponse
    {
        public DateTime? BirthDate { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid ServiceUnitId { get; set; }
        public Guid? DoctorScheduleId { get; set; }
        public string? DoctorSpecialistName { get; set; }
        public string? QueueStatusName { get; set; }
        public DateTime? QueueDate { get; set; }
        public string PaymentLabel { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public List<OutpatientAppointmentRescheduleHistoryResponse> RescheduleHistory { get; set; } = new();
    }

    /// <summary>
    /// Isian reschedule (RJ-REG-DEC-007). Panjang alasan diperiksa service supaya pesannya
    /// seragam dengan aturan, bukan pesan bawaan model binding.
    /// </summary>
    public class OutpatientAppointmentRescheduleRequest
    {
        /// <summary>Tanggal kunjungan baru, <c>yyyy-MM-dd</c>.</summary>
        public string? VisitDate { get; set; }

        public Guid? DoctorScheduleId { get; set; }

        public string? Reason { get; set; }
    }

    public class OutpatientAppointmentRescheduleResponse
    {
        public Guid Id { get; set; }
        public Guid RescheduleId { get; set; }
        public DateTime EncounterDate { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? QueueCode { get; set; }
        public string? CancelledQueueCode { get; set; }
    }

    public class OutpatientAppointmentFilterMetadataResponse
    {
        public List<OutpatientEncounterOptionResponse> StatusOptions { get; set; } = new();
        public List<OutpatientEncounterOptionResponse> ClinicOptions { get; set; } = new();
        public List<OutpatientEncounterOptionResponse> DoctorOptions { get; set; } = new();
    }
}
