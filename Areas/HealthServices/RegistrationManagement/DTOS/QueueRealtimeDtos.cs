using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class QueueRealtimeEventResponse
    {
        public string EventType { get; set; } = string.Empty;

        public Guid QueueId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        public List<Guid> NurseStationClusterIds { get; set; } = new();

        public DateTime QueueDate { get; set; }

        public int QueueNumber { get; set; }

        public string QueueCode { get; set; } = string.Empty;

        public QueueStatus QueueStatus { get; set; }

        public string QueueStatusName { get; set; } = string.Empty;

        public bool IsScreeningRequired { get; set; }

        public bool IsDoctorRequired { get; set; }

        public bool IsPriorityQueue { get; set; }

        /// <summary>Kelompok antrean non-sensitif untuk penyaringan layar (RJ-DOC-DEC-058).</summary>
        public QueueAudience QueueAudience { get; set; }

        public DateTime? NurseCallExpiresAt { get; set; }

        public DateTime? DoctorCallExpiresAt { get; set; }

        public Guid? ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }

        public string? Message { get; set; }
    }

    /// <summary>
    /// Payload ringkas untuk layar display publik (RJ-PRI-DEC-025, RJ-DOC-DEC-057). Dikirim hanya
    /// ke group display dan hanya untuk antrean non-prioritas. Sengaja tidak memuat
    /// <c>EncounterId</c>, <c>PatientId</c>, data member, aktor, maupun pesan bebas.
    /// </summary>
    public class QueueDisplayRealtimeEventResponse
    {
        public string EventType { get; set; } = string.Empty;

        public Guid QueueId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DoctorId { get; set; }

        public List<Guid> NurseStationClusterIds { get; set; } = new();

        public DateTime QueueDate { get; set; }

        public int QueueNumber { get; set; }

        public string QueueCode { get; set; } = string.Empty;

        public QueueStatus QueueStatus { get; set; }

        public string QueueStatusName { get; set; } = string.Empty;

        public DateTime? NurseCallExpiresAt { get; set; }

        public DateTime? DoctorCallExpiresAt { get; set; }

        public DateTime OccurredAt { get; set; }
    }

    /// <summary>
    /// Kabar perubahan Reservasi Prioritas ke group <c>priority-reservation-board</c>
    /// (rawat-jalan Amendment RP, INT-RP-07). Sengaja tanpa nama pasien, No. RM, alasan, maupun
    /// checklist; layar admisi memuat ulang daftar lewat API ber-hak akses.
    /// </summary>
    public class PriorityReservationRealtimeEventResponse
    {
        public Guid ReservationId { get; set; }

        public DateTime VisitDate { get; set; }

        /// <summary>
        /// Tanggal kunjungan sebelum diganti, hanya pada <c>RESCHEDULED</c>, supaya layar yang
        /// menyaring tanggal lama ikut memuat ulang (RJ-PRI-DEC-038).
        /// </summary>
        public DateTime? PreviousVisitDate { get; set; }

        public PriorityReservationStatus Status { get; set; }

        public string EventCode { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }
    }
}
