using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Configuration.Services
{
    /// <summary>
    /// Pengaturan akuntansi per badan hukum: membaca dan menetapkan akun laba ditahan
    /// (<c>ACC-DEC-054</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Akun laba ditahan adalah tujuan seluruh selisih pendapatan dikurangi beban saat tutup
    /// tahun. Salah menunjuknya <b>tidak menimbulkan error</b> — jurnal penutupnya tetap seimbang,
    /// hanya labanya mendarat di akun yang keliru, dan angka itu terbawa ke tahun berikutnya
    /// sebagai saldo awal. Karena itu seluruh pemeriksaan di sini menolak keras, bukan
    /// memperingatkan.
    /// </para>
    /// <para>
    /// Penyusunan jurnal penutupnya sendiri adalah <c>BE-ACC-P2-010</c>; service ini hanya
    /// menetapkan akunnya.
    /// </para>
    /// </remarks>
    public class AccAccountingConfigurationService
    {
        private readonly ApplicationDbContext _db;

        public AccAccountingConfigurationService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Membaca pengaturan sebuah badan hukum.
        /// </summary>
        /// <remarks>
        /// Badan hukum yang belum punya pengaturan mengembalikan <c>200</c> berisi
        /// <see cref="AccountingConfigurationResponse.IsConfigured"/> bernilai salah, <b>bukan</b>
        /// <c>404</c>. Belum diisi adalah keadaan wajar bagi rumah sakit yang baru mulai memakai
        /// modul ini, dan menjadikannya error membuat layar menampilkan kegagalan untuk sesuatu
        /// yang sebenarnya normal.
        /// </remarks>
        public async Task<AccountingServiceResult<AccountingConfigurationResponse>> GetAsync(
            Guid legalEntityId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<AccountingConfigurationResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pengaturan = await AmbilAsync(legalEntityId, lacak: false, ct);

            if (pengaturan is null)
            {
                return AccountingServiceResult<AccountingConfigurationResponse>.Ok(
                    new AccountingConfigurationResponse
                    {
                        LegalEntityId = legalEntityId,
                        IsConfigured = false
                    },
                    "Akun laba ditahan belum ditetapkan pada pengaturan akuntansi.");
            }

            return AccountingServiceResult<AccountingConfigurationResponse>.Ok(
                await PetakanAsync(pengaturan, ct), "Pengaturan akuntansi berhasil diambil.");
        }

        /// <summary>
        /// Menetapkan akun laba ditahan. Membuat pengaturan bila belum ada, memperbaruinya bila
        /// sudah.
        /// </summary>
        /// <remarks>
        /// Bentuknya <c>PUT</c> yang menetapkan, bukan pasangan <c>POST</c> dan <c>PUT</c>
        /// terpisah. Alasannya acceptance (3): satu badan hukum hanya boleh punya satu
        /// pengaturan, dan bentuk menetapkan membuat aturan itu mustahil dilanggar dari sisi
        /// pemanggil. Penjaga terakhirnya tetap unique index
        /// <c>(LegalEntityId)</c> di database.
        /// </remarks>
        public async Task<AccountingServiceResult<AccountingConfigurationResponse>> SetAsync(
            Guid legalEntityId,
            UpdateAccountingConfigurationRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<AccountingConfigurationResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var akun = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.RetainedEarningsAccountId && !x.IsDelete, ct);

            var pelanggaran = PeriksaAkunLabaDitahan(akun, legalEntityId);
            if (pelanggaran is not null) return pelanggaran;

            var pengaturan = await AmbilAsync(legalEntityId, lacak: true, ct);
            var sekarang = DateTime.UtcNow;

            if (pengaturan is null)
            {
                pengaturan = new AccAccountingConfiguration
                {
                    Id = Guid.NewGuid(),
                    LegalEntityId = legalEntityId,
                    RetainedEarningsAccountId = request.RetainedEarningsAccountId,
                    IsActive = request.IsActive,
                    CreateDateTime = sekarang,
                    CreateBy = actorUserId
                };

                _db.Set<AccAccountingConfiguration>().Add(pengaturan);
            }
            else
            {
                pengaturan.RetainedEarningsAccountId = request.RetainedEarningsAccountId;
                pengaturan.IsActive = request.IsActive;
                pengaturan.UpdateDateTime = sekarang;
                pengaturan.UpdateBy = actorUserId;
            }

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<AccountingConfigurationResponse>.Ok(
                await PetakanAsync(pengaturan, ct),
                $"Akun laba ditahan ditetapkan ke {akun!.AccountCode} — {akun.AccountName}.");
        }

        // ------------------------------------------------------------------
        // Dipakai bersama BE-ACC-P2-010
        // ------------------------------------------------------------------

        /// <summary>
        /// Mengambil akun laba ditahan yang berlaku bagi sebuah badan hukum, atau <c>null</c>
        /// bila belum ditetapkan.
        /// </summary>
        /// <remarks>
        /// Dibuat <c>public static</c> menerima <see cref="ApplicationDbContext"/> supaya
        /// <c>BE-ACC-P2-010</c> memakai penilaian yang sama persis dengan endpoint ini, tanpa
        /// registrasi DI baru. Bila keduanya menilai sendiri-sendiri, tutup tahun dapat menolak
        /// dengan alasan "belum ditetapkan" sementara layar pengaturan menampilkan akun yang
        /// sudah terisi.
        /// </remarks>
        public static Task<AccChartOfAccount?> AmbilAkunLabaDitahanAsync(
            ApplicationDbContext db,
            Guid legalEntityId,
            CancellationToken ct = default)
            => db.Set<AccAccountingConfiguration>()
                .AsNoTracking()
                .Where(x => x.LegalEntityId == legalEntityId && !x.IsDelete && x.IsActive)
                .Select(x => x.RetainedEarningsAccount)
                .FirstOrDefaultAsync(ct);

        // ------------------------------------------------------------------
        // Pemeriksaan
        // ------------------------------------------------------------------

        /// <remarks>
        /// Keempat penolakan memakai <c>422</c>, bukan <c>400</c>: bentuk permintaannya benar —
        /// sebuah <c>Guid</c> yang sah — yang salah adalah akun yang ditunjuknya.
        /// </remarks>
        private static AccountingServiceResult<AccountingConfigurationResponse>? PeriksaAkunLabaDitahan(
            AccChartOfAccount? akun,
            Guid legalEntityId)
        {
            if (akun is null)
            {
                return Tolak("Akun yang ditunjuk tidak ditemukan pada daftar akun.");
            }

            // Menunjuk akun milik badan hukum lain akan membuang laba satu badan hukum ke buku
            // besar badan hukum lain — dan jurnalnya tetap seimbang, sehingga tidak ada yang
            // menandainya (ACC-DEC-037).
            if (akun.LegalEntityId != legalEntityId)
            {
                return Tolak("Akun laba ditahan harus milik badan hukum yang sama.");
            }

            // Acceptance (1).
            if (akun.AccountType != AccountType.Equity)
            {
                return Tolak("Akun laba ditahan harus akun berjenis Ekuitas.");
            }

            // Acceptance (2) — akun induk tidak menerima transaksi (ACC-DEC-022), sehingga jurnal
            // penutup tahun tidak akan pernah dapat mendarat di sana.
            if (!akun.IsPostable)
            {
                return Tolak(
                    "Akun laba ditahan harus akun yang menerima transaksi, bukan akun induk.");
            }

            // Akun nonaktif akan membuat tutup tahun gagal pada saat yang paling tidak tepat —
            // akhir tahun buku. Ditolak sekarang, saat masih murah diperbaiki.
            if (!akun.IsActive)
            {
                return Tolak("Akun laba ditahan harus akun yang masih aktif.");
            }

            return null;
        }

        private static AccountingServiceResult<AccountingConfigurationResponse> Tolak(string pesan)
            => AccountingServiceResult<AccountingConfigurationResponse>.Fail(
                StatusCodes.Status422UnprocessableEntity, pesan);

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private Task<AccAccountingConfiguration?> AmbilAsync(
            Guid legalEntityId, bool lacak, CancellationToken ct)
        {
            var q = _db.Set<AccAccountingConfiguration>().AsQueryable();
            if (!lacak) q = q.AsNoTracking();

            return q.FirstOrDefaultAsync(x => x.LegalEntityId == legalEntityId && !x.IsDelete, ct);
        }

        private async Task<AccountingConfigurationResponse> PetakanAsync(
            AccAccountingConfiguration pengaturan,
            CancellationToken ct)
        {
            var akun = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => x.Id == pengaturan.RetainedEarningsAccountId)
                .Select(x => new { x.AccountCode, x.AccountName, x.AccountType })
                .FirstOrDefaultAsync(ct);

            return new AccountingConfigurationResponse
            {
                Id = pengaturan.Id,
                LegalEntityId = pengaturan.LegalEntityId,
                IsConfigured = true,
                RetainedEarningsAccountId = pengaturan.RetainedEarningsAccountId,
                RetainedEarningsAccountCode = akun?.AccountCode,
                RetainedEarningsAccountName = akun?.AccountName,
                RetainedEarningsAccountType = akun?.AccountType,
                IsActive = pengaturan.IsActive,
                UpdateDateTime = pengaturan.UpdateDateTime
            };
        }
    }
}
