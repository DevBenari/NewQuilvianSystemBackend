using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Models
{
    [Table("MstPostalCode", Schema = "public")]
    public class MstPostalCode : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid DistrictId { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? VillageName { get; set; }

        public bool IsActive { get; set; } = true;

        public MstDistrict? District { get; set; }
    }
}
