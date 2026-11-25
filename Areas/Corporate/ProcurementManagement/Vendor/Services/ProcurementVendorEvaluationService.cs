using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Services;

/// <summary>
/// Penilaian vendor berkala per vendor × kategori × periode (BE-PBJ-VD-008; PBJ-DEC-044, 048, 049, 050, 059;
/// PBJ-VD-STATE bagian 4).
/// </summary>
/// <remarks>
/// <para>
/// <b>Alur.</b> Tim Pengadaan membuat draf; server menyalin kriteria aktif kategori (nama + bobot) sehingga perubahan master
/// tidak mengubah penilaian lama. Penilai mengisi nilai 1–5, rekomendasi, dan catatan; nilai akhir = rata-rata berbobot
/// dihitung server. Orang <b>lain</b> (Manajer Pengadaan) memfinalkan. Contoh: bobot 40/40/20, nilai 4/5/3 → 4,20.
/// </para>
/// <para>
/// <b>Final terkunci.</b> Koreksi dibuat lewat revisi: draf baru menunjuk penilaian asal; saat revisi difinalkan, penilaian
/// asal menjadi <c>SUPERSEDED</c> dalam transaksi yang sama. Keduanya tetap terbaca.
/// </para>
/// <para>
/// <b>Rekomendasi hanya saran</b> (PBJ-DEC-048): rekomendasi Blokir tidak mengubah status vendor dan tidak membuat usulan
/// blokir otomatis.
/// </para>
/// </remarks>
public sealed class ProcurementVendorEvaluationService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Vendor";
    private const string NotFoundMessage = "Penilaian vendor tidak ditemukan atau sudah dihapus.";
    private const string StaleMessage = "Penilaian sudah diubah orang lain. Muat ulang lalu ulangi tindakan Anda.";
    private const string ReasonMessage = "Alasan wajib diisi (minimal 10 karakter).";
    private const string SourceManual = "MANUAL";

    public static readonly IReadOnlyDictionary<string, int> PeriodTypes = new Dictionary<string, int>
    {
        ["SEMESTER"] = 2,
        ["QUARTER"] = 4,
        ["YEAR"] = 1
    };

    private static readonly IReadOnlyDictionary<string, string> StatusLabels = new Dictionary<string, string>
    {
        [ProcurementStatuses.VendorEvaluation.Draft] = "Draf",
        [ProcurementStatuses.VendorEvaluation.Final] = "Final",
        [ProcurementStatuses.VendorEvaluation.Superseded] = "Digantikan revisi",
        [ProcurementStatuses.VendorEvaluation.Cancelled] = "Dibatalkan"
    };

    private static readonly IReadOnlyDictionary<string, string> RecommendationLabels = new Dictionary<string, string>
    {
        ["CONTINUE"] = "Lanjutkan",
        ["RESTRICT"] = "Batasi",
        ["BLOCK"] = "Blokir"
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly LoggerService _loggerService;

    public ProcurementVendorEvaluationService(
        ApplicationDbContext dbContext,
        ProcurementDocumentHistoryWriter historyWriter,
        AccessPermissionService accessPermissionService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _historyWriter = historyWriter;
        _accessPermissionService = accessPermissionService;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------ baca

    public async Task<VendorEvaluationFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        new()
        {
            SortOptions =
            [
                new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                new() { Value = "period", Label = "Periode" },
                new() { Value = "supplier", Label = "Vendor" },
                new() { Value = "finalScore", Label = "Nilai akhir" }
            ],
            ItemCategoryOptions = await _dbContext.MstProcurementItemCategories.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.CategoryName)
                .Select(x => new VendorCategoryOptionResponse { Id = x.Id, Code = x.CategoryCode, Name = x.CategoryName, IsActive = x.IsActive })
                .ToListAsync(cancellationToken),
            PeriodTypeOptions =
            [
                new() { Value = "SEMESTER", Label = "Semester", MaxPeriodNo = 2 },
                new() { Value = "QUARTER", Label = "Triwulan", MaxPeriodNo = 4 },
                new() { Value = "YEAR", Label = "Tahunan", MaxPeriodNo = 1 }
            ],
            StatusOptions = StatusLabels.Select(x => new VendorValueOptionResponse { Value = x.Key, Label = x.Value }).ToList(),
            RecommendationOptions = RecommendationLabels.Select(x => new VendorValueOptionResponse { Value = x.Key, Label = x.Value }).ToList()
        };

    public async Task<VendorEvaluationSummaryResponse> GetSummaryAsync(VendorEvaluationSummaryQuery request, CancellationToken cancellationToken)
    {
        var periodType = NormalizePeriodType(request.PeriodType, required: false);
        var query = _dbContext.PrcVendorEvaluations.AsNoTracking().Where(x => !x.IsDelete && !x.VendorProfile!.IsDelete);
        if (request.ItemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == request.ItemCategoryId.Value);
        if (request.PeriodYear.HasValue) query = query.Where(x => x.PeriodYear == request.PeriodYear.Value);
        if (periodType != null) query = query.Where(x => x.PeriodType == periodType);
        if (request.PeriodNo.HasValue) query = query.Where(x => x.PeriodNo == request.PeriodNo.Value);

        var rows = await query.Select(x => new { x.Status, x.Recommendation, x.FinalScore }).ToListAsync(cancellationToken);
        var finals = rows.Where(x => x.Status == ProcurementStatuses.VendorEvaluation.Final).ToList();
        return new VendorEvaluationSummaryResponse
        {
            Draft = rows.Count(x => x.Status == ProcurementStatuses.VendorEvaluation.Draft),
            Final = finals.Count,
            Superseded = rows.Count(x => x.Status == ProcurementStatuses.VendorEvaluation.Superseded),
            Cancelled = rows.Count(x => x.Status == ProcurementStatuses.VendorEvaluation.Cancelled),
            Recommendations = RecommendationLabels
                .Select(x => new VendorEvaluationRecommendationCountResponse
                {
                    Recommendation = x.Key, Label = x.Value, Count = finals.Count(f => f.Recommendation == x.Key)
                })
                .ToList(),
            AverageFinalScore = finals.Count(x => x.FinalScore.HasValue) == 0
                ? null
                : decimal.Round(finals.Where(x => x.FinalScore.HasValue).Average(x => x.FinalScore!.Value), 2, MidpointRounding.AwayFromZero)
        };
    }

    public async Task<PagedResult<VendorEvaluationListItemResponse>> GetPagedAsync(VendorEvaluationQuery request, CancellationToken cancellationToken)
    {
        var periodType = NormalizePeriodType(request.PeriodType, required: false);
        var status = NormalizeOption(request.Status, "Status penilaian", StatusLabels.Keys);
        var recommendation = NormalizeOption(request.Recommendation, "Rekomendasi", RecommendationLabels.Keys);

        var query = _dbContext.PrcVendorEvaluations.AsNoTracking().Where(x => !x.IsDelete && !x.VendorProfile!.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.SupplierId.HasValue) query = query.Where(x => x.VendorProfile!.SupplierId == request.SupplierId.Value);
        if (request.ItemCategoryId.HasValue) query = query.Where(x => x.ItemCategoryId == request.ItemCategoryId.Value);
        if (request.PeriodYear.HasValue) query = query.Where(x => x.PeriodYear == request.PeriodYear.Value);
        if (periodType != null) query = query.Where(x => x.PeriodType == periodType);
        if (request.PeriodNo.HasValue) query = query.Where(x => x.PeriodNo == request.PeriodNo.Value);
        if (status != null) query = query.Where(x => x.Status == status);
        if (recommendation != null) query = query.Where(x => x.Recommendation == recommendation);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.VendorProfile!.Supplier!.SupplierCode.ToUpper().Contains(search)
                || x.VendorProfile.Supplier.SupplierName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "period" => descending
                ? query.OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodType).ThenByDescending(x => x.PeriodNo)
                : query.OrderBy(x => x.PeriodYear).ThenBy(x => x.PeriodType).ThenBy(x => x.PeriodNo),
            "supplier" => descending
                ? query.OrderByDescending(x => x.VendorProfile!.Supplier!.SupplierName)
                : query.OrderBy(x => x.VendorProfile!.Supplier!.SupplierName),
            "finalscore" => descending ? query.OrderByDescending(x => x.FinalScore) : query.OrderBy(x => x.FinalScore),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new VendorEvaluationListItemResponse
            {
                Id = x.Id,
                SupplierId = x.VendorProfile!.SupplierId,
                SupplierCode = x.VendorProfile.Supplier!.SupplierCode,
                SupplierName = x.VendorProfile.Supplier.SupplierName,
                ItemCategoryId = x.ItemCategoryId,
                ItemCategoryName = x.ItemCategory!.CategoryName,
                PeriodYear = x.PeriodYear,
                PeriodType = x.PeriodType,
                PeriodNo = x.PeriodNo,
                Status = x.Status,
                FinalScore = x.FinalScore,
                Recommendation = x.Recommendation,
                IsRevision = x.PreviousEvaluationId != null,
                EvaluatorUserId = x.CreateBy,
                EvaluatorName = _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName).FirstOrDefault(),
                FinalizedByName = _dbContext.Users.Where(u => u.Id == x.FinalizedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                FinalizedAt = x.FinalizedAt,
                CreateDateTime = x.CreateDateTime
            })
            .ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.PeriodLabel = PeriodLabel(item.PeriodType, item.PeriodNo, item.PeriodYear);
            item.StatusLabel = Label(StatusLabels, item.Status);
            item.RecommendationLabel = item.Recommendation == null ? null : Label(RecommendationLabels, item.Recommendation);
        }

        return new PagedResult<VendorEvaluationListItemResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = request.PageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<VendorEvaluationResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await _dbContext.PrcVendorEvaluations.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete)
            .Select(x => new VendorEvaluationResponse
            {
                Id = x.Id,
                SupplierId = x.VendorProfile!.SupplierId,
                SupplierCode = x.VendorProfile.Supplier!.SupplierCode,
                SupplierName = x.VendorProfile.Supplier.SupplierName,
                VendorProfileId = x.VendorProfileId,
                ItemCategoryId = x.ItemCategoryId,
                ItemCategoryName = x.ItemCategory!.CategoryName,
                PeriodYear = x.PeriodYear,
                PeriodType = x.PeriodType,
                PeriodNo = x.PeriodNo,
                SourceType = x.SourceType,
                Status = x.Status,
                FinalScore = x.FinalScore,
                Recommendation = x.Recommendation,
                Notes = x.Notes,
                PreviousEvaluationId = x.PreviousEvaluationId,
                RevisionReason = x.RevisionReason,
                LatestRevisionId = _dbContext.PrcVendorEvaluations
                    .Where(r => r.PreviousEvaluationId == x.Id && !r.IsDelete)
                    .OrderByDescending(r => r.CreateDateTime)
                    .Select(r => (Guid?)r.Id)
                    .FirstOrDefault(),
                EvaluatorUserId = x.CreateBy,
                EvaluatorName = _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName).FirstOrDefault(),
                FinalizedByUserId = x.FinalizedByUserId,
                FinalizedByName = _dbContext.Users.Where(u => u.Id == x.FinalizedByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                FinalizedAt = x.FinalizedAt,
                RowVersion = x.RowVersion,
                CreateDateTime = x.CreateDateTime,
                Scores = x.Scores
                    .Where(s => !s.IsDelete)
                    .OrderByDescending(s => s.WeightPercent).ThenBy(s => s.CriterionName)
                    .Select(s => new VendorEvaluationScoreResponse
                    {
                        Id = s.Id, CriterionId = s.CriterionId, CriterionName = s.CriterionName,
                        WeightPercent = s.WeightPercent, Score = s.Score, Note = s.Note
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException(NotFoundMessage);

        detail.PeriodLabel = PeriodLabel(detail.PeriodType, detail.PeriodNo, detail.PeriodYear);
        detail.StatusLabel = Label(StatusLabels, detail.Status);
        detail.RecommendationLabel = detail.Recommendation == null ? null : Label(RecommendationLabels, detail.Recommendation);
        return detail;
    }

    public async Task<List<DocumentHistoryResponse>> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await _dbContext.PrcVendorEvaluations.AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        return await _historyWriter.ReadAsync(ProcurementDocumentTypes.VendorEvaluation, id, cancellationToken);
    }

    /// <summary>
    /// Hak lampiran pendukung penilaian (<c>PBJ_VENDOR_EVALUATION</c>, kamus data bagian 7): tanpa
    /// <c>ProcurementVendorEvaluation : Read</c> → tidak terlihat; draf dapat ditambah lampiran oleh pemegang <c>Update</c>;
    /// penilaian final atau dibatalkan hanya baca.
    /// </summary>
    public async Task<ProcurementDocumentAccessLevel> GetAccessLevelAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        if (!await _accessPermissionService.HasAccessAsync(user, ProcurementPermissions.VendorEvaluationResource, ProcurementPermissions.ReadAction))
            return ProcurementDocumentAccessLevel.None;
        var status = await _dbContext.PrcVendorEvaluations.AsNoTracking()
            .Where(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete)
            .Select(x => x.Status)
            .FirstOrDefaultAsync(cancellationToken);
        if (status == null) return ProcurementDocumentAccessLevel.None;
        return status == ProcurementStatuses.VendorEvaluation.Draft
            && await _accessPermissionService.HasAccessAsync(user, ProcurementPermissions.VendorEvaluationResource, ProcurementPermissions.UpdateAction)
            ? ProcurementDocumentAccessLevel.Edit
            : ProcurementDocumentAccessLevel.Read;
    }

    // ------------------------------------------------------------------ tulis

    /// <summary>
    /// Buat draf penilaian. Syarat: kategori milik vendor dan total bobot kriteria aktif = 100 (<c>PBJ-VAL-108</c>); belum ada
    /// penilaian <c>DRAFT</c>/<c>FINAL</c> untuk vendor × kategori × periode yang sama (<c>PBJ-VAL-109</c>). Kriteria disalin.
    /// </summary>
    public async Task<VendorEvaluationResponse> CreateAsync(CreateVendorEvaluationRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var periodType = NormalizePeriodType(request.PeriodType, required: true)!;
        if (request.PeriodNo < 1 || request.PeriodNo > PeriodTypes[periodType])
            throw new ProcurementVendorValidationException(null, $"Nomor periode {periodType} harus 1 sampai {PeriodTypes[periodType]}.");

        var supplier = await _dbContext.MstSuppliers.AsNoTracking()
            .Where(x => x.Id == request.SupplierId && !x.IsDelete)
            .Select(x => new { x.Id, x.SupplierCode, x.SupplierName })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ProcurementVendorValidationException(null, "Vendor tidak ditemukan atau sudah dihapus di Master Data.");
        var category = await _dbContext.MstProcurementItemCategories.AsNoTracking()
            .Where(x => x.Id == request.ItemCategoryId && !x.IsDelete)
            .Select(x => new { x.Id, x.CategoryName })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ProcurementVendorValidationException(null, "Kategori tidak ditemukan atau sudah dihapus.");

        var profileId = await _dbContext.PrcVendorCategories.AsNoTracking()
            .Where(x => x.ItemCategoryId == category.Id && !x.IsDelete && x.VendorProfile!.SupplierId == supplier.Id && !x.VendorProfile.IsDelete)
            .Select(x => (Guid?)x.VendorProfileId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!profileId.HasValue)
            throw new ProcurementVendorValidationException("PBJ-VAL-108", $"Vendor tidak melayani kategori {category.CategoryName}; atur kategori vendor lebih dulu.");

        var criteria = await _dbContext.MstProcurementEvaluationCriteria.AsNoTracking()
            .Where(x => x.ItemCategoryId == category.Id && x.IsActive && !x.IsDelete)
            .OrderByDescending(x => x.WeightPercent).ThenBy(x => x.CriterionName)
            .Select(x => new { x.Id, x.CriterionName, x.WeightPercent })
            .ToListAsync(cancellationToken);
        var totalWeight = criteria.Sum(x => x.WeightPercent);
        if (totalWeight != 100m)
            throw new ProcurementVendorValidationException("PBJ-VAL-108",
                $"Total bobot kriteria {category.CategoryName} harus 100%. Saat ini {totalWeight:0.##}%.");

        await EnsurePeriodFreeAsync(profileId.Value, category.Id, request.PeriodYear, periodType, request.PeriodNo,
            supplier.SupplierName, category.CategoryName, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var notes = Optional(request.Notes, "Catatan", 2000);
        var entity = new PrcVendorEvaluation
        {
            VendorProfileId = profileId.Value,
            ItemCategoryId = category.Id,
            PeriodYear = request.PeriodYear,
            PeriodType = periodType,
            PeriodNo = request.PeriodNo,
            SourceType = SourceManual,
            Status = ProcurementStatuses.VendorEvaluation.Draft,
            Notes = notes,
            CreateDateTime = now.UtcDateTime,
            CreateBy = actorUserId
        };
        foreach (var criterion in criteria)
        {
            entity.Scores.Add(new PrcVendorEvaluationScore
            {
                CriterionId = criterion.Id,
                CriterionName = criterion.CriterionName,
                WeightPercent = criterion.WeightPercent,
                CreateDateTime = now.UtcDateTime,
                CreateBy = actorUserId
            });
        }

        _dbContext.PrcVendorEvaluations.Add(entity);
        _historyWriter.Record(ProcurementDocumentTypes.VendorEvaluation, entity.Id, null, ProcurementDocumentHistoryActions.Create,
            null, ProcurementStatuses.VendorEvaluation.Draft,
            $"Penilaian {supplier.SupplierName} {category.CategoryName} {PeriodLabel(periodType, request.PeriodNo, request.PeriodYear)} dibuat.",
            new { criteria = criteria.Select(x => new { x.CriterionName, x.WeightPercent }) }, actorUserId, now);

        await SaveAsync(cancellationToken, supplier.SupplierName, category.CategoryName, PeriodLabel(periodType, request.PeriodNo, request.PeriodYear));
        await AuditAsync("ProcurementVendorEvaluation.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    /// <summary>Isi nilai, rekomendasi, dan catatan draf; nilai akhir dihitung ulang bila semua nilai terisi.</summary>
    public async Task<VendorEvaluationResponse> UpdateAsync(Guid id, UpdateVendorEvaluationRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var entity = await LoadAsync(id, cancellationToken);
        RequireDraft(entity, "diubah");
        if (entity.RowVersion != request.ExpectedRowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);

        var recommendation = NormalizeOption(request.Recommendation, "Rekomendasi", RecommendationLabels.Keys);
        var rows = entity.Scores.Where(x => !x.IsDelete).ToDictionary(x => x.CriterionId);
        foreach (var input in request.Scores ?? new List<VendorEvaluationScoreInput>())
        {
            if (!rows.TryGetValue(input.CriterionId, out var row))
                throw new ProcurementVendorValidationException(null, "Kriteria tidak termasuk dalam penilaian ini.");
            if (input.Score.HasValue && (input.Score.Value < 1 || input.Score.Value > 5))
                throw new ProcurementVendorValidationException("PBJ-VAL-110", "Semua kriteria wajib dinilai 1–5 dan rekomendasi wajib dipilih.");
            row.Score = input.Score;
            row.Note = Optional(input.Note, "Catatan kriteria", 500);
            row.UpdateDateTime = DateTime.UtcNow;
            row.UpdateBy = actorUserId;
        }

        entity.Recommendation = recommendation;
        entity.Notes = Optional(request.Notes, "Catatan", 2000);
        entity.FinalScore = ComputeFinalScore(rows.Values);
        var now = DateTimeOffset.UtcNow;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorEvaluation, id, null, ProcurementDocumentHistoryActions.Update,
            entity.Status, entity.Status, null, new { entity.FinalScore, entity.Recommendation }, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorEvaluation.Update", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Finalkan (<c>DRAFT → FINAL</c>). Pemfinal bukan penilai (<c>PBJ-VAL-111</c>); semua nilai 1–5 dan rekomendasi terisi
    /// (<c>PBJ-VAL-110</c>). Bila ini revisi, penilaian asal menjadi <c>SUPERSEDED</c> dalam transaksi yang sama.
    /// </summary>
    public async Task<VendorEvaluationResponse> FinalizeAsync(
        Guid id, FinalizeVendorEvaluationRequest? request, string? idempotencyKey, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        if (await _historyWriter.HasIdempotencyKeyAsync(
                ProcurementDocumentTypes.VendorEvaluation, id, ProcurementDocumentHistoryActions.Finalize, idempotencyKey, cancellationToken))
            return await GetByIdAsync(id, cancellationToken);

        var entity = await LoadAsync(id, cancellationToken);
        RequireDraft(entity, "difinalkan");
        if (request?.ExpectedRowVersion is Guid expected && expected != entity.RowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        if (entity.CreateBy == actorUserId)
            throw new ProcurementVendorForbiddenException("PBJ-VAL-111", "Penilaian harus difinalkan orang lain.");
        var scores = entity.Scores.Where(x => !x.IsDelete).ToList();
        if (scores.Count == 0 || scores.Any(x => x.Score is null or < 1 or > 5) || string.IsNullOrWhiteSpace(entity.Recommendation))
            throw new ProcurementVendorValidationException("PBJ-VAL-110", "Semua kriteria wajib dinilai 1–5 dan rekomendasi wajib dipilih.");

        var now = DateTimeOffset.UtcNow;
        var supplierCode = entity.VendorProfile?.Supplier?.SupplierCode;
        var label = PeriodLabel(entity.PeriodType, entity.PeriodNo, entity.PeriodYear);
        entity.FinalScore = ComputeFinalScore(scores);
        entity.Status = ProcurementStatuses.VendorEvaluation.Final;
        entity.FinalizedByUserId = actorUserId;
        entity.FinalizedAt = now;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorEvaluation, id, null, ProcurementDocumentHistoryActions.Finalize,
            ProcurementStatuses.VendorEvaluation.Draft, ProcurementStatuses.VendorEvaluation.Final, null,
            new { idempotencyKey = idempotencyKey?.Trim(), entity.FinalScore, entity.Recommendation }, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.Vendor, entity.VendorProfileId, supplierCode, ProcurementDocumentHistoryActions.EvaluationFinal,
            null, null,
            $"Penilaian {entity.ItemCategory?.CategoryName} {label} difinalkan: nilai {entity.FinalScore:0.00}, rekomendasi {Label(RecommendationLabels, entity.Recommendation!)}.",
            new { vendorEvaluationId = id }, actorUserId, now);

        if (entity.PreviousEvaluationId.HasValue)
        {
            var previous = await _dbContext.PrcVendorEvaluations
                .FirstOrDefaultAsync(x => x.Id == entity.PreviousEvaluationId.Value && !x.IsDelete, cancellationToken);
            if (previous is not { Status: ProcurementStatuses.VendorEvaluation.Final })
                throw new ProcurementVendorConflictException("PBJ-VAL-093", "Penilaian asal revisi ini sudah tidak final.");
            previous.Status = ProcurementStatuses.VendorEvaluation.Superseded;
            Touch(previous, actorUserId, now);
            _historyWriter.Record(ProcurementDocumentTypes.VendorEvaluation, previous.Id, null, ProcurementDocumentHistoryActions.Supersede,
                ProcurementStatuses.VendorEvaluation.Final, ProcurementStatuses.VendorEvaluation.Superseded,
                $"Digantikan revisi: {entity.RevisionReason}", new { revisionId = id }, actorUserId, now);
            _historyWriter.Record(ProcurementDocumentTypes.Vendor, entity.VendorProfileId, supplierCode, ProcurementDocumentHistoryActions.EvaluationSupersede,
                null, null, $"Penilaian {entity.ItemCategory?.CategoryName} {label} sebelumnya digantikan revisi.",
                new { vendorEvaluationId = previous.Id, revisionId = id }, actorUserId, now);
        }

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorEvaluation.Finalize", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Buat revisi dari penilaian <c>FINAL</c> (<c>PBJ-VAL-112</c>), dengan alasan (<c>PBJ-VAL-104</c>). Revisi menyalin
    /// kriteria, nilai, rekomendasi, dan catatan penilaian asal; pembuat revisi menjadi penilai revisi.
    /// </summary>
    public async Task<VendorEvaluationResponse> ReviseAsync(Guid id, ReviseVendorEvaluationRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var reason = RequireReason(request.Reason);
        var source = await LoadAsync(id, cancellationToken);
        if (source.Status != ProcurementStatuses.VendorEvaluation.Final
            || await _dbContext.PrcVendorEvaluations.AnyAsync(x => x.PreviousEvaluationId == id && !x.IsDelete
                && x.Status == ProcurementStatuses.VendorEvaluation.Draft, cancellationToken))
            throw new ProcurementVendorConflictException("PBJ-VAL-112", "Revisi hanya dari penilaian final dan hanya satu revisi berjalan.");

        var now = DateTimeOffset.UtcNow;
        var revision = new PrcVendorEvaluation
        {
            VendorProfileId = source.VendorProfileId,
            ItemCategoryId = source.ItemCategoryId,
            PeriodYear = source.PeriodYear,
            PeriodType = source.PeriodType,
            PeriodNo = source.PeriodNo,
            SourceType = source.SourceType,
            Status = ProcurementStatuses.VendorEvaluation.Draft,
            Recommendation = source.Recommendation,
            Notes = source.Notes,
            PreviousEvaluationId = source.Id,
            RevisionReason = reason,
            CreateDateTime = now.UtcDateTime,
            CreateBy = actorUserId
        };
        foreach (var score in source.Scores.Where(x => !x.IsDelete))
        {
            revision.Scores.Add(new PrcVendorEvaluationScore
            {
                CriterionId = score.CriterionId,
                CriterionName = score.CriterionName,
                WeightPercent = score.WeightPercent,
                Score = score.Score,
                Note = score.Note,
                CreateDateTime = now.UtcDateTime,
                CreateBy = actorUserId
            });
        }

        revision.FinalScore = ComputeFinalScore(revision.Scores);
        _dbContext.PrcVendorEvaluations.Add(revision);
        _historyWriter.Record(ProcurementDocumentTypes.VendorEvaluation, revision.Id, null, ProcurementDocumentHistoryActions.Create,
            null, ProcurementStatuses.VendorEvaluation.Draft, $"Revisi dari penilaian final: {reason}",
            new { previousEvaluationId = source.Id }, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorEvaluation, source.Id, null, ProcurementDocumentHistoryActions.Revise,
            source.Status, source.Status, reason, new { revisionId = revision.Id }, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorEvaluation.Revise", revision, actorUserId);
        return await GetByIdAsync(revision.Id, cancellationToken);
    }

    /// <summary>Batalkan draf (<c>DRAFT → CANCELLED</c>) dengan alasan (<c>PBJ-VAL-104</c>). Penilaian asal revisi tetap final.</summary>
    public async Task<VendorEvaluationResponse> CancelAsync(Guid id, CancelVendorEvaluationRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        RequireActor(actorUserId);
        var reason = RequireReason(request.Reason);
        var entity = await LoadAsync(id, cancellationToken);
        RequireDraft(entity, "dibatalkan");
        if (request.ExpectedRowVersion is Guid expected && expected != entity.RowVersion)
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);

        var now = DateTimeOffset.UtcNow;
        entity.Status = ProcurementStatuses.VendorEvaluation.Cancelled;
        entity.IsCancel = true;
        entity.CancelDateTime = now.UtcDateTime;
        entity.CancelBy = actorUserId;
        Touch(entity, actorUserId, now);
        _historyWriter.Record(ProcurementDocumentTypes.VendorEvaluation, id, null, ProcurementDocumentHistoryActions.Cancel,
            ProcurementStatuses.VendorEvaluation.Draft, ProcurementStatuses.VendorEvaluation.Cancelled, reason, null, actorUserId, now);

        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementVendorEvaluation.Cancel", entity, actorUserId);
        return await GetByIdAsync(id, cancellationToken);
    }

    // ------------------------------------------------------------------ bantu

    private async Task EnsurePeriodFreeAsync(
        Guid profileId, Guid categoryId, int year, string periodType, int periodNo, string supplierName, string categoryName,
        CancellationToken cancellationToken)
    {
        // Mencakup revisi juga: setelah penilaian asal SUPERSEDED, revisi FINAL-nya tetap mengisi periode itu.
        var exists = await _dbContext.PrcVendorEvaluations.AnyAsync(x =>
            x.VendorProfileId == profileId && x.ItemCategoryId == categoryId && x.PeriodYear == year && x.PeriodType == periodType
            && x.PeriodNo == periodNo && !x.IsDelete
            && (x.Status == ProcurementStatuses.VendorEvaluation.Draft || x.Status == ProcurementStatuses.VendorEvaluation.Final),
            cancellationToken);
        if (exists)
            throw new ProcurementVendorConflictException("PBJ-VAL-109",
                $"Penilaian {supplierName} {categoryName} {PeriodLabel(periodType, periodNo, year)} sudah ada. Buat revisi bila perlu koreksi.");
    }

    private async Task<PrcVendorEvaluation> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.PrcVendorEvaluations
            .Include(x => x.Scores)
            .Include(x => x.ItemCategory)
            .Include(x => x.VendorProfile!).ThenInclude(x => x.Supplier)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.VendorProfile!.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException(NotFoundMessage);

    /// <summary>Rata-rata berbobot dua desimal; kosong bila ada kriteria yang belum dinilai.</summary>
    private static decimal? ComputeFinalScore(IEnumerable<PrcVendorEvaluationScore> scores)
    {
        var list = scores.Where(x => !x.IsDelete).ToList();
        var totalWeight = list.Sum(x => x.WeightPercent);
        if (list.Count == 0 || totalWeight <= 0 || list.Any(x => !x.Score.HasValue)) return null;
        var weighted = list.Sum(x => x.Score!.Value * x.WeightPercent) / totalWeight;
        return decimal.Round(weighted, 2, MidpointRounding.AwayFromZero);
    }

    public static string PeriodLabel(string periodType, int periodNo, int periodYear) => periodType switch
    {
        "SEMESTER" => $"Semester {Roman(periodNo)} {periodYear}",
        "QUARTER" => $"Triwulan {Roman(periodNo)} {periodYear}",
        "YEAR" => $"Tahun {periodYear}",
        _ => $"{periodType} {periodNo} {periodYear}"
    };

    private static string Roman(int value) => value switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", _ => value.ToString() };

    private static string Label(IReadOnlyDictionary<string, string> labels, string value) =>
        labels.TryGetValue(value, out var label) ? label : value;

    private static void RequireDraft(PrcVendorEvaluation entity, string action)
    {
        if (entity.Status != ProcurementStatuses.VendorEvaluation.Draft)
            throw new ProcurementVendorConflictException("PBJ-VAL-093", $"Hanya penilaian draf yang dapat {action}. Penilaian final dikoreksi lewat revisi.");
    }

    private static string? NormalizePeriodType(string? value, bool required)
    {
        if (string.IsNullOrWhiteSpace(value)) return required ? "SEMESTER" : null;
        var normalized = value.Trim().ToUpperInvariant();
        if (!PeriodTypes.ContainsKey(normalized))
            throw new ProcurementVendorValidationException(null, $"Jenis periode '{value.Trim()}' tidak dikenal. Pilih SEMESTER, QUARTER, atau YEAR.");
        return normalized;
    }

    private static string? NormalizeOption(string? value, string label, IEnumerable<string> allowed)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToUpperInvariant();
        if (!allowed.Contains(normalized))
            throw new ProcurementVendorValidationException(null, $"{label} '{value.Trim()}' tidak dikenal.");
        return normalized;
    }

    private static string? Optional(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ProcurementVendorValidationException(null, $"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private static string RequireReason(string? reason)
    {
        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length < 10)
            throw new ProcurementVendorValidationException("PBJ-VAL-104", ReasonMessage);
        return trimmed;
    }

    private static void RequireActor(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty)
            throw new ProcurementVendorForbiddenException(null, "Sesi pengguna tidak dikenali. Silakan masuk ulang.");
    }

    private static void Touch(PrcVendorEvaluation entity, Guid actorUserId, DateTimeOffset now)
    {
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = now.UtcDateTime;
        entity.UpdateBy = actorUserId;
    }

    private async Task SaveAsync(CancellationToken cancellationToken, string? supplierName = null, string? categoryName = null, string? periodLabel = null)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementVendorConflictException("PBJ-VAL-090", StaleMessage);
        }
        catch (DbUpdateException exception) when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Dua orang membuat penilaian periode yang sama bersamaan; index unik parsial menahan yang kedua.
            throw new ProcurementVendorConflictException("PBJ-VAL-109", supplierName == null
                ? "Penilaian vendor untuk kategori dan periode ini sudah ada. Buat revisi bila perlu koreksi."
                : $"Penilaian {supplierName} {categoryName} {periodLabel} sudah ada. Buat revisi bila perlu koreksi.");
        }
    }

    private Task AuditAsync(string action, PrcVendorEvaluation entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan penilaian vendor.", new
        {
            EntityId = entity.Id,
            entity.VendorProfileId,
            entity.ItemCategoryId,
            entity.Status,
            ActorUserId = actorUserId
        });
}

/// <summary>Penyedia hak lampiran <c>PBJ_VENDOR_EVALUATION</c> untuk <see cref="ProcurementAttachmentService"/> (BE-PBJ-004).</summary>
public sealed class VendorEvaluationAttachmentAccessProvider : IProcurementDocumentAccessProvider
{
    private readonly ProcurementVendorEvaluationService _service;

    public VendorEvaluationAttachmentAccessProvider(ProcurementVendorEvaluationService service) => _service = service;

    public string DocumentType => ProcurementDocumentTypes.VendorEvaluation;

    public Task<ProcurementDocumentAccessLevel> GetAccessAsync(ClaimsPrincipal user, Guid documentId, CancellationToken cancellationToken) =>
        _service.GetAccessLevelAsync(user, documentId, cancellationToken);
}
