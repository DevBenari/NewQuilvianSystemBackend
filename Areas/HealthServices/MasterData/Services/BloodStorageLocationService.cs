using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan master lokasi penyimpanan darah. Controller
    /// <c>BloodStorageLocationController</c> tidak menyentuh <c>ApplicationDbContext</c>
    /// sendiri, sesuai QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// Tiga aturan melekat di sini:
    ///
    /// 1. Kode dan nama lokasi sama-sama tunggal (<c>VAL-BD-067</c>). Kode dijaga index unik
    ///    database; nama dijaga pemeriksaan di service ini — lihat catatan pada
    ///    <see cref="StorageLocationNameIsUsedAsync"/> untuk batasnya.
    /// 2. <b>Menonaktifkan lokasi tidak menyentuh satu baris kantong pun.</b> Ia tidak
    ///    memindahkan kantong dan tidak mengubah status kantong mana pun (<c>DEC-BD-037</c>,
    ///    <c>INV-BD-028</c>). Yang berubah hanya ke depan.
    /// 3. Master ini tidak pernah menawarkan lokasi nonaktif lewat <c>GetOptionsAsync</c>,
    ///    sehingga layar tidak dapat memilihnya walaupun penulis layarnya lupa menyaring.
    /// </remarks>
    public class BloodStorageLocationService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private const string NotFoundMessage =
            "Lokasi penyimpanan darah tidak ditemukan atau sudah dihapus.";

        private readonly ApplicationDbContext _dbContext;

        public BloodStorageLocationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<BloodStorageLocationResponse>> GetPagedAsync(
            string? search,
            bool? isActive,
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
                    x.StorageLocationCode.ToLower().Contains(keyword) ||
                    x.StorageLocationName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var totalData = await query.CountAsync(cancellationToken);

            query = ApplySort(query, sortBy, sortDirection);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodStorageLocationResponse
                {
                    Id = x.Id,
                    StorageLocationCode = x.StorageLocationCode,
                    StorageLocationName = x.StorageLocationName,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<BloodStorageLocationResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>Angka ringkasan, termasuk penanda modul berhenti karena master kosong.</summary>
        public async Task<BloodStorageLocationSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var statuses = await BaseQuery()
                .Select(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var aktif = statuses.Count(x => x);

            return new BloodStorageLocationSummaryResponse
            {
                TotalBloodStorageLocation = statuses.Count,
                ActiveBloodStorageLocation = aktif,
                InactiveBloodStorageLocation = statuses.Count - aktif,
                IsBloodBankHaltedByEmptyActiveLocation = aktif == 0
            };
        }

        /// <summary>Pilihan lokasi <b>aktif saja</b>, untuk kotak isian layar kantong darah.</summary>
        public Task<List<BloodStorageLocationOptionResponse>> GetOptionsAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            var query = BaseQuery().Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.StorageLocationCode.ToLower().Contains(keyword) ||
                    x.StorageLocationName.ToLower().Contains(keyword));
            }

            return query
                .OrderBy(x => x.StorageLocationCode)
                .ThenBy(x => x.StorageLocationName)
                .Select(x => new BloodStorageLocationOptionResponse
                {
                    Id = x.Id,
                    StorageLocationCode = x.StorageLocationCode,
                    StorageLocationName = x.StorageLocationName
                })
                .ToListAsync(cancellationToken);
        }

        public Task<MstBloodStorageLocation?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<BloodStorageLocationResult> CreateAsync(
            CreateBloodStorageLocationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(BloodStorageLocationStatus.Invalid, validationMessage);

            var code = NormalizeCode(request.StorageLocationCode);
            var name = NormalizeText(request.StorageLocationName) ?? string.Empty;

            var duplicateMessage = await FindDuplicateAsync(code, name, excludeId: null, cancellationToken);

            if (duplicateMessage != null)
                return Failed(BloodStorageLocationStatus.DuplicateIdentity, duplicateMessage);

            var now = DateTime.UtcNow;

            var entity = new MstBloodStorageLocation
            {
                Id = Guid.NewGuid(),
                StorageLocationCode = code,
                StorageLocationName = name,
                Description = NormalizeText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstBloodStorageLocation>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir index unik kode. Barisnya dilepas dari pelacakan supaya
                // penyimpanan berikutnya tidak mencoba menyisipkannya sekali lagi.
                _dbContext.Entry(entity).State = EntityState.Detached;

                return Failed(BloodStorageLocationStatus.DuplicateIdentity, DuplicateCodeMessage());
            }

            return new BloodStorageLocationResult(
                BloodStorageLocationStatus.Success,
                entity,
                "Lokasi penyimpanan darah berhasil dibuat.");
        }

        public async Task<BloodStorageLocationResult> UpdateAsync(
            Guid id,
            UpdateBloodStorageLocationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodStorageLocationStatus.NotFound, NotFoundMessage);

            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(BloodStorageLocationStatus.Invalid, validationMessage);

            var code = NormalizeCode(request.StorageLocationCode);
            var name = NormalizeText(request.StorageLocationName) ?? entity.StorageLocationName;

            var duplicateMessage = await FindDuplicateAsync(code, name, excludeId: id, cancellationToken);

            if (duplicateMessage != null)
                return Failed(BloodStorageLocationStatus.DuplicateIdentity, duplicateMessage);

            entity.StorageLocationCode = code;
            entity.StorageLocationName = name;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Failed(BloodStorageLocationStatus.DuplicateIdentity, DuplicateCodeMessage());
            }

            return new BloodStorageLocationResult(
                BloodStorageLocationStatus.Success,
                entity,
                "Lokasi penyimpanan darah berhasil diubah.");
        }

        /// <summary>Mengaktifkan atau menonaktifkan lokasi penyimpanan.</summary>
        /// <remarks>
        /// <b>Method ini TIDAK menyentuh satu baris kantong pun.</b> Ia hanya mengubah
        /// <c>IsActive</c>. Kantong yang masih tercatat di lokasi itu tetap berada di sana
        /// dengan status yang sama persis; yang tertutup adalah gerbang alokasi dan gerbang
        /// penyimpanan ke depan (<c>DEC-BD-037</c>, <c>AC-BD-067</c>).
        ///
        /// <b>Penonaktifan tidak pernah ditolak</b> (<c>VAL-BD-068</c>). Menonaktifkan lokasi
        /// justru dilakukan ketika kulkasnya rusak — menolaknya karena masih ada kantong di
        /// dalamnya akan memaksa petugas memindahkan kantong ke lokasi yang sedang rusak.
        /// </remarks>
        public async Task<BloodStorageLocationResult> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodStorageLocationStatus.NotFound, NotFoundMessage);

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BloodStorageLocationResult(
                BloodStorageLocationStatus.Success,
                entity,
                isActive
                    ? "Lokasi penyimpanan darah berhasil diaktifkan."
                    : "Lokasi penyimpanan darah berhasil dinonaktifkan. Kantong yang masih tercatat di sana tidak berpindah dan tidak berubah status, tetapi belum dapat dialokasikan sampai dipindahkan ke lokasi yang aktif.");
        }

        /// <summary>Menandai lokasi terhapus. Tidak pernah menghapus baris secara fisik.</summary>
        /// <remarks>
        /// <b>Batas yang perlu diketahui pembaca berikutnya.</b> Standar master data menuntut
        /// penghapusan memeriksa relasi pemakainya lebih dulu. Pemakai master ini adalah
        /// <c>BbkBloodUnitPlacement</c>, yang <b>belum ada</b> di source karena dijadwalkan
        /// pada <c>BE-BD-015</c>. Pemeriksaan pemakaian karena itu ditambahkan pada task yang
        /// membuat tabel pemakainya, bukan di sini.
        ///
        /// Untuk keadaan sehari-hari, <b>menonaktifkan lebih tepat daripada menghapus</b>:
        /// penonaktifan menutup gerbang tanpa memutus makna riwayat penempatan lama yang
        /// menyebut lokasi itu.
        /// </remarks>
        public async Task<BloodStorageLocationResult> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(BloodStorageLocationStatus.NotFound, NotFoundMessage);

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new BloodStorageLocationResult(
                BloodStorageLocationStatus.Success,
                entity,
                "Lokasi penyimpanan darah berhasil dihapus.");
        }

        public static BloodStorageLocationFilterMetadataResponse BuildFilterMetadata()
            => new()
            {
                DefaultFilter = new BloodStorageLocationDefaultFilterResponse(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SortOptions = new List<BloodStorageLocationSortOptionResponse>
                {
                    new() { Value = "storageLocationCode", Label = "Kode Lokasi" },
                    new() { Value = "storageLocationName", Label = "Nama Lokasi" },
                    new() { Value = "isActive", Label = "Status Aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                QueryParameters = new List<BloodStorageLocationQueryParameterInfoResponse>
                {
                    new()
                    {
                        Name = "search",
                        Type = "string",
                        Description = "Mencari pada kode, nama, dan keterangan lokasi.",
                        Example = "Kulkas"
                    },
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description =
                            "Menyaring lokasi aktif atau nonaktif. Lokasi nonaktif tidak dapat menjadi tujuan penyimpanan maupun perpindahan.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "sortBy",
                        Type = "string",
                        Description = "Kolom pengurutan. Bawaannya storageLocationCode.",
                        Example = "storageLocationCode"
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

        public static BloodStorageLocationResponse ToResponse(MstBloodStorageLocation entity) => new()
        {
            Id = entity.Id,
            StorageLocationCode = entity.StorageLocationCode,
            StorageLocationName = entity.StorageLocationName,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreateDateTime = entity.CreateDateTime,
            UpdateDateTime = entity.UpdateDateTime
        };

        private static List<BloodStorageLocationFormFieldMetadataResponse> BuildFormFields()
            => new()
            {
                new()
                {
                    Name = "storageLocationCode",
                    Label = "Kode Lokasi",
                    Section = "Identitas",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 30,
                    Description = "Kode singkat yang dikenali petugas BDRS.",
                    Example = "KLK-BSR",
                    SortOrder = 1
                },
                new()
                {
                    Name = "storageLocationName",
                    Label = "Nama Lokasi",
                    Section = "Identitas",
                    InputType = "text",
                    IsRequiredOnCreate = true,
                    IsRequiredOnUpdate = true,
                    RequiredType = "Required",
                    MaxLength = 150,
                    Description = "Nama yang dikenali petugas, misalnya Kulkas Besar.",
                    Example = "Kulkas Besar",
                    SortOrder = 2
                },
                new()
                {
                    Name = "description",
                    Label = "Keterangan",
                    Section = "Tambahan",
                    InputType = "textarea",
                    IsRequiredOnCreate = false,
                    IsRequiredOnUpdate = false,
                    RequiredType = "Optional",
                    MaxLength = 250,
                    Example = "Kulkas darah utama di ruang BDRS lantai 1",
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
                    Description =
                        "Lokasi nonaktif tidak dapat dipilih untuk penyimpanan baru maupun perpindahan, dan menahan alokasi kantong yang masih tercatat di dalamnya. Menonaktifkan TIDAK memindahkan kantong.",
                    Example = "true",
                    SortOrder = 4
                }
            };

        private static IQueryable<MstBloodStorageLocation> ApplySort(
            IQueryable<MstBloodStorageLocation> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "storagelocationname" => descending
                    ? query.OrderByDescending(x => x.StorageLocationName)
                    : query.OrderBy(x => x.StorageLocationName),
                "isactive" => descending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.StorageLocationCode)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.StorageLocationCode),
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.StorageLocationCode)
                    : query.OrderBy(x => x.StorageLocationCode)
            };
        }

        private IQueryable<MstBloodStorageLocation> BaseQuery()
            => _dbContext.Set<MstBloodStorageLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private Task<MstBloodStorageLocation?> TrackedAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<MstBloodStorageLocation>()
                .FirstOrDefaultAsync(
                    x => x.Id == id && !x.IsDelete && !x.IsCancel,
                    cancellationToken);

        /// <summary>
        /// Memeriksa kode dan nama sekaligus, sesuai <c>VAL-BD-067</c> yang menahan keduanya.
        /// </summary>
        private async Task<string?> FindDuplicateAsync(
            string code,
            string name,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            if (await StorageLocationCodeIsUsedAsync(code, excludeId, cancellationToken))
                return DuplicateCodeMessage();

            if (await StorageLocationNameIsUsedAsync(name, excludeId, cancellationToken))
                return "Nama lokasi penyimpanan itu sudah dipakai. Gunakan nama lain.";

            return null;
        }

        private Task<bool> StorageLocationCodeIsUsedAsync(
            string code,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstBloodStorageLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.StorageLocationCode.ToLower() == code.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        /// <remarks>
        /// <b>Batas yang jujur:</b> nama lokasi <b>tidak</b> punya index unik di database,
        /// karena kamus data hanya menetapkan index unik untuk kode. Pemeriksaan ini karena itu
        /// menahan kekeliruan biasa, tetapi menyisakan celah balapan yang sangat sempit bila
        /// dua petugas menyimpan nama yang sama pada saat hampir bersamaan. Kode tetap terjaga
        /// mutlak oleh index unik. Selisih ini dicatat pada laporan task.
        /// </remarks>
        private Task<bool> StorageLocationNameIsUsedAsync(
            string name,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstBloodStorageLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.StorageLocationName.ToLower() == name.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private static string DuplicateCodeMessage()
            => "Kode lokasi penyimpanan itu sudah dipakai. Gunakan kode lain.";

        private static BloodStorageLocationResult Failed(
            BloodStorageLocationStatus status,
            string message)
            => new(status, null, message);

        private static string? ValidateRequest(CreateBloodStorageLocationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.StorageLocationCode))
                return "Kode lokasi penyimpanan wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.StorageLocationName))
                return "Nama lokasi penyimpanan wajib diisi.";

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

    public enum BloodStorageLocationStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        DuplicateIdentity = 3
    }

    public sealed record BloodStorageLocationResult(
        BloodStorageLocationStatus Status,
        MstBloodStorageLocation? Entity,
        string Message);
}
