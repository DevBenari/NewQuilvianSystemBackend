using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    // rawat-jalan Cetak Antrean Admisi v1.1 — kontrak tiket antrean admisi (kiosk, loket, display).
    // Tidak ada satu pun response di berkas ini yang memuat nomor antrean poliklinik internal
    // (bagian 8.1, 8.5, 10 butir 4); identitas pasien hanya muncul pada respons petugas.

    public class AdmissionQueueGroupResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Prefix { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class AdmissionQueueCounterResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public List<string> GroupCodes { get; set; } = new();
    }

    /// <summary>Konfigurasi yang dibaca kiosk sebelum mencetak (bagian 8.1).</summary>
    public class KioskAdmissionQueueConfigResponse
    {
        public bool Enabled { get; set; }

        /// <summary><c>false</c> bila hanya satu kelompok: kiosk langsung mencetak tanpa pilihan.</summary>
        public bool RequiresGroupSelection { get; set; }

        public string? HospitalName { get; set; }

        public string WaitingAreaName { get; set; } = string.Empty;

        public List<AdmissionQueueGroupResponse> Groups { get; set; } = new();
    }

    public class KioskAdmissionTicketIssueRequest
    {
        /// <summary>Kosong berarti kelompok tunggal; wajib bila kelompok aktif lebih dari satu.</summary>
        [MaxLength(30)]
        public string? QueueGroupCode { get; set; }

        /// <summary>Dibuat kiosk sekali per sentuhan Cetak Antrean; request ulang memakai nilai yang sama.</summary>
        [Required]
        [MaxLength(100)]
        public string IdempotencyKey { get; set; } = string.Empty;
    }

    public class KioskAdmissionTicketPrintResultRequest
    {
        [Required]
        public bool Succeeded { get; set; }

        [MaxLength(250)]
        public string? FailureReason { get; set; }
    }

    /// <summary>Isi tiket thermal (bagian 9). Tidak memuat nama, NIK, maupun nomor poli.</summary>
    public class KioskAdmissionTicketResponse
    {
        public Guid Id { get; set; }

        public string DisplayNumber { get; set; } = string.Empty;

        public string QueueGroupCode { get; set; } = string.Empty;

        public string QueueGroupName { get; set; } = string.Empty;

        public DateTime ServiceDate { get; set; }

        public DateTime IssuedAt { get; set; }

        public string? HospitalName { get; set; }

        public string WaitingAreaName { get; set; } = string.Empty;

        public AdmissionTicketPrintStatus PrintStatus { get; set; }

        public string PrintStatusName { get; set; } = string.Empty;

        public int PrintAttemptCount { get; set; }

        /// <summary><c>true</c> bila request ulang memulangkan tiket yang sudah terbit.</summary>
        public bool IsExisting { get; set; }
    }

    public class AdmissionQueueListQuery
    {
        /// <summary>Tanggal pelayanan WIB tunggal; dipakai bila rentang kosong. Kosong semua berarti hari ini.</summary>
        public DateTime? ServiceDate { get; set; }

        /// <summary>Awal rentang tanggal pelayanan WIB (inklusif).</summary>
        public DateTime? ServiceDateFrom { get; set; }

        /// <summary>Akhir rentang tanggal pelayanan WIB (inklusif).</summary>
        public DateTime? ServiceDateTo { get; set; }

        [MaxLength(30)]
        public string? QueueGroupCode { get; set; }

        public AdmissionTicketStatus? Status { get; set; }

        [MaxLength(30)]
        public string? CounterCode { get; set; }

        [MaxLength(50)]
        public string? Search { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 25;
    }

    public class AdmissionQueueSummaryResponse
    {
        /// <summary>Awal rentang; sama dengan <see cref="ServiceDateTo"/> bila satu hari.</summary>
        public DateTime ServiceDate { get; set; }

        public DateTime ServiceDateTo { get; set; }

        public int Total { get; set; }

        public int Waiting { get; set; }

        public int Called { get; set; }

        public int Serving { get; set; }

        public int Completed { get; set; }

        public int Skipped { get; set; }

        public int Cancelled { get; set; }

        /// <summary>Selesai dan tertaut ke kunjungan valid.</summary>
        public int Registered { get; set; }
    }

    public class AdmissionQueueFilterMetadataResponse
    {
        public List<AdmissionQueueGroupResponse> Groups { get; set; } = new();

        public List<AdmissionQueueCounterResponse> Counters { get; set; } = new();

        public List<AdmissionQueueEnumOption> StatusOptions { get; set; } = new();
    }

    public class AdmissionQueueEnumOption
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class AdmissionTicketListItemResponse
    {
        public Guid Id { get; set; }

        public DateTime ServiceDate { get; set; }

        public string DisplayNumber { get; set; } = string.Empty;

        public int SequenceNumber { get; set; }

        public string QueueGroupCode { get; set; } = string.Empty;

        public string QueueGroupName { get; set; } = string.Empty;

        public DateTime IssuedAt { get; set; }

        public AdmissionTicketStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public AdmissionTicketPrintStatus PrintStatus { get; set; }

        public string PrintStatusName { get; set; } = string.Empty;

        public string? CounterCode { get; set; }

        public string? CounterName { get; set; }

        public DateTime? CalledAt { get; set; }

        public int CallAttemptCount { get; set; }

        public DateTime? ServiceStartedAt { get; set; }

        public DateTime? ServiceEndedAt { get; set; }

        public int SkipCount { get; set; }

        public string? SkipReason { get; set; }

        public string? CancelReason { get; set; }

        public string? CompletionNote { get; set; }

        public Guid? EncounterId { get; set; }

        public string? EncounterNumber { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        /// <summary>Aksi yang sah pada status saat ini; tombol layar mengikuti daftar ini.</summary>
        public List<string> AvailableActions { get; set; } = new();
    }

    public class AdmissionTicketHistoryResponse
    {
        public string EventCode { get; set; } = string.Empty;

        public AdmissionTicketStatus? FromStatus { get; set; }

        public AdmissionTicketStatus ToStatus { get; set; }

        public string ToStatusName { get; set; } = string.Empty;

        public string? CounterCode { get; set; }

        public string? Reason { get; set; }

        public Guid? ActorUserId { get; set; }

        public string? ActorName { get; set; }

        public DateTime OccurredAt { get; set; }
    }

    public class AdmissionTicketDetailResponse : AdmissionTicketListItemResponse
    {
        public Guid? CalledByUserId { get; set; }

        public DateTime? SkippedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        public DateTime? RegistrationLinkedAt { get; set; }

        public int PrintAttemptCount { get; set; }

        public DateTime? LastPrintedAt { get; set; }

        public string? PrintFailureReason { get; set; }

        public List<AdmissionTicketHistoryResponse> Histories { get; set; } = new();
    }

    public class AdmissionQueueCallNextRequest
    {
        [Required]
        [MaxLength(30)]
        public string CounterCode { get; set; } = string.Empty;

        /// <summary>Kosong berarti kelompok mana pun yang dilayani loket ini.</summary>
        [MaxLength(30)]
        public string? QueueGroupCode { get; set; }
    }

    public class AdmissionTicketCounterRequest
    {
        /// <summary>Loket pemanggil; kosong berarti loket yang tercatat pada tiket.</summary>
        [MaxLength(30)]
        public string? CounterCode { get; set; }
    }

    public class AdmissionTicketReasonRequest
    {
        [Required]
        [MaxLength(250)]
        public string Reason { get; set; } = string.Empty;
    }

    public class AdmissionTicketCompleteRequest
    {
        /// <summary>Wajib bila tiket belum tertaut ke kunjungan, misalnya registrasi gagal.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>Hasil Panggil Berikutnya; <see cref="Ticket"/> kosong bila tidak ada yang menunggu.</summary>
    public class AdmissionQueueCallNextResponse
    {
        public AdmissionTicketListItemResponse? Ticket { get; set; }

        public QueueVoiceGenerateResponse? Voice { get; set; }
    }

    public class AdmissionTicketActionResponse
    {
        public AdmissionTicketListItemResponse Ticket { get; set; } = new();

        public QueueVoiceGenerateResponse? Voice { get; set; }
    }

    /// <summary>Satu baris display admisi (bagian 8.5): nomor dan loket saja.</summary>
    public class AdmissionQueueDisplayCallResponse
    {
        public Guid TicketId { get; set; }

        public string DisplayNumber { get; set; } = string.Empty;

        public string QueueGroupName { get; set; } = string.Empty;

        public string? CounterName { get; set; }

        public AdmissionTicketStatus Status { get; set; }

        public DateTime? CalledAt { get; set; }

        public int CallAttemptCount { get; set; }

        public string? AudioUrl { get; set; }
    }

    public class AdmissionQueueDisplayResponse
    {
        public DateTime ServiceDate { get; set; }

        public AdmissionQueueDisplayCallResponse? Current { get; set; }

        public List<AdmissionQueueDisplayCallResponse> RecentCalls { get; set; } = new();

        public int WaitingCount { get; set; }

        /// <summary>Nomor menunggu terdepan (Menunggu dan Dilewati), tanpa identitas pasien.</summary>
        public List<AdmissionQueueDisplayWaitingResponse> WaitingTickets { get; set; } = new();
    }

    public class AdmissionQueueDisplayWaitingResponse
    {
        public Guid TicketId { get; set; }

        public string DisplayNumber { get; set; } = string.Empty;

        public string QueueGroupName { get; set; } = string.Empty;

        public AdmissionTicketStatus Status { get; set; }

        public DateTime IssuedAt { get; set; }
    }

    /// <summary>Kabar realtime tanpa identitas pasien (bagian 8.5).</summary>
    public class AdmissionQueueRealtimeEventResponse
    {
        public Guid TicketId { get; set; }

        public DateTime ServiceDate { get; set; }

        public string DisplayNumber { get; set; } = string.Empty;

        public string QueueGroupCode { get; set; } = string.Empty;

        public string? CounterName { get; set; }

        public AdmissionTicketStatus Status { get; set; }

        public string EventCode { get; set; } = string.Empty;

        public int CallAttemptCount { get; set; }

        public string? AudioUrl { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
