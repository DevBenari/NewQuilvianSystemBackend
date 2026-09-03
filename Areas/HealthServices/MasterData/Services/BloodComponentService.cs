using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan katalog komponen darah. Controller
    /// <c>BloodComponentController</c> tidak menyentuh <c>ApplicationDbContext</c> sendiri,
    /// sesuai QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// Tiga aturan melekat di sini:
    ///
    /// 1. Satu komponen tidak boleh terdaftar dua kali. Kode kembar ditolak sebelum menyentuh
    ///    database, dan index unik pada <c>ComponentCode</c> menjadi penjaga terakhirnya bila
    ///    dua petugas menyimpan pada saat hampir bersamaan.
    /// 2. Masa berlaku bukti kecocokan boleh kosong, dan kekosongannya berarti gerbang
    ///    pemberian tertutup untuk komponen itu (<c>VAL-BD-020b</c>). Service ini TIDAK PERNAH
    ///    mengisinya dengan angka bawaan — menebak masa berlaku darah lebih berbahaya
    ///    daripada menahan prosesnya (<c>INV-BD-023</c>).
    /// 3. Komponen yang tidak dipakai lagi dinonaktifkan atau ditandai terhapus, bukan
    ///    dihapus fisik, supaya order darah lama yang menyebutnya tetap terbaca.
    /// </remarks>
    public class BloodComponentService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private const string NotFoundMessage = "Komponen darah tidak ditemukan atau sudah dihapus.";

        private readonly ApplicationDbContext _dbContext;

        public BloodComponentService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<BloodComponentResponse>> GetPagedAsync(
            string? search,
            bool? isActive,
            bool? isValidityConfigured,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BaseQuery();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ComponentCode.ToLower().Contains(keyword) ||
                    x.ComponentName.ToLower().Contains(keyword));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (isValidityConfigured.HasValue)
            {
                query = isValidityConfigured.Value
                    ? query.Where(x => x.CompatibilityEvidenceValidityHours != null)
                    : query.Where(x => x.CompatibilityEvidenceValidityHours == null);
            }

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySort(query, sortBy, sortDirection);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodComponentResponse
                {
                    Id = x.Id,
                    ComponentCode = x.ComponentCode,
                    ComponentName = x.ComponentName,
                    CompatibilityEvidenceValidityHours = x.CompatibilityEvidenceValidityHours,
                    IsIssuanceBlockedByMissingValidity = x.CompatibilityEvidenceValidityHours == null,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<BloodComponentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>Angka ringkasan untuk kartu statistik halaman index.</summary>
        /// <remarks>
        /// Seluruh angka dihitung hanya dari baris yang belum ditandai terhapus. Dua pencacah
        /// masa berlaku dihitung dari komponen <b>aktif</b> saja, karena komponen nonaktif
        /// memang tidak dapat dipilih dan konfigurasinya tidak mendesak.
        /// </remarks>
        public async Task<BloodComponentSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await BaseQuery()
                .Select(x => new
                {
                    x.IsActive,
                    HasValidity = x.CompatibilityEvidenceValidityHours != null
                })
                .ToListAsync(cancellationToken);

            return new BloodComponentSummaryResponse
            {
                TotalBloodComponent = rows.Count,
                ActiveBloodComponent = rows.Count(x => x.IsActive),
                InactiveBloodComponent = rows.Count(x => !x.IsActive),
                ValidityConfiguredBloodComponent = rows.Count(x => x.IsActive && x.HasValidity),
                ValidityNotConfiguredBloodComponent = rows.Count(x => x.IsActive && !x.HasValidity)
            };
        }

        /// <summary>Pilihan komponen yang aktif saja, untuk kotak isian pada layar lain.</summary>
        public Task<List<BloodComponentOptionResponse>> GetOptionsAsync(
            string? search,
            bool onlyActive,
            CancellationToken cancellationToken = default)
        {
            var query = BaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.ComponentCode.ToLower().Contains(keyword) ||
                    x.ComponentName.ToLower().Contains(keyword));
            }

            return query
                .OrderBy(x => x.ComponentCode)
                .ThenBy(x => x.ComponentName)
                .Select(x => new BloodComponentOptionResponse
                {
                    Id = x.Id,
                    ComponentCode = x.ComponentCode,
                    ComponentName = x.ComponentName,
                    CompatibilityEvidenceValidityHours = x.CompatibilityEvidenceValidityHours
                })
                .ToListAsync(cancellationToken);
        }

        public Task<MstBloodComponent?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<BloodComponentResult> CreateAsync(
            CreateBloodComponentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(BloodComponentStatus.Invalid, validationMessage);

            var componentCode = NormalizeCode(request.ComponentCode);

            if (await ComponentCodeIsUsedAsync(componentCode, excludeId: null, cancellationToken))
                return Failed(BloodComponentStatus.DuplicateCode, DuplicateCodeMessage(componentCode));

            var now = DateTime.UtcNow;

            var entity = new MstBloodComponent
            {
                Id = Guid.NewGuid(),
                ComponentCode = componentCode,
                ComponentName = NormalizeText(request.ComponentName) ?? string.Empty,
                CompatibilityEvidenceValidityHours = request.CompatibilityEvidenceValidityHours,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstBloodComponent>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir. Dua petugas yang menyimpan kode yang sama pada saat hampir
                // bersamaan sama-sama lolos pemeriksaan di atas, dan index unik di database
                // yang menolak salah satunya.
                //
                // Barisnya dilepas dari pelacakan supaya penyimpanan berikutnya pada permintaan
                // yang sama tidak mencoba menyisipkannya sekali lagi.
                _dbContext.Entry(entity).State = EntityState.Detached;

                return Failed(BloodComponentStatus.DuplicateCode, DuplicateCodeMessage(componentCode));
            }

            return new BloodComponentResult(
                BloodComponentStatus.Success,
                entity,
                "Komponen darah berhasil dibuat.");
        }

        public async Task<BloodComponentResult> UpdateAsync(
            Guid id,
            UpdateBloodComponentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodComponentStatus.NotFound, NotFoundMessage);

            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(BloodComponentStatus.Invalid, validationMessage);

            var componentCode = NormalizeCode(request.ComponentCode);

            if (await ComponentCodeIsUsedAsync(componentCode, excludeId: id, cancellationToken))
                return Failed(BloodComponentStatus.DuplicateCode, DuplicateCodeMessage(componentCode));

            entity.ComponentCode = componentCode;
            entity.ComponentName = NormalizeText(request.ComponentName) ?? entity.ComponentName;
            entity.CompatibilityEvidenceValidityHours = request.CompatibilityEvidenceValidityHours;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Failed(BloodComponentStatus.DuplicateCode, DuplicateCodeMessage(componentCode));
            }

            return new BloodComponentResult(
                BloodComponentStatus.Success,
                entity,
                "Komponen darah berhasil diubah.");
        }

        /// <remarks>
        /// Menonaktifkan komponen TIDAK menyentuh order darah maupun kantong yang sudah
        /// menyebutnya. Yang berubah hanya ke depan: ia tidak lagi muncul sebagai pilihan.
        /// </remarks>
        public async Task<BloodComponentResult> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodComponentStatus.NotFound, NotFoundMessage);

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BloodComponentResult(
                BloodComponentStatus.Success,
                entity,
                isActive
                    ? "Komponen darah berhasil diaktifkan."
                    : "Komponen darah berhasil dinonaktifkan.");
        }

        /// <summary>Menandai komponen terhapus. Tidak pernah menghapus baris secara fisik.</summary>
        /// <remarks>
        /// <b>Batas yang perlu diketahui pembaca berikutnya.</b> Standar master data menuntut
        /// penghapusan memeriksa seluruh relasi pemakainya lebih dulu. Pemakai katalog ini
        /// adalah <c>BbkBloodOrderLine</c> dan <c>BbkBloodUnit</c> — keduanya <b>belum ada</b>
        /// di source, karena dijadwalkan pada <c>BE-BD-003</c> dan <c>BE-BD-004</c>.
        ///
        /// Memeriksa tabel yang belum ada tidak mungkin dilakukan, dan mengarang pemeriksaannya
        /// akan menjadi kode mati yang menyesatkan. Karena itu pemeriksaan pemakaian
        /// <b>ditambahkan pada task yang membuat tabel pemakainya</b>, bukan di sini. Sampai
        /// saat itu, penghapusan hanya menandai <c>IsDelete</c> dan menonaktifkan barisnya,
        /// sehingga tidak ada data pemakai yang dapat menggantung.
        /// </remarks>
        public async Task<BloodComponentResult> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodComponentStatus.NotFound, NotFoundMessage);

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BloodComponentResult(
                BloodComponentStatus.Success,
                entity,
                "Komponen darah berhasil dihapus.");
        }

        public static BloodComponentFilterMetadataResponse BuildFilterMetadata()
            => new()
            {
                DefaultFilter = new BloodComponentDefaultFilterResponse(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SortOptions = new List<BloodComponentSortOptionResponse>
                {
                    new() { Value = "componentCode", Label = "Kode Komponen" },
                    new() { Value = "componentName", Label = "Nama Komponen" },
                    new() { Value = "compatibilityEvidenceValidityHours", Label = "Masa Berlaku Bukti" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                QueryParameters = new List<BloodComponentQueryParameterInfoResponse>
                {
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Mencari pada kode dan nama komponen.",
                        Example = "PRC"
                    },
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring komponen aktif atau nonaktif.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "isValidityConfigured",
                        Type = "boolean",
                        Description =
                            "Bernilai false menampilkan komponen yang masa berlaku bukti kecocokannya belum ditetapkan, sehingga pemberiannya masih tertahan.",
                        Example = "false"
                    },
                    new()
                    {
                        Name = "sortBy",
                        Type = "string",
                        Description = "Kolom pengurutan. Bawaannya componentCode.",
                        Example = "componentCode"
                    },
                    new()
                    {
                        Name = "sortDirection",
                        Type = "string",
                        Description = "Arah pengurutan, asc atau desc.",
                        Example = "asc"
                    },
                    new()
                    {
                        Name = "pageNumber",
                        Type = "integer",
                        Description = "Nomor halaman, dimulai dari 1.",
                        Example = "1"
                    },
                    new()
                    {
                        Name = "pageSize",
                        Type = "integer",
                        Description = "Jumlah baris per halaman, paling banyak 100.",
                        Example = "25"
                    }
                },
                CreateFields = BuildFormFields(),
                UpdateFields = BuildFormFields()
            };

        public static BloodComponentResponse ToResponse(MstBloodComponent entity) => new()
        {
            Id = entity.Id,
            ComponentCode = entity.ComponentCode,
            ComponentName = entity.ComponentName,
            CompatibilityEvidenceValidityHours = entity.CompatibilityEvidenceValidityHours,
            IsIssuanceBlockedByMissingValidity = entity.CompatibilityEvidenceValidityHours == null,
            IsActive = entity.IsActive,
            CreateDateTime = entity.CreateDateTime,
            UpdateDateTime = entity.UpdateDateTime
        };

        private static List<BloodComponentFormFieldMetadataResponse> BuildFormFields()
            => new()
            {
                new()
                {
                    Name = "componentCode",
                    Label = "Kode Komponen",
                    Section = "Identitas",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 20,
                    Description = "Kode singkat yang dikenali petugas Bank Darah.",
                    Example = "PRC",
                    SortOrder = 1
                },
                new()
                {
                    Name = "componentName",
                    Label = "Nama Komponen",
                    Section = "Identitas",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 100,
                    Description = "Nama lengkap komponen darah.",
                    Example = "Packed Red Cells",
                    SortOrder = 2
                },
                new()
                {
                    Name = "compatibilityEvidenceValidityHours",
                    Label = "Masa Berlaku Bukti Kecocokan (jam)",
                    Section = "Aturan Pemberian",
                    InputType = "number",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Description =
                        "Selama dikosongkan, pemberian komponen ini ditahan sampai masa berlakunya ditetapkan.",
                    Example = "72",
                    SortOrder = 3
                },
                new()
                {
                    Name = "isActive",
                    Label = "Aktif",
                    Section = "Status",
                    InputType = "switch",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    Description = "Komponen nonaktif tidak muncul sebagai pilihan pada layar lain.",
                    Example = "true",
                    SortOrder = 4
                }
            };

        private static IQueryable<MstBloodComponent> ApplySort(
            IQueryable<MstBloodComponent> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "componentname" => descending
                    ? query.OrderByDescending(x => x.ComponentName)
                    : query.OrderBy(x => x.ComponentName),
                "compatibilityevidencevalidityhours" => descending
                    ? query.OrderByDescending(x => x.CompatibilityEvidenceValidityHours)
                        .ThenBy(x => x.ComponentCode)
                    : query.OrderBy(x => x.CompatibilityEvidenceValidityHours)
                        .ThenBy(x => x.ComponentCode),
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.ComponentCode)
                    : query.OrderBy(x => x.ComponentCode)
            };
        }

        private IQueryable<MstBloodComponent> BaseQuery()
            => _dbContext.Set<MstBloodComponent>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private Task<MstBloodComponent?> TrackedAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<MstBloodComponent>()
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

        private Task<bool> ComponentCodeIsUsedAsync(
            string componentCode,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstBloodComponent>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ComponentCode.ToLower() == componentCode.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private static string DuplicateCodeMessage(string componentCode)
            => $"Kode komponen {componentCode} sudah dipakai komponen darah lain.";

        private static BloodComponentResult Failed(BloodComponentStatus status, string message)
            => new(status, null, message);

        private static string? ValidateRequest(CreateBloodComponentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ComponentCode))
                return "Kode komponen wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.ComponentName))
                return "Nama komponen wajib diisi.";

            // Nilai nol atau negatif akan membuat bukti kecocokan kedaluwarsa seketika, yang
            // secara diam-diam menutup seluruh pemberian komponen ini. Ditolak di sini supaya
            // kesalahannya terbaca sebagai kesalahan isian, bukan sebagai gerbang yang rusak.
            if (request.CompatibilityEvidenceValidityHours is <= 0)
                return "Masa berlaku bukti kecocokan harus lebih dari nol jam.";

            return null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static string NormalizeCode(string value)
            => value.Trim().ToUpperInvariant();

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public enum BloodComponentStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        DuplicateCode = 3
    }

    public sealed record BloodComponentResult(
        BloodComponentStatus Status,
        MstBloodComponent? Entity,
        string Message);
}
