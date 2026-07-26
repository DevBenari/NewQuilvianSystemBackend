using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxCandidate", Schema = "public")]
    public class TrxCandidate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string CandidateNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        public Gender? Gender { get; set; }
        public DateTime? BirthDate { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        [MaxLength(100)]
        public string? Nationality { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(50)]
        public string? PhoneNumber { get; set; }

        [MaxLength(50)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(1000)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? PostalCodeId { get; set; }
        public Guid? RecruitmentSourceId { get; set; }
        public Guid? CandidateStatusId { get; set; }
        public Guid? ReferredByWorkforceProfileId { get; set; }

        [MaxLength(500)]
        public string? LinkedInUrl { get; set; }

        [MaxLength(500)]
        public string? PortfolioUrl { get; set; }

        [MaxLength(500)]
        public string? CvFilePath { get; set; }

        public bool HasDataProcessingConsent { get; set; } = false;
        public DateTime? ConsentAt { get; set; }
        public bool IsBlacklisted { get; set; } = false;

        [MaxLength(1000)]
        public string? BlacklistReason { get; set; }

        [MaxLength(30)]
        public string SourceChannel { get; set; } = "Web";

        public string? AdditionalDataJson { get; set; }
        public bool IsActive { get; set; } = true;

        public MstCountry? Country { get; set; }
        public MstProvince? Province { get; set; }
        public MstCity? City { get; set; }
        public MstDistrict? District { get; set; }
        public MstPostalCode? PostalCode { get; set; }
        public MstRecruitmentSource? RecruitmentSource { get; set; }
        public MstCandidateStatus? CandidateStatus { get; set; }
        public MstWorkforceProfile? ReferredByWorkforceProfile { get; set; }
    }
}
