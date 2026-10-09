using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
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
    ///
    /// <para>
    /// <b>Sejak <c>BE-RWI-186</c></b> master ini memegang dua jenis daftar periksa — penutupan
    /// episode dan Serah Terima Pasien Baru — beserta induk sub-butir dan sumber saran
    /// (<c>RWI-DEC-241</c>, validation 15.7). Contoh: butir <c>STPB-02A</c> "Laboratorium" berinduk
    /// <c>STPB-02</c>; mencoba memberi induk berjenis penutupan ditolak <c>MST-ICI-001</c>, dan
    /// mengubah jenis butir yang sudah dipakai dokumen serah terima ditolak <c>MST-ICI-003</c>.
    /// </para>
    /// </remarks>
    public class InpatientClearanceItemService
    {
        public const string CodeParentInvalid = "MST-ICI-001";
        public const string CodeClosureHasExtras = "MST-ICI-002";
        public const string CodeTypeInUse = "MST-ICI-003";
        public const string CodeSuggestionTaken = "MST-ICI-004";

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
                ChecklistTypeOptions = Enum.GetValues<MstClearanceChecklistType>()
                    .Select(x => new InpatientClearanceItemEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = ChecklistTypeLabel(x)
                    })
                    .ToList(),
                HandoverSuggestionSourceOptions = Enum.GetValues<MstHandoverSuggestionSource>()
                    .Select(x => new InpatientClearanceItemEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = SuggestionSourceLabel(x)
                    })
                    .ToList(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildFormFieldMetadata(isUpdate: false),
                UpdateFields = BuildFormFieldMetadata(isUpdate: true)
            };
        }

        public async Task<InpatientClearanceItemSummaryResponse> GetSummaryAsync(
            MstClearanceChecklistType? checklistType = null,
            CancellationToken cancellationToken = default)
        {
            var query = ApplyChecklistType(BuildBaseQuery(), checklistType);

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
            CancellationToken cancellationToken = default,
            MstClearanceChecklistType? checklistType = null)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);
            if (!dateRange.IsValid)
                throw new ArgumentException(dateRange.ErrorMessage ?? "Filter tanggal tidak valid.");

            if (checklistType.HasValue && !Enum.IsDefined(checklistType.Value))
                throw new ArgumentException("checklistType tidak valid.");

            var query = ApplyChecklistType(BuildBaseQuery(), checklistType);

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
                    ChecklistType = (int)x.ChecklistType,
                    ParentItemId = x.ParentItemId,
                    ParentItemName = x.ParentItem != null ? x.ParentItem.ItemName : null,
                    HandoverSuggestionSource = (int)x.HandoverSuggestionSource,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            // Nama enum dibentuk sesudah baris dibaca, bukan di dalam projection.
            foreach (var item in items)
            {
                item.ChecklistTypeName = ((MstClearanceChecklistType)item.ChecklistType).ToString();
                item.HandoverSuggestionSourceName =
                    ((MstHandoverSuggestionSource)item.HandoverSuggestionSource).ToString();
            }

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
            CancellationToken cancellationToken = default,
            MstClearanceChecklistType? checklistType = null)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = ApplyChecklistType(BuildBaseQuery(), checklistType);

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
                    SortOrder = x.SortOrder,
                    ChecklistType = (int)x.ChecklistType,
                    ParentItemId = x.ParentItemId,
                    ParentItemName = x.ParentItem != null ? x.ParentItem.ItemName : null,
                    HandoverSuggestionSource = (int)x.HandoverSuggestionSource
                })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.ChecklistTypeName = ((MstClearanceChecklistType)item.ChecklistType).ToString();
                item.HandoverSuggestionSourceName =
                    ((MstHandoverSuggestionSource)item.HandoverSuggestionSource).ToString();
            }

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
            // Induk ikut dimuat supaya balasan detail dapat menyebut nama butir induknya.
            return _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .Include(x => x.ParentItem)
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

            // BE-RWI-186: permintaan lama tanpa isian jenis tetap membuat butir penutupan.
            var checklistType = request.ChecklistType ?? MstClearanceChecklistType.EpisodeClosure;
            var suggestionSource = request.HandoverSuggestionSource ?? MstHandoverSuggestionSource.None;

            var typeRuleFailure = await ValidateTypeRulesAsync(
                selfId: null,
                currentType: null,
                checklistType,
                request.ParentItemId,
                suggestionSource,
                request.IsActive,
                cancellationToken);

            if (typeRuleFailure != null)
                return typeRuleFailure;

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
                ChecklistType = checklistType,
                ParentItemId = request.ParentItemId,
                HandoverSuggestionSource = suggestionSource,
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
                await GetByIdAsync(entity.Id, cancellationToken) ?? entity,
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

            // BE-RWI-186: isian jenis yang tidak dikirim layar lama mempertahankan nilai tersimpan,
            // supaya layar lama tidak diam-diam memindahkan butir serah terima ke penutupan.
            var checklistType = request.ChecklistType ?? entity.ChecklistType;
            var suggestionSource = request.HandoverSuggestionSource ??
                (request.ChecklistType.HasValue ? MstHandoverSuggestionSource.None : entity.HandoverSuggestionSource);
            var parentItemId = request.ChecklistType.HasValue || request.ParentItemId.HasValue
                ? request.ParentItemId
                : entity.ParentItemId;

            var typeRuleFailure = await ValidateTypeRulesAsync(
                selfId: entity.Id,
                currentType: entity.ChecklistType,
                checklistType,
                parentItemId,
                suggestionSource,
                request.IsActive,
                cancellationToken);

            if (typeRuleFailure != null)
                return typeRuleFailure;

            entity.ItemCode = itemCode;
            entity.ItemName = NormalizeText(request.ItemName) ?? entity.ItemName;
            entity.Description = NormalizeText(request.Description);
            entity.IsMandatory = request.IsMandatory;
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.ChecklistType = checklistType;
            entity.ParentItemId = parentItemId;
            entity.HandoverSuggestionSource = suggestionSource;
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
                await GetByIdAsync(entity.Id, cancellationToken) ?? entity,
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

            // VAL-RWA-53: mengaktifkan kembali butir serah terima tidak boleh membuat satu sumber
            // saran dipakai dua butir aktif.
            if (isActive && !entity.IsActive)
            {
                var suggestionFailure = await ValidateSuggestionSourceAsync(
                    entity.Id,
                    entity.ChecklistType,
                    entity.HandoverSuggestionSource,
                    cancellationToken);

                if (suggestionFailure != null)
                    return suggestionFailure;
            }

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new InpatientClearanceItemResult(
                InpatientClearanceItemStatus.Success,
                await GetByIdAsync(entity.Id, cancellationToken) ?? entity,
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
                ChecklistType = (int)entity.ChecklistType,
                ChecklistTypeName = entity.ChecklistType.ToString(),
                ParentItemId = entity.ParentItemId,
                ParentItemName = entity.ParentItem?.ItemName,
                HandoverSuggestionSource = (int)entity.HandoverSuggestionSource,
                HandoverSuggestionSourceName = entity.HandoverSuggestionSource.ToString(),
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };

        private const string NotFoundMessage = "Butir administrasi tidak ditemukan.";

        private static string DuplicateCodeMessage(string itemCode)
            => $"Kode butir {itemCode} sudah dipakai butir administrasi lain.";

        private static InpatientClearanceItemResult Failed(
            InpatientClearanceItemStatus status,
            string message,
            string? code = null)
            => new(status, null, message, code);

        private static IQueryable<MstInpatientClearanceItem> ApplyChecklistType(
            IQueryable<MstInpatientClearanceItem> query,
            MstClearanceChecklistType? checklistType)
            => checklistType.HasValue ? query.Where(x => x.ChecklistType == checklistType.Value) : query;

        /// <summary>
        /// Aturan jenis, induk, dan sumber saran — validation 15.7 (<c>VAL-RWA-50</c> s.d. <c>53</c>).
        /// </summary>
        /// <remarks>
        /// Urutan pemeriksaannya disengaja: bentuk isian lebih dulu, lalu aturan butir penutupan,
        /// induk, keterpakaian, dan terakhir benturan sumber saran yang membutuhkan baca database.
        ///
        /// <para>
        /// <b>Contoh <c>MST-ICI-003</c>.</b> Butir <c>STPB-13</c> "PASANG GELANG" sudah dibekukan di
        /// serah terima Tn. Budi. Admin mencoba mengubahnya menjadi butir penutupan; ditolak, karena
        /// dokumen lama akan menunjuk butir yang jenisnya tidak lagi sama.
        /// </para>
        /// </remarks>
        private async Task<InpatientClearanceItemResult?> ValidateTypeRulesAsync(
            Guid? selfId,
            MstClearanceChecklistType? currentType,
            MstClearanceChecklistType checklistType,
            Guid? parentItemId,
            MstHandoverSuggestionSource suggestionSource,
            bool willBeActive,
            CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(checklistType))
                return Failed(InpatientClearanceItemStatus.Invalid, "Jenis daftar periksa tidak valid.");

            if (!Enum.IsDefined(suggestionSource))
                return Failed(InpatientClearanceItemStatus.Invalid, "Sumber saran tidak valid.");

            // VAL-RWA-51: butir penutupan tidak punya induk maupun sumber saran.
            if (checklistType == MstClearanceChecklistType.EpisodeClosure &&
                (parentItemId.HasValue || suggestionSource != MstHandoverSuggestionSource.None))
            {
                return Failed(
                    InpatientClearanceItemStatus.BusinessRuleRejected,
                    "Butir penutupan tidak boleh punya induk atau sumber saran.",
                    CodeClosureHasExtras);
            }

            // VAL-RWA-50: induk harus ada, aktif, jenis sama, butir utama, dan bukan dirinya sendiri.
            if (parentItemId.HasValue)
            {
                var parent = await _dbContext.Set<MstInpatientClearanceItem>()
                    .AsNoTracking()
                    .Where(x => x.Id == parentItemId.Value && !x.IsDelete)
                    .Select(x => new { x.Id, x.IsActive, x.ChecklistType, x.ParentItemId })
                    .FirstOrDefaultAsync(cancellationToken);

                // Kedalaman satu: butir yang sudah punya sub-butir tidak boleh menjadi sub-butir.
                var selfHasChildren = selfId.HasValue && await _dbContext.Set<MstInpatientClearanceItem>()
                    .AsNoTracking()
                    .AnyAsync(x => x.ParentItemId == selfId.Value && !x.IsDelete, cancellationToken);

                if (parent == null ||
                    !parent.IsActive ||
                    parent.ChecklistType != checklistType ||
                    parent.ParentItemId.HasValue ||
                    parent.Id == selfId ||
                    selfHasChildren)
                {
                    return Failed(
                        InpatientClearanceItemStatus.BusinessRuleRejected,
                        "Induk butir harus butir utama dengan jenis yang sama.",
                        CodeParentInvalid);
                }
            }

            // VAL-RWA-52: jenis butir tidak dapat diubah setelah dipakai dokumen atau penandaan.
            if (selfId.HasValue && currentType.HasValue && currentType.Value != checklistType)
            {
                var usedByClosureMark = await _dbContext.Set<InpClearanceMark>()
                    .AsNoTracking()
                    .AnyAsync(x => x.ClearanceItemId == selfId.Value && !x.IsDelete, cancellationToken);

                var usedByHandover = await _dbContext.Set<InpAdmissionHandoverItem>()
                    .AsNoTracking()
                    .AnyAsync(x => x.ClearanceItemId == selfId.Value, cancellationToken);

                if (usedByClosureMark || usedByHandover)
                {
                    return Failed(
                        InpatientClearanceItemStatus.BusinessRuleRejected,
                        "Jenis butir tidak dapat diubah karena sudah dipakai dokumen atau penandaan.",
                        CodeTypeInUse);
                }

                // Butir yang menjadi induk tidak boleh berganti jenis tanpa sub-butirnya.
                var hasChildren = await _dbContext.Set<MstInpatientClearanceItem>()
                    .AsNoTracking()
                    .AnyAsync(x => x.ParentItemId == selfId.Value && !x.IsDelete, cancellationToken);

                if (hasChildren)
                {
                    return Failed(
                        InpatientClearanceItemStatus.BusinessRuleRejected,
                        "Induk butir harus butir utama dengan jenis yang sama.",
                        CodeParentInvalid);
                }
            }

            // VAL-RWA-53: satu sumber saran hanya untuk satu butir serah terima aktif.
            if (willBeActive)
                return await ValidateSuggestionSourceAsync(selfId, checklistType, suggestionSource, cancellationToken);

            return null;
        }

        private async Task<InpatientClearanceItemResult?> ValidateSuggestionSourceAsync(
            Guid? selfId,
            MstClearanceChecklistType checklistType,
            MstHandoverSuggestionSource suggestionSource,
            CancellationToken cancellationToken)
        {
            if (checklistType != MstClearanceChecklistType.NewPatientHandover ||
                suggestionSource == MstHandoverSuggestionSource.None)
            {
                return null;
            }

            var takenBy = await _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ChecklistType == MstClearanceChecklistType.NewPatientHandover &&
                    x.HandoverSuggestionSource == suggestionSource &&
                    (!selfId.HasValue || x.Id != selfId.Value))
                .Select(x => x.ItemCode)
                .FirstOrDefaultAsync(cancellationToken);

            return takenBy == null
                ? null
                : Failed(
                    InpatientClearanceItemStatus.Conflict,
                    $"Sumber saran ini sudah dipakai butir {takenBy}.",
                    CodeSuggestionTaken);
        }

        private static string ChecklistTypeLabel(MstClearanceChecklistType type) => type switch
        {
            MstClearanceChecklistType.NewPatientHandover => "Serah Terima Pasien Baru",
            _ => "Penutupan Episode"
        };

        private static string SuggestionSourceLabel(MstHandoverSuggestionSource source) => source switch
        {
            MstHandoverSuggestionSource.ReferralLetter => "Surat pengantar rawat inap terbit",
            MstHandoverSuggestionSource.CostEstimateCompleted => "Estimasi Biaya lengkap",
            MstHandoverSuggestionSource.DepositStatementCompleted => "Pelunasan Deposit lengkap",
            MstHandoverSuggestionSource.BaseDataPrinted => "IPD sudah dicetak",
            MstHandoverSuggestionSource.LabelAndGeneralConsent => "Label dan General Consent",
            MstHandoverSuggestionSource.WristbandPrinted => "Gelang sudah dicetak",
            _ => "Tanpa saran"
        };

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
                new() { Name = "checklistType", Type = "MstClearanceChecklistType?", Description = "Jenis daftar periksa: 1 EpisodeClosure, 2 NewPatientHandover; kosong = semua.", Example = "2" },
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
                new() { Name = "sortOrder", Label = "Urutan Tampil", Section = "Aturan", InputType = "number", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", Description = "Bilangan bulat antara 0 dan 9999.", Example = "10", SortOrder = 5 },
                new() { Name = "checklistType", Label = "Jenis Daftar Periksa", Section = "Aturan", InputType = "select", RequiredType = "Optional", OptionsSource = "ChecklistTypeOptions", Description = "Kosong berarti butir penutupan episode. Tidak dapat diubah setelah butir dipakai.", Example = "2", SortOrder = 6 },
                new() { Name = "parentItemId", Label = "Induk Butir", Section = "Aturan", InputType = "select", RequiredType = "Optional", OptionsSource = "inpatient-clearance-items/options?checklistType=2", Description = "Hanya untuk sub-butir serah terima; induk harus butir utama dengan jenis yang sama.", SortOrder = 7 },
                new() { Name = "handoverSuggestionSource", Label = "Sumber Saran Sistem", Section = "Aturan", InputType = "select", RequiredType = "Optional", OptionsSource = "HandoverSuggestionSourceOptions", Description = "Hanya untuk butir serah terima; satu sumber untuk satu butir aktif.", Example = "6", SortOrder = 8 }
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
        DuplicateCode = 3,

        /// <summary>Aturan jenis, induk, atau keterpakaian butir menolak — 422 (<c>BE-RWI-186</c>).</summary>
        BusinessRuleRejected = 4,

        /// <summary>Sumber saran sudah dipakai butir aktif lain — 409 (<c>MST-ICI-004</c>).</summary>
        Conflict = 5
    }

    /// <param name="Code">Kode alasan validation 15.7, misalnya <c>MST-ICI-001</c>; kosong untuk kegagalan lama.</param>
    public sealed record InpatientClearanceItemResult(
        InpatientClearanceItemStatus Status,
        MstInpatientClearanceItem? Entity,
        string Message,
        string? Code = null);
}
