using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

/// <summary>
/// PAY-BE-16 (blueprint payroll-gaji PGJ-C1, PRL-DEC-02/13): kelola tunjangan tetap per Struktur Gaji.
/// Validation matrix SA-01..SA-07. Sub-resource master: baris selalu dikelola dari dalam struktur induknya.
/// Nominal tidak pernah ditulis ke log.
/// </summary>
public sealed class PayrollSalaryStructureAllowanceService
{
    private const string LogCategory = "Corporate.PayrollManagement.SalaryStructureAllowance";

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PayrollSalaryStructureAllowanceService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<List<SalaryStructureAllowanceResponse>> ListAsync(Guid salaryStructureId, CancellationToken cancellationToken)
    {
        await EnsureStructureExistsAsync(salaryStructureId, requireActive: false, cancellationToken);

        return await Project(_dbContext.MstSalaryStructureAllowances
                .AsNoTracking()
                .Where(x => x.SalaryStructureId == salaryStructureId && !x.IsDelete))
            .OrderBy(x => x.AllowanceTypeName)
            .ToListAsync(cancellationToken);
    }

    public async Task<SalaryStructureAllowanceResponse> CreateAsync(
        Guid salaryStructureId,
        CreateSalaryStructureAllowanceRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        // SA-05: struktur dan jenis tunjangan harus aktif saat baris ditambahkan.
        await EnsureStructureExistsAsync(salaryStructureId, requireActive: true, cancellationToken);
        var allowanceTypeName = await EnsureAllowanceTypeActiveAsync(request.AllowanceTypeId, cancellationToken);

        var (method, amount, percentage) = NormalizeCalculation(request);
        await EnsureNotActivityBasedAsync(request.AllowanceTypeId, allowanceTypeName, cancellationToken);
        await EnsureNotDuplicateAsync(salaryStructureId, request.AllowanceTypeId, allowanceTypeName, excludeId: null, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new MstSalaryStructureAllowance
        {
            Id = Guid.NewGuid(),
            SalaryStructureId = salaryStructureId,
            AllowanceTypeId = request.AllowanceTypeId,
            CalculationMethod = method,
            Amount = amount,
            Percentage = percentage,
            Notes = NormalizeText(request.Notes),
            IsActive = request.IsActive,
            CreateDateTime = now,
            CreateBy = actorUserId,
            IsDelete = false,
            IsCancel = false
        };

        _dbContext.MstSalaryStructureAllowances.Add(entity);
        await SaveAsync(allowanceTypeName, cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "SalaryStructureAllowance.Create",
            "Baris tunjangan struktur dibuat.",
            new { EntityId = entity.Id, SalaryStructureId = salaryStructureId, entity.AllowanceTypeId, entity.CalculationMethod, Status = "Created", ActorUserId = actorUserId });

        return await GetAsync(entity.Id, cancellationToken);
    }

    public async Task<SalaryStructureAllowanceResponse> UpdateAsync(
        Guid salaryStructureId,
        Guid id,
        UpdateSalaryStructureAllowanceRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await FindAsync(salaryStructureId, id, cancellationToken);
        PayrollVersionToken.EnsureMatches(request.ExpectedRowVersion, entity.CreateDateTime, entity.UpdateDateTime, "Baris tunjangan ini");

        // Jenis tunjangan yang diganti harus aktif; jenis yang sama boleh tetap dipakai walau kini nonaktif.
        var allowanceTypeName = request.AllowanceTypeId == entity.AllowanceTypeId
            ? await GetAllowanceTypeNameAsync(entity.AllowanceTypeId, cancellationToken)
            : await EnsureAllowanceTypeActiveAsync(request.AllowanceTypeId, cancellationToken);

        var (method, amount, percentage) = NormalizeCalculation(request);
        await EnsureNotActivityBasedAsync(request.AllowanceTypeId, allowanceTypeName, cancellationToken);
        await EnsureNotDuplicateAsync(salaryStructureId, request.AllowanceTypeId, allowanceTypeName, excludeId: id, cancellationToken);

        entity.AllowanceTypeId = request.AllowanceTypeId;
        entity.CalculationMethod = method;
        entity.Amount = amount;
        entity.Percentage = percentage;
        entity.Notes = NormalizeText(request.Notes);
        entity.IsActive = request.IsActive;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;

        await SaveAsync(allowanceTypeName, cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "SalaryStructureAllowance.Update",
            "Baris tunjangan struktur diperbarui.",
            new { EntityId = entity.Id, SalaryStructureId = salaryStructureId, entity.AllowanceTypeId, entity.CalculationMethod, entity.IsActive, Status = "Updated", ActorUserId = actorUserId });

        return await GetAsync(entity.Id, cancellationToken);
    }

    public async Task<object?> DeleteAsync(
        Guid salaryStructureId,
        Guid id,
        string? expectedRowVersion,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await FindAsync(salaryStructureId, id, cancellationToken);
        PayrollVersionToken.EnsureMatches(expectedRowVersion, entity.CreateDateTime, entity.UpdateDateTime, "Baris tunjangan ini");

        var now = DateTime.UtcNow;
        entity.IsDelete = true;
        entity.IsActive = false;
        entity.DeleteDateTime = now;
        entity.DeleteBy = actorUserId;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "SalaryStructureAllowance.Delete",
            "Baris tunjangan struktur dihapus (ditandai).",
            new { EntityId = entity.Id, SalaryStructureId = salaryStructureId, entity.AllowanceTypeId, Status = "Deleted", ActorUserId = actorUserId });

        return null;
    }

    private async Task<SalaryStructureAllowanceResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await Project(_dbContext.MstSalaryStructureAllowances.AsNoTracking().Where(x => x.Id == id))
            .FirstAsync(cancellationToken);

    private static IQueryable<SalaryStructureAllowanceResponse> Project(IQueryable<MstSalaryStructureAllowance> query) =>
        query.Select(x => new SalaryStructureAllowanceResponse
        {
            Id = x.Id,
            SalaryStructureId = x.SalaryStructureId,
            AllowanceTypeId = x.AllowanceTypeId,
            AllowanceTypeCode = x.AllowanceType != null ? x.AllowanceType.AllowanceTypeCode : string.Empty,
            AllowanceTypeName = x.AllowanceType != null ? x.AllowanceType.AllowanceTypeName : string.Empty,
            CalculationMethod = x.CalculationMethod,
            Amount = x.Amount,
            Percentage = x.Percentage,
            MaximumAmount = x.AllowanceType != null ? x.AllowanceType.MaximumAmount : null,
            IsTaxable = x.AllowanceType != null && x.AllowanceType.IsTaxable,
            IsProrated = x.AllowanceType != null && x.AllowanceType.IsProrated,
            Notes = x.Notes,
            IsActive = x.IsActive,
            // Dihitung di sisi klien pada proyeksi akhir; sama persis dengan token yang diperiksa EnsureMatches.
            RowVersion = PayrollVersionToken.From(x.CreateDateTime, x.UpdateDateTime)
        });

