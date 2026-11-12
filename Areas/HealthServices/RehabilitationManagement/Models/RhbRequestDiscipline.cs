using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>Disiplin yang diminta pengirim — <c>data-dictionary.md</c> bagian 2.2.</summary>
    [Table("RhbRequestDiscipline", Schema = "public")]
    public class RhbRequestDiscipline : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RequestId { get; set; }

        public RhbRequest? Request { get; set; }

        [Required]
        public Guid DisciplineId { get; set; }

        public RhbDiscipline? Discipline { get; set; }
    }
}
