using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.DTOs;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services
{
    /// <summary>
    /// Membaca keadaan deret nomor untuk layar pemantauan administrator (<c>PLT-BE-005</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Service ini hanya membaca, dan itu batas yang keras.</b> Tidak ada satu pun method di
    /// sini yang memanggil <c>SaveChanges</c>. Menyetel ulang atau menyunting pencacah dari layar
    /// berarti menerbitkan ulang nomor yang sudah menempel pada catatan lain
    /// (<c>INV-PLT-001</c>), dan deret yang berlubang justru keadaan sah yang tidak boleh
    /// dirapikan (<c>INV-PLT-002</c>).
    /// </para>
    /// <para>
    /// <b>Terpisah dari <see cref="NumberSeriesAllocator"/> dengan sengaja.</b> Alokator menulis
    /// pencacah lewat transaksi dan koneksinya sendiri; menumpangkan pembacaan layar ke sana akan
    /// mencampur jalur terpanas sistem dengan jalur yang dibuka administrator beberapa kali
    /// setahun.
    /// </para>
    /// <para>
    /// Seluruh query memakai <c>AsNoTracking</c>: tidak ada satu pun entity di sini yang akan
    /// disunting, sehingga melacaknya hanya menambah beban tanpa guna.
    /// </para>
    /// </remarks>
    public class NumberSeriesQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public NumberSeriesQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menyusun konfigurasi penyaring dan pengurutan halaman pemantauan.
        /// </summary>
        public NumberSeriesFilterMetadataResponse GetFilterMetadata()
        {
            return new NumberSeriesFilterMetadataResponse
            {
                DefaultFilter = new NumberSeriesDefaultFilterResponse(),
                SortOptions = new List<NumberSeriesSortOptionResponse>
                {
                    new() { Value = "sequenceKey", Label = "Penanda deret" },
                    new() { Value = "scopeKey", Label = "Periode" },
                    new() { Value = "resetPolicy", Label = "Kebijakan pengulangan" },
                    new() { Value = "currentValue", Label = "Nilai terakhir terbit" },
                    new() { Value = "lastAllocatedAt", Label = "Alokasi terakhir" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ResetPolicyOptions = NumberSeriesResetPolicies.All
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .ToList(),
                ResetButtonLabel = "Reset"
            };
        }

        /// <summary>
        /// Menghitung jumlah deret, jumlah baris, dan waktu alokasi terakhir.
        /// </summary>
        /// <remarks>
        /// <c>TotalSeries</c> menghitung <c>SequenceKey</c> yang berbeda, sedangkan
        /// <c>TotalScope</c> menghitung barisnya. Keduanya sama selama seluruh deret memakai
        /// kebijakan <c>NEVER</c>, dan mulai berbeda begitu ada deret yang diulang per periode —
        /// karena itu keduanya dilaporkan terpisah, bukan disederhanakan menjadi satu angka.
        /// </remarks>
        public async Task<NumberSeriesSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var query = BuildBaseQuery();

            var totalScope = await query.CountAsync(cancellationToken);

            var totalSeries = await query
                .Select(x => x.SequenceKey)
                .Distinct()
                .CountAsync(cancellationToken);

            // Tabel lahir kosong dan barisnya baru muncul pada alokasi pertama, sehingga
            // ketiadaan baris adalah keadaan sah yang dilaporkan sebagai null, bukan sebagai
            // tanggal minimum yang menyesatkan.
            //
            // Nilai terbesar dicari di sisi klien, bukan lewat MAX atau ORDER BY di database.
            // Alasannya mekanis: SQLite yang dipakai uji menolak DateTimeOffset pada kedua
            // bentuk itu, sehingga keduanya membuat jalur ini mustahil dibuktikan di luar
            // PostgreSQL — bentuk kegagalan yang sudah menahan PLT-BE-004.
            //
            // Ongkosnya sadar dan terbatas: satu kolom milik seluruh baris deret ditarik ke
            // memori. Tabel ini berisi satu baris per deret per periode — puluhan, bukan
            // jutaan — dan layar ini dibuka administrator beberapa kali setahun. Bila kelak
            // jumlah deret tumbuh jauh di luar dugaan, pindahkan kolomnya ke DateTime agar
            // agregat dapat diterjemahkan kembali.
            DateTimeOffset? lastAllocatedAt = totalScope == 0
                ? null
                : (await query
                        .Select(x => x.LastAllocatedAt)
                        .ToListAsync(cancellationToken))
                    .Max();

            return new NumberSeriesSummaryResponse
            {
                TotalSeries = totalSeries,
                TotalScope = totalScope,
                LastAllocatedAt = lastAllocatedAt
            };
        }

        /// <summary>
        /// Menyusun daftar deret beserta nilai pencacahnya.
        /// </summary>
        public async Task<PagedResult<NumberSeriesResponse>> GetPagedAsync(
            NumberSeriesPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new NumberSeriesPagedQuery();

            var (pageNumber, pageSize) = NormalizePaging(query.PageNumber, query.PageSize);

            var filtered = ApplyFilter(BuildBaseQuery(), query);

            var totalData = await filtered.CountAsync(cancellationToken);

            var descending = string.Equals(
                query.SortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            List<NumberSeriesResponse> items;

            if (IsLastAllocatedAtSort(query.SortBy))
            {
                // Satu-satunya pengurutan yang tidak dapat diserahkan ke database: SQLite yang
                // dipakai uji menolak DateTimeOffset pada ORDER BY. Barisnya ditarik lebih dulu,
                // lalu diurutkan dan dipotong di memori.
                //
                // Batasnya sama dengan pada ringkasan: tabel ini berisi satu baris per deret per
                // periode, sehingga jumlahnya puluhan. Empat pengurutan lain tetap dikerjakan
                // database beserta pagingnya, jadi ongkos ini hanya dibayar ketika administrator
                // benar-benar mengurutkan menurut waktu alokasi.
                var semua = await filtered.ToListAsync(cancellationToken);

                items = (descending
                        ? semua
                            .OrderByDescending(x => x.LastAllocatedAt)
                            .ThenBy(x => x.SequenceKey, StringComparer.Ordinal)
                        : semua
                            .OrderBy(x => x.LastAllocatedAt)
                            .ThenBy(x => x.SequenceKey, StringComparer.Ordinal))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapResponse)
                    .ToList();
            }
            else
            {
                items = await ApplySorting(filtered, query.SortBy, query.SortDirection)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => MapResponse(x))
                    .ToListAsync(cancellationToken);
            }

            return new PagedResult<NumberSeriesResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Mengambil detail satu deret. Memulangkan <c>null</c> ketika barisnya tidak ada,
        /// sehingga controller yang memutuskan bentuk <c>404</c>-nya.
        /// </summary>
        public async Task<NumberSeriesResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => MapResponse(x))
                .FirstOrDefaultAsync(cancellationToken);
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private IQueryable<NumNumberSeries> BuildBaseQuery()
            => _dbContext.NumNumberSeries.AsNoTracking();

        private static IQueryable<NumNumberSeries> ApplyFilter(
            IQueryable<NumNumberSeries> query,
            NumberSeriesPagedQuery filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.SequenceKey))
            {
                var sequenceKey = filter.SequenceKey.Trim();
                query = query.Where(x => x.SequenceKey == sequenceKey);
            }

            if (!string.IsNullOrWhiteSpace(filter.ScopeKey))
            {
                var scopeKey = filter.ScopeKey.Trim();
                query = query.Where(x => x.ScopeKey == scopeKey);
            }

            if (!string.IsNullOrWhiteSpace(filter.ResetPolicy))
            {
                var resetPolicy = filter.ResetPolicy.Trim();
                query = query.Where(x => x.ResetPolicy == resetPolicy);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var keyword = filter.Search.Trim().ToLower();

                query = query.Where(x =>
                    x.SequenceKey.ToLower().Contains(keyword) ||
                    x.ScopeKey.ToLower().Contains(keyword));
            }

            return query;
        }

        /// <summary>
        /// Menandai pengurutan menurut waktu alokasi, satu-satunya yang dikerjakan di sisi klien.
        /// </summary>
        private static bool IsLastAllocatedAtSort(string? sortBy)
            => string.Equals(
                (sortBy ?? string.Empty).Trim(),
                "lastAllocatedAt",
                StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Mengurutkan daftar di database. Bawaannya <c>SequenceKey</c> lalu <c>ScopeKey</c>
        /// supaya periode milik satu deret berkumpul dan terbaca berurutan.
        /// </summary>
        /// <remarks>
        /// <c>lastAllocatedAt</c> <b>tidak</b> ditangani di sini — ia dikerjakan
        /// <see cref="GetPagedAsync"/> di sisi klien karena SQLite menolak <c>DateTimeOffset</c>
        /// pada <c>ORDER BY</c>. Menambahkannya kembali ke sini akan mengembalikan kegagalan itu.
        /// </remarks>
        private static IQueryable<NumNumberSeries> ApplySorting(
            IQueryable<NumNumberSeries> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "scopekey" => descending
                    ? query.OrderByDescending(x => x.ScopeKey).ThenBy(x => x.SequenceKey)
                    : query.OrderBy(x => x.ScopeKey).ThenBy(x => x.SequenceKey),
                "resetpolicy" => descending
                    ? query.OrderByDescending(x => x.ResetPolicy).ThenBy(x => x.SequenceKey)
                    : query.OrderBy(x => x.ResetPolicy).ThenBy(x => x.SequenceKey),
                "currentvalue" => descending
                    ? query.OrderByDescending(x => x.CurrentValue).ThenBy(x => x.SequenceKey)
                    : query.OrderBy(x => x.CurrentValue).ThenBy(x => x.SequenceKey),
                _ => descending
                    ? query.OrderByDescending(x => x.SequenceKey).ThenByDescending(x => x.ScopeKey)
                    : query.OrderBy(x => x.SequenceKey).ThenBy(x => x.ScopeKey)
            };
        }

        private static NumberSeriesResponse MapResponse(NumNumberSeries entity)
            => new()
            {
                Id = entity.Id,
                SequenceKey = entity.SequenceKey,
                ScopeKey = entity.ScopeKey,
                ResetPolicy = entity.ResetPolicy,
                CurrentValue = entity.CurrentValue,
                LastAllocatedAt = entity.LastAllocatedAt
            };

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }
    }
}
