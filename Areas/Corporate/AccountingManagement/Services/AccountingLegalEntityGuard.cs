using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services
{
    /// <summary>
    /// Penjaga jumlah badan hukum — syarat yang mengikat <c>ACC-DEC-041</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>ACC-DEC-041</c> menurunkan MVP Accounting menjadi <b>satu badan hukum</b>, dan menunda
    /// penyaringan badan hukum per pengguna karena mekanismenya tidak ada di platform
    /// (<c>ACC-DEP-008</c>, dibuktikan <c>BE-ACC-002</c>: nol klaim badan hukum pada JWT, nol
    /// <c>HasQueryFilter</c>, dan <c>LegalEntityId</c> selalu datang dari pengirim permintaan).
    /// </para>
    /// <para>
    /// Tanpa penjaga ini, keputusan tersebut menyimpan cacat yang muncul diam-diam: begitu badan
    /// hukum kedua didaftarkan lewat <c>LegalEntityController</c> yang sudah ada, <b>setiap
    /// pengguna langsung memperoleh akses ke dua buku besar sekaligus</b> tanpa ada yang
    /// menyadarinya — dan jurnal yang sudah disahkan tidak dapat dihapus (<c>ACC-DEC-015</c>),
    /// sehingga koreksinya harus lewat jurnal pembalik satu per satu.
    /// </para>
    /// <para>
    /// Karena itu ia <b>menolak keras</b>, bukan sekadar mencatat peringatan di log. Pembukuan
    /// tidak punya jalan mundur yang murah: tercampurnya dua buku besar baru ketahuan saat tutup
    /// buku.
    /// </para>
    /// <para>
    /// Ini <b>bukan</b> sistem hak akses tandingan. Ia tidak menentukan siapa berhak atas apa —
    /// ia hanya menolak berjalan pada keadaan yang belum dapat dijaga. Penyaringan yang
    /// sesungguhnya tetap milik Security/Platform lewat <c>ACC-DEP-008</c>.
    /// </para>
    /// <para>
    /// Dibuat <c>static</c> dan menerima <see cref="ApplicationDbContext"/> sebagai parameter
    /// supaya seluruh service Accounting berikutnya dapat memakainya tanpa menambah registrasi
    /// baru di <c>Program.cs</c>, sesuai <c>02-backend-architecture.md</c> bagian 6.
    /// </para>
    /// </remarks>
    public static class AccountingLegalEntityGuard
    {
        public const string PesanPenolakan =
            "Terdapat lebih dari satu badan hukum aktif, sementara penyaringan badan hukum per " +
            "pengguna belum tersedia. Modul Accounting berhenti demi mencegah pembukuan dua badan " +
            "hukum tercampur. Selesaikan ACC-DEP-008 lebih dahulu bersama Security/Platform.";

        /// <summary>
        /// Menghitung badan hukum yang masih hidup. Badan hukum nonaktif maupun yang sudah
        /// dihapus lunak tidak dihitung — keduanya tidak dapat menerima pembukuan baru.
        /// </summary>
        public static Task<int> HitungBadanHukumAktifAsync(
            ApplicationDbContext db,
            CancellationToken ct = default)
        {
            return db.Set<MstLegalEntity>()
                .Where(x => !x.IsDelete && x.IsActive)
                .CountAsync(ct);
        }

        /// <summary>
        /// Mengembalikan hasil gagal berkode <c>409</c> bila badan hukum aktif lebih dari satu,
        /// dan <c>null</c> bila aman dilanjutkan.
        /// </summary>
        /// <remarks>
        /// <c>409 Conflict</c> dipilih, bukan <c>403 Forbidden</c>. Penolakan ini bukan soal hak
        /// akses pengguna — pengguna mana pun ditolak, termasuk yang berhak penuh. Yang bentrok
        /// adalah keadaan data terhadap batas yang ditetapkan <c>ACC-DEC-041</c>.
        /// </remarks>
        public static async Task<AccountingServiceResult<T>?> PeriksaAsync<T>(
            ApplicationDbContext db,
            CancellationToken ct = default)
        {
            var jumlah = await HitungBadanHukumAktifAsync(db, ct);

            return jumlah > 1
                ? AccountingServiceResult<T>.Fail(StatusCodes.Status409Conflict, PesanPenolakan)
                : null;
        }
    }
}
