using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models
{
    [Table("MstPatientRelationship", Schema = "public")]
    public class MstPatientRelationship : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PatientId { get; set; }

        public Guid? RelatedPatientId { get; set; }

        public PatientRelationshipType RelationshipType { get; set; } = PatientRelationshipType.Unknown;

        [MaxLength(200)]
        public string? RelatedPersonName { get; set; }

        [MaxLength(50)]
        public string? RelatedPersonIdentityType { get; set; }

        [MaxLength(100)]
        public string? RelatedPersonIdentityNumber { get; set; }

        [MaxLength(30)]
        public string? RelatedPersonPhoneNumber { get; set; }

        [MaxLength(30)]
        public string? RelatedPersonWhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? RelatedPersonEmail { get; set; }

        [MaxLength(500)]
        public string? RelatedPersonAddress { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsEmergencyContact { get; set; } = false;

        public bool IsResponsiblePerson { get; set; } = false;

        public bool IsLegalGuardian { get; set; } = false;

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPatient? Patient { get; set; }

        public MstPatient? RelatedPatient { get; set; }
    }
}