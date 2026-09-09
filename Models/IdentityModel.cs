namespace QuilvianSystemBackend.Models
{
    /// <summary>
    /// Kolom jejak pembuatan yang dapat distempel tanpa perlu tahu tipe konkretnya.
    /// </summary>
    /// <remarks>
    /// Dipisahkan sebagai antarmuka supaya penulis data generik — seeder, importer — dapat
    /// menstempel siapa dan kapan lewat batasan tipe, bukan lewat uji tipe saat berjalan.
    /// </remarks>
    public interface IAuditStamped
    {
        DateTime CreateDateTime { get; set; }

        Guid CreateBy { get; set; }
    }

    public class IdentityModel : IAuditStamped
    {
        public DateTime CreateDateTime { get; set; } = DateTime.UtcNow;

        public Guid CreateBy { get; set; } = Guid.Empty;

        public DateTime? UpdateDateTime { get; set; }

        public Guid UpdateBy { get; set; } = Guid.Empty;

        public DateTime? DeleteDateTime { get; set; }

        public Guid DeleteBy { get; set; } = Guid.Empty;

        public DateTime? CancelDateTime { get; set; }

        public Guid CancelBy { get; set; } = Guid.Empty;

        public bool IsCancel { get; set; } = false;

        public bool IsDelete { get; set; } = false;
    }
}
