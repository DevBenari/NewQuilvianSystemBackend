using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
public sealed class StartEquipmentUsageRequest
{
    public Guid EpisodeId { get; set; }
    public Guid MedicalEquipmentId { get; set; }
    public Guid ResponsibleDoctorId { get; set; }
    public DateTime StartedAt { get; set; }
    [Range(typeof(decimal), "0.01", "999999.99")] public decimal? Quantity { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}
public sealed class FinishEquipmentUsageRequest
{
    public DateTime EndedAt { get; set; }
    [Range(typeof(decimal), "0.01", "999999.99")] public decimal? Quantity { get; set; }
    [Range(1, int.MaxValue)] public int ExpectedVersion { get; set; }
}
public sealed class CancelEquipmentUsageRequest
{
    [Required, MaxLength(500)] public string Reason { get; set; } = "";
    [Range(1, int.MaxValue)] public int ExpectedVersion { get; set; }
}
public sealed class CorrectEquipmentUsageRequest
{
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = "";
    [Range(1, int.MaxValue)] public int ExpectedVersion { get; set; }
}
public sealed class EquipmentUsageResponse
{
    public Guid Id { get; set; }
    public string EquipmentName { get; set; } = "";
    public MstEquipmentChargeUnit ChargeUnit { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? BilledUnits { get; set; }
    public CliEquipmentUsageStatus Status { get; set; }
    public string? ResponsibleDoctorName { get; set; }
    public string? PerformedByName { get; set; }
    public bool RequiresNurseReview { get; set; }
    public string ChargeState { get; set; } = "PENDING";
    public int Version { get; set; }
}
