using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstAccountingOutbox", Schema = "public")]
public sealed class AstAccountingOutbox : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string EventTypeCode { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string SourceTransactionId { get; set; } = string.Empty;

    [Required, MaxLength(10)] public string SourceVersion { get; set; } = "1";

    public DateOnly AccountingDate { get; set; }

    public DateOnly DocumentDate { get; set; }

    public decimal Amount { get; set; }

    public Guid? CostCenterId { get; set; }

    public string? ComponentsJson { get; set; }

    public string? SubledgerJson { get; set; }

    [Required, MaxLength(40)] public string SourceDocumentType { get; set; } = string.Empty;

    public Guid SourceDocumentId { get; set; }

    public AccountingOutboxStatus Status { get; set; } = AccountingOutboxStatus.Pending;

    public int AttemptCount { get; set; } = 0;

    public DateTime? LastAttemptAt { get; set; }

    [MaxLength(1000)] public string? LastError { get; set; }

    public Guid? AccountingEventId { get; set; }
}
