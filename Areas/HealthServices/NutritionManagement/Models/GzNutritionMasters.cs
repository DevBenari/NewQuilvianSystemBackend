using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

/// <summary>
/// Jenis diet yang berlaku di rumah sakit, misalnya diet rendah garam atau diet diabetes.
/// </summary>
/// <remarks>
/// Master ini dibuat KOSONG dan diisi admin lewat layar. Nama diet berbeda antar rumah
/// sakit; mengisinya dengan daftar karangan menghasilkan master yang terlihat resmi padahal
/// tidak pernah disahkan siapa pun, dan diet yang salah menempel pada rekam medis pasien.
/// </remarks>
[Table("GzDietType", Schema = "public")]
public class GzDietType : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string DietTypeCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string DietTypeName { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }

    /// <summary>Diet yang membutuhkan perhatian khusus, ditandai agar terlihat di dapur.</summary>
    public bool IsSpecialDiet { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Bentuk makanan yang disajikan, misalnya biasa, lunak, saring, atau cair.
/// </summary>
[Table("GzFoodForm", Schema = "public")]
public class GzFoodForm : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string FoodFormCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string FoodFormName { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Jadwal makan harian, misalnya makan pagi, siang, sore, dan selingan.
/// </summary>
/// <remarks>
/// Jam makan berbeda antar rumah sakit, karena itu master ini juga dibuat kosong.
/// <c>ServingTime</c> dipakai mengurutkan tampilan dan merekap kebutuhan produksi.
/// </remarks>
[Table("GzMealSchedule", Schema = "public")]
public class GzMealSchedule : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string MealScheduleCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string MealScheduleName { get; set; } = string.Empty;

    /// <summary>Jam penyajian, dipakai mengurutkan dan mengelompokkan produksi.</summary>
    public TimeOnly ServingTime { get; set; }

    /// <summary>Membedakan makan utama dari selingan.</summary>
    public bool IsMainMeal { get; set; } = true;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
