using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

/// <summary>
/// Permintaan pencatatan kelahiran bayi intraoperatif di meja bedah SC (BE-OPR-014, OPS-DEC-041).
/// </summary>
public class RecordBirthEventRequest
{
    [Required]
    public DateTime BirthDateTime { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(20)]
    public string Gender { get; set; } = "Male"; // Male, Female

    [Range(100, 10000, ErrorMessage = "Berat lahir bayi harus di antara 100 s.d. 10.000 gram.")]
    public decimal WeightGrams { get; set; }

    [Range(10, 100, ErrorMessage = "Panjang badan bayi harus di antara 10 s.d. 100 cm.")]
    public decimal LengthCentimeters { get; set; }

    [Range(10, 60, ErrorMessage = "Lingkar kepala bayi harus di antara 10 s.d. 60 cm.")]
    public decimal? HeadCircumferenceCm { get; set; }

    [Range(0, 10, ErrorMessage = "Skor APGAR 1 menit harus antara 0 s.d. 10.")]
    public int ApgarScore1Min { get; set; }

    [Range(0, 10, ErrorMessage = "Skor APGAR 5 menit harus antara 0 s.d. 10.")]
    public int ApgarScore5Min { get; set; }

    [Range(0, 10, ErrorMessage = "Skor APGAR 10 menit harus antara 0 s.d. 10.")]
    public int? ApgarScore10Min { get; set; }

    [MaxLength(1000)]
    public string? ResuscitationNote { get; set; }

    [MaxLength(1000)]
    public string? ConditionNote { get; set; }

    public Guid? RecordedByDoctorId { get; set; }
}

/// <summary>
/// Respons peristiwa kelahiran bayi intraoperatif (BE-OPR-014).
/// </summary>
public class OprBirthEventResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public DateTime BirthDateTime { get; set; }
    public string Gender { get; set; } = string.Empty;
    public decimal WeightGrams { get; set; }
    public decimal LengthCentimeters { get; set; }
    public decimal? HeadCircumferenceCm { get; set; }
    public int ApgarScore1Min { get; set; }
    public int ApgarScore5Min { get; set; }
    public int? ApgarScore10Min { get; set; }
    public string? ResuscitationNote { get; set; }
    public string? ConditionNote { get; set; }
    public Guid? RecordedByDoctorId { get; set; }
    public DateTime CreatedAt { get; set; }
}
