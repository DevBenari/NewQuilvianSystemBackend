using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Services;

/// <summary>
/// Persiapan paket (BE-PBJ-PS-007; PBJ-PS-API 1.0 <c>PUT /{id}</c>, <c>method-suggestion</c>, <c>criteria</c>, <c>evaluators</c>;
/// PBJ-PS-VAL <c>PBJ-VAL-090</c>, <c>125</c>, <c>126</c>, <c>128</c>, <c>149</c>).
/// </summary>
/// <remarks>
/// <para><b>Metode disalin saat dipilih</b> (PBJ-DEC-068): kode, nama, rentang HPS, minimum penawaran, dan wajib evaluasi
/// teknis disimpan di paket. Mengubah master sesudahnya tidak mengubah paket (<c>AT-PS-17</c>). Contoh: paket memilih
/// Penunjukan Langsung (contoh) ≤ Rp 50.000.000; admin lalu menurunkan batasnya ke Rp 45.000.000 → paket tetap mencatat
/// Rp 50.000.000.</para>
/// <para><b>Rentang HPS:</b> <c>Min &lt; HPS ≤ Max</c>, batas kosong = tanpa batas (sama dengan master, BE-PBJ-PS-002). HPS di
/// luar rentang salinan metode wajib beralasan (422 <c>PBJ-VAL-126</c>).</para>
/// <para><b>Checklist disalin saat metode dipilih</b> (PBJ-ASM-007): gabungan dokumen wajib metode dan dokumen wajib jenis
/// pengadaan paket. Status terpenuhinya dihitung dari lampiran paket berkategori kode jenis dokumen.</para>
/// <para><b>Status:</b> hanya <c>DRAFT</c>/<c>PREPARATION_RETURNED</c>. Ubah HPS/metode pada paket yang persiapannya sudah
/// disetujui → 409 <c>PBJ-VAL-149</c>; status lain → 409 <c>PBJ-VAL-128</c>.</para>
/// </remarks>
public sealed class ProcurementPackagePreparationService
{
    private const string LogCategory = "Corporate.ProcurementManagement.Sourcing";
    private const string Resource = ProcurementPermissions.PackageResource;
    private const string NotFoundMessage = "Paket tidak ditemukan atau di luar cakupan Anda.";
    private const string StaleMessage = "Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda.";

