using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

/// <summary>
/// Jejak perubahan status order gizi, sekaligus penyimpan kunci idempotensi.
/// </summary>
/// <remarks>
/// <c>Source</c> menyimpan sidik jari isi permintaan dalam bentuk <c>API:{fingerprint}</c>.
/// Permintaan berulang dengan kunci yang sama tetapi isi berbeda ditolak, sehingga penekanan
/// tombol dua kali tidak menghasilkan dua tindakan, dan permintaan yang berbeda tidak
/// diam-diam dianggap sebagai pengulangan.
/// </remarks>
[Table("GzNutritionOrderHistory", Schema = "public")]
public class GzNutritionOrderHistory : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid NutritionOrderId { get; set; }

    public GzOrderStatus? FromStatus { get; set; }
    public GzOrderStatus ToStatus { get; set; }

    [Required, MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(1000)] public string? Reason { get; set; }

    [Required] public Guid ActorUserId { get; set; }
    public DateTime OccurredAt { get; set; }

    [Required, MaxLength(100)]
    public string Source { get; set; } = string.Empty;

    [MaxLength(100)] public string? CorrelationId { get; set; }

    public GzNutritionOrder? NutritionOrder { get; set; }
}
