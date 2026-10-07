using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan master butir persiapan bedah (<c>BE-RWI-173</c>,
    /// kontrak <c>0.10.0</c> API 11.4). Controller <c>SurgicalPreparationItemController</c> tidak
    /// menyentuh <c>ApplicationDbContext</c> sendiri, sesuai QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// Empat aturan melekat di sini:
    ///
    /// 1. Kode butir unik di antara butir yang belum dihapus. Kode kembar ditolak <c>409</c>
    ///    <c>MST-SPI-001</c>; index unik parsial <c>UX_MstSurgicalPreparationItem_Code</c> menjadi
    ///    penjaga terakhir bila dua admin menyimpan hampir bersamaan.
    /// 2. Ubah memakai <c>RowVersion</c>. Admin A dan B membuka butir yang sama; A menyimpan lebih
    ///    dulu sehingga <c>RowVersion</c> berganti; simpanan B ditolak <c>409</c> alih-alih
    ///    menimpa perubahan A tanpa ada yang tahu.
    /// 3. Butir nonaktif tidak ikut tersalin ke versi Catatan Pra-Operasi yang baru, tetapi versi
    ///    lama tetap utuh karena nama dan sifat wajibnya sudah disalin ke butir versi.
    /// 4. Butir yang sudah dipakai versi pra-operasi mana pun tidak dapat dihapus — cukup
    ///    dinonaktifkan — supaya riwayat pra-operasi tetap dapat dirunut ke definisinya.
    /// </remarks>
    public class SurgicalPreparationItemService
    {
        /// <summary>Kode galat kontrak API 11.4 untuk kode butir yang sudah dipakai.</summary>
        public const string DuplicateCodeErrorCode = "MST-SPI-001";

        /// <summary>
        /// Empat kelompok baku <c>RWI-DEC-173</c> butir 3. Dipakai sebagai pilihan bawaan form dan
        /// urutan kelompok; backend tidak menolak kelompok lain yang sengaja dibuat admin.
        /// </summary>
        public static readonly IReadOnlyList<string> StandardGroupNames = new[]
        {
            "Verifikasi pasien",
            "Persiapan fisik",
            "Hasil pemeriksaan",
            "Persiapan lain"
        };

        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private const string NotFoundMessage = "Butir persiapan bedah tidak ditemukan atau sudah dihapus.";

        private const string VersionConflictMessage =
            "Butir ini sudah diubah pengguna lain sejak Anda membukanya. Muat ulang datanya, lalu ulangi perubahan Anda.";

        private readonly ApplicationDbContext _dbContext;

        public SurgicalPreparationItemService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<SurgicalPreparationItemResponse>> GetPagedAsync(
            string? search,
            string? groupName,
            bool? isActive,
            bool? isMandatory,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = ApplyFilters(BaseQuery(), search, groupName, isActive, isMandatory);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySort(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SurgicalPreparationItemResponse
                {
                    Id = x.Id,
                    Code = x.Code,
                    GroupName = x.GroupName,
                    ItemName = x.ItemName,
                    IsMandatory = x.IsMandatory,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    RowVersion = x.RowVersion,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<SurgicalPreparationItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>Angka ringkasan dari baris yang belum ditandai terhapus.</summary>
        public async Task<SurgicalPreparationItemSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await BaseQuery()
                .Select(x => new { x.IsActive, x.IsMandatory, x.GroupName })
                .ToListAsync(cancellationToken);

            return new SurgicalPreparationItemSummaryResponse
            {
                TotalSurgicalPreparationItem = rows.Count,
                ActiveSurgicalPreparationItem = rows.Count(x => x.IsActive),
                InactiveSurgicalPreparationItem = rows.Count(x => !x.IsActive),
                ActiveMandatorySurgicalPreparationItem = rows.Count(x => x.IsActive && x.IsMandatory),
                ActiveOptionalSurgicalPreparationItem = rows.Count(x => x.IsActive && !x.IsMandatory),
                ActiveGroupCount = rows
                    .Where(x => x.IsActive)
                    .Select(x => x.GroupName.Trim().ToLowerInvariant())
                    .Distinct()
                    .Count()
            };
        }

        /// <summary>
        /// Pilihan butir, aktif saja secara bawaan, diurutkan menurut kelompok baku lalu urutan
        /// tampil. Bentuk ini juga yang dipakai saat menyusun butir versi pra-operasi baru.
        /// </summary>
        public async Task<List<SurgicalPreparationItemOptionResponse>> GetOptionsAsync(
            string? search,
            string? groupName,
            bool onlyActive,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyFilters(
                BaseQuery(),
                search,
                groupName,
                onlyActive ? true : null,
                isMandatory: null);

            var rows = await query
                .Select(x => new SurgicalPreparationItemOptionResponse
                {
                    Id = x.Id,
                    Code = x.Code,
                    GroupName = x.GroupName,
                    ItemName = x.ItemName,
                    IsMandatory = x.IsMandatory,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return rows
                .OrderBy(x => GroupRank(x.GroupName))
                .ThenBy(x => x.GroupName)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.ItemName)
                .ToList();
        }

        public async Task<SurgicalPreparationItemResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            return entity == null ? null : ToResponse(entity);
        }

        public async Task<SurgicalPreparationItemResult> CreateAsync(
            CreateSurgicalPreparationItemRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(SurgicalPreparationItemStatus.Invalid, validationMessage);

            var code = NormalizeCode(request.Code);

            if (await CodeIsUsedAsync(code, excludeId: null, cancellationToken))
                return Failed(SurgicalPreparationItemStatus.DuplicateCode, DuplicateCodeMessage(code));

            var now = DateTime.UtcNow;

            var entity = new MstSurgicalPreparationItem
            {
                Id = Guid.NewGuid(),
                Code = code,
                GroupName = NormalizeGroupName(request.GroupName),
                ItemName = NormalizeText(request.ItemName) ?? string.Empty,
                IsMandatory = request.IsMandatory,
                SortOrder = request.SortOrder,
                Description = NormalizeText(request.Description),
                IsActive = true,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstSurgicalPreparationItem>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir: dua admin menyimpan kode yang sama hampir bersamaan dan index
                // unik menolak salah satunya. Baris dilepas dari pelacakan supaya penyimpanan lain
                // pada permintaan yang sama tidak mencobanya lagi.
                _dbContext.Entry(entity).State = EntityState.Detached;

                return Failed(SurgicalPreparationItemStatus.DuplicateCode, DuplicateCodeMessage(code));
            }

            return new SurgicalPreparationItemResult(
                SurgicalPreparationItemStatus.Success,
                ToResponse(entity),
                "Butir persiapan bedah berhasil dibuat.");
        }

        public async Task<SurgicalPreparationItemResult> UpdateAsync(
            Guid id,
            UpdateSurgicalPreparationItemRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(SurgicalPreparationItemStatus.NotFound, NotFoundMessage);

            if (request.RowVersion == Guid.Empty || request.RowVersion != entity.RowVersion)
                return Failed(SurgicalPreparationItemStatus.VersionConflict, VersionConflictMessage);

            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(SurgicalPreparationItemStatus.Invalid, validationMessage);

            var code = NormalizeCode(request.Code);

            if (await CodeIsUsedAsync(code, excludeId: id, cancellationToken))
                return Failed(SurgicalPreparationItemStatus.DuplicateCode, DuplicateCodeMessage(code));

            entity.Code = code;
            entity.GroupName = NormalizeGroupName(request.GroupName);
            entity.ItemName = NormalizeText(request.ItemName) ?? entity.ItemName;
            entity.IsMandatory = request.IsMandatory;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeText(request.Description);
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            return await SaveTrackedAsync(
                entity,
                code,
                "Butir persiapan bedah berhasil diubah.",
                cancellationToken);
        }

        /// <remarks>
        /// Menonaktifkan butir TIDAK mengubah versi pra-operasi yang sudah ada. Contoh: butir
        /// "Hasil radiologi terlampir" dinonaktifkan pukul 10.00; versi pra-operasi Budi yang dibuat
        /// pukul 08.00 tetap memuatnya, versi yang dibuat pukul 11.00 tidak lagi memuatnya.
        /// </remarks>
        public async Task<SurgicalPreparationItemResult> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid? rowVersion,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(SurgicalPreparationItemStatus.NotFound, NotFoundMessage);

            if (rowVersion.HasValue && rowVersion.Value != entity.RowVersion)
                return Failed(SurgicalPreparationItemStatus.VersionConflict, VersionConflictMessage);

            entity.IsActive = isActive;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            return await SaveTrackedAsync(
                entity,
                entity.Code,
                isActive
                    ? "Butir persiapan bedah berhasil diaktifkan."
                    : "Butir persiapan bedah berhasil dinonaktifkan. Versi pra-operasi baru tidak lagi memuatnya.",
                cancellationToken);
        }

        /// <summary>Menandai butir terhapus. Tidak pernah menghapus baris fisik.</summary>
        /// <remarks>
        /// Ditolak bila butir sudah dipakai versi Catatan Pra-Operasi mana pun
        /// (<c>OprWardPreOpItem.PreparationItemId</c>). Untuk butir seperti itu admin cukup
        /// menonaktifkannya.
        /// </remarks>
        public async Task<SurgicalPreparationItemResult> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(SurgicalPreparationItemStatus.NotFound, NotFoundMessage);

            var usedByPreOp = await _dbContext.Set<OprWardPreOpItem>()
                .AsNoTracking()
                .AnyAsync(x => x.PreparationItemId == id && !x.IsDelete, cancellationToken);

            if (usedByPreOp)
            {
                return Failed(
                    SurgicalPreparationItemStatus.InUse,
                    $"Butir {entity.Code} sudah dipakai Catatan Pra-Operasi, sehingga tidak dapat dihapus. Nonaktifkan butir ini supaya tidak muncul di versi baru.");
            }

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            return await SaveTrackedAsync(
                entity,
                entity.Code,
                "Butir persiapan bedah berhasil dihapus.",
                cancellationToken);
        }

        public async Task<SurgicalPreparationItemFilterMetadataResponse> BuildFilterMetadataAsync(
            CancellationToken cancellationToken = default)
        {
            var storedGroups = await BaseQuery()
                .Select(x => x.GroupName)
                .Distinct()
                .ToListAsync(cancellationToken);

            var groupOptions = StandardGroupNames
                .Concat(storedGroups.Where(g => !StandardGroupNames.Contains(g.Trim(), StringComparer.OrdinalIgnoreCase)))
                .Select(g => g.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(GroupRank)
                .ThenBy(g => g)
                .ToList();

            return new SurgicalPreparationItemFilterMetadataResponse
            {
                DefaultFilter = new SurgicalPreparationItemDefaultFilterResponse(),
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                GroupNameOptions = groupOptions,
                SortOptions = new List<SurgicalPreparationItemSortOptionResponse>
                {
                    new() { Value = "groupName", Label = "Kelompok, lalu urutan tampil" },
                    new() { Value = "code", Label = "Kode Butir" },
                    new() { Value = "itemName", Label = "Nama Butir" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                QueryParameters = new List<SurgicalPreparationItemQueryParameterInfoResponse>
                {
                    new() { Name = "search", Type = "string", Description = "Mencari pada kode dan nama butir.", Example = "gelang" },
                    new() { Name = "groupName", Type = "string", Description = "Menyaring satu kelompok, tanpa membedakan huruf besar kecil.", Example = "Verifikasi pasien" },
                    new() { Name = "isActive", Type = "boolean", Description = "Menyaring butir aktif atau nonaktif.", Example = "true" },
                    new() { Name = "isMandatory", Type = "boolean", Description = "Menyaring butir wajib atau tidak wajib.", Example = "true" },
                    new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan. Bawaannya groupName (kelompok, lalu urutan tampil).", Example = "groupName" },
                    new() { Name = "sortDirection", Type = "string", Description = "Arah pengurutan, asc atau desc.", Example = "asc" },
                    new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "25" }
                },
                CreateFields = BuildFormFields(isUpdate: false),
                UpdateFields = BuildFormFields(isUpdate: true)
            };
        }

        public static SurgicalPreparationItemResponse ToResponse(MstSurgicalPreparationItem entity) => new()
        {
            Id = entity.Id,
            Code = entity.Code,
            GroupName = entity.GroupName,
            ItemName = entity.ItemName,
            IsMandatory = entity.IsMandatory,
            SortOrder = entity.SortOrder,
            Description = entity.Description,
            IsActive = entity.IsActive,
            RowVersion = entity.RowVersion,
            CreateDateTime = entity.CreateDateTime,
            CreateBy = entity.CreateBy,
            UpdateDateTime = entity.UpdateDateTime,
            UpdateBy = entity.UpdateBy
        };

        /// <summary>Urutan kelompok baku; kelompok lain di belakangnya.</summary>
        internal static int GroupRank(string? groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                return int.MaxValue;

            for (var index = 0; index < StandardGroupNames.Count; index++)
            {
                if (string.Equals(StandardGroupNames[index], groupName.Trim(), StringComparison.OrdinalIgnoreCase))
                    return index;
            }

            return StandardGroupNames.Count;
        }

        private async Task<SurgicalPreparationItemResult> SaveTrackedAsync(
            MstSurgicalPreparationItem entity,
            string code,
            string successMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // RowVersion adalah token konkurensi: admin lain menyimpan di antara pembacaan dan
                // penyimpanan ini.
                return Failed(SurgicalPreparationItemStatus.VersionConflict, VersionConflictMessage);
            }
            catch (DbUpdateException)
            {
                return Failed(SurgicalPreparationItemStatus.DuplicateCode, DuplicateCodeMessage(code));
            }

            return new SurgicalPreparationItemResult(
                SurgicalPreparationItemStatus.Success,
                ToResponse(entity),
                successMessage);
        }

        private static List<SurgicalPreparationItemFormFieldMetadataResponse> BuildFormFields(bool isUpdate)
        {
            var fields = new List<SurgicalPreparationItemFormFieldMetadataResponse>
            {
                new()
                {
                    Name = "code", Label = "Kode Butir", Section = "Identitas", InputType = "text",
                    IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 30,
                    Description = "Kode unik butir. Huruf diubah menjadi kapital.", Example = "SPI-ID-01", SortOrder = 1
                },
                new()
                {
                    Name = "groupName", Label = "Kelompok", Section = "Identitas", InputType = "select",
                    IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 100,
                    OptionsSource = "groupNameOptions",
                    Description = "Kelompok checklist pada form pra-operasi.", Example = "Verifikasi pasien", SortOrder = 2
                },
                new()
                {
                    Name = "itemName", Label = "Nama Butir", Section = "Identitas", InputType = "text",
                    IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200,
                    Description = "Kalimat yang dicentang perawat.", Example = "Gelang identitas terpasang", SortOrder = 3
                },
                new()
                {
                    Name = "isMandatory", Label = "Wajib", Section = "Aturan", InputType = "switch",
                    IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional",
                    Description = "Butir wajib harus dikonfirmasi pengirim sebelum mengirim dan penerima sebelum kasus Siap.",
                    Example = "true", SortOrder = 4
                },
                new()
                {
                    Name = "sortOrder", Label = "Urutan Tampil", Section = "Aturan", InputType = "number",
                    IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional",
                    Description = "Urutan butir di dalam kelompoknya, 0–9999.", Example = "1", SortOrder = 5
                },
                new()
                {
                    Name = "description", Label = "Keterangan", Section = "Aturan", InputType = "textarea",
                    IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional", MaxLength = 500,
                    Description = "Petunjuk pengisian bagi perawat.", Example = "Catat jam mulai puasa pada catatan butir.", SortOrder = 6
                }
            };

            if (isUpdate)
            {
                fields.Add(new()
                {
                    Name = "rowVersion", Label = "Versi Data", Section = "Sistem", InputType = "hidden",
                    IsRequiredOnCreate = false, IsRequiredOnUpdate = true, RequiredType = "Required",
                    Description = "Diambil dari GET /{id}; mencegah perubahan admin lain tertimpa.", SortOrder = 7
                });
            }

            return fields;
        }

        private static IQueryable<MstSurgicalPreparationItem> ApplyFilters(
            IQueryable<MstSurgicalPreparationItem> query,
            string? search,
            string? groupName,
            bool? isActive,
            bool? isMandatory)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.Code.ToLower().Contains(keyword) ||
                    x.ItemName.ToLower().Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(groupName))
            {
                var group = groupName.Trim().ToLower();

                query = query.Where(x => x.GroupName.ToLower() == group);
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (isMandatory.HasValue)
                query = query.Where(x => x.IsMandatory == isMandatory.Value);

            return query;
        }

        private static IQueryable<MstSurgicalPreparationItem> ApplySort(
            IQueryable<MstSurgicalPreparationItem> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "code" => descending
                    ? query.OrderByDescending(x => x.Code)
                    : query.OrderBy(x => x.Code),
                "itemname" => descending
                    ? query.OrderByDescending(x => x.ItemName)
                    : query.OrderBy(x => x.ItemName),
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.GroupName).ThenByDescending(x => x.SortOrder).ThenBy(x => x.Code)
                    : query.OrderBy(x => x.GroupName).ThenBy(x => x.SortOrder).ThenBy(x => x.Code)
            };
        }

        private IQueryable<MstSurgicalPreparationItem> BaseQuery()
            => _dbContext.Set<MstSurgicalPreparationItem>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private Task<MstSurgicalPreparationItem?> TrackedAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<MstSurgicalPreparationItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.IsCancel, cancellationToken);

        private Task<bool> CodeIsUsedAsync(string code, Guid? excludeId, CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstSurgicalPreparationItem>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.Code.ToLower() == code.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private static string DuplicateCodeMessage(string code)
            => $"Kode butir {code} sudah dipakai butir persiapan bedah lain.";

        private static SurgicalPreparationItemResult Failed(SurgicalPreparationItemStatus status, string message)
            => new(status, null, message);

        private static string? ValidateRequest(CreateSurgicalPreparationItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Code))
                return "Kode butir wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.GroupName))
                return "Kelompok butir wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.ItemName))
                return "Nama butir wajib diisi.";

            if (request.SortOrder is < 0 or > 9999)
                return "Urutan tampil harus antara 0 dan 9999.";

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

        /// <summary>
        /// Kelompok yang sama dengan kelompok baku (tanpa membedakan huruf) disimpan persis seperti
        /// ejaan baku, supaya "verifikasi pasien" dan "Verifikasi Pasien" tidak menjadi dua kelompok.
        /// </summary>
        private static string NormalizeGroupName(string value)
        {
            var trimmed = value.Trim();

            return StandardGroupNames.FirstOrDefault(
                       g => string.Equals(g, trimmed, StringComparison.OrdinalIgnoreCase))
                   ?? trimmed;
        }

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public enum SurgicalPreparationItemStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        DuplicateCode = 3,
        VersionConflict = 4,
        InUse = 5
    }

    public sealed record SurgicalPreparationItemResult(
        SurgicalPreparationItemStatus Status,
        SurgicalPreparationItemResponse? Item,
        string Message);
}
