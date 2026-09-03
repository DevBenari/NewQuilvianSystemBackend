using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.JournalManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.Seeders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Services
{
    /// <summary>
    /// Aturan jenis jurnal, `ACC-VALIDATION-0.2` bagian 2.
    /// </summary>
    /// <remarks>
    /// Roadmap `BE-ACC-008` mencatat "ApplicationDbContext langsung, tanpa service". Catatan itu
    /// **tidak diikuti**, dan bukan atas keputusan sepihak: roadmap yang sama menetapkan
    /// kesesuaian engineering diselesaikan dari dokumen canonical, bukan dari roadmap. Dokumen
    /// canonical — <c>BACKEND_ENGINEERING_CONTRACT.md</c> bagian *Boundary API/service* —
    /// menetapkan alur baru adalah Controller → Module Service → DbContext. Rinciannya di laporan
    /// task bagian 6.
    /// </remarks>
    public class AccJournalTypeService
    {
        private readonly ApplicationDbContext _db;

        public AccJournalTypeService(ApplicationDbContext db)
        {
            _db = db;
        }

        private const int PanjangKodeMaksimum = 10;
        private const int PanjangNamaMaksimum = 100;
        private const int PanjangAwalanMaksimum = 10;

        // ------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<PagedResult<JournalTypeResponse>>> GetPagedAsync(
            JournalTypePagedQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<JournalTypeResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            IQueryable<AccJournalType> q = _db.Set<AccJournalType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                q = q.Where(x => x.IsActive == query.IsActive.Value);

            if (query.IsSystemType.HasValue)
                q = q.Where(x => x.IsSystemType == query.IsSystemType.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var cari = query.Search.Trim().ToLower();
                q = q.Where(x => x.JournalTypeCode.ToLower().Contains(cari)
                              || x.JournalTypeName.ToLower().Contains(cari));
            }

            var menurun = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "journaltypename" => menurun ? q.OrderByDescending(x => x.JournalTypeName) : q.OrderBy(x => x.JournalTypeName),
                "createdatetime" => menurun ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => menurun ? q.OrderByDescending(x => x.JournalTypeCode) : q.OrderBy(x => x.JournalTypeCode)
            };

            var total = await q.CountAsync(ct);

            var baris = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            var items = new List<JournalTypeResponse>(baris.Count);
            foreach (var x in baris)
            {
                items.Add(await PetakanAsync(x, ct));
            }

            return AccountingServiceResult<PagedResult<JournalTypeResponse>>.Ok(
                new PagedResult<JournalTypeResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = total,
                    TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                    Items = items
                },
                "Daftar jenis jurnal berhasil diambil.");
        }

        public async Task<AccountingServiceResult<List<JournalTypeOptionResponse>>> GetOptionsAsync(
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<List<JournalTypeOptionResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var items = await _db.Set<AccJournalType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive)
                .OrderBy(x => x.JournalTypeCode)
                .Select(x => new JournalTypeOptionResponse
                {
                    Id = x.Id,
                    JournalTypeCode = x.JournalTypeCode,
                    JournalTypeName = x.JournalTypeName,
                    NumberPrefix = x.NumberPrefix,
                    RequiresApproval = x.RequiresApproval
                })
                .ToListAsync(ct);

            return AccountingServiceResult<List<JournalTypeOptionResponse>>.Ok(
                items, "Pilihan jenis jurnal berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Tulis
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<JournalTypeResponse>> CreateAsync(
            CreateJournalTypeRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalTypeResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var kode = request.JournalTypeCode?.Trim() ?? string.Empty;
            var nama = request.JournalTypeName?.Trim() ?? string.Empty;
            var awalan = request.NumberPrefix?.Trim() ?? string.Empty;

            var dasar = PeriksaIsianDasar<JournalTypeResponse>(kode, nama, awalan);
            if (dasar is not null) return dasar;

            if (await KodeTerpakaiAsync(kode, kecualiId: null, ct))
                return KodeKembar<JournalTypeResponse>();

            var jenis = new AccJournalType
            {
                Id = Guid.NewGuid(),
                JournalTypeCode = kode,
                JournalTypeName = nama,
                NumberPrefix = awalan,
                RequiresApproval = request.RequiresApproval,

                // Tanda sistem tidak pernah datang dari permintaan pengguna — lihat keterangan
                // pada CreateJournalTypeRequest.
                IsSystemType = false,

                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _db.Set<AccJournalType>().Add(jenis);
            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<JournalTypeResponse>.Ok(
                await PetakanAsync(jenis, ct),
                "Jenis jurnal berhasil ditambahkan.",
                StatusCodes.Status201Created);
        }

        /// <remarks>
        /// Acceptance (2). Jenis bertanda sistem — `JB` dan `SA` — terkunci pada **kode dan
        /// awalan nomor** saja. Nama dan keaktifannya tetap boleh disesuaikan: keduanya tidak
        /// dipakai proses pembalikan maupun saldo awal untuk menemukan jenisnya.
        /// </remarks>
        public async Task<AccountingServiceResult<JournalTypeResponse>> UpdateAsync(
            Guid id,
            UpdateJournalTypeRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalTypeResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var jenis = await _db.Set<AccJournalType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (jenis is null)
            {
                return AccountingServiceResult<JournalTypeResponse>.Fail(
                    StatusCodes.Status404NotFound, "Jenis jurnal tidak ditemukan.");
            }

            var kode = request.JournalTypeCode?.Trim() ?? string.Empty;
            var nama = request.JournalTypeName?.Trim() ?? string.Empty;
            var awalan = request.NumberPrefix?.Trim() ?? string.Empty;

            var dasar = PeriksaIsianDasar<JournalTypeResponse>(kode, nama, awalan);
            if (dasar is not null) return dasar;

            var kodeBerubah = !string.Equals(kode, jenis.JournalTypeCode, StringComparison.Ordinal);
            var awalanBerubah = !string.Equals(awalan, jenis.NumberPrefix, StringComparison.Ordinal);

            if (jenis.IsSystemType && (kodeBerubah || awalanBerubah))
            {
                return AccountingServiceResult<JournalTypeResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Jenis jurnal {jenis.JournalTypeCode} dipakai sistem dan kode maupun awalan " +
                    "nomornya tidak dapat diubah.");
            }

            if (kodeBerubah && await KodeTerpakaiAsync(kode, kecualiId: jenis.Id, ct))
                return KodeKembar<JournalTypeResponse>();

            jenis.JournalTypeCode = kode;
            jenis.JournalTypeName = nama;
            jenis.NumberPrefix = awalan;
            jenis.RequiresApproval = request.RequiresApproval;
            jenis.IsActive = request.IsActive;
            jenis.UpdateDateTime = DateTime.UtcNow;
            jenis.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            return AccountingServiceResult<JournalTypeResponse>.Ok(
                await PetakanAsync(jenis, ct), "Jenis jurnal berhasil diperbarui.");
        }

        /// <summary>
        /// Mengisi empat jenis jurnal bawaan lewat <see cref="AccountingMasterDataSeeder"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Inilah <b>call site</b> seeder `BE-ACC-006`, yang sampai sekarang belum punya pemanggil
        /// sehingga `AccJournalType` di database masih kosong (`ACC-TD-004`).
        /// </para>
        /// <para>
        /// Ditempatkan di sini, bukan di <c>Program.cs</c>, karena
        /// <c>02-backend-architecture.md</c> bagian 6 melarang pemanggilan seeder dari sana. Ia
        /// juga tidak dipanggil diam-diam dari jalur baca: mengisi data sebagai efek samping
        /// sebuah `GET` menyembunyikan siapa yang mengisinya dan kapan.
        /// </para>
        /// <para>
        /// Aman dipanggil berulang — idempotensinya dijamin seeder dan dibuktikan
        /// <c>AccountingMasterDataSeederTests</c>.
        /// </para>
        /// </remarks>
        public async Task<AccountingServiceResult<JournalTypeSeedResponse>> SeedAsync(
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<JournalTypeSeedResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var hasil = await AccountingMasterDataSeeder.SeedAsync(_db, actorUserId, ct);

            var isi = await _db.Set<AccJournalType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.JournalTypeCode)
                .ToListAsync(ct);

            var response = new JournalTypeSeedResponse
            {
                Inserted = hasil.JournalTypeInserted,
                Skipped = hasil.JournalTypeSkipped,
                SkippedReason = hasil.JournalTypeSkippedReason
            };

            foreach (var x in isi)
            {
                response.Items.Add(await PetakanAsync(x, ct));
            }

            var pesan = hasil.JournalTypeInserted > 0
                ? $"{hasil.JournalTypeInserted} jenis jurnal berhasil diisi."
                : hasil.JournalTypeSkippedReason
                  ?? "Seluruh jenis jurnal bawaan sudah ada, tidak ada yang ditambahkan.";

            return AccountingServiceResult<JournalTypeSeedResponse>.Ok(response, pesan);
        }

        // ------------------------------------------------------------------
        // Dipakai bersama task lain
        // ------------------------------------------------------------------

        /// <summary>
        /// Mencari jenis jurnal menurut kodenya. Dipakai `BE-ACC-010` untuk mengambil awalan
        /// nomor, dan `BE-ACC-013` untuk menemukan jenis `JB` saat membalik jurnal.
        /// </summary>
        /// <remarks>
        /// <c>public static</c> menerima <see cref="ApplicationDbContext"/> supaya dapat dipakai
        /// tanpa registrasi DI baru, sesuai <c>02-backend-architecture.md</c> bagian 6.
        /// </remarks>
        public static Task<AccJournalType?> CariMenurutKodeAsync(
            ApplicationDbContext db,
            string journalTypeCode,
            CancellationToken ct = default)
        {
            var kode = journalTypeCode.Trim().ToLower();

            return db.Set<AccJournalType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => !x.IsDelete && x.IsActive && x.JournalTypeCode.ToLower() == kode, ct);
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private Task<bool> KodeTerpakaiAsync(string kode, Guid? kecualiId, CancellationToken ct)
        {
            var pembanding = kode.ToLower();

            return _db.Set<AccJournalType>()
                .AnyAsync(x => !x.IsDelete
                               && (kecualiId == null || x.Id != kecualiId)
                               && x.JournalTypeCode.ToLower() == pembanding, ct);
        }

        private static AccountingServiceResult<T> KodeKembar<T>()
            => AccountingServiceResult<T>.Fail(
                StatusCodes.Status409Conflict,
                "Kode jenis jurnal wajib diisi, maksimal 10 karakter, dan belum boleh dipakai jenis lain.");

        private static AccountingServiceResult<T>? PeriksaIsianDasar<T>(
            string kode, string nama, string awalan)
        {
            if (string.IsNullOrWhiteSpace(kode) || kode.Length > PanjangKodeMaksimum)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Kode jenis jurnal wajib diisi, maksimal 10 karakter, dan belum boleh dipakai jenis lain.");
            }

            if (string.IsNullOrWhiteSpace(awalan) || awalan.Length > PanjangAwalanMaksimum)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest, "Awalan nomor jurnal wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(nama) || nama.Length > PanjangNamaMaksimum)
            {
                return AccountingServiceResult<T>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nama jenis jurnal wajib diisi dan maksimal 100 karakter.");
            }

            return null;
        }

        private async Task<JournalTypeResponse> PetakanAsync(AccJournalType x, CancellationToken ct)
        {
            return new JournalTypeResponse
            {
                Id = x.Id,
                JournalTypeCode = x.JournalTypeCode,
                JournalTypeName = x.JournalTypeName,
                NumberPrefix = x.NumberPrefix,
                RequiresApproval = x.RequiresApproval,
                IsSystemType = x.IsSystemType,
                IsActive = x.IsActive,
                HasJournals = await _db.Set<AccJournal>()
                    .AnyAsync(j => !j.IsDelete && j.JournalTypeId == x.Id, ct),
                CreateDateTime = x.CreateDateTime
            };
        }
    }
}
