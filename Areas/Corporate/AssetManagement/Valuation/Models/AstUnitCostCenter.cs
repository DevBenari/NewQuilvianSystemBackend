using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstUnitCostCenter", Schema = "public")]
public sealed class AstUnitCostCenter : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizationUnitId { get; set; }
    public MstOrganizationUnit? OrganizationUnit { get; set; }

    public Guid CostCenterId { get; set; }
    public MstCostCenter? CostCenter { get; set; }
}
