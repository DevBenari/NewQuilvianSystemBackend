using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

public sealed class InpatientNewbornAdmissionContext
{
    [Required, MaxLength(20)] public string Source { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string CareMode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string CareSource { get; set; } = string.Empty;
    public bool RequiresIntensiveCare { get; set; }
    [Required, MaxLength(20)] public string FamilyBillingPreference { get; set; } = "Separate";
    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal? PlannedDepositAmount { get; set; }
}
