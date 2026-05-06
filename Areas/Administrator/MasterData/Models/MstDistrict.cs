using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstDistrict", Schema = "public")]
    public class MstDistrict : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CityId { get; set; }

        [Required]
        [MaxLength(20)]
        public string DistrictCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DistrictName { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public MstCity? City { get; set; }

        public ICollection<MstPostalCode> PostalCodes { get; set; } = new List<MstPostalCode>();
    }
}
