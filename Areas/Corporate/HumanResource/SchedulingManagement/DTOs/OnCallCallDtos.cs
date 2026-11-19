using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs
{
    public class OnCallBackupResponse
    {
        public Guid Id { get; set; }
        public int BackupSequence { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? ProfileCode { get; set; }
        public string? DisplayName { get; set; }
    }

    public class SetOnCallBackupsRequest
    {
        /// <summary>Seluruh daftar cadangan (ganti utuh). Daftar kosong menghapus semua cadangan.</summary>
        public List<SetOnCallBackupItemRequest> Items { get; set; } = new();
    }

    public class SetOnCallBackupItemRequest
    {
        /// <summary>1, 2, 3, ...; tidak boleh ganda.</summary>
        public int BackupSequence { get; set; }

        public Guid WorkforceProfileId { get; set; }
    }

    public class CreateOnCallCallRequest
    {
        /// <summary>Siaga utama atau salah satu cadangan penugasan ini (VR-PK-09).</summary>
        public Guid CalledWorkforceProfileId { get; set; }

        public Guid CallingOrganizationUnitId { get; set; }

        /// <summary>Alasan panggilan. Jangan memuat nama atau nomor rekam medis pasien.</summary>
        [Required, MaxLength(500)]
        public string CallReason { get; set; } = string.Empty;

        /// <summary>Jam panggil (dengan zona waktu atau UTC).</summary>
        public DateTime CalledAt { get; set; }
    }

    public class AnswerOnCallCallRequest
    {
        public DateTime AnsweredAt { get; set; }

        /// <summary>RowVersion catatan panggilan terakhir yang dibaca; berbeda → 409.</summary>
        public Guid RowVersion { get; set; }
    }

    public class DeclineOnCallCallRequest
    {
        public DateTime AnsweredAt { get; set; }

        /// <summary>Alasan yang disampaikan petugas siaga; wajib (VR-PK-11).</summary>
        [MaxLength(500)]
        public string DeclineReason { get; set; } = string.Empty;

        public Guid RowVersion { get; set; }
    }

    public class ExpireOnCallCallRequest
    {
        public Guid RowVersion { get; set; }
    }

    public class CompleteOnCallCallRequest
    {
        /// <summary>Jam tiba; tidak boleh lebih awal dari jam panggil (VR-PK-12).</summary>
        public DateTime ArrivedAt { get; set; }

        public Guid RowVersion { get; set; }
    }

    public class OnCallCallResponse
    {
        public Guid Id { get; set; }
        public Guid OnCallAssignmentId { get; set; }
        public Guid CalledWorkforceProfileId { get; set; }
        public string? CalledDisplayName { get; set; }

        /// <summary>0 = siaga utama; 1, 2, ... = urutan cadangan.</summary>
        public int BackupSequence { get; set; }

        public Guid CallingOrganizationUnitId { get; set; }
        public string? CallingOrganizationUnitName { get; set; }
        public string CallReason { get; set; } = string.Empty;

        /// <summary>Sent, Accepted, Declined, Expired, Completed.</summary>
        public string CallStatus { get; set; } = string.Empty;

        public string CallStatusLabel { get; set; } = string.Empty;
        public DateTime CalledAt { get; set; }
        public Guid RecordedByUserId { get; set; }
        public string? RecordedByName { get; set; }
        public DateTime? AnsweredAt { get; set; }
        public Guid? AnswerRecordedByUserId { get; set; }
        public string? AnswerRecordedByName { get; set; }
        public string? DeclineReason { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public Guid? ExpiredByUserId { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public int? ResponseMinutes { get; set; }
        public Guid RowVersion { get; set; }

        /// <summary>Terisi sesudah Declined/Expired: cadangan berikutnya yang belum dipanggil pada penugasan ini (HRD-DEC-205).</summary>
        public OnCallNextBackupResponse? NextBackup { get; set; }

        /// <summary>Terisi bila cadangan sudah habis sesudah Declined/Expired.</summary>
        public string? NoBackupMessage { get; set; }
    }

    public class OnCallNextBackupResponse
    {
        public int BackupSequence { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? DisplayName { get; set; }

        /// <summary>Nomor kontak dinas; hanya dikirim pada endpoint pencatatan panggilan (HRD-DEC-205 butir 6).</summary>
        public string? PhoneNumber { get; set; }
    }
}
