using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;

public class VerifyBloodInstructionRequest
{
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class BloodInstructionVerificationQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? Search { get; set; }
}

public class BloodOrderVerificationItem
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string? PatientName { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public Guid EncounterId { get; set; }
    public Guid RequestingDoctorId { get; set; }
    public string? RequestingDoctorName { get; set; }
    public Guid? InputByUserId { get; set; }
    public BbkInstructionVerificationStatus InstructionVerificationStatus { get; set; }
    public BbkBloodOrderStatus OrderStatus { get; set; }
    public DateTime CreateDateTime { get; set; }
    public int Version { get; set; }
}
