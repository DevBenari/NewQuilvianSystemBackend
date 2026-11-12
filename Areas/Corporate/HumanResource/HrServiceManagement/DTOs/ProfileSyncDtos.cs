using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs
{
    public class ProfileSyncResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? ProfileChangeHistoryId { get; set; }
        public string ComponentModified { get; set; } = string.Empty;
        public string OldValuesJson { get; set; } = "{}";
        public string NewValuesJson { get; set; } = "{}";
        public DateTime AppliedAtUtc { get; set; }
    }

    public class BankAccountChangePayloadDto
    {
        [Required]
        public string BankName { get; set; } = string.Empty;

        [Required]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        public string AccountHolderName { get; set; } = string.Empty;

        public string? BankBranch { get; set; }
        public string AccountType { get; set; } = "Savings";
        public string CurrencyCode { get; set; } = "IDR";
        public string? Notes { get; set; }
    }

    public class PersonalInfoChangePayloadDto
    {
        public string? MaritalStatus { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Email { get; set; }
        public string? CurrentAddress { get; set; }
        public string? Notes { get; set; }
    }

    public class ProfileChangeHistoryItemDto
    {
        public Guid Id { get; set; }
        public Guid HrServiceRequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ComponentModified { get; set; } = string.Empty;
        public string OldValuesJson { get; set; } = "{}";
        public string NewValuesJson { get; set; } = "{}";
        public Guid ApprovedByUserId { get; set; }
        public string ApprovedByUserName { get; set; } = string.Empty;
        public DateTime AppliedAtUtc { get; set; }
    }
}
