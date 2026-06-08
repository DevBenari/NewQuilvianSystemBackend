using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstIdentityScannerProfile", Schema = "public")]
    public class MstIdentityScannerProfile : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ProfileCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ProfileName { get; set; } = string.Empty;

        public IdentityScannerProfileType ProfileType { get; set; } = IdentityScannerProfileType.Unknown;

        [MaxLength(100)]
        public string? ScannerVendorName { get; set; }

        [MaxLength(100)]
        public string? ScannerModel { get; set; }

        [MaxLength(100)]
        public string? InputFormat { get; set; }

        [MaxLength(100)]
        public string? OutputFormat { get; set; }

        [MaxLength(250)]
        public string? IdentityNumberRegex { get; set; }

        [MaxLength(250)]
        public string? MemberNumberRegex { get; set; }

        [MaxLength(250)]
        public string? CardNumberRegex { get; set; }

        [MaxLength(100)]
        public string? IdentityNumberFieldName { get; set; }

        [MaxLength(100)]
        public string? FullNameFieldName { get; set; }

        [MaxLength(100)]
        public string? BirthDateFieldName { get; set; }

        [MaxLength(100)]
        public string? GenderFieldName { get; set; }

        [MaxLength(100)]
        public string? AddressFieldName { get; set; }

        public bool IsForIdentityCard { get; set; } = true;

        public bool IsForPatientCard { get; set; } = true;

        public bool IsForMembershipCard { get; set; } = true;

        public bool IsForInsuranceCard { get; set; } = false;

        public bool IsOcrEnabled { get; set; } = false;

        public bool IsBarcodeEnabled { get; set; } = false;

        public bool IsQrEnabled { get; set; } = false;

        public bool IsManualInputAllowed { get; set; } = true;

        public bool IsAutoCreatePatientAllowed { get; set; } = false;

        public bool IsVerificationRequired { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? ConfigurationJson { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
