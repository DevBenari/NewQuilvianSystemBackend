using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceTransportAllowancePolicyResponse
    {
        public Guid Id { get; set; }

        public string PolicyCode { get; set; } = string.Empty;

        public string PolicyName { get; set; } = string.Empty;

        public string AllowanceMode { get; set; } = string.Empty;

        public decimal DefaultMonthlyAmount { get; set; }

        public decimal DefaultDailyAmount { get; set; }

        public decimal DefaultNightAmount { get; set; }

        public TimeOnly? NightStartTime { get; set; }

        public TimeOnly? NightEndTime { get; set; }

        public bool RequireAttendance { get; set; }

        public bool ExcludeIfAbsent { get; set; }

        public bool ExcludeIfLeave { get; set; }

        public bool ExcludeIfHoliday { get; set; }

        public bool IsTaxable { get; set; }

        public bool IsPayrollComponent { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTransportAllowancePolicyListResponse
    {
        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PayrollComponentData { get; set; }

        public int TaxableData { get; set; }

        public List<WorkforceTransportAllowancePolicyResponse> Items { get; set; } = new();
    }

    public class WorkforceTransportAllowancePolicyOptionResponse
    {
        public Guid Id { get; set; }

        public string PolicyCode { get; set; } = string.Empty;

        public string PolicyName { get; set; } = string.Empty;

        public string AllowanceMode { get; set; } = string.Empty;

        public decimal DefaultMonthlyAmount { get; set; }

        public decimal DefaultDailyAmount { get; set; }

        public decimal DefaultNightAmount { get; set; }
    }

    public class CreateWorkforceTransportAllowancePolicyRequest
    {
        [Required]
        [MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "DailyAttendance";

        public decimal DefaultMonthlyAmount { get; set; } = 0;

        public decimal DefaultDailyAmount { get; set; } = 0;

        public decimal DefaultNightAmount { get; set; } = 0;

        public TimeOnly? NightStartTime { get; set; }

        public TimeOnly? NightEndTime { get; set; }

        public bool RequireAttendance { get; set; } = true;

        public bool ExcludeIfAbsent { get; set; } = true;

        public bool ExcludeIfLeave { get; set; } = true;

        public bool ExcludeIfHoliday { get; set; } = false;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceTransportAllowancePolicyRequest
    {
        [Required]
        [MaxLength(50)]
        public string PolicyCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "DailyAttendance";

        public decimal DefaultMonthlyAmount { get; set; } = 0;

        public decimal DefaultDailyAmount { get; set; } = 0;

        public decimal DefaultNightAmount { get; set; } = 0;

        public TimeOnly? NightStartTime { get; set; }

        public TimeOnly? NightEndTime { get; set; }

        public bool RequireAttendance { get; set; } = true;

        public bool ExcludeIfAbsent { get; set; } = true;

        public bool ExcludeIfLeave { get; set; } = true;

        public bool ExcludeIfHoliday { get; set; } = false;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceTransportAllowancePolicyStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
