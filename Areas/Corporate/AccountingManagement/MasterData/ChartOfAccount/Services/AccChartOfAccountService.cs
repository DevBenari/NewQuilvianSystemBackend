using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Services
{
    /// <summary>
    /// Aturan daftar akun. Seluruh aturan `ACC-VALIDATION-0.2` bagian 1 ditegakkan di sini,
    /// bukan di controller, supaya `BE-ACC-010` dapat memakainya kembali saat memvalidasi baris
    /// jurnal.
    /// </summary>
    public class AccChartOfAccountService
    {
        private readonly ApplicationDbContext _db;

        public AccChartOfAccountService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Tingkat akun yang diizinkan `ACC-VALIDATION-0.2` bagian 1.</summary>
        private const int TingkatMinimum = 1;
        private const int TingkatMaksimum = 5;

        // ------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<PagedResult<ChartOfAccountListResponse>>> GetPagedAsync(
            ChartOfAccountPagedQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<ChartOfAccountListResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            IQueryable<AccChartOfAccount> q = _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.LegalEntityId.HasValue)
                q = q.Where(x => x.LegalEntityId == query.LegalEntityId.Value);

            if (query.AccountType.HasValue)
                q = q.Where(x => x.AccountType == query.AccountType.Value);

            if (query.IsActive.HasValue)
                q = q.Where(x => x.IsActive == query.IsActive.Value);

            if (query.IsPostable.HasValue)
                q = q.Where(x => x.IsPostable == query.IsPostable.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var cari = query.Search.Trim().ToLower();
                q = q.Where(x => x.AccountCode.ToLower().Contains(cari)
                              || x.AccountName.ToLower().Contains(cari));
            }

            var menurun = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "accountname" => menurun ? q.OrderByDescending(x => x.AccountName) : q.OrderBy(x => x.AccountName),
                "accountlevel" => menurun ? q.OrderByDescending(x => x.AccountLevel) : q.OrderBy(x => x.AccountLevel),
                "createdatetime" => menurun ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => menurun ? q.OrderByDescending(x => x.AccountCode) : q.OrderBy(x => x.AccountCode)
            };

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ChartOfAccountListResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    NormalBalance = x.NormalBalance,
                    AccountLevel = x.AccountLevel,
                    ParentAccountId = x.ParentAccountId,
                    ParentAccountCode = x.ParentAccount != null ? x.ParentAccount.AccountCode : null,
                    IsPostable = x.IsPostable,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);

            var hasil = new PagedResult<ChartOfAccountListResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };

            return AccountingServiceResult<PagedResult<ChartOfAccountListResponse>>.Ok(
                hasil, "Daftar akun berhasil diambil.");
        }

        public async Task<AccountingServiceResult<ChartOfAccountDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<ChartOfAccountDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var akun = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Include(x => x.ParentAccount)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (akun is null)
                return TidakDitemukan<ChartOfAccountDetailResponse>();

            return AccountingServiceResult<ChartOfAccountDetailResponse>.Ok(
                await PetakanRincianAsync(akun, ct), "Rincian akun berhasil diambil.");
        }

        public async Task<AccountingServiceResult<List<ChartOfAccountTreeResponse>>> GetTreeAsync(
            Guid legalEntityId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<List<ChartOfAccountTreeResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var semua = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.LegalEntityId == legalEntityId)
                .OrderBy(x => x.AccountCode)
                .Select(x => new
                {
                    x.Id,
                    x.AccountCode,
                    x.AccountName,
                    x.AccountType,
                    x.NormalBalance,
                    x.AccountLevel,
                    x.IsPostable,
                    x.IsActive,
                    x.ParentAccountId
                })
                .ToListAsync(ct);

            var simpul = semua.ToDictionary(
                x => x.Id,
                x => new ChartOfAccountTreeResponse
                {
                    Id = x.Id,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    NormalBalance = x.NormalBalance,
                    AccountLevel = x.AccountLevel,
                    IsPostable = x.IsPostable,
                    IsActive = x.IsActive
                });

            var akar = new List<ChartOfAccountTreeResponse>();

            foreach (var baris in semua)
            {
                // Induk yang menunjuk ke luar badan hukum ini tidak akan ditemukan, dan
                // simpulnya sengaja diperlakukan sebagai akar supaya tidak hilang dari susunan.
                if (baris.ParentAccountId.HasValue
                    && simpul.TryGetValue(baris.ParentAccountId.Value, out var induk))
                {
                    induk.Children.Add(simpul[baris.Id]);
                }
                else
                {
                    akar.Add(simpul[baris.Id]);
                }
            }

            return AccountingServiceResult<List<ChartOfAccountTreeResponse>>.Ok(
                akar, "Susunan akun berhasil diambil.");
        }

        /// <remarks>
        /// Hanya akun yang menerima transaksi dan aktif. Dengan begitu petugas tidak pernah
        /// melihat akun induk pada daftar pilihan, dan `ACC-DEC-022` terjaga sejak di layar —
        /// bukan hanya saat penyimpanan.
        /// </remarks>
        public async Task<AccountingServiceResult<List<ChartOfAccountOptionResponse>>> GetOptionsAsync(
            Guid legalEntityId,
            string? search,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<List<ChartOfAccountOptionResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            IQueryable<AccChartOfAccount> q = _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.LegalEntityId == legalEntityId
                            && x.IsActive
                            && x.IsPostable);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cari = search.Trim().ToLower();
                q = q.Where(x => x.AccountCode.ToLower().Contains(cari)
                              || x.AccountName.ToLower().Contains(cari));
            }

            var items = await q
                .OrderBy(x => x.AccountCode)
                .Select(x => new ChartOfAccountOptionResponse
                {
                    Id = x.Id,
                    AccountCode = x.AccountCode,
                    AccountName = x.AccountName,
                    AccountType = x.AccountType,
                    NormalBalance = x.NormalBalance,
                    RequiresCostCenter = x.AccountType == AccountType.Expense
                })
                .ToListAsync(ct);

            return AccountingServiceResult<List<ChartOfAccountOptionResponse>>.Ok(
                items, "Pilihan akun berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Tulis
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<ChartOfAccountDetailResponse>> CreateAsync(
            CreateChartOfAccountRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<ChartOfAccountDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var kode = request.AccountCode?.Trim() ?? string.Empty;
            var nama = request.AccountName?.Trim() ?? string.Empty;

            var dasar = PeriksaIsianDasar<ChartOfAccountDetailResponse>(kode, nama, request.AccountLevel);
            if (dasar is not null) return dasar;

            var badanHukumAda = await _db.Set<MstLegalEntity>()
                .AnyAsync(x => x.Id == request.LegalEntityId && !x.IsDelete, ct);

            if (!badanHukumAda)
            {
                return AccountingServiceResult<ChartOfAccountDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Badan hukum tidak ditemukan.");
            }

            var kodeTerpakai = await _db.Set<AccChartOfAccount>()
                .AnyAsync(x => !x.IsDelete
                               && x.LegalEntityId == request.LegalEntityId
                               && x.AccountCode == kode, ct);

            if (kodeTerpakai) return KodeKembar<ChartOfAccountDetailResponse>(kode);

            var induk = await PeriksaIndukAsync<ChartOfAccountDetailResponse>(
                request.ParentAccountId, request.LegalEntityId, idSaatIni: null, ct);
            if (induk.Gagal is not null) return induk.Gagal;

            // Akun yang sudah bertransaksi tidak boleh diberi turunan: saldonya sudah melekat
            // pada akun itu sendiri, dan menambah anak akan membuat saldo induk tidak lagi
            // dapat dijelaskan dari turunannya.
            if (induk.Akun is not null)
            {
                var indukBertransaksi = await PunyaBarisJurnalDisahkanAsync(induk.Akun.Id, ct);
                if (indukBertransaksi)
                {
                    return AccountingServiceResult<ChartOfAccountDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        $"Akun {induk.Akun.AccountCode} sudah memiliki transaksi, sehingga tidak dapat diberi akun turunan.");
                }
            }

            var akun = new AccChartOfAccount
            {
                Id = Guid.NewGuid(),
                LegalEntityId = request.LegalEntityId,
                AccountCode = kode,
                AccountName = nama,
                AccountType = request.AccountType,
                NormalBalance = request.NormalBalance,
                ParentAccountId = request.ParentAccountId,
                AccountLevel = request.AccountLevel,
                IsPostable = request.IsPostable,
                Description = request.Description?.Trim(),
                EffectiveStartDate = request.EffectiveStartDate,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            // Akun baru belum punya turunan, sehingga aturan "akun induk tidak menerima
            // transaksi" tidak dapat dilanggar di sini. Ia baru menggigit saat akun ini
            // kelak diberi anak, dan itu dijaga pada CreateAsync anak tersebut.
            _db.Set<AccChartOfAccount>().Add(akun);
            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<ChartOfAccountDetailResponse>.Ok(
                await PetakanRincianAsync(akun, ct),
                "Akun berhasil ditambahkan.",
                StatusCodes.Status201Created);
        }

        public async Task<AccountingServiceResult<ChartOfAccountDetailResponse>> UpdateAsync(
            Guid id,
            UpdateChartOfAccountRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<ChartOfAccountDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var akun = await _db.Set<AccChartOfAccount>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (akun is null) return TidakDitemukan<ChartOfAccountDetailResponse>();

            var kode = request.AccountCode?.Trim() ?? string.Empty;
            var nama = request.AccountName?.Trim() ?? string.Empty;

            var dasar = PeriksaIsianDasar<ChartOfAccountDetailResponse>(kode, nama, request.AccountLevel);
            if (dasar is not null) return dasar;

            var bertransaksi = await PunyaBarisJurnalDisahkanAsync(akun.Id, ct);

            // Acceptance (4). Hanya berlaku bila kodenya benar-benar berubah — menyimpan ulang
            // akun bertransaksi tanpa mengubah kode tetap boleh.
            if (bertransaksi && !string.Equals(kode, akun.AccountCode, StringComparison.Ordinal))
            {
                return AccountingServiceResult<ChartOfAccountDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Kode akun tidak dapat diubah karena sudah dipakai pada jurnal yang disahkan.");
            }

            if (!string.Equals(kode, akun.AccountCode, StringComparison.Ordinal))
            {
                var kodeTerpakai = await _db.Set<AccChartOfAccount>()
                    .AnyAsync(x => !x.IsDelete
                                   && x.Id != akun.Id
                                   && x.LegalEntityId == akun.LegalEntityId
                                   && x.AccountCode == kode, ct);

                if (kodeTerpakai) return KodeKembar<ChartOfAccountDetailResponse>(kode);
            }

            var induk = await PeriksaIndukAsync<ChartOfAccountDetailResponse>(
                request.ParentAccountId, akun.LegalEntityId, akun.Id, ct);
            if (induk.Gagal is not null) return induk.Gagal;

            // Acceptance (2). Diperiksa dari keadaan tersimpan, bukan dari isian, karena
            // turunannya dapat dibuat orang lain sesudah layar ini dibuka.
            if (request.IsPostable)
            {
                var punyaAnak = await _db.Set<AccChartOfAccount>()
                    .AnyAsync(x => !x.IsDelete && x.ParentAccountId == akun.Id, ct);

                if (punyaAnak)
                {
                    return AccountingServiceResult<ChartOfAccountDetailResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Akun induk tidak dapat menerima transaksi. Gunakan akun turunannya.");
                }
            }

            akun.AccountCode = kode;
            akun.AccountName = nama;
            akun.ParentAccountId = request.ParentAccountId;
            akun.AccountLevel = request.AccountLevel;
            akun.IsPostable = request.IsPostable;
            akun.Description = request.Description?.Trim();
            akun.EffectiveStartDate = request.EffectiveStartDate;
            akun.UpdateDateTime = DateTime.UtcNow;
            akun.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<ChartOfAccountDetailResponse>.Ok(
                await PetakanRincianAsync(akun, ct), "Akun berhasil diperbarui.");
        }

        /// <remarks>
        /// Acceptance (3). Saldo dihitung **hanya** dari baris jurnal yang jurnalnya berstatus
        /// <c>Posted</c>. Jurnal `Draft` atau `PendingApproval` belum menjadi transaksi; ikut
        /// menghitungnya akan mengunci akun yang sebenarnya masih bebas dinonaktifkan.
        /// </remarks>
        public async Task<AccountingServiceResult<ChartOfAccountDetailResponse>> DeactivateAsync(
            Guid id,
            DeactivateChartOfAccountRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<ChartOfAccountDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var akun = await _db.Set<AccChartOfAccount>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (akun is null) return TidakDitemukan<ChartOfAccountDetailResponse>();

            var saldo = await HitungSaldoAsync(akun.Id, ct);

            if (saldo != 0m)
            {
                return AccountingServiceResult<ChartOfAccountDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Akun masih bersaldo Rp {Math.Abs(saldo):N0} dan tidak dapat dinonaktifkan. " +
                    "Pindahkan saldonya lebih dahulu lewat jurnal.");
            }

            akun.IsActive = false;
            akun.UpdateDateTime = DateTime.UtcNow;
            akun.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request.Reason))
            {
                akun.Description = request.Reason.Trim();
            }

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<ChartOfAccountDetailResponse>.Ok(
                await PetakanRincianAsync(akun, ct), "Akun berhasil dinonaktifkan.");
        }

        public async Task<AccountingServiceResult<ChartOfAccountDetailResponse>> ActivateAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<ChartOfAccountDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var akun = await _db.Set<AccChartOfAccount>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (akun is null) return TidakDitemukan<ChartOfAccountDetailResponse>();

            akun.IsActive = true;
            akun.UpdateDateTime = DateTime.UtcNow;
            akun.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<ChartOfAccountDetailResponse>.Ok(
                await PetakanRincianAsync(akun, ct), "Akun berhasil diaktifkan kembali.");
        }

        // ------------------------------------------------------------------
        // Dipakai bersama task lain
        // ------------------------------------------------------------------

        /// <summary>
        /// Saldo akun dari baris jurnal yang **disahkan** saja. Positif berarti condong debit.
        /// </summary>
        /// <remarks>
        /// Dibuat <c>public static</c> menerima <see cref="ApplicationDbContext"/> supaya
        /// `BE-ACC-012` (buku besar) dan `BE-ACC-010` dapat memakainya tanpa registrasi baru,
        /// sesuai `02-backend-architecture.md` bagian 6.
        /// </remarks>
        public static async Task<decimal> HitungSaldoAsync(
            ApplicationDbContext db,
            Guid accountId,
            CancellationToken ct = default)
        {
            var baris = await db.Set<AccJournalLine>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.AccountId == accountId
                            && db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                             && !j.IsDelete
                                                             && j.JournalStatus == JournalStatus.Posted))
                .Select(x => new { x.DebitAmount, x.CreditAmount })
                .ToListAsync(ct);

            return baris.Sum(x => x.DebitAmount - x.CreditAmount);
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private Task<decimal> HitungSaldoAsync(Guid accountId, CancellationToken ct)
            => HitungSaldoAsync(_db, accountId, ct);

        private Task<bool> PunyaBarisJurnalDisahkanAsync(Guid accountId, CancellationToken ct)
        {
            return _db.Set<AccJournalLine>()
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete
                               && x.AccountId == accountId
                               && _db.Set<AccJournal>().Any(j => j.Id == x.JournalId
                                                                 && !j.IsDelete
                                                                 && j.JournalStatus == JournalStatus.Posted), ct);
        }

        private static AccountingServiceResult<T> TidakDitemukan<T>()
            => AccountingServiceResult<T>.Fail(StatusCodes.Status404NotFound, "Akun tidak ditemukan.");

        private static AccountingServiceResult<T> KodeKembar<T>(string kode)
            => AccountingServiceResult<T>.Fail(
                StatusCodes.Status409Conflict,
                $"Kode akun {kode} sudah dipakai pada badan hukum ini.");

        private static AccountingServiceResult<T>? PeriksaIsianDasar<T>(
            string kode, string nama, int tingkat)
        {
            if (string.IsNullOrWhiteSpace(kode) || kode.Length > 20)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kode akun wajib diisi dan maksimal 20 karakter.");
            }

            if (string.IsNullOrWhiteSpace(nama) || nama.Length > 200)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama akun wajib diisi dan maksimal 200 karakter.");
            }

            if (tingkat < TingkatMinimum || tingkat > TingkatMaksimum)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Tingkat akun harus antara {TingkatMinimum} sampai {TingkatMaksimum}.");
            }

            return null;
        }

        /// <summary>
        /// Memeriksa induk: ada, badan hukum sama, bukan dirinya sendiri, dan tidak membentuk
        /// lingkaran.
        /// </summary>
        private async Task<(AccountingServiceResult<T>? Gagal, AccChartOfAccount? Akun)> PeriksaIndukAsync<T>(
            Guid? parentAccountId,
            Guid legalEntityId,
            Guid? idSaatIni,
            CancellationToken ct)
        {
            if (!parentAccountId.HasValue) return (null, null);

            if (idSaatIni.HasValue && parentAccountId.Value == idSaatIni.Value)
            {
                return (AccountingServiceResult<T>.Fail(
                    StatusCodes.Status409Conflict,
                    "Akun tidak dapat menjadi induk bagi dirinya sendiri."), null);
            }

            var induk = await _db.Set<AccChartOfAccount>()
                .FirstOrDefaultAsync(x => x.Id == parentAccountId.Value && !x.IsDelete, ct);

            if (induk is null)
            {
                return (AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest, "Akun induk tidak ditemukan."), null);
            }

            if (induk.LegalEntityId != legalEntityId)
            {
                return (AccountingServiceResult<T>.Fail(
                    StatusCodes.Status409Conflict,
                    "Akun induk harus berasal dari badan hukum yang sama."), null);
            }

            // Lingkaran tidak langsung: A -> B -> A. Ditelusuri ke atas dari calon induk;
            // bila bertemu akun yang sedang disunting, rantainya melingkar.
            if (idSaatIni.HasValue)
            {
                var penelusur = induk;
                var pagar = 0;

                while (penelusur?.ParentAccountId is not null && pagar++ < TingkatMaksimum + 1)
                {
                    if (penelusur.ParentAccountId.Value == idSaatIni.Value)
                    {
                        return (AccountingServiceResult<T>.Fail(
                            StatusCodes.Status409Conflict,
                            "Akun tidak dapat menjadi induk bagi dirinya sendiri."), null);
                    }

                    var indukId = penelusur.ParentAccountId.Value;
                    penelusur = await _db.Set<AccChartOfAccount>()
                        .FirstOrDefaultAsync(x => x.Id == indukId && !x.IsDelete, ct);
                }
            }

            return (null, induk);
        }

        private async Task<ChartOfAccountDetailResponse> PetakanRincianAsync(
            AccChartOfAccount akun,
            CancellationToken ct)
        {
            var induk = akun.ParentAccountId.HasValue
                ? await _db.Set<AccChartOfAccount>()
                    .AsNoTracking()
                    .Where(x => x.Id == akun.ParentAccountId.Value)
                    .Select(x => new { x.AccountCode, x.AccountName })
                    .FirstOrDefaultAsync(ct)
                : null;

            return new ChartOfAccountDetailResponse
            {
                Id = akun.Id,
                LegalEntityId = akun.LegalEntityId,
                AccountCode = akun.AccountCode,
                AccountName = akun.AccountName,
                AccountType = akun.AccountType,
                NormalBalance = akun.NormalBalance,
                AccountLevel = akun.AccountLevel,
                ParentAccountId = akun.ParentAccountId,
                ParentAccountCode = induk?.AccountCode,
                ParentAccountName = induk?.AccountName,
                IsPostable = akun.IsPostable,
                IsActive = akun.IsActive,
                Description = akun.Description,
                EffectiveStartDate = akun.EffectiveStartDate,
                HasChildAccounts = await _db.Set<AccChartOfAccount>()
                    .AnyAsync(x => !x.IsDelete && x.ParentAccountId == akun.Id, ct),
                HasPostedJournalLines = await PunyaBarisJurnalDisahkanAsync(akun.Id, ct),
                RequiresCostCenter = akun.AccountType == AccountType.Expense,
                CreateDateTime = akun.CreateDateTime
            };
        }
    }
}
