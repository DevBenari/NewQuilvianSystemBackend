using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("HrdPreEmploymentMedicalCheck", Schema = "public")]
    public class HrdPreEmploymentMedicalCheck : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OfferId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReferralNumber { get; set; } = string.Empty;

        public DateTime ReferralDate { get; set; } = DateTime.UtcNow;

        public DateTime? CheckDate { get; set; }

        [MaxLength(150)]
        public string ClinicOrFacility { get; set; } = string.Empty;

        public McuClearanceResult ClearanceResult { get; set; } = McuClearanceResult.Pending;

        [MaxLength(1000)]
        public string? DoctorNotes { get; set; }

        [MaxLength(500)]
        public string? CertificateFilePath { get; set; }

        [MaxLength(150)]
        public string? ExaminingDoctorName { get; set; }

        public HrdJobOffer? Offer { get; set; }
    }
}
