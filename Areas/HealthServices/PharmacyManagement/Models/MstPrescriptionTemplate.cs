using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    [Table("MstPrescriptionTemplate", Schema = "public")]
    public class MstPrescriptionTemplate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TemplateCategory { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public Guid OwnerDoctorId { get; set; }

        public bool IsShared { get; set; }

        public bool IsFavorite { get; set; }

        public int UsageCount { get; set; }

        public DateTime? LastUsedAt { get; set; }

        public int RegularItemCount { get; set; }

        public int CompoundCount { get; set; }

        public int CompoundIngredientCount { get; set; }

        public int TotalItemCount { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDoctor? OwnerDoctor { get; set; }

        public ICollection<MstPrescriptionTemplateItem> Items { get; set; }
            = new List<MstPrescriptionTemplateItem>();

        public ICollection<MstPrescriptionTemplateCompound> Compounds { get; set; }
            = new List<MstPrescriptionTemplateCompound>();
    }
}
