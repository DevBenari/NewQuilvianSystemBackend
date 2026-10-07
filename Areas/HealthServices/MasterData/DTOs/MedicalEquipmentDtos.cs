using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
public class CreateMedicalEquipmentRequest
{
    [Required, MaxLength(30)] public string EquipmentCode { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string EquipmentName { get; set; } = string.Empty;
    [MaxLength(100)] public string? CategoryName { get; set; }
    public MstEquipmentChargeUnit ChargeUnit { get; set; }
    public MstEquipmentRoundingRule RoundingRule { get; set; } = MstEquipmentRoundingRule.CeilingWholeUnit;
    [MaxLength(250)] public string? Description { get; set; }
}
public class UpdateMedicalEquipmentRequest : CreateMedicalEquipmentRequest { public Guid RowVersion { get; set; } }
public class MedicalEquipmentStatusRequest { public bool IsActive { get; set; } public Guid? RowVersion { get; set; } }
public class MedicalEquipmentQuery
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public class MedicalEquipmentResponse : CreateMedicalEquipmentRequest
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public Guid RowVersion { get; set; }
}
public class MedicalEquipmentOption { public Guid Id { get; set; } public string EquipmentCode { get; set; } = string.Empty; public string EquipmentName { get; set; } = string.Empty; public MstEquipmentChargeUnit ChargeUnit { get; set; } }
public class MedicalEquipmentSummary { public int Total { get; set; } public int Active { get; set; } public int Inactive { get; set; } }
public class MedicalEquipmentFilterMetadata
{
    public MedicalEquipmentQuery DefaultFilter { get; set; } = new();
    public string[] SortOptions { get; set; } = ["equipmentName", "equipmentCode", "createDateTime"];
    public string[] SortDirections { get; set; } = ["asc", "desc"];
    public int[] PageSizeOptions { get; set; } = [10, 25, 50, 100];
    public string[] QueryParameters { get; set; } = ["search", "isActive", "sortBy", "sortDirection", "pageNumber", "pageSize"];
    public object[] ChargeUnits { get; set; } = Enum.GetValues<MstEquipmentChargeUnit>().Select(x => (object)new { Value = (int)x, Label = x.ToString() }).ToArray();
    public object[] RoundingRules { get; set; } = Enum.GetValues<MstEquipmentRoundingRule>().Select(x => (object)new { Value = (int)x, Label = x.ToString() }).ToArray();
}
