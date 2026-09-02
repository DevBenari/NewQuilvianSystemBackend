using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services
{
    /// <summary>
    /// Penjaga badan hukum Accounting — syarat yang mengikat <c>ACC-DEC-041</c>, dengan mekanisme
    /// yang disempurnakan <c>ACC-DEC-043</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>ACC-DEC-041</c> menurunkan MVP Accounting menjadi <b>satu badan hukum</b>, dan menunda
    /// penyaringan badan hukum per pengguna karena mekanismenya tidak ada di platform
    /// (<c>ACC-DEP-008</c>, dibuktikan <c>BE-ACC-002</c>: nol klaim badan hukum pada JWT, nol
    /// <c>HasQueryFilter</c>, dan <c>LegalEntityId</c> selalu datang dari pengirim permintaan).
    /// </para>
    /// <para>
    /// <b>Kenapa mekanismenya berubah.</b> Versi pertama penjaga ini menolak bila badan hukum
    /// aktif lebih dari satu. Saat diperiksa terhadap database sungguhan pada 2 September 2026,
    /// ternyata sudah ada <b>tiga</b> badan hukum aktif yang dibuat modul lain — dan hanya satu,
    /// <c>LE-MMC-001</c>, yang benar-benar dipakai: ia satu-satunya yang punya unit organisasi,
    /// cost center, dan lokasi kerja, sekaligus satu-satunya bertanda <see cref="MstLegalEntity.IsDefault"/>.
    /// Dua lainnya kosong.
    /// </para>
    /// <para>
    /// Menolak berdasarkan jumlah akan mematikan Accounting tanpa alasan yang sebenarnya, karena
    /// bahaya yang dijaga bukanlah "ada lebih dari satu badan hukum di master", melainkan
    /// <b>ketidakjelasan buku besar mana yang sedang disentuh</b>. <c>IsDefault</c> menjawab
    /// pertanyaan itu, dan ia kolom platform yang sudah ada — bukan konsep baru yang dikarang
    /// Accounting.
    /// </para>
    /// <para>
    /// Karena itu penjaga ini menuntut <b>tepat satu</b> badan hukum bertanda default. Nol default
    /// berarti tidak ada yang dapat ditunjuk; lebih dari satu berarti ambigu. Keduanya ditolak
    /// keras — pembukuan tidak punya jalan mundur yang murah, dan tercampurnya dua buku besar baru
    /// ketahuan saat tutup buku, sementara jurnal yang sudah disahkan tidak dapat dihapus
    /// (<c>ACC-DEC-015</c>).
    /// </para>
    /// <para>
    /// Ini <b>bukan</b> sistem hak akses tandingan. Ia tidak menentukan siapa berhak atas apa —
    /// ia hanya memastikan hanya ada satu buku besar yang mungkin disentuh. Penyaringan per
    /// pengguna tetap milik Security/Platform lewat <c>ACC-DEP-008</c>.
    /// </para>
    /// <para>
    /// Dibuat <c>static</c> dan menerima <see cref="ApplicationDbContext"/> sebagai parameter
    /// supaya seluruh service Accounting berikutnya dapat memakainya tanpa menambah registrasi
    /// baru di <c>Program.cs</c>, sesuai <c>02-backend-architecture.md</c> bagian 6.
    /// </para>
    /// </remarks>
    public static class AccountingLegalEntityGuard
    {
        public const string PesanTanpaDefault =
            "Belum ada badan hukum bertanda utama (IsDefault). Modul Accounting tidak dapat " +
            "menentukan buku besar mana yang harus dipakai. Tetapkan satu badan hukum utama pada " +
            "master badan hukum lebih dahulu.";

        public const string PesanDefaultGanda =
            "Terdapat lebih dari satu badan hukum bertanda utama (IsDefault), sementara " +
            "penyaringan badan hukum per pengguna belum tersedia. Modul Accounting berhenti demi " +
            "mencegah pembukuan dua badan hukum tercampur. Sisakan satu badan hukum utama, atau " +
            "selesaikan ACC-DEP-008 lebih dahulu bersama Security/Platform.";

        /// <summary>
        /// Badan hukum yang menjadi tumpuan seluruh pembukuan Accounting selama
        /// <c>ACC-DEP-008</c> belum selesai. <c>null</c> bila tidak tepat satu.
        /// </summary>
        public static async Task<Guid?> AmbilBadanHukumUtamaAsync(
            ApplicationDbContext db,
            CancellationToken ct = default)
        {
            var utama = await db.Set<MstLegalEntity>()
                .Where(x => !x.IsDelete && x.IsActive && x.IsDefault)
                .Select(x => x.Id)
                .Take(2)
                .ToListAsync(ct);

            return utama.Count == 1 ? utama[0] : null;
        }

        /// <summary>
        /// Mengembalikan hasil gagal berkode <c>409</c> bila badan hukum utama tidak tepat satu,
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
            var jumlahDefault = await db.Set<MstLegalEntity>()
                .Where(x => !x.IsDelete && x.IsActive && x.IsDefault)
                .Take(2)
                .CountAsync(ct);

            if (jumlahDefault == 0)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status409Conflict, PesanTanpaDefault);
            }

            return jumlahDefault > 1
                ? AccountingServiceResult<T>.Fail(StatusCodes.Status409Conflict, PesanDefaultGanda)
                : null;
        }
    }
}
