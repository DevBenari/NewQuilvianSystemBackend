using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstDepreciationRunLine", Schema = "public")]
public sealed class AstDepreciationRunLine : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RunId { get; set; }
    public AstDepreciationRun? Run { get; set; }

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public Guid AssetGroupId { get; set; }
    public MstAssetGroup? AssetGroup { get; set; }

    public Guid OrganizationUnitId { get; set; }
    public MstOrganizationUnit? OrganizationUnit { get; set; }

    public Guid CostCenterId { get; set; }
    public MstCostCenter? CostCenter { get; set; }

    public decimal Amount { get; set; }

    public DepreciationAdjustmentReason AdjustmentReason { get; set; } = DepreciationAdjustmentReason.Tidak;

    public DateOnly? AdjustmentPeriod { get; set; }
}
