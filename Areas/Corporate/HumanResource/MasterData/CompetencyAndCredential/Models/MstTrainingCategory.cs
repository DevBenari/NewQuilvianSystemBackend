using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models
{
    [Table("MstTrainingCategory", Schema = "public")]
    public class MstTrainingCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TrainingCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TrainingCategoryName { get; set; } = string.Empty;

        public bool IsMandatoryCategory { get; set; } = false;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstTrainingCatalog> TrainingCatalogs { get; set; }
            = new List<MstTrainingCatalog>();
    }
}
