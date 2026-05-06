using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstCountry", Schema = "public")]
    public class MstCountry : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(20)]
        public string CountryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CountryName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? PhoneCode { get; set; }

        public bool IsDefault { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public ICollection<MstProvince> Provinces { get; set; } = new List<MstProvince>();
    }
}
