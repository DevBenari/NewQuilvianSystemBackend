using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Services
{
    /// <summary>
    /// Membentuk dan menilai kesiapan unit HD per tanggal dan shift (<c>BE-HMD-06</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Gerbang air.</b> Butir yang <c>RequiresResultDate</c> — pemeriksaan pengolahan air —
    /// dinilai terhadap <c>HmdSetting.WaterResultValidityHours</c> yang dibaca langsung dari basis
    /// data pada saat pernyataan siap, sehingga perubahan pengaturan berlaku pada shift berikutnya
    /// tanpa restart.
    /// </para>
    /// <para>
    /// <b>Contoh berangka.</b> Pengaturan 720 jam (30 hari). Uji air terakhir tanggal 1 Agustus.
    /// Koordinator menyatakan unit siap untuk Shift Pagi 10 September: umur hasil 40 hari, melewati
    /// 30 hari, sehingga ditolak <c>422 HMD-VAL-102</c> dan rinciannya menyebut butir air. Bila
    /// pengaturan dinaikkan menjadi 1440 jam (60 hari), pernyataan yang sama diterima.
    /// </para>
    /// <para>
    /// <b>Tidak siap bukan penghentian.</b> Menyatakan unit <c>NotReady</c> di tengah shift tidak
    /// menyentuh satu pun sesi. Sesi yang belum dimulai tidak dapat dinyatakan siap
    /// (<c>HMD-VAL-042</c>), sedangkan sesi yang sudah berjalan tetap berjalan — penghentian
    /// adalah keputusan klinis.
    /// </para>
    /// </remarks>
    public class HmdUnitReadinessService
    {
        private readonly ApplicationDbContext _dbContext;

        public HmdUnitReadinessService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResult<HmdUnitReadinessResponse>> GetListAsync(
            HmdReadinessQuery query,
            CancellationToken cancellationToken)
        {
            var (pageNumber, pageSize) = HmdServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);

            var rows = _dbContext.HmdUnitReadinesses.AsNoTracking().Where(x => !x.IsDelete);

            if (query.ServiceUnitId.HasValue)
                rows = rows.Where(x => x.ServiceUnitId == query.ServiceUnitId.Value);
            if (query.StartDate.HasValue)
                rows = rows.Where(x => x.ReadinessDate >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                rows = rows.Where(x => x.ReadinessDate <= query.EndDate.Value);
            if (query.Shift.HasValue)
                rows = rows.Where(x => x.Shift == query.Shift.Value);
            if (query.ReadinessStatus.HasValue)
                rows = rows.Where(x => x.ReadinessStatus == query.ReadinessStatus.Value);

            rows = rows.OrderByDescending(x => x.ReadinessDate).ThenBy(x => x.Shift);

            var total = await rows.CountAsync(cancellationToken);
            var items = await Project(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            items.ForEach(Label);

            return new PagedResult<HmdUnitReadinessResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = HmdServiceSupport.TotalPage(total, pageSize),
                Items = items
            };
        }

        public async Task<HmdUnitReadinessDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var header = await Project(_dbContext.HmdUnitReadinesses.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);
            if (header == null)
                return null;

            Label(header);

            var setting = await HmdServiceSupport.FindSettingAsync(_dbContext, header.ServiceUnitId, cancellationToken);
            var validityHours = setting?.WaterResultValidityHours;

            var items = await _dbContext.HmdUnitReadinessDetails.AsNoTracking()
                .Where(x => x.UnitReadinessId == id && !x.IsDelete)
                .Select(x => new HmdUnitReadinessItemResponse
                {
                    Id = x.Id,
                    ReadinessItemId = x.ReadinessItemId,
                    ItemCode = x.ReadinessItem != null ? x.ReadinessItem.ItemCode : string.Empty,
                    ItemName = x.ReadinessItem != null ? x.ReadinessItem.ItemName : string.Empty,
                    Category = x.ReadinessItem != null ? x.ReadinessItem.Category : HmdReadinessCategory.Machine,
                    IsMandatory = x.ReadinessItem != null && x.ReadinessItem.IsMandatory,
                    RequiresResultDate = x.ReadinessItem != null && x.ReadinessItem.RequiresResultDate,
                    CheckSequence = x.ReadinessItem != null ? x.ReadinessItem.CheckSequence : 0,
                    Result = x.Result,
                    ResultDate = x.ResultDate,
                    ReferenceNumber = x.ReferenceNumber,
                    VerifiedByUserId = x.VerifiedByUserId,
                    VerifiedAt = x.VerifiedAt,
                    Note = x.Note
                })
                .ToListAsync(cancellationToken);

            var nowUtc = DateTime.UtcNow;
            foreach (var item in items)
            {
                item.CategoryName = HmdLabels.ReadinessCategory(item.Category);
                item.ResultName = HmdLabels.ChecklistResult(item.Result);

                if (item.RequiresResultDate && item.ResultDate.HasValue && validityHours.HasValue)
                {
                    item.ResultValidUntil = ValidUntilUtc(item.ResultDate.Value, validityHours.Value);
                    item.IsResultExpired = nowUtc > item.ResultValidUntil.Value;
                }
            }

            var detail = new HmdUnitReadinessDetailResponse
            {
                Id = header.Id,
                ServiceUnitId = header.ServiceUnitId,
                ServiceUnitName = header.ServiceUnitName,
                ReadinessDate = header.ReadinessDate,
                Shift = header.Shift,
                ShiftName = header.ShiftName,
                ReadinessStatus = header.ReadinessStatus,
                ReadinessStatusName = header.ReadinessStatusName,
                DeclaredByUserId = header.DeclaredByUserId,
                DeclaredByName = header.DeclaredByName,
                DeclaredAt = header.DeclaredAt,
                NotReadyReason = header.NotReadyReason,
                MandatoryItemCount = header.MandatoryItemCount,
                MandatoryItemMetCount = header.MandatoryItemMetCount,
                CreateDateTime = header.CreateDateTime,
                WaterResultValidityHours = validityHours ?? 0,
                Items = items.OrderBy(x => x.CheckSequence).ThenBy(x => x.ItemCode).ToList()
            };

            detail.AvailableActions = header.ReadinessStatus switch
            {
                HmdReadinessStatus.Draft => ["SaveItems", "DeclareReady", "DeclareNotReady"],
                HmdReadinessStatus.NotReady => ["SaveItems", "DeclareReady"],
                HmdReadinessStatus.Ready => ["DeclareNotReady"],
                _ => []
            };

            return detail;
        }

        /// <summary>Membentuk lembar pemeriksaan baru berisi seluruh butir kesiapan yang aktif.</summary>
        public async Task<HmdResult<HmdUnitReadinessResponse>> CreateAsync(
            CreateHmdUnitReadinessRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (!await _dbContext.Set<MstServiceUnit>().AnyAsync(x => x.Id == request.ServiceUnitId && !x.IsDelete && x.IsActive, cancellationToken))
                return HmdResult<HmdUnitReadinessResponse>.Invalid(HmdErrorCodes.InvalidRequest, "Unit layanan tidak ditemukan atau tidak aktif.");

            if (await _dbContext.HmdUnitReadinesses.AnyAsync(x =>
                    x.ServiceUnitId == request.ServiceUnitId &&
                    x.ReadinessDate == request.ReadinessDate &&
                    x.Shift == request.Shift &&
                    !x.IsDelete,
                    cancellationToken))
            {
                return HmdResult<HmdUnitReadinessResponse>.Conflict(HmdErrorCodes.Val100, HmdMessages.Val100);
            }

            var items = await _dbContext.HmdReadinessItems.AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                return HmdResult<HmdUnitReadinessResponse>.Rule(
                    HmdErrorCodes.SettingMissing,
                    "Belum ada butir kesiapan unit yang aktif. Hubungi admin unit untuk mengisinya lebih dulu.");
            }

            var now = DateTime.UtcNow;
            var sheet = new HmdUnitReadiness
            {
                ServiceUnitId = request.ServiceUnitId,
                ReadinessDate = request.ReadinessDate,
                Shift = request.Shift,
                ReadinessStatus = HmdReadinessStatus.Draft,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.HmdUnitReadinesses.Add(sheet);
            foreach (var itemId in items)
            {
                _dbContext.HmdUnitReadinessDetails.Add(new HmdUnitReadinessDetail
                {
                    UnitReadinessId = sheet.Id,
                    ReadinessItemId = itemId,
                    Result = HmdChecklistResult.NotChecked,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            var failure = await TrySaveAsync(cancellationToken, HmdErrorCodes.Val100, HmdMessages.Val100);
            if (failure != null)
                return HmdResult<HmdUnitReadinessResponse>.From(failure);

            return HmdResult<HmdUnitReadinessResponse>.Created((await GetDetailAsync(sheet.Id, cancellationToken))!);
        }

        /// <summary>Menyimpan hasil pemeriksaan tiap butir. Lembar yang sudah <c>Ready</c> harus dinyatakan tidak siap lebih dulu.</summary>
        public async Task<HmdResult<HmdUnitReadinessDetailResponse>> SaveItemsAsync(
            Guid id,
            SaveHmdReadinessItemsRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var sheet = await _dbContext.HmdUnitReadinesses
                .Include(x => x.Details.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (sheet == null)
                return HmdResult<HmdUnitReadinessDetailResponse>.NotFound("Lembar kesiapan unit tidak ditemukan.");

            if (sheet.ReadinessStatus == HmdReadinessStatus.Ready)
            {
                return HmdResult<HmdUnitReadinessDetailResponse>.Rule(
                    HmdErrorCodes.InvalidTransition,
                    "Unit sudah dinyatakan siap. Nyatakan tidak siap lebih dulu bila hasil pemeriksaan perlu diubah.");
            }

            var now = DateTime.UtcNow;
            foreach (var input in request.Items)
            {
                var detail = sheet.Details.FirstOrDefault(x => x.ReadinessItemId == input.ReadinessItemId);
                if (detail == null)
                {
                    return HmdResult<HmdUnitReadinessDetailResponse>.Invalid(
                        HmdErrorCodes.InvalidRequest,
                        "Butir kesiapan yang dikirim bukan bagian dari lembar pemeriksaan ini.");
                }

                detail.Result = input.Result;
                detail.ResultDate = input.ResultDate;
                detail.ReferenceNumber = HmdServiceSupport.Normalize(input.ReferenceNumber);
                detail.Note = HmdServiceSupport.Normalize(input.Note);
                detail.VerifiedByUserId = input.Result == HmdChecklistResult.NotChecked ? null : actorUserId;
                detail.VerifiedAt = input.Result == HmdChecklistResult.NotChecked ? null : now;
                detail.UpdateDateTime = now;
                detail.UpdateBy = actorUserId;
            }

            sheet.UpdateDateTime = now;
            sheet.UpdateBy = actorUserId;
            sheet.Version++;

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdUnitReadinessDetailResponse>.From(failure);

            return HmdResult<HmdUnitReadinessDetailResponse>.Ok((await GetDetailAsync(id, cancellationToken))!);
        }

        /// <summary>
        /// Menyatakan unit siap. Seluruh butir wajib harus terpenuhi dan hasil air masih berlaku;
        /// waktu pernyataan memakai waktu server dan penyatanya pengguna yang sedang masuk.
        /// </summary>
        public async Task<HmdResult<HmdUnitReadinessResponse>> DeclareReadyAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var sheet = await _dbContext.HmdUnitReadinesses
                .Include(x => x.Details.Where(d => !d.IsDelete))
                    .ThenInclude(d => d.ReadinessItem)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (sheet == null)
                return HmdResult<HmdUnitReadinessResponse>.NotFound("Lembar kesiapan unit tidak ditemukan.");

            if (sheet.ReadinessStatus is not (HmdReadinessStatus.Draft or HmdReadinessStatus.NotReady))
            {
                return HmdResult<HmdUnitReadinessResponse>.Rule(
                    HmdErrorCodes.InvalidTransition, "Unit sudah dinyatakan siap untuk shift ini.");
            }

            var setting = await HmdServiceSupport.FindSettingAsync(_dbContext, sheet.ServiceUnitId, cancellationToken);
            if (setting == null)
                return HmdResult<HmdUnitReadinessResponse>.Rule(HmdErrorCodes.SettingMissing, HmdMessages.SettingMissing);

            var unmet = sheet.Details
                .Where(x => x.ReadinessItem != null && x.ReadinessItem.IsMandatory && x.Result != HmdChecklistResult.Met)
                .Select(x => new HmdReadinessBlockingItem
                {
                    ItemCode = x.ReadinessItem!.ItemCode,
                    ItemName = x.ReadinessItem.ItemName,
                    Reason = HmdLabels.ChecklistResult(x.Result)
                })
                .ToList();

            if (unmet.Count > 0)
                return HmdResult<HmdUnitReadinessResponse>.Rule(HmdErrorCodes.Val101, HmdMessages.Val101, unmet);

            var nowUtc = DateTime.UtcNow;
            var expired = sheet.Details
                .Where(x => x.ReadinessItem != null && x.ReadinessItem.RequiresResultDate)
                .Where(x => !x.ResultDate.HasValue ||
                            nowUtc > ValidUntilUtc(x.ResultDate.Value, setting.WaterResultValidityHours))
                .Select(x => new HmdReadinessBlockingItem
                {
                    ItemCode = x.ReadinessItem!.ItemCode,
                    ItemName = x.ReadinessItem.ItemName,
                    Reason = x.ResultDate.HasValue
                        ? $"Hasil tanggal {x.ResultDate.Value:yyyy-MM-dd} melewati masa berlaku {setting.WaterResultValidityHours} jam."
                        : "Tanggal hasil pemeriksaan belum diisi."
                })
                .ToList();

            if (expired.Count > 0)
                return HmdResult<HmdUnitReadinessResponse>.Rule(HmdErrorCodes.Val102, HmdMessages.Val102, expired);

            sheet.ReadinessStatus = HmdReadinessStatus.Ready;
            sheet.DeclaredByUserId = actorUserId;
            sheet.DeclaredAt = nowUtc;
            sheet.NotReadyReason = null;
            sheet.UpdateDateTime = nowUtc;
            sheet.UpdateBy = actorUserId;
            sheet.Version++;

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdUnitReadinessResponse>.From(failure);

            return HmdResult<HmdUnitReadinessResponse>.Ok((await GetDetailAsync(id, cancellationToken))!);
        }

        public async Task<HmdResult<HmdUnitReadinessResponse>> DeclareNotReadyAsync(
            Guid id,
            DeclareNotReadyRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var reason = HmdServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return HmdResult<HmdUnitReadinessResponse>.Invalid(HmdErrorCodes.Val103, HmdMessages.Val103);

            var sheet = await _dbContext.HmdUnitReadinesses.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (sheet == null)
                return HmdResult<HmdUnitReadinessResponse>.NotFound("Lembar kesiapan unit tidak ditemukan.");

            if (sheet.ReadinessStatus is not (HmdReadinessStatus.Draft or HmdReadinessStatus.Ready))
            {
                return HmdResult<HmdUnitReadinessResponse>.Rule(
                    HmdErrorCodes.InvalidTransition, "Unit sudah dinyatakan tidak siap untuk shift ini.");
            }

            var now = DateTime.UtcNow;
            sheet.ReadinessStatus = HmdReadinessStatus.NotReady;
            sheet.NotReadyReason = reason;
            sheet.DeclaredByUserId = actorUserId;
            sheet.DeclaredAt = now;
            sheet.UpdateDateTime = now;
            sheet.UpdateBy = actorUserId;
            sheet.Version++;

            // Sengaja tidak menyentuh HmdSession mana pun (BE-HMD-06 kriteria 3).
            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return HmdResult<HmdUnitReadinessResponse>.From(failure);

            return HmdResult<HmdUnitReadinessResponse>.Ok((await GetDetailAsync(id, cancellationToken))!);
        }

        /// <summary>Apakah unit sudah dinyatakan siap pada tanggal dan shift itu — dipakai gerbang <c>HMD-VAL-042</c>.</summary>
        public Task<bool> IsUnitReadyAsync(Guid serviceUnitId, DateOnly date, HmdShift shift, CancellationToken cancellationToken) =>
            _dbContext.HmdUnitReadinesses.AsNoTracking().AnyAsync(x =>
                x.ServiceUnitId == serviceUnitId &&
                x.ReadinessDate == date &&
                x.Shift == shift &&
                !x.IsDelete &&
                x.ReadinessStatus == HmdReadinessStatus.Ready,
                cancellationToken);

        /// <summary>
        /// Batas berlaku hasil: tengah malam tanggal hasil menurut zona waktu rumah sakit, ditambah
        /// masa berlaku pada pengaturan.
        /// </summary>
        public static DateTime ValidUntilUtc(DateOnly resultDate, int validityHours)
        {
            var localStart = DateTime.SpecifyKind(resultDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, HmdServiceSupport.BusinessTimeZone());
            return utcStart.AddHours(validityHours);
        }

        private IQueryable<HmdUnitReadinessResponse> Project(IQueryable<HmdUnitReadiness> rows) =>
            rows.Select(x => new HmdUnitReadinessResponse
            {
                Id = x.Id,
                ServiceUnitId = x.ServiceUnitId,
                ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                ReadinessDate = x.ReadinessDate,
                Shift = x.Shift,
                ReadinessStatus = x.ReadinessStatus,
                DeclaredByUserId = x.DeclaredByUserId,
                DeclaredByName = _dbContext.Users.Where(u => u.Id == x.DeclaredByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                DeclaredAt = x.DeclaredAt,
                NotReadyReason = x.NotReadyReason,
                MandatoryItemCount = x.Details.Count(d => !d.IsDelete && d.ReadinessItem != null && d.ReadinessItem.IsMandatory),
                MandatoryItemMetCount = x.Details.Count(d => !d.IsDelete && d.ReadinessItem != null && d.ReadinessItem.IsMandatory && d.Result == HmdChecklistResult.Met),
                CreateDateTime = x.CreateDateTime
            });

        private static void Label(HmdUnitReadinessResponse row)
        {
            row.ShiftName = HmdLabels.Shift(row.Shift);
            row.ReadinessStatusName = HmdLabels.ReadinessStatus(row.ReadinessStatus);
        }

        private async Task<HmdResult<bool>?> TrySaveAsync(
            CancellationToken cancellationToken,
            string uniqueCode = HmdErrorCodes.ConcurrencyConflict,
            string uniqueMessage = HmdMessages.Val902)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<bool>.Conflict(HmdErrorCodes.ConcurrencyConflict, HmdMessages.Val902);
            }
            catch (DbUpdateException exception) when (HmdServiceSupport.IsUniqueViolation(exception))
            {
                HmdServiceSupport.DetachFailedEntries(_dbContext);
                return HmdResult<bool>.Conflict(uniqueCode, uniqueMessage);
            }
        }
    }
}
