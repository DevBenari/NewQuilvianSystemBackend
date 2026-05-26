using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstTariffCategory", Schema = "public")]
    public class MstTariffCategory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string TariffCategoryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string TariffCategoryName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? TariffGroupName { get; set; }

        public bool IsRegistrationFee { get; set; } = false;

        public bool IsAdministrationFee { get; set; } = false;

        public bool IsConsultationFee { get; set; } = false;

        public bool IsRoomCharge { get; set; } = false;

        public bool IsProcedure { get; set; } = false;

        public bool IsLaboratory { get; set; } = false;

        public bool IsRadiology { get; set; } = false;

        public bool IsPharmacy { get; set; } = false;

        public bool IsSurgery { get; set; } = false;

        public bool IsPackage { get; set; } = false;

        public bool IsCoveredByInsuranceDefault { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
