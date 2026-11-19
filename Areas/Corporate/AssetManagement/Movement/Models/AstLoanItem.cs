using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

[Table("AstLoanItem", Schema = "public")]
public sealed class AstLoanItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LoanId { get; set; }
    public AstLoan? Loan { get; set; }

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public AssetCondition ConditionAtPickup { get; set; }

    public AssetCondition? ReturnCondition { get; set; }
}
