namespace QuilvianSystemBackend.Models
{
    public class IdentityModel
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
