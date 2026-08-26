using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Dtos;

public sealed class OpenShiftRequest
{
    public Guid RegisterId { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal OpeningCash { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class HandoverShiftRequest
{
    public Guid ReceivingCashierId { get; set; }
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class CloseShiftRequest
{
    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal PhysicalCash { get; set; }
    public Guid ExpectedRowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class ReviewVarianceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Resolution { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class ReopenShiftRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class CashierShiftResponse
{
    public Guid Id { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public Guid RegisterId { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal SystemCash { get; set; }
    public decimal ExpectedClosingCash { get; set; }
    public decimal PhysicalCash { get; set; }
    public decimal Variance { get; set; }
    public string Status { get; set; } = CashierShiftStatuses.Open;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid RowVersion { get; set; }
    public bool VarianceRequiresReview { get; set; }
    public string? PendingHandoverStatus { get; set; }
    public Guid? ReceivingCashierId { get; set; }
    public bool IsReplay { get; set; }
}

public sealed class CashVarianceResponse
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public Guid ReviewerId { get; set; }
    public decimal Variance { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset ReviewedAt { get; set; }
    public CashierShiftResponse Shift { get; set; } = new();
    public bool IsReplay { get; set; }
}