    private async Task<MstSalaryStructureAllowance> FindAsync(Guid salaryStructureId, Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MstSalaryStructureAllowances
            .FirstOrDefaultAsync(x => x.Id == id && x.SalaryStructureId == salaryStructureId && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Baris tunjangan tidak ditemukan pada struktur ini.");

    private async Task EnsureStructureExistsAsync(Guid salaryStructureId, bool requireActive, CancellationToken cancellationToken)
    {
        var structure = await _dbContext.MstSalaryStructures
            .AsNoTracking()
            .Where(x => x.Id == salaryStructureId && !x.IsDelete)
            .Select(x => new { x.IsActive })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Struktur gaji tidak ditemukan.");

        if (requireActive && !structure.IsActive)
            throw new ArgumentException("Struktur gaji atau jenis tunjangan tidak aktif.");
    }

    private async Task<string> EnsureAllowanceTypeActiveAsync(Guid allowanceTypeId, CancellationToken cancellationToken)
    {
        var type = await _dbContext.MstAllowanceTypes
            .AsNoTracking()
            .Where(x => x.Id == allowanceTypeId && !x.IsDelete)
            .Select(x => new { x.AllowanceTypeName, x.IsActive })
            .FirstOrDefaultAsync(cancellationToken);

        if (type == null || !type.IsActive)
            throw new ArgumentException("Struktur gaji atau jenis tunjangan tidak aktif.");

        return type.AllowanceTypeName;
    }

    private async Task<string> GetAllowanceTypeNameAsync(Guid allowanceTypeId, CancellationToken cancellationToken) =>
        await _dbContext.MstAllowanceTypes
            .AsNoTracking()
            .Where(x => x.Id == allowanceTypeId)
            .Select(x => x.AllowanceTypeName)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

    /// <summary>SA-01..SA-03. Kolom yang tidak dipakai metode terpilih disimpan 0 agar tidak menyesatkan.</summary>
    private static (string Method, decimal Amount, decimal Percentage) NormalizeCalculation(CreateSalaryStructureAllowanceRequest request)
    {
        var method = SalaryStructureAllowanceCalculationMethods.All
            .FirstOrDefault(x => string.Equals(x, request.CalculationMethod?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Cara hitung tunjangan harus nominal tetap atau persentase gaji pokok.");

        if (method == SalaryStructureAllowanceCalculationMethods.FixedAmount)
        {
            if (request.Amount <= 0m)
                throw new ArgumentException("Nominal tunjangan harus lebih dari nol.");
            return (method, request.Amount, 0m);
        }

        if (request.Percentage <= 0m || request.Percentage > 100m)
            throw new ArgumentException("Persentase tunjangan harus di atas 0 dan paling tinggi 100.");
        return (method, 0m, request.Percentage);
    }

    /// <summary>SA-06: jenis tunjangan yang dihitung dari aktivitas kerja tidak boleh menjadi tunjangan tetap struktur.</summary>
    private async Task EnsureNotActivityBasedAsync(Guid allowanceTypeId, string allowanceTypeName, CancellationToken cancellationToken)
    {
        var usedByActivityPolicy =
            await _dbContext.MstShiftAllowancePolicies.AnyAsync(x => x.AllowanceTypeId == allowanceTypeId && x.IsActive && !x.IsDelete, cancellationToken)
            || await _dbContext.MstOnCallAllowancePolicies.AnyAsync(x => x.AllowanceTypeId == allowanceTypeId && x.IsActive && !x.IsDelete, cancellationToken)
            || await _dbContext.MstHazardAllowancePolicies.AnyAsync(x => x.AllowanceTypeId == allowanceTypeId && x.IsActive && !x.IsDelete, cancellationToken);

        if (usedByActivityPolicy)
            throw new ArgumentException(
                $"Tunjangan {allowanceTypeName} dihitung dari aktivitas kerja, jadi tidak boleh dipasang sebagai tunjangan tetap struktur.");
    }

    /// <summary>SA-04: satu jenis tunjangan paling banyak satu baris per struktur (juga dijaga index unik parsial).</summary>
    private async Task EnsureNotDuplicateAsync(
        Guid salaryStructureId, Guid allowanceTypeId, string allowanceTypeName, Guid? excludeId, CancellationToken cancellationToken)
    {
        var duplicate = await _dbContext.MstSalaryStructureAllowances.AnyAsync(x =>
            x.SalaryStructureId == salaryStructureId &&
            x.AllowanceTypeId == allowanceTypeId &&
            !x.IsDelete &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

        if (duplicate)
            throw DuplicateConflict(allowanceTypeName);
    }

    /// <summary>Dua petugas menambah jenis yang sama hampir bersamaan: index unik menolak salah satunya; dipetakan ke 409 SA-04.</summary>
    private async Task SaveAsync(string allowanceTypeName, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw DuplicateConflict(allowanceTypeName);
        }
    }

    private static PayrollRunConflictException DuplicateConflict(string allowanceTypeName) =>
        new($"Tunjangan {allowanceTypeName} sudah ada di struktur ini. Ubah baris yang sudah ada.");

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
