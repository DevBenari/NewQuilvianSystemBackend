using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Rincian item utang jasa tenaga medis (02-backend-architecture.md §A.5, data-dictionary.md §A.2).
/// Menggantikan rencana tabel FinDoctorPayableItem. Menyimpan rincian tindakan atau layanan
/// medis yang disalin dari Medical Fee tanpa dihitung ulang di Finance.
/// </summary>
[Table("FinMedicalServicePayableItem", Schema = "public")]
public sealed class FinMedicalServicePayableItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK ke induk FinMedicalServicePayable.</summary>
    public Guid PayableId { get; set; }
    public FinMedicalServicePayable? Payable { get; set; }

    /// <summary>Rujukan ke rincian hasil jasa di Medical Fee (MdfServiceFeeDetail).</summary>
    public Guid? SourceServiceFeeDetailId { get; set; }

    [Required, MaxLength(300)] public string Description { get; set; } = string.Empty;

    /// <summary>Disalin dari hasil jasa Medical Fee, tidak dihitung ulang.</summary>
    public decimal Amount { get; set; }
}
