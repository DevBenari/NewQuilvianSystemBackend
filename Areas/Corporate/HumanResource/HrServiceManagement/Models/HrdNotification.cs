using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    /// <summary>
    /// Pemberitahuan dalam aplikasi ("lonceng") untuk pegawai dan pejabat modul SDM
    /// (BE-HRD-314, WF-B7, INT-HR-95; remediasi Gelombang 2, 10 November 2026).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu baris per penerima. Kanal yang tersedia saat ini hanya <c>InApp</c>: pemberitahuan disimpan
    /// di tabel ini, dibaca lewat endpoint pusat notifikasi, dan didorong secara realtime lewat hub SignalR
    /// <c>/hubs/notifications</c> bila penerima sedang terhubung. Platform belum memiliki pengirim email
    /// maupun WhatsApp, sehingga permintaan kanal lain dicatat pada <see cref="RequestedChannel"/> dan tetap
    /// disalurkan ke kanal dalam aplikasi.
    /// </para>
    /// <para>
    /// <see cref="DedupKey"/> mencegah worker berkala mengirim pemberitahuan yang sama berulang kali
    /// (misalnya satu pengingat per tiket per hari). Nilainya unik per penerima.
    /// </para>
    /// </remarks>
    [Table("HrdNotification", Schema = "public")]
    public class HrdNotification : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Akun penerima (<c>AspNetUsers.Id</c>).</summary>
        public Guid RecipientUserId { get; set; }

        /// <summary>Profil tenaga kerja penerima bila diketahui, untuk penelusuran per pegawai.</summary>
        public Guid? RecipientWorkforceProfileId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Body { get; set; } = string.Empty;

        /// <summary>Kategori tampilan: WORKFLOW, DISCIPLINE, GRIEVANCE, SEPARATION, PERFORMANCE, OCCUPATIONAL_HEALTH, TRAVEL, TRAINING, SYSTEM.</summary>
        [Required]
        [MaxLength(60)]
        public string Category { get; set; } = "SYSTEM";

        /// <summary>Normal, High, atau Critical.</summary>
        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Normal";

        /// <summary>Kanal yang benar-benar dipakai. Saat ini selalu <c>InApp</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string Channel { get; set; } = "InApp";

        /// <summary>Kanal yang diminta pemanggil (misalnya <c>EmailDigest</c>), untuk jejak bila kanal lain kelak tersedia.</summary>
        [MaxLength(30)]
        public string? RequestedChannel { get; set; }

        [MaxLength(80)]
        public string? SourceModule { get; set; }

        [MaxLength(100)]
        public string? ReferenceType { get; set; }

        public Guid? ReferenceId { get; set; }

        [MaxLength(300)]
        public string? DeepLinkUrl { get; set; }

        /// <summary>Kunci anti-duplikat per penerima; kosong untuk pemberitahuan sekali kirim.</summary>
        [MaxLength(250)]
        public string? DedupKey { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

        /// <summary>Benar bila sempat didorong lewat SignalR ke penerima yang sedang terhubung.</summary>
        public bool IsPushedRealtime { get; set; } = false;

        public DateTime? ExpiresAt { get; set; }
    }
}
