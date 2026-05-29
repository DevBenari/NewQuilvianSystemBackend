using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceTransportAllowanceTransactionResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? TransportAllowanceId { get; set; }

        public Guid? TransportAllowancePolicyId { get; set; }

        public Guid? AttendanceId { get; set; }

        public DateTime TransactionDate { get; set; }

        public string PeriodYearMonth { get; set; } = string.Empty;

        public string AllowanceType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool IsGeneratedFromAttendance { get; set; }

        public bool IsNightShift { get; set; }

        public string TransactionStatus { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTransportAllowanceTransactionListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int DraftData { get; set; }

        public int CalculatedData { get; set; }

        public int ApprovedData { get; set; }

        public int PostedToPayrollData { get; set; }

        public int CancelledData { get; set; }

        public decimal TotalAmount { get; set; }

        public List<WorkforceTransportAllowanceTransactionResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceTransportAllowanceTransactionRequest
    {
        public Guid? TransportAllowanceId { get; set; }

        public Guid? TransportAllowancePolicyId { get; set; }

        public Guid? AttendanceId { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [MaxLength(20)]
        public string? PeriodYearMonth { get; set; }

        [Required]
        [MaxLength(50)]
        public string AllowanceType { get; set; } = "Regular";

        public decimal Amount { get; set; } = 0;

        public bool IsGeneratedFromAttendance { get; set; } = false;

        public bool IsNightShift { get; set; } = false;

        [MaxLength(50)]
        public string TransactionStatus { get; set; } = "Draft";

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceTransportAllowanceTransactionStatusRequest
    {
        [Required]
        [MaxLength(50)]
        public string TransactionStatus { get; set; } = "Draft";

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
