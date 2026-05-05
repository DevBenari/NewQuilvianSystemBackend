using Microsoft.AspNetCore.Identity;

namespace QuilvianSystemBackend.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }

        public bool IsSystemRole { get; set; } = false;

        public DateTime CreateDateTime { get; set; } = DateTime.Now;

        public DateTime? UpdateDateTime { get; set; }
    }
}
