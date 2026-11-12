using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdCandidateApplication", Schema = "public")]
    public class HrdCandidateApplication : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ApplicationNumber { get; set; } = string.Empty;

        [Required]
        public Guid CandidateId { get; set; }

        [Required]
        public Guid VacancyId { get; set; }

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public ApplicationStage CurrentStage { get; set; } = ApplicationStage.Screening;

        [MaxLength(50)]
        public string StageStatus { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public HrdCandidate? Candidate { get; set; }
        public HrdJobVacancy? Vacancy { get; set; }
        public HrdCandidateScreening? Screening { get; set; }
        public ICollection<HrdCandidateEvaluation> Evaluations { get; set; } = new List<HrdCandidateEvaluation>();
        public ICollection<HrdJobOffer> Offers { get; set; } = new List<HrdJobOffer>();
    }
}
