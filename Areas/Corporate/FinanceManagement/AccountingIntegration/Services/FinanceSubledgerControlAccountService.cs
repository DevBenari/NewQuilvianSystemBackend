using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Layanan konfigurasi pemetaan akun control subledger ke akun COA Accounting (BE-FIN-065, FIN-DES-080, FIN-DEC-113).
/// Bertanggung jawab atas pengelolaan pemetaan akun per kelompok dan segmen, serta audit kelengkapan cakupan sebelum snapshot.
/// </summary>
public sealed class FinanceSubledgerControlAccountService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FinanceSubledgerControlAccountService> _logger;

    private static readonly string[] ValidPiutangSegments =
    [
        FinReceivableDebtorTypes.Payer,
        FinReceivableDebtorTypes.PatientGuarantor,
        FinReceivableDebtorTypes.EmployeeBenefit
    ];

    private static readonly string[] ValidUtangJasaMedisSegments =
    [
        FinMedicalServicePayeeTypes.Doctor,
        FinMedicalServicePayeeTypes.Nurse,
        FinMedicalServicePayeeTypes.OtherPractitioner
    ];

    public FinanceSubledgerControlAccountService(
        ApplicationDbContext dbContext,
        ILogger<FinanceSubledgerControlAccountService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Mengambil daftar pemetaan akun control subledger secara berpaging (FIN-API-1.5 F.1).
    /// </summary>
    public async Task<PagedResult<ControlAccountMapResponse>> GetPagedAsync(
        ControlAccountMapPagedQuery query,
        CancellationToken cancellationToken = default)
    {
        var queryable = _dbContext.FinSubledgerControlAccountMaps
            .AsNoTracking()
            .Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(query.BalanceGroup))
        {
            var filterGroup = query.BalanceGroup.Trim().ToUpperInvariant();
            queryable = queryable.Where(x => x.BalanceGroup == filterGroup);
        }

        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            queryable = queryable.Where(x =>
                x.ControlAccountCode.ToLower().Contains(search) ||
                (x.SegmentKey != null && x.SegmentKey.ToLower().Contains(search)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(search)));
        }

        var totalData = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderBy(x => x.BalanceGroup)
            .ThenBy(x => x.SegmentKey ?? string.Empty)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => MapToResponse(x))
            .ToListAsync(cancellationToken);

        var totalPage = (int)Math.Ceiling((double)totalData / query.PageSize);

        return new PagedResult<ControlAccountMapResponse>
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = totalData,
            TotalPage = totalPage,
            Items = items
        };
    }

    /// <summary>
    /// Menganalisis kelengkapan cakupan pemetaan akun control untuk seluruh kelompok saldo (FIN-DES-080).
    /// Mengembalikan daftar kelompok dan segmen yang belum terpetakan sebelum snapshot bulanan diterbitkan.
    /// </summary>
    public async Task<ControlAccountCoverageResponse> GetCoverageAsync(CancellationToken cancellationToken = default)
    {
        var activeMappings = await _dbContext.FinSubledgerControlAccountMaps
            .AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .ToListAsync(cancellationToken);

        var summaries = new List<ControlAccountGroupCoverageSummary>();
        var unmappedItems = new List<UnmappedControlAccountItem>();

        // 1. Kas Kasir (KAS-KASIR)
        EvaluateOverallGroup(
            FinSubledgerBalanceGroups.KasKasir,
            "Kas Kasir",
            activeMappings,
            summaries,
            unmappedItems);

        // 2. Kas Kecil (KAS-KECIL)
        EvaluateOverallGroup(
            FinSubledgerBalanceGroups.KasKecil,
            "Kas Kecil",
            activeMappings,
            summaries,
            unmappedItems);

        // 3. Utang Supplier (UTANG-SUPPLIER)
        EvaluateOverallGroup(
            FinSubledgerBalanceGroups.UtangSupplier,
            "Utang Usaha / Supplier",
            activeMappings,
            summaries,
            unmappedItems);

        // 4. Piutang (PIUTANG)
        EvaluateSegmentableGroup(
            FinSubledgerBalanceGroups.Piutang,
            "Piutang Usaha / Pasien & Penjamin",
            ValidPiutangSegments,
            activeMappings,
            summaries,
            unmappedItems);

        // 5. Utang Jasa Medis (UTANG-JASA-MEDIS)
        EvaluateSegmentableGroup(
            FinSubledgerBalanceGroups.UtangJasaMedis,
            "Utang Jasa Medis",
            ValidUtangJasaMedisSegments,
            activeMappings,
            summaries,
            unmappedItems);

        var isComplete = summaries.All(x => x.IsComplete);

        return new ControlAccountCoverageResponse
        {
            IsComplete = isComplete,
            GroupSummaries = summaries,
            UnmappedItems = unmappedItems
        };
    }

    /// <summary>
    /// Menambah satu baris pemetaan akun control subledger baru (FIN-VAL-172..176, 179).
    /// </summary>
    public async Task<ControlAccountMapResponse> CreateAsync(
        CreateControlAccountMapRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var balanceGroup = request.BalanceGroup?.Trim().ToUpperInvariant() ?? string.Empty;
        var segmentKey = string.IsNullOrWhiteSpace(request.SegmentKey) ? null : request.SegmentKey.Trim().ToUpperInvariant();
        var controlAccountCode = request.ControlAccountCode?.Trim() ?? string.Empty;

        // FIN-VAL-172: BalanceGroup harus salah satu dari lima nilai yang sah
        if (!FinSubledgerBalanceGroups.All.Contains(balanceGroup))
        {
            throw new FinanceSubledgerBadRequestException("Kelompok saldo tidak dikenal.");
        }

        // FIN-VAL-179: Panjang kode akun maksimal 50 karakter
        if (string.IsNullOrWhiteSpace(controlAccountCode) || controlAccountCode.Length > 50)
        {
            throw new FinanceSubledgerBadRequestException("Kode akun maksimal 50 karakter.");
        }

        // FIN-VAL-173: SegmentKey harus sah untuk kelompok saldo yang dipilih
        ValidateSegmentKeyForGroup(balanceGroup, segmentKey);

        // Ambil pemetaan aktif yang ada untuk kelompok ini
        var existingGroupMappings = await _dbContext.FinSubledgerControlAccountMaps
            .Where(x => x.BalanceGroup == balanceGroup && x.IsActive && !x.IsDelete)
            .ToListAsync(cancellationToken);

        // FIN-VAL-176: Satu kelompok saldo tidak boleh memakai pemetaan menyeluruh (NULL) dan per segmen sekaligus
        if (existingGroupMappings.Count > 0)
        {
            if (segmentKey == null && existingGroupMappings.Any(x => x.SegmentKey != null))
            {
                throw new FinanceSubledgerValidationException(
                    "Satu kelompok saldo tidak boleh memakai pemetaan menyeluruh dan pemetaan per segmen sekaligus.");
            }

            if (segmentKey != null && existingGroupMappings.Any(x => x.SegmentKey == null))
            {
                throw new FinanceSubledgerValidationException(
                    "Satu kelompok saldo tidak boleh memakai pemetaan menyeluruh dan pemetaan per segmen sekaligus.");
            }
        }

        // FIN-VAL-174: Kelompok dan segmen yang sama sudah punya baris aktif
        if (existingGroupMappings.Any(x => x.SegmentKey == segmentKey))
        {
            throw new FinanceSubledgerConflictException("Kelompok dan segmen ini sudah dipetakan.");
        }

        // FIN-VAL-175: Kode akun control sudah dipakai baris aktif lain
        var isAccountCodeInUse = await _dbContext.FinSubledgerControlAccountMaps.AnyAsync(
            x => x.ControlAccountCode == controlAccountCode && x.IsActive && !x.IsDelete,
            cancellationToken);

        if (isAccountCodeInUse)
        {
            throw new FinanceSubledgerConflictException("Kode akun ini sudah dipakai pemetaan lain.");
        }

        var entity = new FinSubledgerControlAccountMap
        {
            Id = Guid.NewGuid(),
            BalanceGroup = balanceGroup,
            SegmentKey = segmentKey,
            ControlAccountCode = controlAccountCode,
            IsActive = true,
            Notes = request.Notes?.Trim(),
            CreateBy = userId,
            CreateDateTime = DateTime.UtcNow,
            IsCancel = false,
            IsDelete = false
        };

        _dbContext.FinSubledgerControlAccountMaps.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Pemetaan akun control subledger baru berhasil dibuat. Id: {Id}, BalanceGroup: {Group}, Segment: {Segment}, Account: {Account}",
            entity.Id, entity.BalanceGroup, entity.SegmentKey ?? "(seluruh)", entity.ControlAccountCode);

        return MapToResponse(entity);
    }

    /// <summary>
    /// Mengoreksi kode akun control atau catatan pada pemetaan yang ada (FIN-VAL-175, 179).
    /// </summary>
    public async Task<ControlAccountMapResponse> UpdateAsync(
        Guid id,
        UpdateControlAccountMapRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.FinSubledgerControlAccountMaps
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException("Pemetaan akun control tidak ditemukan.");
        }

        var controlAccountCode = request.ControlAccountCode?.Trim() ?? string.Empty;

        // FIN-VAL-179: Panjang kode akun maksimal 50 karakter
        if (string.IsNullOrWhiteSpace(controlAccountCode) || controlAccountCode.Length > 50)
        {
            throw new FinanceSubledgerBadRequestException("Kode akun maksimal 50 karakter.");
        }

        // FIN-VAL-175: Kode akun control sudah dipakai baris aktif lain
        if (entity.IsActive)
        {
            var isAccountCodeInUse = await _dbContext.FinSubledgerControlAccountMaps.AnyAsync(
                x => x.Id != id && x.ControlAccountCode == controlAccountCode && x.IsActive && !x.IsDelete,
                cancellationToken);

            if (isAccountCodeInUse)
            {
                throw new FinanceSubledgerConflictException("Kode akun ini sudah dipakai pemetaan lain.");
            }
        }

        entity.ControlAccountCode = controlAccountCode;
        entity.Notes = request.Notes?.Trim();
        entity.UpdateBy = userId;
        entity.UpdateDateTime = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Pemetaan akun control subledger berhasil diperbarui. Id: {Id}, Account: {Account}",
            entity.Id, entity.ControlAccountCode);

        return MapToResponse(entity);
    }

    /// <summary>
    /// Menonaktifkan pemetaan akun control agar tidak lagi dipakai pada penerbitan snapshot baru (idempoten).
    /// </summary>
    public async Task<ControlAccountMapResponse> DeactivateAsync(
        Guid id,
        DeactivateControlAccountMapRequest? request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.FinSubledgerControlAccountMaps
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException("Pemetaan akun control tidak ditemukan.");
        }

        if (!entity.IsActive)
        {
            return MapToResponse(entity);
        }

        entity.IsActive = false;

        if (!string.IsNullOrWhiteSpace(request?.Reason))
        {
            var reasonSuffix = $"[Nonaktif: {request.Reason.Trim()}]";
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                ? (reasonSuffix.Length > 300 ? reasonSuffix[..300] : reasonSuffix)
                : ($"{entity.Notes} {reasonSuffix}".Length > 300 ? $"{entity.Notes} {reasonSuffix}"[..300] : $"{entity.Notes} {reasonSuffix}");
        }

        entity.UpdateBy = userId;
        entity.UpdateDateTime = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Pemetaan akun control subledger berhasil dinonaktifkan. Id: {Id}, BalanceGroup: {Group}, Segment: {Segment}",
            entity.Id, entity.BalanceGroup, entity.SegmentKey ?? "(seluruh)");

        return MapToResponse(entity);
    }

    private static void ValidateSegmentKeyForGroup(string balanceGroup, string? segmentKey)
    {
        if (balanceGroup is FinSubledgerBalanceGroups.KasKasir
            or FinSubledgerBalanceGroups.KasKecil
            or FinSubledgerBalanceGroups.UtangSupplier)
        {
            if (segmentKey != null)
            {
                throw new FinanceSubledgerBadRequestException("Segmen ini tidak berlaku untuk kelompok saldo yang dipilih.");
            }
        }
        else if (balanceGroup == FinSubledgerBalanceGroups.Piutang)
        {
            if (segmentKey != null && !ValidPiutangSegments.Contains(segmentKey))
            {
                throw new FinanceSubledgerBadRequestException("Segmen ini tidak berlaku untuk kelompok saldo yang dipilih.");
            }
        }
        else if (balanceGroup == FinSubledgerBalanceGroups.UtangJasaMedis)
        {
            if (segmentKey != null && !ValidUtangJasaMedisSegments.Contains(segmentKey))
            {
                throw new FinanceSubledgerBadRequestException("Segmen ini tidak berlaku untuk kelompok saldo yang dipilih.");
            }
        }
    }

    private static void EvaluateOverallGroup(
        string balanceGroup,
        string groupName,
        List<FinSubledgerControlAccountMap> activeMappings,
        List<ControlAccountGroupCoverageSummary> summaries,
        List<UnmappedControlAccountItem> unmappedItems)
    {
        var mappings = activeMappings.Where(x => x.BalanceGroup == balanceGroup).ToList();

        if (mappings.Count > 0)
        {
            summaries.Add(new ControlAccountGroupCoverageSummary
            {
                BalanceGroup = balanceGroup,
                GroupName = groupName,
                MappingMode = "OVERALL",
                IsComplete = true,
                ActiveMappingsCount = mappings.Count,
                MissingSegments = new List<string>()
            });
        }
        else
        {
            summaries.Add(new ControlAccountGroupCoverageSummary
            {
                BalanceGroup = balanceGroup,
                GroupName = groupName,
                MappingMode = "UNMAPPED",
                IsComplete = false,
                ActiveMappingsCount = 0,
                MissingSegments = new List<string> { "(seluruh)" }
            });

            unmappedItems.Add(new UnmappedControlAccountItem
            {
                BalanceGroup = balanceGroup,
                SegmentKey = null,
                Description = $"Kelompok {groupName} belum memiliki kode akun control."
            });
        }
    }

    private static void EvaluateSegmentableGroup(
        string balanceGroup,
        string groupName,
        string[] validSegments,
        List<FinSubledgerControlAccountMap> activeMappings,
        List<ControlAccountGroupCoverageSummary> summaries,
        List<UnmappedControlAccountItem> unmappedItems)
    {
        var mappings = activeMappings.Where(x => x.BalanceGroup == balanceGroup).ToList();

        if (mappings.Count == 0)
        {
            summaries.Add(new ControlAccountGroupCoverageSummary
            {
                BalanceGroup = balanceGroup,
                GroupName = groupName,
                MappingMode = "UNMAPPED",
                IsComplete = false,
                ActiveMappingsCount = 0,
                MissingSegments = new List<string> { "(seluruh) atau per segmen" }
            });

            unmappedItems.Add(new UnmappedControlAccountItem
            {
                BalanceGroup = balanceGroup,
                SegmentKey = null,
                Description = $"Kelompok {groupName} belum memiliki kode akun control (menyeluruh ataupun per segmen)."
            });
            return;
        }

        // Mode menyeluruh (NULL)
        var overallMapping = mappings.FirstOrDefault(x => x.SegmentKey == null);
        if (overallMapping != null)
        {
            summaries.Add(new ControlAccountGroupCoverageSummary
            {
                BalanceGroup = balanceGroup,
                GroupName = groupName,
                MappingMode = "OVERALL",
                IsComplete = true,
                ActiveMappingsCount = 1,
                MissingSegments = new List<string>()
            });
            return;
        }

        // Mode per segmen
        var mappedSegments = mappings
            .Where(x => x.SegmentKey != null)
            .Select(x => x.SegmentKey!)
            .ToHashSet();

        var missingSegments = validSegments.Where(seg => !mappedSegments.Contains(seg)).ToList();
        var isComplete = missingSegments.Count == 0;

        summaries.Add(new ControlAccountGroupCoverageSummary
        {
            BalanceGroup = balanceGroup,
            GroupName = groupName,
            MappingMode = "SEGMENTED",
            IsComplete = isComplete,
            ActiveMappingsCount = mappedSegments.Count,
            MissingSegments = missingSegments
        });

        foreach (var missing in missingSegments)
        {
            unmappedItems.Add(new UnmappedControlAccountItem
            {
                BalanceGroup = balanceGroup,
                SegmentKey = missing,
                Description = $"Segmen {missing} pada kelompok {groupName} belum memiliki kode akun control."
            });
        }
    }

    private static ControlAccountMapResponse MapToResponse(FinSubledgerControlAccountMap entity) => new()
    {
        Id = entity.Id,
        BalanceGroup = entity.BalanceGroup,
        SegmentKey = entity.SegmentKey,
        ControlAccountCode = entity.ControlAccountCode,
        IsActive = entity.IsActive,
        Notes = entity.Notes,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };
}
