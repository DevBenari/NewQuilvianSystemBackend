using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdJobVacancy", Schema = "public")]
    public class HrdJobVacancy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string VacancyNumber { get; set; } = string.Empty;

        [Required]
        public Guid RequisitionId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Description { get; set; }

        public int QuotaSeats { get; set; }

        public bool IsClinical { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateTime? ClosingDate { get; set; }

        public JobVacancyStatus Status { get; set; } = JobVacancyStatus.Draft;

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdJobRequisition? Requisition { get; set; }
        public ICollection<HrdCandidateApplication> Applications { get; set; } = new List<HrdCandidateApplication>();
    }
}
