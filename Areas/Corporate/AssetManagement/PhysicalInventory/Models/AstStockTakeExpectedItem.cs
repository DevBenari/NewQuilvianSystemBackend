using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

[Table("AstStockTakeExpectedItem", Schema = "public")]
public sealed class AstStockTakeExpectedItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }
    public AstStockTakeSession? Session { get; set; }

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public Guid ExpectedLocationId { get; set; }
    public MstAssetLocation? ExpectedLocation { get; set; }

    public Guid ExpectedCustodianEmployeeId { get; set; }
    public MstEmployee? ExpectedCustodianEmployee { get; set; }

    public AssetCondition ExpectedCondition { get; set; }

    public Guid? LoanBorrowerOrganizationUnitId { get; set; }
    public MstOrganizationUnit? LoanBorrowerOrganizationUnit { get; set; }
}
