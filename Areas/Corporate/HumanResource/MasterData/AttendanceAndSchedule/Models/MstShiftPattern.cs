using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models
{
    [Table("MstShiftPattern", Schema = "public")]
    public class MstShiftPattern : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ShiftGroupId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShiftPatternCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ShiftPatternName { get; set; } = string.Empty;

        public int CycleLengthDays { get; set; } = 1;

        [Required]
        public string PatternDefinitionJson { get; set; } = "[]";
        // Contoh JSON: [{"day":1,"shiftCode":"PAGI"},{"day":2,"shiftCode":"MALAM"}]

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public MstShiftGroup? ShiftGroup { get; set; }
    }
}
