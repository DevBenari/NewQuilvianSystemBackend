using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Services
{
    /// <summary>
    /// Master jenis kejadian keuangan — <c>BE-ACC-P2-017</c>, kontrak <c>ACC-API</c> grup Event
    /// Type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Isi datanya bukan urusan service ini.</b> Daftar jenis kejadian yang benar-benar
    /// diterbitkan Finance menunggu <c>DEC-ACC-P2-002</c>, jadi tidak ada seeder. Yang disediakan
    /// hanyalah tempat mengisinya.
    /// </para>
    /// <para>
    /// Aturan yang ditegakkan: kode unik tanpa membedakan huruf besar kecil, kode tidak dapat
    /// diubah sesudah dibuat, dan jenis yang masih dipakai aturan posting aktif tidak dapat
    /// dinonaktifkan. Jenis kejadian berlaku sama untuk semua badan hukum, sehingga tidak disaring
    /// per badan hukum — penjaga badan hukum utama tetap dipanggil seperti seluruh service
    /// Accounting.
    /// </para>
    /// </remarks>
    public class AccEventTypeService
    {
        private readonly ApplicationDbContext _db;

        public AccEventTypeService(ApplicationDbContext db)
        {
            _db = db;
        }

        private const int PanjangKodeMaksimum = 50;
        private const int PanjangNamaMaksimum = 200;
        private const int PanjangModulMaksimum = 50;

        // ------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<PagedResult<EventTypeListResponse>>> GetPagedAsync(
            EventTypePagedQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<EventTypeListResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            IQueryable<AccEventType> q = _db.Set<AccEventType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                q = q.Where(x => x.IsActive == query.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(query.SourceModule))
            {
                var modul = query.SourceModule.Trim().ToLower();
                q = q.Where(x => x.SourceModule.ToLower() == modul);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var cari = query.Search.Trim().ToLower();
                q = q.Where(x => x.EventTypeCode.ToLower().Contains(cari)
                              || x.EventTypeName.ToLower().Contains(cari));
            }

            var menurun = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "eventtypename" => menurun ? q.OrderByDescending(x => x.EventTypeName) : q.OrderBy(x => x.EventTypeName),
                "sourcemodule" => menurun ? q.OrderByDescending(x => x.SourceModule) : q.OrderBy(x => x.SourceModule),
                "createdatetime" => menurun ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => menurun ? q.OrderByDescending(x => x.EventTypeCode) : q.OrderBy(x => x.EventTypeCode)
            };

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new EventTypeListResponse
                {
                    Id = x.Id,
                    EventTypeCode = x.EventTypeCode,
                    EventTypeName = x.EventTypeName,
                    SourceModule = x.SourceModule,
                    IsActive = x.IsActive,
                    ActivePostingRuleCount = _db.Set<AccPostingRule>()
                        .Count(r => r.EventTypeId == x.Id && r.IsActive && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(ct);

            return AccountingServiceResult<PagedResult<EventTypeListResponse>>.Ok(
                new PagedResult<EventTypeListResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = total,
                    TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                    Items = items
                },
                "Daftar jenis kejadian berhasil diambil.");
        }

        public async Task<AccountingServiceResult<EventTypeDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<EventTypeDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jenis = await _db.Set<AccEventType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            return jenis is null
                ? TidakDitemukan<EventTypeDetailResponse>()
                : AccountingServiceResult<EventTypeDetailResponse>.Ok(
                    await PetakanRincianAsync(jenis, ct), "Rincian jenis kejadian berhasil diambil.");
        }

        public async Task<AccountingServiceResult<List<EventTypeOptionResponse>>> GetOptionsAsync(
            string? search,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<List<EventTypeOptionResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var q = _db.Set<AccEventType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cari = search.Trim().ToLower();
                q = q.Where(x => x.EventTypeCode.ToLower().Contains(cari)
                              || x.EventTypeName.ToLower().Contains(cari));
            }

            var items = await q
                .OrderBy(x => x.EventTypeCode)
                .Select(x => new EventTypeOptionResponse
                {
                    Id = x.Id,
                    EventTypeCode = x.EventTypeCode,
                    EventTypeName = x.EventTypeName,
                    SourceModule = x.SourceModule
                })
                .ToListAsync(ct);

            return AccountingServiceResult<List<EventTypeOptionResponse>>.Ok(
                items, "Pilihan jenis kejadian berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Tulis
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<EventTypeDetailResponse>> CreateAsync(
            CreateEventTypeRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<EventTypeDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var kode = request.EventTypeCode?.Trim() ?? string.Empty;
            var nama = request.EventTypeName?.Trim() ?? string.Empty;
            var modul = request.SourceModule?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(kode) || kode.Length > PanjangKodeMaksimum)
            {
                return AccountingServiceResult<EventTypeDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kode jenis kejadian wajib diisi dan maksimal 50 karakter.");
            }

            var dasar = PeriksaNamaDanModul<EventTypeDetailResponse>(nama, modul);
            if (dasar is not null) return dasar;

            var pembanding = kode.ToLower();
            var kembar = await _db.Set<AccEventType>()
                .AnyAsync(x => !x.IsDelete && x.EventTypeCode.ToLower() == pembanding, ct);

            if (kembar)
            {
                return AccountingServiceResult<EventTypeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Kode jenis kejadian {kode} sudah dipakai.");
            }

            var jenis = new AccEventType
            {
                Id = Guid.NewGuid(),
                EventTypeCode = kode,
                EventTypeName = nama,
                SourceModule = modul,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _db.Set<AccEventType>().Add(jenis);
            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<EventTypeDetailResponse>.Ok(
                await PetakanRincianAsync(jenis, ct),
                "Jenis kejadian berhasil ditambahkan.",
                StatusCodes.Status201Created);
        }

        public async Task<AccountingServiceResult<EventTypeDetailResponse>> UpdateAsync(
            Guid id,
            UpdateEventTypeRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<EventTypeDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jenis = await _db.Set<AccEventType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (jenis is null) return TidakDitemukan<EventTypeDetailResponse>();

            var nama = request.EventTypeName?.Trim() ?? string.Empty;
            var modul = request.SourceModule?.Trim() ?? string.Empty;

            var dasar = PeriksaNamaDanModul<EventTypeDetailResponse>(nama, modul);
            if (dasar is not null) return dasar;

            jenis.EventTypeName = nama;
            jenis.SourceModule = modul;
            jenis.UpdateDateTime = DateTime.UtcNow;
            jenis.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<EventTypeDetailResponse>.Ok(
                await PetakanRincianAsync(jenis, ct), "Jenis kejadian berhasil diperbarui.");
        }

        /// <summary>
        /// Menonaktifkan jenis kejadian.
        /// </summary>
        /// <remarks>
        /// Ditolak <c>409</c> selama masih ada aturan posting <b>aktif</b> yang memakainya, di badan
        /// hukum mana pun (<c>ACC-API</c> grup Event Type). Contoh: jenis <c>PENGAKUAN-PIUTANG</c>
        /// masih dipetakan satu aturan aktif milik PT Metropolitan Medical Centre — aturan itu
        /// harus dinonaktifkan lebih dahulu, supaya tidak ada aturan aktif yang menunjuk jenis
        /// yang sudah tidak diterima.
        /// </remarks>
        public async Task<AccountingServiceResult<EventTypeDetailResponse>> DeactivateAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<EventTypeDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jenis = await _db.Set<AccEventType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (jenis is null) return TidakDitemukan<EventTypeDetailResponse>();

            if (!jenis.IsActive)
            {
                return AccountingServiceResult<EventTypeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Jenis kejadian {jenis.EventTypeCode} sudah tidak aktif.");
            }

            var aturanAktif = await _db.Set<AccPostingRule>()
                .CountAsync(x => x.EventTypeId == jenis.Id && x.IsActive && !x.IsDelete, ct);

            if (aturanAktif > 0)
            {
                return AccountingServiceResult<EventTypeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Jenis kejadian {jenis.EventTypeCode} masih dipakai {aturanAktif} aturan posting "
                    + "aktif. Nonaktifkan aturan posting itu lebih dahulu.");
            }

            jenis.IsActive = false;
            jenis.UpdateDateTime = DateTime.UtcNow;
            jenis.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<EventTypeDetailResponse>.Ok(
                await PetakanRincianAsync(jenis, ct), "Jenis kejadian berhasil dinonaktifkan.");
        }

        /// <summary>
        /// Mengaktifkan kembali jenis kejadian.
        /// </summary>
        /// <remarks>
        /// Endpoint di luar enam yang tercantum kontrak — tanpanya jenis yang pernah dinonaktifkan
        /// tidak akan pernah dapat dipakai lagi. Pola yang sama dengan
        /// <c>ChartOfAccountController.Activate</c>. Dicatat sebagai delta kontrak.
        /// </remarks>
        public async Task<AccountingServiceResult<EventTypeDetailResponse>> ActivateAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<EventTypeDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jenis = await _db.Set<AccEventType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (jenis is null) return TidakDitemukan<EventTypeDetailResponse>();

            if (jenis.IsActive)
            {
                return AccountingServiceResult<EventTypeDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Jenis kejadian {jenis.EventTypeCode} sudah aktif.");
            }

            jenis.IsActive = true;
            jenis.UpdateDateTime = DateTime.UtcNow;
            jenis.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<EventTypeDetailResponse>.Ok(
                await PetakanRincianAsync(jenis, ct), "Jenis kejadian berhasil diaktifkan kembali.");
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private static AccountingServiceResult<T>? PeriksaNamaDanModul<T>(string nama, string modul)
        {
            if (string.IsNullOrWhiteSpace(nama) || nama.Length > PanjangNamaMaksimum)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama jenis kejadian wajib diisi dan maksimal 200 karakter.");
            }

            if (string.IsNullOrWhiteSpace(modul) || modul.Length > PanjangModulMaksimum)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Modul asal wajib diisi dan maksimal 50 karakter.");
            }

            return null;
        }

        private static AccountingServiceResult<T> TidakDitemukan<T>()
            => AccountingServiceResult<T>.Fail(
                StatusCodes.Status404NotFound, "Jenis kejadian tidak ditemukan atau sudah dihapus.");

        private async Task<EventTypeDetailResponse> PetakanRincianAsync(AccEventType x, CancellationToken ct)
        {
            return new EventTypeDetailResponse
            {
                Id = x.Id,
                EventTypeCode = x.EventTypeCode,
                EventTypeName = x.EventTypeName,
                SourceModule = x.SourceModule,
                IsActive = x.IsActive,
                ActivePostingRuleCount = await _db.Set<AccPostingRule>()
                    .CountAsync(r => r.EventTypeId == x.Id && r.IsActive && !r.IsDelete, ct),
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy
            };
        }
    }
}
