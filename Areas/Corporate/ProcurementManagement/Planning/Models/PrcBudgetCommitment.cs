using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models
{
    /// <summary>
    /// Komitmen yang mengikat pagu — kamus data bagian 3 (BE-PBJ-010; PBJ-DEC-009).
    /// </summary>
    /// <remarks>
    /// Ditulis sub-modul lain (mis. penetapan pemenang di <c>pemilihan-penyedia</c>) hanya lewat
    /// <c>ProcurementBudgetCommitmentService</c>. Satu dokumen sumber hanya boleh punya satu komitmen
    /// aktif per garis keturunan pagu (unik parsial di basis data). Komitmen yang dilepas tidak
    /// dihapus: <see cref="IsActive"/> menjadi false dan <see cref="ReleasedAt"/> diisi.
    /// </remarks>
    [Table("PrcBudgetCommitment", Schema = "public")]
    public class PrcBudgetCommitment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary><c>PrcBudget.LineageKey</c>.</summary>
        public Guid BudgetLineageKey { get; set; }

        [Required]
        [MaxLength(40)]
        public string SourceDocumentType { get; set; } = string.Empty;

        public Guid SourceDocumentId { get; set; }

        /// <summary>SENSITIF.</summary>
        [Column(TypeName = "numeric(18,2)")]
        public decimal Amount { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset? ReleasedAt { get; set; }
    }
}
