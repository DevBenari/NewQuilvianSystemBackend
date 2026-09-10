using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models
{
    [Table("MstPaymentMethodAccount", Schema = "public")]
    public class MstPaymentMethodAccount : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PaymentMethodId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Purpose { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public MstPaymentMethod PaymentMethod { get; set; } = null!;
    }
}
