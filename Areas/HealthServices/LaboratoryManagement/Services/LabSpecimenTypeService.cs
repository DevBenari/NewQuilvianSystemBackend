using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pengelolaan data induk jenis specimen (<c>LAB-DEC-040</c>, <c>BR-35</c>).
    ///
    /// Satu ketegangan membentuk seluruh berkas ini: <b>kosakata yang terkendali tanpa jalan
    /// buntu di meja penerimaan.</b>
    ///
    /// Daftar terkendali dibutuhkan karena teks bebas membuat "cairan kista", "Cairan Kista",
    /// dan "c. kista" terhitung tiga jenis berbeda, dan laporan jenis sampel tidak pernah dapat
    /// dipercaya. Tetapi menutup daftarnya rapat-rapat punya harga sendiri: sampel cairan kista
    /// yang datang pukul 21.00 tidak dapat diterima sampai kepala instalasi menambahkan
    /// jenisnya keesokan hari — dan sampelnya tidak menunggu, ia rusak.
    ///
    /// Jalan tengahnya adalah baris <c>Lainnya</c> yang wajib berketerangan. Sampel aneh tetap
    /// diterima, keterangannya tercatat, dan kepala instalasi dapat menaikkannya menjadi jenis
    /// tetap setelah melihat ia sering muncul. Karena itu tiga aturan menjaga baris itu:
    /// hanya satu yang boleh aktif (<c>VAL-62</c>), ia tidak boleh dinonaktifkan selama
    /// satu-satunya (<c>VAL-63</c>), dan penandanya tidak dapat disetel dari layar mana pun.
    ///
    /// <b>Tidak ada penghapusan di sini.</b> Jenis yang pernah menempel pada wadah adalah bagian
    /// riwayat penerimaan; ia dinonaktifkan, bukan dihapus. Pola ini sama dengan
    /// <see cref="LabRejectionReasonService"/>.
    /// </summary>
    public class LabSpecimenTypeService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabSpecimenTypeService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        // =================================================================
        // Baca
        // =================================================================

        /// <summary>
        /// Keterangan bentuk layar pengelolaan jenis specimen. Tidak menyentuh database.
        /// </summary>
        public LabSpecimenTypeFilterMetadataResponse GetFilterMetadata() =>
            LabFilterMetadataFactory.LabSpecimenType();

        /// <summary>
        /// Rekap jenis specimen, dihitung dari baris yang belum ditandai terhapus.
        ///
        /// Tanpa rentang waktu; ini data induk, bukan catatan kejadian. Angka
        /// <c>JalanKeluarLainnyaAktif</c> sengaja ditampilkan agar keadaan berbahaya terlihat
        /// sebagai angka: nilai <c>0</c> berarti sampel berjenis belum terdaftar akan tertahan.
        /// </summary>
        public async Task<LabSpecimenTypeSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await _dbContext.LabSpecimenTypes
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => new { x.IsActive, x.IsOtherBucket })
                .ToListAsync(cancellationToken);

            return new LabSpecimenTypeSummaryResponse
            {
                TotalJenis = rows.Count,
                Aktif = rows.Count(x => x.IsActive),
                Nonaktif = rows.Count(x => !x.IsActive),
                JalanKeluarLainnyaAktif = rows.Count(x => x.IsActive && x.IsOtherBucket)
            };
        }

        /// <summary>
        /// Daftar jenis specimen untuk layar pengelolaan. Memuat yang nonaktif juga, karena
        /// kepala instalasi perlu melihat dan dapat mengaktifkannya kembali.
        /// </summary>
        public async Task<PagedResult<LabSpecimenTypeResponse>> GetListAsync(
            LabSpecimenTypePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            var source = _dbContext.LabSpecimenTypes
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.IsActive.HasValue)
                source = source.Where(x => x.IsActive == query.IsActive.Value);

            var search = Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.SpecimenTypeCode, pattern) ||
                    EF.Functions.ILike(x.SpecimenTypeName, pattern) ||
                    (x.Description != null && EF.Functions.ILike(x.Description, pattern)));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SpecimenTypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabSpecimenTypeResponse
                {
                    Id = x.Id,
                    SpecimenTypeCode = x.SpecimenTypeCode,
                    SpecimenTypeName = x.SpecimenTypeName,
                    Description = x.Description,
                    IsOtherBucket = x.IsOtherBucket,
                    IsActive = x.IsActive,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabSpecimenTypeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Daftar pilihan untuk layar penerimaan. Berbeda dari
        /// <see cref="GetListAsync"/>: hanya jenis <b>aktif</b> yang dikembalikan, sehingga
        /// petugas tidak pernah dapat memilih jenis yang sudah ditarik dari peredaran.
        /// </summary>
        public async Task<PagedResult<LabSpecimenTypeOptionResponse>> GetOptionsAsync(
            LabSpecimenTypeOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 50 : query.PageSize;

            var source = _dbContext.LabSpecimenTypes
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive);

            var search = Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x =>
                    EF.Functions.ILike(x.SpecimenTypeCode, pattern) ||
                    EF.Functions.ILike(x.SpecimenTypeName, pattern));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SpecimenTypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabSpecimenTypeOptionResponse
                {
                    Id = x.Id,
                    SpecimenTypeCode = x.SpecimenTypeCode,
                    SpecimenTypeName = x.SpecimenTypeName,
                    IsOtherBucket = x.IsOtherBucket,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabSpecimenTypeOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Daftar pantau pemakaian <c>Lainnya</c> (<c>FR-11.2</c>, <c>LAB-DEC-040</c> butir 4-5,
        /// <c>AC-60</c>).
        ///
        /// <b>Inilah jalur yang menutup lingkaran `Lainnya`.</b> Baris jalan keluar itu ada
        /// supaya sampel berjenis belum terdaftar tetap dapat diterima. Tanpa layar ini, jalan
        /// keluar tersebut menjadi tempat pembuangan yang tidak pernah ditengok: keterangan
        /// menumpuk, tidak seorang pun tahu "cairan kista" sudah muncul dua puluh kali, dan
        /// daftar jenis specimen tidak pernah tumbuh.
        ///
        /// <b>Nol tabel ringkasan.</b> Seluruh isinya diturunkan dengan mengelompokkan
        /// <see cref="LabSpecimen"/> langsung. Tabel ringkasan adalah salinan yang bisa basi
        /// tanpa menambah satu pun jawaban baru, dan rekap yang diturunkan seperti ini berubah
        /// <b>seketika</b> begitu wadah baru dicatat.
        ///
        /// <b>Keterangannya sengaja tidak dinormalkan.</b> "cairan kista", "Cairan Kista", dan
        /// "c. kista" muncul sebagai tiga baris terpisah. Itu bukan kekurangan: kepala instalasi
        /// justru perlu <b>melihat</b> keragaman ejaannya untuk menyimpulkan ketiganya satu hal.
        /// Menggabungkannya di sini akan menyembunyikan persoalan yang menjadi alasan layar ini
        /// dibuat.
        /// </summary>
        public async Task<PagedResult<LabSpecimenOtherUsageResponse>> GetOtherUsageAsync(
            LabSpecimenOtherUsageQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

            // Rentang bawaan 90 hari. Yang dicari adalah keterangan yang BERULANG, dan
            // pengulangan tidak terlihat pada jendela satu minggu.
            var akhir = query.EndDate ?? DateTime.UtcNow;
            var awal = query.StartDate ?? akhir.AddDays(-90);

            // Waktu yang dipakai sama persis dengan rekap penerimaan wadah: waktu nyata bila
            // dicatat, waktu sistem bila tidak (LAB-DEC-042). Dua layar tentang wadah yang sama
            // tidak boleh memakai batas hari yang berbeda.
            var source = _dbContext.LabSpecimens
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.SpecimenTypeOtherNote != null &&
                    x.SpecimenType != null &&
                    x.SpecimenType.IsOtherBucket &&
                    (x.PhysicallyReceivedAt ?? x.CreateDateTime) >= awal &&
                    (x.PhysicallyReceivedAt ?? x.CreateDateTime) <= akhir);

            var search = Normalize(query.Search);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = $"%{search}%";

                source = source.Where(x => EF.Functions.ILike(x.SpecimenTypeOtherNote!, pattern));
            }

            // Wadah yang ditolak maupun dibatalkan TETAP dihitung. Yang ditanyakan layar ini
            // adalah seberapa sering sebuah keterangan dipakai, bukan berapa sampel yang lolos;
            // jenis specimen yang belum terdaftar tetap perlu didaftarkan walaupun sampelnya
            // kebetulan ditolak.
            var rekap = source
                .GroupBy(x => x.SpecimenTypeOtherNote!)
                .Select(g => new LabSpecimenOtherUsageResponse
                {
                    OtherNote = g.Key,
                    UsageCount = g.Count(),
                    LastUsedAt = g.Max(x => x.PhysicallyReceivedAt ?? x.CreateDateTime),
                    FirstUsedAt = g.Min(x => x.PhysicallyReceivedAt ?? x.CreateDateTime)
                });

            var totalData = await rekap.CountAsync(cancellationToken);

            // Yang paling sering lebih dulu — itulah yang paling pantas dinaikkan menjadi jenis
            // tetap. Pada jumlah yang sama, yang paling baru dipakai lebih dulu.
            var items = await rekap
                .OrderByDescending(x => x.UsageCount)
                .ThenByDescending(x => x.LastUsedAt)
                .ThenBy(x => x.OtherNote)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<LabSpecimenOtherUsageResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<LabSpecimenTypeResponse> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabSpecimenTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Jenis specimen tidak ditemukan.");

            return Map(entity);
        }

        // =================================================================
        // Menambah
        // =================================================================

        public async Task<LabSpecimenTypeResponse> CreateAsync(
            CreateLabSpecimenTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            var specimenTypeCode = NormalizeCode(request.SpecimenTypeCode);

            if (string.IsNullOrEmpty(specimenTypeCode))
                throw new ArgumentException("Kode jenis wajib diisi.");

            var specimenTypeName = Normalize(request.SpecimenTypeName);

            if (string.IsNullOrEmpty(specimenTypeName))
                throw new ArgumentException("Nama jenis wajib diisi.");

            // VAL-61. Diperiksa di sini supaya pemanggil menerima pesan yang berarti, sementara
            // index unik database tetap menjadi penjaga terakhir bila dua permintaan datang
            // bersamaan.
            await EnsureCodeIsFreeAsync(specimenTypeCode, cancellationToken);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new LabSpecimenType
            {
                SpecimenTypeCode = specimenTypeCode,
                SpecimenTypeName = specimenTypeName,
                Description = Normalize(request.Description),
                SortOrder = request.SortOrder,

                // VAL-62. Jenis baru selalu lahir sebagai jenis biasa. Baris `Lainnya` berasal
                // dari data awal dan tidak dapat dilahirkan lewat permintaan, karena hanya satu
                // yang boleh aktif.
                IsOtherBucket = false,

                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabSpecimenTypes.Add(entity);

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimenType.Create",
                "Menambah jenis specimen.",
                new
                {
                    entity.Id,
                    entity.SpecimenTypeCode,
                    entity.SpecimenTypeName,
                    entity.SortOrder,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Mengubah nama, keterangan, dan urutan
        // =================================================================

        public async Task<LabSpecimenTypeResponse> UpdateAsync(
            Guid id,
            UpdateLabSpecimenTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken);

            // VAL-62. Penanda `Lainnya` tidak dapat disetel dari layar mana pun. Permintaannya
            // tetap menerima ruas ini lalu menolaknya secara terbuka, karena pemanggil yang
            // mengira penandanya sudah berubah padahal tidak adalah keadaan yang justru
            // berbahaya.
            if (request.IsOtherBucket.HasValue && request.IsOtherBucket.Value != entity.IsOtherBucket)
            {
                throw new LabSpecimenTypeValidationException(
                    "Hanya boleh ada satu jenis Lainnya yang aktif.");
            }

            var specimenTypeName = Normalize(request.SpecimenTypeName);

            if (string.IsNullOrEmpty(specimenTypeName))
                throw new ArgumentException("Nama jenis wajib diisi.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.SpecimenTypeName = specimenTypeName;
            entity.Description = Normalize(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimenType.Update",
                "Mengubah jenis specimen.",
                new
                {
                    entity.Id,
                    entity.SpecimenTypeCode,
                    entity.SpecimenTypeName,
                    entity.SortOrder,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Mengaktifkan dan menonaktifkan
        // =================================================================

        public async Task<LabSpecimenTypeResponse> SetActivationAsync(
            Guid id,
            SetLabSpecimenTypeActivationRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await FindAsync(id, cancellationToken);

            if (entity.IsActive == request.IsActive)
                return Map(entity);

            if (!request.IsActive && entity.IsOtherBucket)
            {
                // VAL-63. Baris `Lainnya` adalah jalan keluar ketika jenis sampel yang datang
                // belum terdaftar. Bila ia dapat dinonaktifkan, jalan buntu di meja penerimaan
                // kembali — dan kembalinya diam-diam, lewat satu klik pada layar pengelolaan
                // yang tidak terlihat hubungannya dengan penerimaan sampel.
                var otherEscapeHatch = await _dbContext.LabSpecimenTypes
                    .AsNoTracking()
                    .AnyAsync(
                        x => !x.IsDelete && x.IsActive && x.IsOtherBucket && x.Id != entity.Id,
                        cancellationToken);

                if (!otherEscapeHatch)
                {
                    throw new LabSpecimenTypeValidationException(
                        "Jenis Lainnya harus tetap aktif, karena menjadi jalan keluar ketika jenis specimen belum terdaftar.");
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await SaveAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabSpecimenType.SetActivation",
                request.IsActive
                    ? "Mengaktifkan jenis specimen."
                    : "Menonaktifkan jenis specimen.",
                new
                {
                    entity.Id,
                    entity.SpecimenTypeCode,
                    entity.IsActive,
                    ActorUserId = actorUserId
                });

            return Map(entity);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        private async Task<LabSpecimenType> FindAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.LabSpecimenTypes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Jenis specimen tidak ditemukan.");

            return entity;
        }

        private async Task EnsureCodeIsFreeAsync(
            string specimenTypeCode,
            CancellationToken cancellationToken)
        {
            var duplicate = await _dbContext.LabSpecimenTypes
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.SpecimenTypeCode == specimenTypeCode, cancellationToken);

            if (duplicate)
            {
                throw new LabSpecimenTypeConflictException(
                    "Kode jenis ini sudah dipakai data lain, jadi tidak bisa disimpan.");
            }
        }

        private async Task SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                // Penjaga terakhir VAL-61 dan VAL-62 bila dua permintaan datang bersamaan dan
                // keduanya lolos pemeriksaan awal.
                throw new LabSpecimenTypeConflictException(
                    "Kode jenis ini sudah dipakai data lain, jadi tidak bisa disimpan.");
            }
        }

        private static bool IsUniqueViolation(DbUpdateException exception)
        {
            return exception.InnerException?.GetType().Name == "PostgresException" &&
                   exception.InnerException.Message.Contains("duplicate key value", StringComparison.OrdinalIgnoreCase);
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        /// <summary>
        /// Menormalkan kode jenis menjadi huruf kapital tanpa spasi tepi, mengikuti bentuk baris
        /// baseline seperti <c>BLOOD</c> dan <c>OTHER</c>. Tanpa normalisasi ini, "blood" dan
        /// "BLOOD" akan lolos sebagai dua kode berbeda.
        /// </summary>
        private static string NormalizeCode(string? value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static LabSpecimenTypeResponse Map(LabSpecimenType x) =>
            new()
            {
                Id = x.Id,
                SpecimenTypeCode = x.SpecimenTypeCode,
                SpecimenTypeName = x.SpecimenTypeName,
                Description = x.Description,
                IsOtherBucket = x.IsOtherBucket,
                IsActive = x.IsActive,
                SortOrder = x.SortOrder
            };
    }

    /// <summary>Pelanggaran aturan isi jenis specimen. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabSpecimenTypeValidationException(string message) : Exception(message);

    /// <summary>Bentrokan dengan baris yang sudah ada. Dipetakan menjadi <c>409</c>.</summary>
    public sealed class LabSpecimenTypeConflictException(string message) : Exception(message);
}
