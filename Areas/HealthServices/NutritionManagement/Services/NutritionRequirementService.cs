using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

/// <summary>
/// Kebutuhan nutrisi pasien: penetapan, revisi, dan riwayatnya (`GIZ-DEC-012`).
/// </summary>
/// <remarks>
/// <para>
/// Perubahan tidak pernah menimpa. Setiap penetapan melahirkan revisi baru bernomor urut, dan
/// revisi sebelumnya tetap terbaca — itulah bentuk histori yang diminta pemilik proses.
/// </para>
/// <para>
/// Service ini <b>tidak memuat rumus apa pun</b>. Selama registry rumus kosong (`GIZ-OQ-007`
/// ditunda), nilai kalkulasi dibiarkan kosong dan ahli gizi mengisi nilai final sendiri.
/// </para>
/// </remarks>
public sealed class NutritionRequirementService
{
    private const string LogCategory = "NutritionManagement";
    private const string SaveAction = "SaveRequirement";

    private static readonly GziOrderStatus[] OpenStatuses =
        [GziOrderStatus.Requested, GziOrderStatus.InProgress];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly NutritionRequirementCalculator _calculator;

    public NutritionRequirementService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        NutritionRequirementCalculator calculator)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _calculator = calculator;
    }

    // ================================================================== pembacaan

    /// <summary>Seluruh revisi kebutuhan pada satu order, terbaru di atas.</summary>
    public async Task<List<GziRequirementResponse>> GetHistoryAsync(Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var rows = await BaseQuery()
            .Where(x => x.NutritionOrderId == orderId)
            .OrderByDescending(x => x.RevisionNumber)
            .ToListAsync(cancellationToken);

        return rows.Select(Map).ToList();
    }

    public async Task<GziRequirementResponse?> GetCurrentAsync(Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var row = await BaseQuery()
            .FirstOrDefaultAsync(x => x.NutritionOrderId == orderId && x.IsCurrent, cancellationToken);

        return row == null ? null : Map(row);
    }

    // =================================================================== perintah

    /// <summary>
    /// Menetapkan revisi kebutuhan nutrisi baru. Revisi sebelumnya tidak dihapus, hanya
    /// berhenti berlaku.
    /// </summary>
    public async Task<GziRequirementResponse> SaveAsync(Guid orderId,
        SaveGzRequirementRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new ArgumentException("Idempotency key wajib diisi.");

        var actorUserId = GetCurrentUserId();
        var fingerprint = Hash(string.Join('|', orderId, request.CareRecordId,
            request.DeterminedByWorkforceId, request.CalculationFormulaId,
            Normalize(request.ChangeReason),
            string.Join(';', request.Items
                .OrderBy(x => x.NutritionParameterId)
                .Select(x => $"{x.NutritionParameterId}:{x.CalculatedValue}:{x.FinalValue}"))));

        var prior = await _dbContext.GziNutritionOrderHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Action == SaveAction &&
                                      x.CorrelationId == request.IdempotencyKey.Trim() &&
                                      !x.IsDelete, cancellationToken);
        if (prior != null)
        {
            if (!string.Equals(prior.Source, BuildSource(fingerprint), StringComparison.Ordinal))
                throw new NutritionConflictException("GIZ013",
                    "Idempotency key dipakai dengan isi permintaan yang berbeda.");

            return (await GetCurrentAsync(orderId, cancellationToken))
                ?? throw new KeyNotFoundException("Kebutuhan nutrisi tidak ditemukan.");
        }

        var order = await _dbContext.GziNutritionOrders
            .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Order konsultasi gizi tidak ditemukan.");

        if (!OpenStatuses.Contains(order.Status))
            throw new NutritionConflictException("GIZ004",
                "Kebutuhan nutrisi hanya dapat ditetapkan pada order yang masih berjalan.");

        await EnsureWorkforceActiveAsync(request.DeterminedByWorkforceId, cancellationToken);

        if (request.CareRecordId.HasValue)
        {
            var validRecord = await _dbContext.GziNutritionCareRecords.AsNoTracking()
                .AnyAsync(x => x.Id == request.CareRecordId && x.NutritionOrderId == orderId &&
                               !x.IsDelete, cancellationToken);
            if (!validRecord)
                throw new NutritionUnprocessableException("GIZ004",
                    "Kunjungan yang dirujuk bukan milik order ini.");
        }

        var formula = await ResolveFormulaAsync(request.CalculationFormulaId, cancellationToken);
        var parameters = await LoadActiveParametersAsync(cancellationToken);

        ValidateItems(request.Items, parameters);

        var previous = await _dbContext.GziNutritionRequirements
            .Where(x => x.NutritionOrderId == orderId && !x.IsDelete)
            .OrderByDescending(x => x.RevisionNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var revisionNumber = (previous?.RevisionNumber ?? 0) + 1;

        // `GIZ017`. Revisi pertama adalah penetapan awal, jadi belum ada yang perlu dijelaskan.
        // Revisi berikutnya mengubah angka yang sudah dipakai merawat pasien, dan perubahan
        // tanpa alasan tidak dapat ditelaah ketika hasilnya dipersoalkan.
        if (revisionNumber > 1 && string.IsNullOrWhiteSpace(request.ChangeReason))
            throw new NutritionUnprocessableException("GIZ017",
                "Alasan perubahan wajib diisi mulai revisi kedua.");

        var now = DateTime.UtcNow;

        if (previous is { IsCurrent: true })
        {
            previous.IsCurrent = false;
            previous.UpdateDateTime = now;
            previous.UpdateBy = actorUserId;
        }

        var requirement = new GziNutritionRequirement
        {
            Id = DeterministicId(request.IdempotencyKey),
            NutritionOrderId = orderId,
            CareRecordId = request.CareRecordId,
            RevisionNumber = revisionNumber,
            IsCurrent = true,
            EffectiveFrom = request.EffectiveFrom?.ToUniversalTime() ?? now,
            CalculationFormulaId = formula?.Id,
            CalculationInput = request.CalculationInput == null
                ? null
                : JsonSerializer.Serialize(request.CalculationInput),
            CalculationPerformedAt = formula == null ? null : now,
            DeterminedByWorkforceId = request.DeterminedByWorkforceId,
            ChangeReason = Normalize(request.ChangeReason),
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        // Ditambahkan lewat DbSet, bukan lewat navigasi induk yang sudah dilacak, agar entity
        // baru pasti berstatus Added walaupun kuncinya diisi dari sisi aplikasi.
        _dbContext.GziNutritionRequirements.Add(requirement);

        foreach (var item in request.Items)
        {
            var adjusted = item.CalculatedValue.HasValue &&
                           item.CalculatedValue.Value != item.FinalValue;

            _dbContext.GziNutritionRequirementItems.Add(new GziNutritionRequirementItem
            {
                Id = Guid.NewGuid(),
                NutritionRequirementId = requirement.Id,
                NutritionParameterId = item.NutritionParameterId,
                CalculatedValue = item.CalculatedValue,
                FinalValue = item.FinalValue,
                AdjustmentReason = Normalize(item.AdjustmentReason),
                AdjustedByWorkforceId = adjusted ? request.DeterminedByWorkforceId : null,
                AdjustedAt = adjusted ? now : null,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        _dbContext.GziNutritionOrderHistories.Add(new GziNutritionOrderHistory
        {
            NutritionOrderId = orderId,
            FromStatus = order.Status,
            ToStatus = order.Status,
            Action = SaveAction,
            Reason = revisionNumber == 1
                ? "Penetapan kebutuhan nutrisi."
                : $"Revisi kebutuhan nutrisi ke-{revisionNumber}.",
            ActorUserId = actorUserId,
            OccurredAt = now,
            Source = BuildSource(fingerprint),
            CorrelationId = request.IdempotencyKey.Trim(),
            CreateDateTime = now,
            CreateBy = actorUserId
        });

        await SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "NutritionRequirement.Save",
            "Menetapkan kebutuhan nutrisi pasien.",
            new
            {
                requirement.Id,
                requirement.NutritionOrderId,
                requirement.RevisionNumber,
                ItemCount = request.Items.Count,
                ActorUserId = actorUserId
            });

        return (await GetCurrentAsync(orderId, cancellationToken))!;
    }

    /// <summary>
    /// Pratinjau kalkulasi tanpa menyimpan apa pun.
    /// </summary>
    /// <remarks>
    /// Selama registry rumus kosong, jawabannya selalu "tidak ada rumus terdaftar" dan seluruh
    /// nilai kalkulasi kosong. Itu bukan kegagalan, melainkan keadaan yang diputuskan pemilik
    /// proses ketika `GIZ-OQ-007` ditunda.
    /// </remarks>
    public async Task<GziCalculationPreviewResponse> CalculateAsync(
        GziCalculationPreviewRequest request, CancellationToken cancellationToken = default)
    {
        var parameters = await LoadActiveParametersAsync(cancellationToken);
        var formula = await ResolveFormulaAsync(request.CalculationFormulaId, cancellationToken);

        var input = new NutritionCalculationInput(
            request.Input.WeightKg, request.Input.HeightCm, request.Input.AgeYears,
            request.Input.Gender, request.Input.ActivityFactor, request.Input.StressFactor);

        var outputs = _calculator.Calculate(formula, input);
        var byCode = outputs.ToDictionary(x => x.ParameterCode, x => x.Value,
            StringComparer.OrdinalIgnoreCase);

        return new GziCalculationPreviewResponse
        {
            CalculationFormulaId = formula?.Id,
            CalculationFormulaCode = formula?.FormulaCode,
            Calculated = outputs.Count > 0,
            Message = outputs.Count > 0
                ? "Nilai awal dihitung. Ahli gizi tetap dapat mengoreksinya sebelum disimpan."
                : "Belum ada rumus terdaftar, sehingga nilai kebutuhan diisi ahli gizi.",
            Values = parameters.Select(p => new GziCalculatedValueResponse
            {
                NutritionParameterId = p.Id,
                ParameterCode = p.ParameterCode,
                ParameterName = p.ParameterName,
                UnitCode = p.UnitCode,
                CalculatedValue = byCode.TryGetValue(p.ParameterCode, out var v) ? v : null
            }).ToList()
        };
    }

    /// <summary>
    /// Mengganti seluruh diagnosis gizi pada satu kunjungan (`GIZ-DEC-011`).
    /// </summary>
    /// <remarks>
    /// Daftar pengganti, bukan tambahan. Menambah satu per satu membuat pencabutan diagnosis
    /// memerlukan perintah tersendiri, dan layar yang menampilkan daftar harus menebak selisih
    /// antara apa yang terlihat dan apa yang tersimpan.
    /// </remarks>
    public async Task<List<NutritionCareRecordDiagnosisResponse>> SaveCareRecordDiagnosesAsync(
        Guid orderId, Guid careRecordId, SaveGzCareRecordDiagnosesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new ArgumentException("Idempotency key wajib diisi.");

        var actorUserId = GetCurrentUserId();

        var record = await _dbContext.GziNutritionCareRecords
            .Include(x => x.Diagnoses)
            .FirstOrDefaultAsync(x => x.Id == careRecordId && x.NutritionOrderId == orderId &&
                                      !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Catatan kunjungan gizi tidak ditemukan.");

        await EnsureDiagnosesAsync(request.Diagnoses, cancellationToken);

        var now = DateTime.UtcNow;
        var keep = request.Diagnoses.Select(x => x.NutritionDiagnosisId).ToHashSet();

        foreach (var existing in record.Diagnoses.Where(x => !x.IsDelete))
        {
            if (keep.Contains(existing.NutritionDiagnosisId)) continue;
            existing.IsDelete = true;
            existing.DeleteDateTime = now;
            existing.DeleteBy = actorUserId;
        }

        foreach (var wanted in request.Diagnoses)
        {
            var existing = record.Diagnoses
                .FirstOrDefault(x => x.NutritionDiagnosisId == wanted.NutritionDiagnosisId);

            if (existing != null)
            {
                existing.IsDelete = false;
                existing.IsPrimary = wanted.IsPrimary;
                existing.Note = Normalize(wanted.Note);
                existing.SortOrder = wanted.SortOrder;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;
                continue;
            }

            _dbContext.GziNutritionCareRecordDiagnoses.Add(new GziNutritionCareRecordDiagnosis
            {
                Id = Guid.NewGuid(),
                CareRecordId = careRecordId,
                NutritionDiagnosisId = wanted.NutritionDiagnosisId,
                IsPrimary = wanted.IsPrimary,
                Note = Normalize(wanted.Note),
                SortOrder = wanted.SortOrder,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        record.Version++;
        record.UpdateDateTime = now;
        record.UpdateBy = actorUserId;

        await SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "NutritionCareRecordDiagnosis.Save",
            "Menyimpan diagnosis gizi pada kunjungan.",
            new { CareRecordId = careRecordId, orderId, Count = request.Diagnoses.Count, ActorUserId = actorUserId });

        return await GetCareRecordDiagnosesAsync(orderId, careRecordId, cancellationToken);
    }

    public async Task<List<NutritionCareRecordDiagnosisResponse>> GetCareRecordDiagnosesAsync(
        Guid orderId, Guid careRecordId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.GziNutritionCareRecordDiagnoses.AsNoTracking()
            .Where(x => x.CareRecordId == careRecordId && !x.IsDelete &&
                        x.CareRecord != null && x.CareRecord.NutritionOrderId == orderId)
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder)
            .Select(x => new NutritionCareRecordDiagnosisResponse
            {
                Id = x.Id,
                NutritionDiagnosisId = x.NutritionDiagnosisId,
                DiagnosisCode = x.NutritionDiagnosis!.DiagnosisCode,
                DiagnosisName = x.NutritionDiagnosis.DiagnosisName,
                DomainCode = x.NutritionDiagnosis.DiagnosisDomain!.DomainCode,
                IsPrimary = x.IsPrimary,
                Note = x.Note,
                SortOrder = x.SortOrder
            })
            .ToListAsync(cancellationToken);
    }

    // =================================================================== penolong

    private IQueryable<GziNutritionRequirement> BaseQuery() =>
        _dbContext.GziNutritionRequirements.AsNoTracking()
            .Where(x => !x.IsDelete)
            .Include(x => x.CalculationFormula)
            .Include(x => x.DeterminedByWorkforce)
            .Include(x => x.Items.Where(i => !i.IsDelete))
                .ThenInclude(i => i.NutritionParameter);

    private async Task<List<GziNutritionParameter>> LoadActiveParametersAsync(
        CancellationToken cancellationToken) =>
        await _dbContext.GziNutritionParameters.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive)
            .OrderBy(x => x.SortOrder).ThenBy(x => x.ParameterName)
            .ToListAsync(cancellationToken);

    private async Task<GziNutritionFormula?> ResolveFormulaAsync(Guid? formulaId,
        CancellationToken cancellationToken)
    {
        if (!formulaId.HasValue) return null;

        var formula = await _dbContext.GziNutritionFormulas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == formulaId && !x.IsDelete && x.IsActive,
                cancellationToken);

        // `GIZ019`. Rumus yang tidak terdaftar ditolak terang-terangan, bukan diabaikan diam-diam
        // — nilai yang mengaku berasal dari rumus tertentu harus benar-benar berasal dari sana.
        if (formula == null)
            throw new NutritionUnprocessableException("GIZ019",
                "Rumus yang dipilih tidak terdaftar atau tidak aktif.");

        return formula;
    }

    /// <summary>
    /// Menegakkan `GIZ006`, `GIZ014`, `GIZ015`, dan `GIZ016` atas daftar nilai yang dikirim.
    /// </summary>
    private static void ValidateItems(List<GziRequirementItemRequest> items,
        List<GziNutritionParameter> parameters)
    {
        var known = parameters.ToDictionary(x => x.Id);

        var duplicate = items.GroupBy(x => x.NutritionParameterId).Any(g => g.Count() > 1);
        if (duplicate)
            throw new NutritionUnprocessableException("GIZ014",
                "Satu parameter nutrisi hanya boleh dikirim sekali.");

        foreach (var item in items)
        {
            if (!known.TryGetValue(item.NutritionParameterId, out var parameter))
                throw new NutritionUnprocessableException("GIZ014",
                    "Parameter nutrisi yang dikirim tidak dikenal atau tidak aktif.");

            if (parameter.MinValue.HasValue && item.FinalValue < parameter.MinValue.Value)
                throw new NutritionUnprocessableException("GIZ006",
                    $"Nilai {parameter.ParameterName} di bawah batas {parameter.MinValue} {parameter.UnitCode}.");

            if (parameter.MaxValue.HasValue && item.FinalValue > parameter.MaxValue.Value)
                throw new NutritionUnprocessableException("GIZ006",
                    $"Nilai {parameter.ParameterName} di atas batas {parameter.MaxValue} {parameter.UnitCode}.");

            // `GIZ016`. Koreksi terhadap hasil rumus wajib beralasan; tanpa itu tidak ada yang
            // dapat menilai apakah koreksinya wajar ketika hasilnya dipersoalkan kemudian.
            if (item.CalculatedValue.HasValue && item.CalculatedValue.Value != item.FinalValue &&
                string.IsNullOrWhiteSpace(item.AdjustmentReason))
                throw new NutritionUnprocessableException("GIZ016",
                    $"Alasan perubahan wajib diisi karena nilai {parameter.ParameterName} berbeda dari hasil hitungan.");
        }

        // `GIZ015`. Satu revisi harus utuh. Revisi yang hanya memuat sebagian parameter membuat
        // pembacanya menyangka parameter yang hilang bernilai nol, padahal ia sekadar tidak ikut
        // dikirim.
        var missing = parameters.Where(p => items.All(i => i.NutritionParameterId != p.Id)).ToList();
        if (missing.Count > 0)
            throw new NutritionUnprocessableException("GIZ015",
                $"Nilai wajib diisi untuk seluruh parameter aktif. Belum terisi: {string.Join(", ", missing.Select(x => x.ParameterName))}.");
    }

    private async Task EnsureDiagnosesAsync(
        IReadOnlyCollection<NutritionCareRecordDiagnosisRequest> diagnoses,
        CancellationToken cancellationToken)
    {
        if (diagnoses.Count == 0) return;

        var ids = diagnoses.Select(x => x.NutritionDiagnosisId).Distinct().ToList();
        if (ids.Count != diagnoses.Count)
            throw new NutritionUnprocessableException("GIZ005",
                "Satu diagnosis gizi hanya boleh ditegakkan sekali pada satu kunjungan.");

        if (diagnoses.Count(x => x.IsPrimary) > 1)
            throw new NutritionUnprocessableException("GIZ018",
                "Hanya boleh ada satu diagnosis gizi primer pada satu kunjungan.");

        var selectable = await _dbContext.GziNutritionDiagnoses.AsNoTracking()
            .CountAsync(x => ids.Contains(x.Id) && !x.IsDelete && x.IsActive && x.IsSelectable,
                cancellationToken);

        if (selectable != ids.Count)
            throw new NutritionUnprocessableException("GIZ005",
                "Diagnosis yang dipilih bukan diagnosis gizi yang aktif dan dapat dipilih.");
    }

    private async Task EnsureWorkforceActiveAsync(Guid workforceId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceId && !x.IsDelete, cancellationToken);
        if (!exists)
            throw new NutritionUnprocessableException("GIZ004",
                "Ahli gizi yang dipilih tidak ditemukan.");
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new NutritionConflictException("GIZ012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Indeks tersaring pada basis data yang menegakkan `GIZ020` dan `GIZ018`. Pesannya
            // dijadikan benturan, bukan galat sistem, karena penyebabnya memang dua permintaan
            // yang berlomba.
            throw new NutritionConflictException("GIZ020",
                "Kebutuhan nutrisi atau diagnosis primer sudah diubah pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException?.GetType().Name == "PostgresException" &&
        ex.InnerException.Message.Contains("23505", StringComparison.Ordinal);

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new NutritionForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static GziRequirementResponse Map(GziNutritionRequirement x) => new()
    {
        Id = x.Id,
        NutritionOrderId = x.NutritionOrderId,
        CareRecordId = x.CareRecordId,
        RevisionNumber = x.RevisionNumber,
        IsCurrent = x.IsCurrent,
        EffectiveFrom = x.EffectiveFrom,
        CalculationFormulaId = x.CalculationFormulaId,
        CalculationFormulaCode = x.CalculationFormula?.FormulaCode,
        CalculationFormulaName = x.CalculationFormula?.FormulaName,
        CalculationInput = string.IsNullOrWhiteSpace(x.CalculationInput)
            ? null
            : JsonSerializer.Deserialize<GziCalculationInputDto>(x.CalculationInput),
        CalculationPerformedAt = x.CalculationPerformedAt,
        DeterminedByWorkforceId = x.DeterminedByWorkforceId,
        DeterminedByName = x.DeterminedByWorkforce?.DisplayName ?? string.Empty,
        ChangeReason = x.ChangeReason,
        Version = x.Version,
        CreateDateTime = x.CreateDateTime,
        Items = x.Items
            .Where(i => !i.IsDelete)
            .OrderBy(i => i.NutritionParameter != null ? i.NutritionParameter.SortOrder : 0)
            .Select(i => new GziRequirementItemResponse
            {
                Id = i.Id,
                NutritionParameterId = i.NutritionParameterId,
                ParameterCode = i.NutritionParameter?.ParameterCode ?? string.Empty,
                ParameterName = i.NutritionParameter?.ParameterName ?? string.Empty,
                UnitCode = i.NutritionParameter?.UnitCode ?? string.Empty,
                ValueScale = i.NutritionParameter?.ValueScale ?? 0,
                CalculatedValue = i.CalculatedValue,
                FinalValue = i.FinalValue,
                AdjustmentReason = i.AdjustmentReason,
                AdjustedByWorkforceId = i.AdjustedByWorkforceId,
                AdjustedAt = i.AdjustedAt
            })
            .ToList()
    };

    private static string BuildSource(string fingerprint) => $"API:{fingerprint[..46]}";
    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"GziRequirement:{key.Trim()}"))[..16]);
}
