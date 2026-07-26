using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    [Table("MstHrServiceCategory", Schema = "public")]
    public class MstHrServiceCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ServiceCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ServiceCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? IconName { get; set; }

        [MaxLength(20)]
        public string? DisplayColor { get; set; }

        public int DefaultSlaHours { get; set; } = 24;
        public int SortOrder { get; set; } = 0;
        public bool IsEmployeeVisible { get; set; } = true;
        public bool IsManagerVisible { get; set; } = true;
        public bool IsConfidentialByDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public ICollection<MstHrServiceType> ServiceTypes { get; set; } = new List<MstHrServiceType>();
        public ICollection<TrxHrServiceRequest> ServiceRequests { get; set; } = new List<TrxHrServiceRequest>();
    }
}
