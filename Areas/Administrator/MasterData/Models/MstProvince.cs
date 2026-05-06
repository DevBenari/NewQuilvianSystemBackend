using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstProvince", Schema = "public")]
    public class MstProvince : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CountryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ProvinceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ProvinceName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public MstCountry? Country { get; set; }

        public ICollection<MstCity> Cities { get; set; } = new List<MstCity>();
    }
}
