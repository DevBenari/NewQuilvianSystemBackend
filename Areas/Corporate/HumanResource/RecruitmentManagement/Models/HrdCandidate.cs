using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdCandidate", Schema = "public")]
    public class HrdCandidate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string CandidateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(16)]
        public string NationalIdNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        public DateOnly BirthDate { get; set; }

        [MaxLength(100)]
        public string BirthPlace { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        public bool IsRehire { get; set; }

        public Guid? PreviousWorkforceProfileId { get; set; }

        public CandidateStatus Status { get; set; } = CandidateStatus.Registered;

        [ConcurrencyCheck]
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public MstWorkforceProfile? PreviousWorkforceProfile { get; set; }
        public ICollection<HrdCandidateApplication> Applications { get; set; } = new List<HrdCandidateApplication>();
        public ICollection<HrdCandidateDocument> Documents { get; set; } = new List<HrdCandidateDocument>();
    }
}
