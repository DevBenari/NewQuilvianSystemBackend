using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class DepositTopUpRequest
{
    public Guid PaymentMethodId { get; set; }
    public Guid? PaymentMethodAccountId { get; set; }
    [MaxLength(150)] public string? ReferenceNumber { get; set; }
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }
    public Guid? ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class ReverseDepositMovementRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class DepositResponse
{
    public Guid Id { get; set; }
    public Guid EncounterId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RowVersion { get; set; }
    public IReadOnlyList<DepositMovementResponse> Movements { get; set; } = [];
}

public sealed class DepositMovementResponse
{
    public Guid Id { get; set; }
    /// <summary>Nomor transaksi tampilan, diturunkan stabil dari Id mutasi (tidak disimpan).</summary>
    public string TransactionNumber { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    /// <summary>Label jenis transaksi untuk tampilan (Deposit Awal, Top Up, Pemakaian, Refund, Koreksi).</summary>
    public string MovementTypeName { get; set; } = string.Empty;
    /// <summary>POSTED atau REVERSED (top-up yang sudah dibalik oleh mutasi REVERSAL).</summary>
    public string Status { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceEffect { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? SettlementId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public Guid? PaymentMethodAccountId { get; set; }
    public string? PaymentMethodName { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? CashierName { get; set; }
    public Guid? CashierShiftId { get; set; }
    public Guid CorrelationId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? ReversesMovementId { get; set; }
}

public sealed class DepositWorkspaceResponse
{
    // Encounter context
    public Guid EncounterId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string EncounterNumber { get; set; } = string.Empty;
    public EncounterType EncounterType { get; set; }
    public string EncounterTypeName { get; set; } = string.Empty;
    public EncounterStatus EncounterStatus { get; set; }
    public string EncounterStatusName { get; set; } = string.Empty;
    public string ServiceUnitName { get; set; } = string.Empty;
    public DateTime EncounterDate { get; set; }

    // Deposit summary
    public bool HasDepositAccount { get; set; }
    public Guid? DepositAccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public Guid? RowVersion { get; set; }
    public decimal TotalTopUpGross { get; set; }
    public decimal TotalTopUpReversed { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal TotalReleasedOrRefunded { get; set; }

    // Kelayakan top-up (aturan sama dengan yang ditegakkan BillingDepositService.TopUpAsync)
    public bool IsTopUpEligible { get; set; }
    public string? TopUpIneligibleReason { get; set; }

    // Paginated movements
    public IReadOnlyList<DepositMovementResponse> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalData { get; set; }
    public int TotalPage { get; set; }
}