    /// <summary>Status sesudah persiapan disetujui: HPS dan metode terkunci (PBJ-DEC-073).</summary>
    private static readonly HashSet<string> HpsLockedStatuses = new(StringComparer.Ordinal)
    {
        ProcurementStatuses.Package.ReadyForRfq,
        ProcurementStatuses.Package.RfqOpen,
        ProcurementStatuses.Package.Evaluation,
        ProcurementStatuses.Package.AwardPendingApproval,
        ProcurementStatuses.Package.Awarded,
        ProcurementStatuses.Package.Ordered
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly ProcurementPackageService _packageService;
    private readonly AccessPermissionService _accessPermissionService;
    private readonly ProcurementDocumentHistoryWriter _historyWriter;
    private readonly LoggerService _loggerService;

    public ProcurementPackagePreparationService(
        ApplicationDbContext dbContext,
        ProcurementPackageService packageService,
        AccessPermissionService accessPermissionService,
        ProcurementDocumentHistoryWriter historyWriter,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _packageService = packageService;
        _accessPermissionService = accessPermissionService;
        _historyWriter = historyWriter;
        _loggerService = loggerService;
    }

    /// <summary>Apakah HPS berada di rentang metode: <c>Min &lt; HPS ≤ Max</c>, batas kosong = tanpa batas.</summary>
    public static bool IsInRange(decimal hps, decimal? minHps, decimal? maxHps) =>
        (!minHps.HasValue || hps > minHps.Value) && (!maxHps.HasValue || hps <= maxHps.Value);

    // ------------------------------------------------------------------ baca

    public async Task<MethodSuggestionResponse> GetMethodSuggestionAsync(ClaimsPrincipal user, Guid id, CancellationToken cancellationToken)
    {
        if (!await _packageService.IsInScopeAsync(user, id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        var package = await _dbContext.PrcPackages.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.HpsTotal, x.MethodId, x.MethodMinHpsAmount, x.MethodMaxHpsAmount })
            .FirstAsync(cancellationToken);

        var methods = await _dbContext.MstProcurementMethods.AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .OrderBy(x => x.MinHpsAmount.HasValue).ThenBy(x => x.MinHpsAmount).ThenBy(x => x.MethodName)
            .Select(x => new MethodSuggestionItem
            {
                Id = x.Id, Code = x.MethodCode, Name = x.MethodName, MinHpsAmount = x.MinHpsAmount, MaxHpsAmount = x.MaxHpsAmount,
                MinimumQuotationCount = x.MinimumQuotationCount, RequiresTechnicalEvaluation = x.RequiresTechnicalEvaluation,
                IsSample = x.IsSample
            })
            .ToListAsync(cancellationToken);

        return new MethodSuggestionResponse
        {
            HpsTotal = await CanViewPriceAsync(user) ? package.HpsTotal : null,
            CurrentMethodId = package.MethodId,
            IsCurrentMethodInRange = package.MethodId.HasValue
                ? IsInRange(package.HpsTotal, package.MethodMinHpsAmount, package.MethodMaxHpsAmount)
                : null,
            Suggestions = methods.Where(x => IsInRange(package.HpsTotal, x.MinHpsAmount, x.MaxHpsAmount)).ToList()
        };
    }

    // ------------------------------------------------------------------ tulis

    /// <summary>Ubah judul, spesifikasi dan HPS satuan barang, metode, dan alasan di luar rentang.</summary>
    public async Task<PackageResponse> UpdateAsync(
        ClaimsPrincipal user, Guid id, UpdatePackagePreparationRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        var package = await LoadForPreparationAsync(user, id, request.ExpectedRowVersion, isHpsOrMethodChange: true, cancellationToken);
        var title = request.Title?.Trim() ?? string.Empty;
        if (title.Length == 0) throw new ProcurementSourcingBadRequestException("Judul paket wajib diisi.");

        var changes = request.Items ?? [];
        if (changes.Select(x => x.PackageItemId).Distinct().Count() != changes.Count)
            throw new ProcurementSourcingBadRequestException("Satu barang paket hanya boleh disebut sekali.");
        var canViewPrice = await CanViewPriceAsync(user);
        if (!canViewPrice && changes.Any(x => x.HpsUnitPrice.HasValue))
            throw new ProcurementSourcingForbiddenException("Anda tidak berhak mengubah HPS karena tidak berhak melihat harga paket.");

        var now = DateTimeOffset.UtcNow;
        var before = new
        {
            package.Title, package.MethodCode, package.HpsTotal, package.IsMethodOutOfRange, package.MethodDeviationReason
        };
        var items = await _dbContext.PrcPackageItems
            .Where(x => x.PackageId == id && !x.IsDelete)
            .ToListAsync(cancellationToken);
        foreach (var change in changes)
        {
            var item = items.FirstOrDefault(x => x.Id == change.PackageItemId)
                ?? throw new ProcurementSourcingBadRequestException("Barang paket tidak dikenal pada paket ini. Muat ulang paket.");
            item.Specification = string.IsNullOrWhiteSpace(change.Specification) ? null : change.Specification.Trim();
            if (change.HpsUnitPrice.HasValue)
            {
                item.HpsUnitPrice = Math.Round(change.HpsUnitPrice.Value, 2, MidpointRounding.AwayFromZero);
                item.HpsAmount = Math.Round(item.Quantity * item.HpsUnitPrice, 2, MidpointRounding.AwayFromZero);
            }
            item.UpdateBy = actorUserId;
            item.UpdateDateTime = now.UtcDateTime;
        }
        package.HpsTotal = items.Sum(x => x.HpsAmount);

        var methodChanged = request.MethodId != package.MethodId;
        if (methodChanged) await CopyMethodAsync(package, request.MethodId, cancellationToken);

        var outOfRange = package.MethodId.HasValue &&
                         !IsInRange(package.HpsTotal, package.MethodMinHpsAmount, package.MethodMaxHpsAmount);
        var reason = string.IsNullOrWhiteSpace(request.MethodDeviationReason) ? null : request.MethodDeviationReason.Trim();
        if (outOfRange && reason == null)
            throw new ProcurementSourcingValidationException("PBJ-VAL-126", "Nilai HPS di luar rentang metode ini. Tuliskan alasannya.");
        package.IsMethodOutOfRange = outOfRange;
        package.MethodDeviationReason = outOfRange ? reason : null;

        if (methodChanged || !await _dbContext.PrcPackageChecklistItems.AnyAsync(x => x.PackageId == id && !x.IsDelete, cancellationToken))
            await RebuildChecklistAsync(package, actorUserId, now, cancellationToken);

        package.Title = title;
        Touch(package, actorUserId, now);
        // Cuplikan memuat HPS lama → baru (PBJ-PS-PERM bagian 5); riwayat API tidak mengembalikan cuplikan, logger tidak memuatnya.
        _historyWriter.Record(
            ProcurementDocumentTypes.Package, package.Id, package.PackageNumber, ProcurementDocumentHistoryActions.Update,
            package.Status, package.Status, package.MethodDeviationReason,
            new
            {
                Before = before,
                After = new { package.Title, package.MethodCode, package.HpsTotal, package.IsMethodOutOfRange, package.MethodDeviationReason },
                ItemCount = changes.Count
            },
            actorUserId, now);
        await SaveAsync(cancellationToken);

        await AuditAsync("ProcurementPackage.UpdatePreparation", package, actorUserId);
        return await _packageService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Ganti kriteria teknis satu barang. Butir lama ditimpa sesuai nomornya, kelebihannya dihapus lunak, sehingga nomor butir
    /// tetap 1, 2, 3, … tanpa bentrok index unik. Contoh: ["Rangka baja", "Roda belakang 24 inci"].
    /// </summary>
    public async Task<PackageResponse> UpdateCriteriaAsync(
        ClaimsPrincipal user, Guid id, Guid itemId, UpdateTechnicalCriteriaRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        var package = await LoadForPreparationAsync(user, id, request.ExpectedRowVersion, isHpsOrMethodChange: false, cancellationToken);
        var item = await _dbContext.PrcPackageItems.AsNoTracking()
            .AnyAsync(x => x.Id == itemId && x.PackageId == id && !x.IsDelete, cancellationToken);
        if (!item) throw new KeyNotFoundException("Barang paket tidak ditemukan. Muat ulang paket.");

        var texts = (request.Criteria ?? []).Select(x => x.CriterionText?.Trim() ?? string.Empty).ToList();
        if (texts.Any(x => x.Length == 0))
            throw new ProcurementSourcingBadRequestException("Isi setiap butir kriteria teknis, atau hapus butir yang kosong.");

        var now = DateTimeOffset.UtcNow;
        var existing = await _dbContext.PrcTechnicalCriteria
            .Where(x => x.PackageItemId == itemId && !x.IsDelete)
            .OrderBy(x => x.CriterionNo)
            .ToListAsync(cancellationToken);
        for (var index = 0; index < texts.Count; index++)
        {
            var no = index + 1;
            var row = existing.FirstOrDefault(x => x.CriterionNo == no);
            if (row == null)
            {
                _dbContext.PrcTechnicalCriteria.Add(new PrcTechnicalCriterion
                {
                    PackageItemId = itemId, CriterionNo = no, CriterionText = texts[index],
                    CreateBy = actorUserId, CreateDateTime = now.UtcDateTime
                });
            }
            else if (!string.Equals(row.CriterionText, texts[index], StringComparison.Ordinal))
            {
                row.CriterionText = texts[index];
                row.UpdateBy = actorUserId;
                row.UpdateDateTime = now.UtcDateTime;
            }
        }
        foreach (var row in existing.Where(x => x.CriterionNo > texts.Count))
        {
            row.IsDelete = true;
            row.DeleteBy = actorUserId;
            row.DeleteDateTime = now.UtcDateTime;
        }

        Touch(package, actorUserId, now);
        _historyWriter.Record(
            ProcurementDocumentTypes.Package, package.Id, package.PackageNumber, ProcurementDocumentHistoryActions.Update,
            package.Status, package.Status, null, new { PackageItemId = itemId, CriteriaCount = texts.Count }, actorUserId, now);
        await SaveAsync(cancellationToken);

        await AuditAsync("ProcurementPackage.UpdateCriteria", package, actorUserId);
        return await _packageService.GetByIdAsync(user, id, cancellationToken);
    }

    /// <summary>
    /// Ganti evaluator aktif paket (PBJ-DEC-074). Evaluator yang tidak disebut dinonaktifkan; yang baru ditambahkan dengan
    /// pemberi tugas = pengguna login. Contoh: [Dedi (IPSRS)] → Dedi dapat membaca lampiran non-harga paket ini.
    /// </summary>
    public async Task<PackageResponse> UpdateEvaluatorsAsync(
        ClaimsPrincipal user, Guid id, UpdateEvaluatorsRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = ProcurementDataScopeService.GetUserId(user);
        var package = await LoadForPreparationAsync(user, id, request.ExpectedRowVersion, isHpsOrMethodChange: false, cancellationToken);
        var ids = (request.EvaluatorUserIds ?? []).Where(x => x != Guid.Empty).Distinct().ToList();
        var known = await _dbContext.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id) && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
        if (known.Count != ids.Count)
            throw new ProcurementSourcingBadRequestException("Evaluator tidak ditemukan atau akunnya tidak aktif.");

        var now = DateTimeOffset.UtcNow;
        var active = await _dbContext.PrcPackageEvaluators
            .Where(x => x.PackageId == id && x.IsActive && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var removed = active.Where(x => !ids.Contains(x.EvaluatorUserId)).ToList();
        foreach (var row in removed)
        {
            row.IsActive = false;
            row.UpdateBy = actorUserId;
            row.UpdateDateTime = now.UtcDateTime;
        }
        var added = ids.Where(x => active.All(a => a.EvaluatorUserId != x)).ToList();
        foreach (var evaluatorId in added)
        {
            _dbContext.PrcPackageEvaluators.Add(new PrcPackageEvaluator
            {
                PackageId = id, EvaluatorUserId = evaluatorId, AssignedByUserId = actorUserId, IsActive = true,
                CreateBy = actorUserId, CreateDateTime = now.UtcDateTime
            });
        }

        Touch(package, actorUserId, now);
        _historyWriter.Record(
            ProcurementDocumentTypes.Package, package.Id, package.PackageNumber, ProcurementDocumentHistoryActions.Update,
            package.Status, package.Status, null,
            new { EvaluatorsAdded = added, EvaluatorsRemoved = removed.Select(x => x.EvaluatorUserId).ToList() }, actorUserId, now);
        await SaveAsync(cancellationToken);

        await AuditAsync("ProcurementPackage.UpdateEvaluators", package, actorUserId);
        return await _packageService.GetByIdAsync(user, id, cancellationToken);
    }

    // ------------------------------------------------------------------ bantuan

    private async Task<PrcPackage> LoadForPreparationAsync(
        ClaimsPrincipal user, Guid id, Guid expectedRowVersion, bool isHpsOrMethodChange, CancellationToken cancellationToken)
    {
        if (!await _packageService.IsInScopeAsync(user, id, cancellationToken))
            throw new KeyNotFoundException(NotFoundMessage);
        var package = await _dbContext.PrcPackages.FirstAsync(x => x.Id == id, cancellationToken);
        if (!ProcurementStatuses.Package.IsPreparationEditable(package.Status))
        {
            if (isHpsOrMethodChange && HpsLockedStatuses.Contains(package.Status))
                throw new ProcurementSourcingConflictException("PBJ-VAL-149", "HPS sudah disetujui. Buka ulang persiapan untuk mengubahnya.");
            throw new ProcurementSourcingConflictException("PBJ-VAL-128",
                $"Tindakan ini tidak dapat dilakukan pada status {ProcurementPackageService.LabelOf(package.Status)}.");
        }
        if (package.RowVersion != expectedRowVersion)
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);
        return package;
    }

    /// <summary>Salin metode dan aturannya ke paket (PBJ-DEC-068); <c>null</c> mengosongkan metode.</summary>
    private async Task CopyMethodAsync(PrcPackage package, Guid? methodId, CancellationToken cancellationToken)
    {
        if (!methodId.HasValue)
        {
            package.MethodId = null;
            package.MethodCode = null;
            package.MethodName = null;
            package.MethodMinHpsAmount = null;
            package.MethodMaxHpsAmount = null;
            package.MethodMinimumQuotationCount = null;
            package.MethodRequiresTechnicalEvaluation = false;
            return;
        }

        var method = await _dbContext.MstProcurementMethods.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == methodId.Value && x.IsActive && !x.IsDelete, cancellationToken)
            ?? throw new ProcurementSourcingValidationException("PBJ-VAL-125", "Pilih metode pengadaan yang aktif.");
        package.MethodId = method.Id;
        package.MethodCode = method.MethodCode;
        package.MethodName = method.MethodName;
        package.MethodMinHpsAmount = method.MinHpsAmount;
        package.MethodMaxHpsAmount = method.MaxHpsAmount;
        package.MethodMinimumQuotationCount = method.MinimumQuotationCount;
        package.MethodRequiresTechnicalEvaluation = method.RequiresTechnicalEvaluation;
    }

