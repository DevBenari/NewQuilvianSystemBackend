using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
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

        public async Task<PagedResult<InpatientClearanceItemResponse>> GetPagedAsync(
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

            var query = _dbContext.Set<MstInpatientClearanceItem>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

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

        private static string NormalizeCode(string value)
            => value.Trim().ToUpperInvariant();

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
