using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstBank", Schema = "public")]
    public class MstBank : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string BankCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string BankName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? BankShortName { get; set; }

        public BankCategory BankCategory { get; set; } = BankCategory.Commercial;

        [MaxLength(50)]
        public string? ClearingCode { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
