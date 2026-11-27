using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

[Table("AstLoanExtension", Schema = "public")]
public sealed class AstLoanExtension : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LoanId { get; set; }
    public AstLoan? Loan { get; set; }

    public DateOnly PreviousReturnDate { get; set; }

    public DateOnly RequestedReturnDate { get; set; }

    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;

    public LoanExtensionStatus Status { get; set; } = LoanExtensionStatus.Diajukan;

    public Guid? RequestedBy { get; set; }
    public DateTime? RequestedAt { get; set; }

    public Guid? DecidedBy { get; set; }
    public DateTime? DecidedAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }
}
