using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

[Table("AstStockTakeFollowUp", Schema = "public")]
public sealed class AstStockTakeFollowUp : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }
    public AstStockTakeSession? Session { get; set; }

    public Guid ResultId { get; set; }
    public AstStockTakeResult? Result { get; set; }

    public StockTakeFollowUpType Type { get; set; }

    public StockTakeFollowUpStatus Status { get; set; } = StockTakeFollowUpStatus.Usulan;

    [MaxLength(40)] public string? TargetDocumentType { get; set; }

    public Guid? TargetDocumentId { get; set; }

    [MaxLength(30)] public string? TargetDocumentNumber { get; set; }

    public Guid? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public Guid? DismissedBy { get; set; }
    public DateTime? DismissedAt { get; set; }

    [MaxLength(500)] public string? DismissReason { get; set; }
}
