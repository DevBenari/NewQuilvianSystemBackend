using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstCity", Schema = "public")]
    public class MstCity : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProvinceId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CityCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CityName { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? CityType { get; set; }

        public bool IsActive { get; set; } = true;

        public MstProvince? Province { get; set; }

        public ICollection<MstDistrict> Districts { get; set; } = new List<MstDistrict>();
    }
}
