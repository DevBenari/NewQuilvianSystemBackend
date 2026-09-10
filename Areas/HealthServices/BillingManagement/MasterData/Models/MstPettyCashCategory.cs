using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

/// <summary>
/// Kategori pengeluaran kas kecil yang dikelola Finance (PC-DES-002, PC-DEC-012).
/// Bukan pengganti dan bukan turunan MstExpenseCategory milik Corporate/HumanResource.
/// </summary>
[Table("MstPettyCashCategory", Schema = "public")]
public sealed class MstPettyCashCategory : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kode kategori yang diisi pengguna, mengikuti pola MstTaxRule.Code.</summary>
    [Required, MaxLength(30)] public string CategoryCode { get; set; } = string.Empty;

    [Required, MaxLength(100)] public string CategoryName { get; set; } = string.Empty;

    [MaxLength(300)] public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
