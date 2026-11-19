using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models
{
    [Table("MstJobDescriptionItem", Schema = "public")]
    public class MstJobDescriptionItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid JobDescriptionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SectionType { get; set; } = "MainDuty";
        // MainDuty, Responsibility, Authority, WorkRelation

        public int Sequence { get; set; } = 1;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public MstJobDescription? JobDescription { get; set; }
    }
}
