using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstTariffCategory", Schema = "public")]
    public class MstTariffCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string TariffCategoryCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string TariffCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TariffGroupName { get; set; }

        public bool IsRegistrationFee { get; set; }
        public bool IsAdministrationFee { get; set; }
        public bool IsConsultationFee { get; set; }
        public bool IsRoomCharge { get; set; }
        public bool IsProcedure { get; set; }
        public bool IsLaboratory { get; set; }
        public bool IsRadiology { get; set; }
        public bool IsPharmacy { get; set; }
        public bool IsSurgery { get; set; }
        public bool IsPackage { get; set; }
        public int SortOrder { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
