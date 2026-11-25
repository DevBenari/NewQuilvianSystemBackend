using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

[Table("AstStockTakeTeamMember", Schema = "public")]
public sealed class AstStockTakeTeamMember : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }
    public AstStockTakeSession? Session { get; set; }

    public Guid EmployeeId { get; set; }
    public MstEmployee? Employee { get; set; }

    public bool IsLeader { get; set; }
}
