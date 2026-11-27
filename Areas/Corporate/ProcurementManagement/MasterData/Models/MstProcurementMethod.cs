using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models
{
    /// <summary>
    /// Metode pengadaan beserta aturannya (BE-PBJ-PS-001; PBJ-DEC-068). Contoh: "Penunjukan Langsung" untuk HPS
    /// paket sampai Rp 50.000.000, cukup satu penawaran, tanpa evaluasi teknis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Rentang HPS.</b> Metode berlaku bila <c>MinHpsAmount &lt; HPS ≤ MaxHpsAmount</c>; batas yang kosong berarti
    /// tanpa batas di sisi itu. Contoh nilai awal: Penunjukan Langsung (batas bawah kosong, batas atas 50 juta) dan
    /// Pembandingan Penawaran (batas bawah 50 juta, batas atas kosong) — HPS tepat Rp 50.000.000 jatuh ke Penunjukan
    /// Langsung. Rentang dua metode aktif tidak boleh tumpang tindih; aturan itu dijaga service master (PBJ-DEC-095).
    /// </para>
    /// <para>
    /// <b>Nilai contoh.</b> <see cref="IsSample"/> = true hanya untuk baris dari seeder (PBJ-DEC-069); admin
    /// menggantinya dengan metode SOP (PBJ-OQ-005) sebelum produksi. Paket menyalin metode dan aturannya saat dipilih,
    /// sehingga perubahan master tidak mengubah paket yang sudah berjalan.
    /// </para>
    /// <para>
    /// Urutan tampil mengikuti batas bawah HPS lalu nama; tidak ada <c>SortOrder</c> tersimpan (QBE: urutan bisnis
    /// memakai field semantik).
    /// </para>
    /// </remarks>
    [Table("MstProcurementMethod", Schema = "public")]
    public class MstProcurementMethod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Huruf besar, angka, garis bawah. Contoh: <c>PENUNJUKAN_LANGSUNG</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string MethodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string MethodName { get; set; } = string.Empty;

        /// <summary>Batas bawah HPS (eksklusif), termasuk PPN (PBJ-DEC-084). Kosong = tanpa batas bawah.</summary>
        public decimal? MinHpsAmount { get; set; }

        /// <summary>Batas atas HPS (inklusif), termasuk PPN (PBJ-DEC-084). Kosong = tanpa batas atas.</summary>
        public decimal? MaxHpsAmount { get; set; }

        /// <summary>Jumlah minimum penawaran sah sebelum penetapan boleh diajukan (1–10).</summary>
        public int MinimumQuotationCount { get; set; } = 1;

        public bool RequiresTechnicalEvaluation { get; set; }

        /// <summary>True hanya untuk nilai contoh dari seeder (PBJ-DEC-069).</summary>
        public bool IsSample { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
