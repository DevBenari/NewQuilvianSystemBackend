using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Services;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Services;

/// <summary>
/// CRUD master metode pengadaan (BE-PBJ-PS-002; PBJ-DEC-068, PBJ-DEC-069). Kode disimpan huruf besar dan unik di antara
/// data yang belum dihapus.
/// </summary>
/// <remarks>
/// <para>
/// <b>Rentang HPS tidak boleh tumpang tindih di antara metode aktif</b> (PBJ-DEC-095). Rentang dibaca
/// <c>MinHpsAmount &lt; HPS ≤ MaxHpsAmount</c>, batas kosong = tanpa batas. Contoh: Penunjukan Langsung (kosong–50 juta) dan
/// Pembandingan Penawaran (50 juta–kosong) tidak tumpang tindih; metode baru 40–60 juta yang diaktifkan ditolak 409.
/// Metode nonaktif boleh tumpang tindih, sehingga admin dapat menyiapkan metode SOP lebih dulu, menonaktifkan metode
/// contoh, lalu mengaktifkan metode SOP (UAT-PS-01).
/// </para>
/// <para>
/// <b>Nilai contoh.</b> <c>IsSample</c> hanya diisi seeder (PBJ-DEC-069): metode yang dibuat lewat API selalu bukan contoh,
/// dan mengubah metode contoh tidak menghapus tandanya.
/// </para>
/// <para>
/// Metode yang sudah dipakai dokumen wajib tidak dapat dihapus (400 <c>PBJ-VAL-071</c>); cukup dinonaktifkan. Pemeriksaan
/// pemakaian oleh paket ditambahkan saat tabel paket ada (<c>BE-PBJ-PS-004</c>/<c>007</c>).
/// </para>
/// </remarks>
public sealed partial class ProcurementMethodService
{
    private const string LogCategory = "Corporate.ProcurementManagement.MasterData";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public ProcurementMethodService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public Task<ProcurementMethodFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new ProcurementMethodFilterMetadataResponse
        {
            SortOptions =
            [
                new() { Value = "hps", Label = "Batas bawah HPS" },
                new() { Value = "code", Label = "Kode" },
                new() { Value = "name", Label = "Nama" },
                new() { Value = "isActive", Label = "Status aktif" }
            ]
        });

    public async Task<ProcurementMethodSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementMethods.AsNoTracking().Where(x => !x.IsDelete);
        return new ProcurementMethodSummaryResponse
        {
            Total = await query.CountAsync(cancellationToken),
            Active = await query.CountAsync(x => x.IsActive, cancellationToken),
            Inactive = await query.CountAsync(x => !x.IsActive, cancellationToken),
            ActiveSample = await query.CountAsync(x => x.IsActive && x.IsSample, cancellationToken),
            RequiresTechnicalEvaluation = await query.CountAsync(x => x.RequiresTechnicalEvaluation, cancellationToken)
        };
    }

    public async Task<PagedResult<ProcurementMethodResponse>> GetPagedAsync(ProcurementMethodQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementMethods.AsNoTracking().Where(x => !x.IsDelete);
        query = query.WhereCreatedWithin(ProcurementPeriodFilter.Resolve(request.StartDate, request.EndDate, request.CustomPeriod));
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.IsSample.HasValue) query = query.Where(x => x.IsSample == request.IsSample.Value);
        if (request.RequiresTechnicalEvaluation.HasValue)
            query = query.Where(x => x.RequiresTechnicalEvaluation == request.RequiresTechnicalEvaluation.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.MethodCode.ToUpper().Contains(search) || x.MethodName.ToUpper().Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "code" => descending ? query.OrderByDescending(x => x.MethodCode) : query.OrderBy(x => x.MethodCode),
            "name" => descending ? query.OrderByDescending(x => x.MethodName) : query.OrderBy(x => x.MethodName),
            "isactive" => descending
                ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.MethodName)
                : query.OrderBy(x => x.IsActive).ThenBy(x => x.MethodName),
            // Batas bawah kosong (tanpa batas) diurutkan paling awal, lalu menurut nilainya.
            _ => descending
                ? query.OrderByDescending(x => x.MinHpsAmount.HasValue).ThenByDescending(x => x.MinHpsAmount).ThenBy(x => x.MethodName)
                : query.OrderBy(x => x.MinHpsAmount.HasValue).ThenBy(x => x.MinHpsAmount).ThenBy(x => x.MethodName)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await Project(query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProcurementMethodResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = ProcurementMasterDataSupport.TotalPage(total, request.PageSize), Items = items
        };
    }

    public async Task<List<ProcurementMethodOptionResponse>> GetOptionsAsync(bool onlyActive, string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.MstProcurementMethods.AsNoTracking().Where(x => !x.IsDelete);
        if (onlyActive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToUpper();
            query = query.Where(x => x.MethodCode.ToUpper().Contains(keyword) || x.MethodName.ToUpper().Contains(keyword));
        }

        return await query
            .OrderBy(x => x.MinHpsAmount.HasValue).ThenBy(x => x.MinHpsAmount).ThenBy(x => x.MethodName)
            .Select(x => new ProcurementMethodOptionResponse
            {
                Id = x.Id, Code = x.MethodCode, Name = x.MethodName, MinHpsAmount = x.MinHpsAmount, MaxHpsAmount = x.MaxHpsAmount,
                MinimumQuotationCount = x.MinimumQuotationCount, RequiresTechnicalEvaluation = x.RequiresTechnicalEvaluation,
                IsSample = x.IsSample, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProcurementMethodResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await Project(_dbContext.MstProcurementMethods.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new KeyNotFoundException("Metode pengadaan tidak ditemukan atau sudah dihapus.");

    public async Task<ProcurementMethodResponse> CreateAsync(UpsertProcurementMethodRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var input = await ValidateAsync(request, null, cancellationToken);
        if (request.IsActive) await EnsureNoOverlapAsync(null, input.MinHps, input.MaxHps, cancellationToken);
        var entity = new MstProcurementMethod
        {
            MethodCode = input.Code, MethodName = input.Name, MinHpsAmount = input.MinHps, MaxHpsAmount = input.MaxHps,
            MinimumQuotationCount = request.MinimumQuotationCount, RequiresTechnicalEvaluation = request.RequiresTechnicalEvaluation,
            IsSample = false, Description = input.Description, IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow, CreateBy = actorUserId
        };
        _dbContext.MstProcurementMethods.Add(entity);
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementMethod.Create", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMethodResponse> UpdateAsync(
        Guid id, UpsertProcurementMethodRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        var input = await ValidateAsync(request, id, cancellationToken);
        if (request.IsActive) await EnsureNoOverlapAsync(id, input.MinHps, input.MaxHps, cancellationToken);
        entity.MethodCode = input.Code;
        entity.MethodName = input.Name;
        entity.MinHpsAmount = input.MinHps;
        entity.MaxHpsAmount = input.MaxHps;
        entity.MinimumQuotationCount = request.MinimumQuotationCount;
        entity.RequiresTechnicalEvaluation = request.RequiresTechnicalEvaluation;
        entity.Description = input.Description;
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementMethod.Update", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMethodResponse> UpdateStatusAsync(
        Guid id, UpdateProcurementMasterStatusRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (request.IsActive && !entity.IsActive)
            await EnsureNoOverlapAsync(id, entity.MinHpsAmount, entity.MaxHpsAmount, cancellationToken);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync(request.IsActive ? "ProcurementMethod.Activate" : "ProcurementMethod.Deactivate", entity, actorUserId);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ProcurementMasterDeleteResponse> DeleteAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        if (await _dbContext.MstProcurementPackageDocumentRequirements.AnyAsync(x => x.MethodId == id && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterInUseException(
                "Metode ini tidak dapat dihapus karena masih tercatat pada dokumen wajib paket. Hapus dokumen wajibnya, atau nonaktifkan metode ini saja.");
        // BE-PBJ-PS-007 (UAT-PS-02; BE-PBJ-PS-002 D-7): paket menyimpan salinan metode, tetapi rujukan MethodId tetap dijaga.
        if (await _dbContext.PrcPackages.AnyAsync(x => x.MethodId == id && !x.IsDelete, cancellationToken))
            throw new ProcurementMasterInUseException(
                "Metode ini tidak dapat dihapus karena sudah dipakai paket pengadaan. Nonaktifkan saja bila tidak dipakai lagi.");

        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = DateTime.UtcNow;
        entity.DeleteBy = actorUserId;
        await SaveAsync(cancellationToken);
        await AuditAsync("ProcurementMethod.Delete", entity, actorUserId);
        return new ProcurementMasterDeleteResponse { Id = entity.Id, Code = entity.MethodCode, Name = entity.MethodName, IsDelete = true };
    }

    /// <summary>
    /// Menolak rentang yang beririsan dengan metode aktif lain. Dua rentang <c>(a, b]</c> dan <c>(c, d]</c> beririsan bila
    /// <c>max(a, c) &lt; min(b, d)</c>, dengan batas kosong dianggap tak terhingga.
    /// </summary>
    private async Task EnsureNoOverlapAsync(Guid? excludedId, decimal? minHps, decimal? maxHps, CancellationToken cancellationToken)
    {
        var activeMethods = await _dbContext.MstProcurementMethods.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && x.Id != excludedId)
            .Select(x => new { x.MethodName, x.MinHpsAmount, x.MaxHpsAmount })
            .ToListAsync(cancellationToken);

        var clash = activeMethods.FirstOrDefault(x => Overlaps(minHps, maxHps, x.MinHpsAmount, x.MaxHpsAmount));
        if (clash != null)
            throw new ProcurementMasterConflictException(
                $"Rentang HPS {DescribeRange(minHps, maxHps)} tumpang tindih dengan metode aktif {clash.MethodName} ({DescribeRange(clash.MinHpsAmount, clash.MaxHpsAmount)}). Nonaktifkan salah satunya atau ubah rentangnya.");
    }

    private static bool Overlaps(decimal? aMin, decimal? aMax, decimal? bMin, decimal? bMax)
    {
        var lower = aMin.HasValue && bMin.HasValue ? Math.Max(aMin.Value, bMin.Value) : aMin ?? bMin;
        var upper = aMax.HasValue && bMax.HasValue ? Math.Min(aMax.Value, bMax.Value) : aMax ?? bMax;
        return lower == null || upper == null || lower.Value < upper.Value;
    }

    private static string DescribeRange(decimal? minHps, decimal? maxHps) =>
        (minHps, maxHps) switch
        {
            (null, null) => "tanpa batas",
            (null, { } max) => $"sampai {Rupiah(max)}",
            ({ } min, null) => $"di atas {Rupiah(min)}",
            ({ } min, { } max) => $"di atas {Rupiah(min)} sampai {Rupiah(max)}"
        };

    /// <summary>Contoh: 50000000 → "Rp 50.000.000" (format Indonesia, tidak bergantung budaya server).</summary>
    private static string Rupiah(decimal value) => "Rp " + value.ToString("N0", IndonesianCulture);

    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

    private async Task<(string Code, string Name, decimal? MinHps, decimal? MaxHps, string? Description)> ValidateAsync(
        UpsertProcurementMethodRequest request, Guid? excludedId, CancellationToken cancellationToken)
    {
        var code = ProcurementMasterDataSupport.Required(request.Code, "Kode", 30).ToUpperInvariant();
        if (!CodePattern().IsMatch(code))
            throw new ProcurementMasterValidationException("Kode hanya boleh berisi huruf, angka, dan garis bawah. Contoh: PENUNJUKAN_LANGSUNG.");
        var name = ProcurementMasterDataSupport.Required(request.Name, "Nama", 150);
        var description = ProcurementMasterDataSupport.Optional(request.Description, "Deskripsi", 500);

        if (request.MinHpsAmount is < 0)
            throw new ProcurementMasterValidationException("Batas bawah HPS tidak boleh negatif.");
        if (request.MaxHpsAmount is <= 0)
            throw new ProcurementMasterValidationException("Batas atas HPS harus lebih dari 0.");
        if (request.MinHpsAmount.HasValue && request.MaxHpsAmount.HasValue && request.MinHpsAmount.Value >= request.MaxHpsAmount.Value)
            throw new ProcurementMasterValidationException("Batas bawah HPS harus lebih kecil dari batas atas HPS.");
        if (request.MinimumQuotationCount is < 1 or > 10)
            throw new ProcurementMasterValidationException("Jumlah minimum penawaran harus 1 sampai 10.");

        if (await _dbContext.MstProcurementMethods.AnyAsync(x => !x.IsDelete && x.Id != excludedId && x.MethodCode == code, cancellationToken))
            throw new ProcurementMasterConflictException("Kode sudah dipakai metode lain. Gunakan kode yang berbeda.");
        return (code, name, RoundMoney(request.MinHpsAmount), RoundMoney(request.MaxHpsAmount), description);
    }

    private static decimal? RoundMoney(decimal? value) => value.HasValue ? Math.Round(value.Value, 2, MidpointRounding.AwayFromZero) : null;

    [GeneratedRegex("^[A-Z0-9_]+$")]
    private static partial Regex CodePattern();

    private IQueryable<ProcurementMethodResponse> Project(IQueryable<MstProcurementMethod> query) =>
        query.Select(x => new ProcurementMethodResponse
        {
            Id = x.Id, Code = x.MethodCode, Name = x.MethodName, MinHpsAmount = x.MinHpsAmount, MaxHpsAmount = x.MaxHpsAmount,
            MinimumQuotationCount = x.MinimumQuotationCount, RequiresTechnicalEvaluation = x.RequiresTechnicalEvaluation,
            IsSample = x.IsSample, Description = x.Description, IsActive = x.IsActive,
            ActiveRequirementCount = _dbContext.MstProcurementPackageDocumentRequirements
                .Count(r => r.MethodId == x.Id && r.IsActive && !r.IsDelete),
            CreateDateTime = x.CreateDateTime, CreateBy = x.CreateBy, UpdateDateTime = x.UpdateDateTime, UpdateBy = x.UpdateBy
        });

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (ProcurementMasterDataSupport.IsUniqueViolation(exception))
        {
            throw new ProcurementMasterConflictException("Kode sudah dipakai metode lain. Gunakan kode yang berbeda.");
        }
    }

    private async Task<MstProcurementMethod> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstProcurementMethods.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Metode pengadaan tidak ditemukan atau sudah dihapus.");

    private Task AuditAsync(string action, MstProcurementMethod entity, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Perubahan master metode pengadaan.", new
        {
            EntityId = entity.Id,
            entity.MethodCode,
            entity.IsActive,
            entity.IsSample,
            ActorUserId = actorUserId
        });
}
