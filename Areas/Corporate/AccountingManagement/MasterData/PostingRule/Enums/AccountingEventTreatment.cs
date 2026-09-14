using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Enums
{
    /// <summary>
    /// Perlakuan jurnal yang dihasilkan aturan posting dari satu kejadian keuangan
    /// (<c>ACC-DEC-045</c>).
    ///
    /// Jenis kejadian bervolume tinggi yang pemetaan akunnya sudah pasti — misalnya pengakuan
    /// piutang rawat jalan — langsung menjadi jurnal <c>Posted</c>. Jenis yang jarang dan bernilai
    /// besar — misalnya penghapusan piutang — menjadi jurnal <c>Draft</c> yang menunggu pemeriksaan
    /// manusia.
    ///
    /// Diletakkan pada <c>MasterData/PostingRule</c>, bukan <c>AccountingEvent</c> seperti tertulis
    /// di <c>02-backend-architecture.md</c> bagian 17, karena pemakainya saat ini hanya aturan
    /// posting; kotak masuk kejadian belum dibangun (<c>BE-ACC-P2-015</c>).
    ///
    /// Nilai integer mengikuti <c>02-backend-architecture.md</c> bagian 16.
    /// </summary>
    public enum AccountingEventTreatment
    {
        [Display(Name = "Langsung Disahkan")]
        LangsungSahkan = 1,

        [Display(Name = "Buat Draft")]
        BuatDraft = 2
    }
}
