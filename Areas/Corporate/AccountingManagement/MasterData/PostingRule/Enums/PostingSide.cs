using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Enums
{
    /// <summary>
    /// Sisi baris aturan posting (<c>ACC-DEC-058</c>).
    ///
    /// Baris aturan tidak membawa nominal. Nominalnya datang dari komponen kejadian yang ditunjuk
    /// <c>ComponentCode</c>, lalu dicatat pada sisi ini. Contoh: baris ber-<c>ComponentCode</c>
    /// <c>JASA_MEDIS</c> bersisi <see cref="Debit"/> menjadi baris jurnal debit sebesar nilai
    /// komponen jasa medis kejadian itu.
    ///
    /// Nilai integer mengikuti <c>02-backend-architecture.md</c> bagian 16.
    /// </summary>
    public enum PostingSide
    {
        [Display(Name = "Debit")]
        Debit = 1,

        [Display(Name = "Kredit")]
        Kredit = 2
    }
}
