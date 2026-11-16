using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstAssetValueEntry", Schema = "public")]
public sealed class AstAssetValueEntry : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public ValueEntryType EntryType { get; set; }

    public DateOnly Period { get; set; }

    public DateTime EntryDate { get; set; }

    public decimal CostDelta { get; set; }

    public decimal AccumulatedDepreciationDelta { get; set; }

    public decimal AccumulatedImpairmentDelta { get; set; }

    public Guid? CostCenterId { get; set; }
    public MstCostCenter? CostCenter { get; set; }

    [MaxLength(40)] public string? SourceDocumentType { get; set; }

    public Guid? SourceDocumentId { get; set; }

    [MaxLength(30)] public string? SourceDocumentNumber { get; set; }

    [MaxLength(500)] public string? Note { get; set; }

    public Guid ActorUserId { get; set; }
}
