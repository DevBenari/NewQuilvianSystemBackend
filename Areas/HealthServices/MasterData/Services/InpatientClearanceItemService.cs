using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan butir administrasi yang menahan penutupan
    /// episode Rawat Inap. Controller <c>InpatientClearanceItemController</c> tidak menyentuh
    /// <c>ApplicationDbContext</c> sendiri, sesuai QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// Dua aturan bisnis melekat pada service ini, keduanya dari RWI-DEC-026 dan RWI-DEC-032:
    ///
    /// 1. Satu butir tidak boleh terdaftar dua kali. Kode butir yang kembar ditolak sebelum
    ///    menyentuh database, dan index unik <c>IX_MstInpatientClearanceItem_ItemCode</c>
    ///    menjadi penjaga terakhirnya bila dua admin menyimpan pada saat hampir bersamaan.
    /// 2. Butir yang tidak berlaku lagi dinonaktifkan, bukan dihapus, dan menonaktifkannya
    ///    TIDAK pernah menghapus penandaan yang sudah ada pada episode lama.
    /// </remarks>
    public class InpatientClearanceItemService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public InpatientClearanceItemService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public InpatientClearanceItemFilterMetadataResponse GetFilterMetadata()
        {
            return new InpatientClearanceItemFilterMetadataResponse
            {
                DefaultFilter = new InpatientClearanceItemDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<InpatientClearanceItemSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "itemCode", Label = "Kode butir" },
                    new() { Value = "itemName", Label = "Nama butir" },
                    new() { Value = "isMandatory", Label = "Sifat wajib" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                MandatoryOptions = BuildMandatoryOptions(),
                StatusOptions = BuildStatusOptions(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildFormFieldMetadata(isUpdate: false),
                UpdateFields = BuildFormFieldMetadata(isUpdate: true)
            };
        }

        public async Task<InpatientClearanceItemSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var query = BuildBaseQuery();

            return new InpatientClearanceItemSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                ActiveData = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveData = await query.CountAsync(x => !x.IsActive, cancellationToken),
                MandatoryData = await query.CountAsync(x => x.IsMandatory, cancellationToken),
                OptionalData = await query.CountAsync(x => !x.IsMandatory, cancellationToken)
            };
        }

        public async Task<PagedResult<InpatientClearanceItemResponse>> GetPagedAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            string? search,
            bool? isMandatory,
            bool? isActive,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);
            if (!dateRange.IsValid)
                throw new ArgumentException(dateRange.ErrorMessage ?? "Filter tanggal tidak valid.");

            var query = BuildBaseQuery();

            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ItemCode.ToLower().Contains(keyword) ||
                    x.ItemName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isMandatory.HasValue)
                query = query.Where(x => x.IsMandatory == isMandatory.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "itemcode" => descending ? query.OrderByDescending(x => x.ItemCode) : query.OrderBy(x => x.ItemCode),
                "itemname" => descending ? query.OrderByDescending(x => x.ItemName) : query.OrderBy(x => x.ItemName),
                "ismandatory" => descending ? query.OrderByDescending(x => x.IsMandatory) : query.OrderBy(x => x.IsMandatory),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder)
            };

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InpatientClearanceItemResponse
                {
                    Id = x.Id,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    Description = x.Description,
                    IsMandatory = x.IsMandatory,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<InpatientClearanceItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<PagedResult<InpatientClearanceItemOptionResponse>> GetOptionsAsync(
            bool onlyActive,
            bool? isMandatory,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (isMandatory.HasValue)
                query = query.Where(x => x.IsMandatory == isMandatory.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ItemCode.ToLower().Contains(keyword) ||
                    x.ItemName.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ItemName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InpatientClearanceItemOptionResponse
                {
                    Id = x.Id,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    IsMandatory = x.IsMandatory,
                    SortOrder = x.SortOrder
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<InpatientClearanceItemOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public Task<MstInpatientClearanceItem?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        }

        public async Task<InpatientClearanceItemResult> CreateAsync(
            CreateInpatientClearanceItemRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(InpatientClearanceItemStatus.Invalid, validationMessage);

            var itemCode = NormalizeCode(request.ItemCode);

            if (await ItemCodeIsUsedAsync(itemCode, excludeId: null, cancellationToken))
                return Failed(InpatientClearanceItemStatus.DuplicateCode, DuplicateCodeMessage(itemCode));

            var now = DateTime.UtcNow;

            var entity = new MstInpatientClearanceItem
            {
                Id = Guid.NewGuid(),
                ItemCode = itemCode,
                ItemName = NormalizeText(request.ItemName) ?? string.Empty,
                Description = NormalizeText(request.Description),
                IsMandatory = request.IsMandatory,
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstInpatientClearanceItem>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir. Dua admin yang menyimpan kode yang sama pada saat hampir
                // bersamaan sama-sama lolos pemeriksaan di atas, dan index unik di database
                // yang menolak salah satunya.
                //
                // Barisnya dilepas dari pelacakan supaya penyimpanan berikutnya pada
                // permintaan yang sama tidak mencoba menyisipkannya sekali lagi.
                _dbContext.Entry(entity).State = EntityState.Detached;

                return Failed(InpatientClearanceItemStatus.DuplicateCode, DuplicateCodeMessage(itemCode));
            }

            return new InpatientClearanceItemResult(
                InpatientClearanceItemStatus.Success,
                entity,
                "Butir administrasi berhasil dibuat.");
        }

        public async Task<InpatientClearanceItemResult> UpdateAsync(
            Guid id,
            UpdateInpatientClearanceItemRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstInpatientClearanceItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return Failed(InpatientClearanceItemStatus.NotFound, NotFoundMessage);

            var validationMessage = ValidateRequest(request);

            if (validationMessage != null)
                return Failed(InpatientClearanceItemStatus.Invalid, validationMessage);

            var itemCode = NormalizeCode(request.ItemCode);

            if (await ItemCodeIsUsedAsync(itemCode, excludeId: id, cancellationToken))
                return Failed(InpatientClearanceItemStatus.DuplicateCode, DuplicateCodeMessage(itemCode));

            entity.ItemCode = itemCode;
            entity.ItemName = NormalizeText(request.ItemName) ?? entity.ItemName;
            entity.Description = NormalizeText(request.Description);
            entity.IsMandatory = request.IsMandatory;
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Failed(InpatientClearanceItemStatus.DuplicateCode, DuplicateCodeMessage(itemCode));
            }

            return new InpatientClearanceItemResult(
                InpatientClearanceItemStatus.Success,
                entity,
                "Butir administrasi berhasil diubah.");
        }

        /// <remarks>
        /// Method ini mengubah <c>IsActive</c> saja. Ia tidak menyentuh satu pun baris
        /// <c>InpClearanceMark</c>.
        ///
        /// <b>Contoh.</b> Butir <c>DISCHARGE-MED</c> sudah ditandai selesai pada episode
        /// Ny. Sari yang ditutup bulan lalu. Hari ini admin menonaktifkan butir itu karena
        /// penyerahan obat pulang pindah ke modul Farmasi. Penandaan pada episode Ny. Sari
        /// tetap ada apa adanya, sehingga riwayat penutupan episodenya tetap dapat dibaca
        /// utuh oleh auditor.
        /// </remarks>
        public async Task<InpatientClearanceItemResult> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstInpatientClearanceItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return Failed(InpatientClearanceItemStatus.NotFound, NotFoundMessage);

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new InpatientClearanceItemResult(
                InpatientClearanceItemStatus.Success,
                entity,
                isActive
                    ? "Butir administrasi berhasil diaktifkan."
                    : "Butir administrasi berhasil dinonaktifkan.");
        }

        /// <remarks>
        /// Penghapusan bersifat lunak: baris ditandai terhapus dan dinonaktifkan, tetapi
        /// tetap tersimpan. Penandaan pada episode lama juga tidak ikut terhapus, dengan
        /// alasan yang sama seperti pada penonaktifan.
        /// </remarks>
        public async Task<InpatientClearanceItemResult> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<MstInpatientClearanceItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return Failed(InpatientClearanceItemStatus.NotFound, NotFoundMessage);

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.IsActive = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new InpatientClearanceItemResult(
                InpatientClearanceItemStatus.Success,
                entity,
                "Butir administrasi berhasil dihapus.");
        }

        public static InpatientClearanceItemResponse ToResponse(MstInpatientClearanceItem entity)
            => new()
            {
                Id = entity.Id,
                ItemCode = entity.ItemCode,
                ItemName = entity.ItemName,
                Description = entity.Description,
                IsMandatory = entity.IsMandatory,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };

        private const string NotFoundMessage = "Butir administrasi tidak ditemukan.";

        private static string DuplicateCodeMessage(string itemCode)
            => $"Kode butir {itemCode} sudah dipakai butir administrasi lain.";

        private static InpatientClearanceItemResult Failed(
            InpatientClearanceItemStatus status,
            string message)
            => new(status, null, message);

        private Task<bool> ItemCodeIsUsedAsync(
            string itemCode,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ItemCode.ToLower() == itemCode.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private IQueryable<MstInpatientClearanceItem> BuildBaseQuery()
        {
            return _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static string? ValidateRequest(CreateInpatientClearanceItemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ItemCode))
                return "Kode butir wajib diisi.";

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

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLowerInvariant();
            var today = AppDateTimeHelper.OperationalDate();
            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (period)
            {
                case null:
                case "":
                case "custom":
                    if (startDate.HasValue)
                        start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    if (endDate.HasValue)
                        endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    break;

                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    break;

                case "last7days":
                    start = today.AddDays(-6);
                    endExclusive = today.AddDays(1);
                    break;

                case "last30days":
                    start = today.AddDays(-29);
                    endExclusive = today.AddDays(1);
                    break;

                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddMonths(1);
                    break;

                case "lastmonth":
                    var currentMonthStart = new DateTime(
                        today.Year,
                        today.Month,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc);
                    start = currentMonthStart.AddMonths(-1);
                    endExclusive = currentMonthStart;
                    break;

                default:
                    return DateRangeResolveResult.Invalid(
                        $"customPeriod '{customPeriod}' tidak valid.");
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return DateRangeResolveResult.Invalid(
                    "startDate tidak boleh lebih besar atau sama dengan endDate.");

            return DateRangeResolveResult.Valid(start, endExclusive);
        }

        private static List<InpatientClearanceItemCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<InpatientClearanceItemCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Kustom", Description = "Gunakan tanggal mulai dan tanggal akhir.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari Ini", Description = "Data yang dibuat hari ini." },
                new() { Value = "last7days", Label = "7 Hari Terakhir", Description = "Data yang dibuat dalam tujuh hari terakhir." },
                new() { Value = "last30days", Label = "30 Hari Terakhir", Description = "Data yang dibuat dalam 30 hari terakhir." },
                new() { Value = "thismonth", Label = "Bulan Ini", Description = "Data yang dibuat pada bulan berjalan." },
                new() { Value = "lastmonth", Label = "Bulan Lalu", Description = "Data yang dibuat pada bulan sebelumnya." }
            };
        }

        private static List<InpatientClearanceItemBooleanOptionResponse> BuildMandatoryOptions()
        {
            return new List<InpatientClearanceItemBooleanOptionResponse>
            {
                new() { Value = true, Label = "Wajib" },
                new() { Value = false, Label = "Tidak Wajib" }
            };
        }

        private static List<InpatientClearanceItemBooleanOptionResponse> BuildStatusOptions()
        {
            return new List<InpatientClearanceItemBooleanOptionResponse>
            {
                new() { Value = true, Label = "Aktif" },
                new() { Value = false, Label = "Nonaktif" }
            };
        }

        private static List<InpatientClearanceItemQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<InpatientClearanceItemQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal berdasarkan CreateDateTime.", Example = "2026-08-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir berdasarkan CreateDateTime.", Example = "2026-08-31" },
                new() { Name = "customPeriod", Type = "string", Description = "Periode cepat: custom, today, last7days, last30days, thismonth, atau lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari kode, nama, atau deskripsi butir.", Example = "administrasi" },
                new() { Name = "isMandatory", Type = "bool?", Description = "Filter sifat wajib butir.", Example = "true" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah pengurutan: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<InpatientClearanceItemFormFieldMetadataResponse> BuildFormFieldMetadata(
            bool isUpdate)
        {
            var fields = new List<InpatientClearanceItemFormFieldMetadataResponse>
            {
                new() { Name = "itemCode", Label = "Kode Butir", Section = "Utama", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 50, Description = "Kode unik butir administrasi.", Example = "ADM-DOC", SortOrder = 1 },
                new() { Name = "itemName", Label = "Nama Butir", Section = "Utama", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200, Example = "Berkas administrasi lengkap", SortOrder = 2 },
                new() { Name = "description", Label = "Keterangan", Section = "Utama", InputType = "textarea", MaxLength = 500, SortOrder = 3 },
                new() { Name = "isMandatory", Label = "Butir Wajib", Section = "Aturan", InputType = "switch", Description = "Butir wajib menahan penutupan episode bila belum ditandai.", SortOrder = 4 },
                new() { Name = "sortOrder", Label = "Urutan Tampil", Section = "Aturan", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", Description = "Bilangan bulat antara 0 dan 9999.", Example = "10", SortOrder = 5 }
            };

            if (isUpdate)
            {
                fields.Add(new InpatientClearanceItemFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private static string NormalizeCode(string value)
            => value.Trim().ToUpperInvariant();

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }

            public string? ErrorMessage { get; private set; }

            public DateTime? Start { get; private set; }

            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(DateTime? start, DateTime? endExclusive)
                => new()
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };

            public static DateRangeResolveResult Invalid(string errorMessage)
                => new()
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
        }
    }

    public enum InpatientClearanceItemStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        DuplicateCode = 3
    }

    public sealed record InpatientClearanceItemResult(
        InpatientClearanceItemStatus Status,
        MstInpatientClearanceItem? Entity,
        string Message);
}
