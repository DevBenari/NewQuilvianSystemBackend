using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxJobVacancy", Schema = "public")]
    public class TrxJobVacancy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string VacancyNumber { get; set; } = string.Empty;

        [Required]
        public Guid JobRequisitionId { get; set; }

        public Guid? RecruitmentSourceId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? WorkLocationId { get; set; }

        [Required]
        [MaxLength(200)]
        public string VacancyTitle { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string? VacancyDescription { get; set; }

        [MaxLength(3000)]
        public string? Responsibilities { get; set; }

        [MaxLength(3000)]
        public string? Requirements { get; set; }

        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public int VacancyCount { get; set; } = 1;
        public int FilledCount { get; set; } = 0;
        public int ApplicationCount { get; set; } = 0;

        [MaxLength(30)]
        public string PublicationStatus { get; set; } = "Draft";
        // Draft, Published, Paused, Closed, Cancelled.

        [MaxLength(30)]
        public string EmploymentLocationType { get; set; } = "OnSite";
        // OnSite, Hybrid, Remote.

        public decimal? PublishedSalaryMinimum { get; set; }
        public decimal? PublishedSalaryMaximum { get; set; }

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public bool IsSalaryConfidential { get; set; } = true;
        public bool AllowExternalApplication { get; set; } = true;
        public bool AllowInternalApplication { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public DateTime? PublishedAt { get; set; }
        public Guid? PublishedByUserId { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxJobRequisition? JobRequisition { get; set; }
        public MstRecruitmentSource? RecruitmentSource { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public ApplicationUser? PublishedByUser { get; set; }
    }
}
