using QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;
using QuilvianSystemBackend.Areas.Administrator.UserManagement.Models;
using QuilvianSystemBackend.Enum;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models
{
    [Table("MstExternalUser", Schema = "public")]
    public class MstExternalUser : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ExternalCode { get; set; } = string.Empty;

        public ExternalUserType ExternalUserType { get; set; } = ExternalUserType.Other;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? CompanyCode { get; set; }

        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [MaxLength(200)]
        public string? ContactPersonName { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? TaxNumber { get; set; }

        [MaxLength(100)]
        public string? BusinessLicenseNumber { get; set; }

        [MaxLength(100)]
        public string? ExternalStatus { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<ExtUserContract> Contracts { get; set; } = new List<ExtUserContract>();

        public ICollection<ExtUserDocument> Documents { get; set; } = new List<ExtUserDocument>();
    }
}