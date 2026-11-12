using System;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs
{
    /// <summary>
    /// Permintaan pengiriman pemberitahuan dari service atau worker modul SDM ke pusat notifikasi.
    /// </summary>
    /// <param name="Title">Judul singkat, maksimal 200 karakter.</param>
    /// <param name="Body">Isi pemberitahuan yang dibaca pegawai, maksimal 2000 karakter.</param>
    /// <param name="Category">Kategori tampilan, lihat <see cref="HrdNotificationCategories"/>.</param>
    /// <param name="Priority">Normal, High, atau Critical.</param>
    /// <param name="DeepLinkUrl">Tautan relatif layar yang membuka objeknya, misalnya <c>/hrd/approvals/inbox</c>.</param>
    /// <param name="SourceModule">Nama modul pengirim, untuk penelusuran.</param>
    /// <param name="ReferenceType">Jenis objek sumber, misalnya <c>TrxWorkflowInstance</c>.</param>
    /// <param name="ReferenceId">ID objek sumber.</param>
    /// <param name="DedupKey">Kunci anti-duplikat; bila diisi, penerima yang sama tidak menerima kunci yang sama dua kali.</param>
    /// <param name="RequestedChannel">Kanal yang diinginkan pemanggil; saat ini semua disalurkan ke kanal dalam aplikasi.</param>
    /// <param name="ActorUserId">Akun pemicu untuk stempel audit; kosong berarti aktor sistem.</param>
    public sealed record HrdNotificationRequest(
        string Title,
        string Body,
        string Category,
        string Priority = HrdNotificationPriorities.Normal,
        string? DeepLinkUrl = null,
        string? SourceModule = null,
        string? ReferenceType = null,
        Guid? ReferenceId = null,
        string? DedupKey = null,
        string? RequestedChannel = null,
        Guid? ActorUserId = null);

    public static class HrdNotificationCategories
    {
        public const string Workflow = "WORKFLOW";
        public const string Discipline = "DISCIPLINE";
        public const string Grievance = "GRIEVANCE";
        public const string Separation = "SEPARATION";
        public const string Performance = "PERFORMANCE";
        public const string OccupationalHealth = "OCCUPATIONAL_HEALTH";
        public const string Travel = "TRAVEL";
        public const string Training = "TRAINING";
        public const string System = "SYSTEM";
    }

    public static class HrdNotificationPriorities
    {
        public const string Normal = "Normal";
        public const string High = "High";
        public const string Critical = "Critical";
    }

    public class HrdNotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string? SourceModule { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? DeepLinkUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime DeliveredAt { get; set; }
    }

    public class HrdNotificationUnreadCountDto
    {
        public int UnreadCount { get; set; }
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }

    public class HrdNotificationMarkReadResultDto
    {
        public int AffectedCount { get; set; }
        public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
    }
}
