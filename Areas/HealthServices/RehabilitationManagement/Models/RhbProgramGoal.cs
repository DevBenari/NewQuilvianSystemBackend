using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>Tujuan yang dikejar program — <c>data-dictionary.md</c> bagian 2.10.</summary>
    [Table("RhbProgramGoal", Schema = "public")]
    public class RhbProgramGoal : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProgramId { get; set; }

        public RhbProgram? Program { get; set; }

        [Required]
        public Guid GoalId { get; set; }

        public RhbGoal? Goal { get; set; }
    }
}
