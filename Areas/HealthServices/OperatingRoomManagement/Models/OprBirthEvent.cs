using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

/// <summary>
/// Catatan peristiwa kelahiran bayi intraoperatif pada persalinan Sectio Caesarea (BE-OPR-014, OPS-DEC-041, OPS-DEC-042).
/// </summary>
[Table("OprBirthEvent", Schema = "public")]
public class OprBirthEvent : IdentityModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid OprCaseId { get; set; }

    [Required]
    public DateTime BirthDateTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string Gender { get; set; } = "Male"; // Male, Female

    [Column(TypeName = "decimal(8,2)")]
    public decimal WeightGrams { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal LengthCentimeters { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal? HeadCircumferenceCm { get; set; }

    public int ApgarScore1Min { get; set; }

    public int ApgarScore5Min { get; set; }

    public int? ApgarScore10Min { get; set; }

    [MaxLength(1000)]
    public string? ResuscitationNote { get; set; }

    [MaxLength(1000)]
    public string? ConditionNote { get; set; }

    public Guid? RecordedByDoctorId { get; set; }

    [ForeignKey(nameof(OprCaseId))]
    public OprCase? OprCase { get; set; }
}