    /// <summary>
    /// Salin checklist = dokumen wajib metode paket ∪ dokumen wajib jenis pengadaan paket (PBJ-ASM-007), hanya yang aktif.
    /// Butir yang sudah ada dipertahankan (salinan nama tetap); butir yang tidak lagi wajib dihapus lunak.
    /// </summary>
    private async Task RebuildChecklistAsync(PrcPackage package, Guid actorUserId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var methodId = package.MethodId;
        var procurementTypeId = package.ProcurementTypeId;
        var required = await _dbContext.MstProcurementPackageDocumentRequirements.AsNoTracking()
            .Where(r => r.IsActive && !r.IsDelete && r.DocumentType!.IsActive && !r.DocumentType.IsDelete
                && ((methodId != null && r.MethodId == methodId) || r.ProcurementTypeId == procurementTypeId))
            .Select(r => new { r.DocumentTypeId, r.DocumentType!.DocumentTypeCode, r.DocumentType.DocumentTypeName })
            .Distinct()
            .ToListAsync(cancellationToken);
        var existing = await _dbContext.PrcPackageChecklistItems
            .Where(x => x.PackageId == package.Id && !x.IsDelete)
            .ToListAsync(cancellationToken);

        foreach (var row in existing.Where(x => required.All(r => r.DocumentTypeId != x.DocumentTypeId)))
        {
            row.IsDelete = true;
            row.DeleteBy = actorUserId;
            row.DeleteDateTime = now.UtcDateTime;
        }
        foreach (var doc in required.Where(r => existing.All(x => x.DocumentTypeId != r.DocumentTypeId)))
        {
            _dbContext.PrcPackageChecklistItems.Add(new PrcPackageChecklistItem
            {
                PackageId = package.Id, DocumentTypeId = doc.DocumentTypeId, DocumentTypeCode = doc.DocumentTypeCode,
                DocumentTypeName = doc.DocumentTypeName, IsRequired = true, CreateBy = actorUserId, CreateDateTime = now.UtcDateTime
            });
        }
    }

    private static void Touch(PrcPackage package, Guid actorUserId, DateTimeOffset now)
    {
        package.RowVersion = Guid.NewGuid();
        package.UpdateBy = actorUserId;
        package.UpdateDateTime = now.UtcDateTime;
    }

    private Task<bool> CanViewPriceAsync(ClaimsPrincipal user) =>
        _accessPermissionService.HasAccessAsync(user, Resource, ProcurementPermissions.ReadPriceAction);

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);
        }
        catch (DbUpdateException exception) when ((exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            // Dua petugas menyimpan checklist, kriteria, atau evaluator paket yang sama bersamaan.
            throw new ProcurementSourcingConflictException("PBJ-VAL-090", StaleMessage);
        }
    }

    /// <summary>Jejak audit tanpa field harga (PBJ-PS-PERM bagian 6).</summary>
    private Task AuditAsync(string action, PrcPackage package, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan persiapan paket pengadaan.", new
        {
            EntityId = package.Id,
            package.Status,
            package.MethodCode,
            package.IsMethodOutOfRange,
            ActorUserId = actorUserId
        });
}
