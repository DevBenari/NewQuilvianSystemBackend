using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;

public class VerifyNutritionInstructionRequest
{
    [Range(0, int.MaxValue)]
    public int ExpectedVersion { get; set; }
}

public class NutritionOrderVerificationItem
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public Guid RequesterDoctorId { get; set; }
    public string RequesterDoctorName { get; set; } = string.Empty;
    public GziInstructionVerificationStatus InstructionVerificationStatus { get; set; }
    public GziOrderStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public int Version { get; set; }
}
