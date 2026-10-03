using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Layanan pengelolaan saldo awal cutover subledger (BE-FIN-066, FIN-DES-088, FIN-DEC-128).
/// Mengelola siklus hidup saldo awal DRAFT -> APPROVED -> LOCKED dan menerbitkan mutasi kas pembuka saat penguncian Kas Kasir.
/// </summary>
public sealed class FinanceOpeningBalanceService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceSubledgerMovementService _movementService;
    private readonly ILogger<FinanceOpeningBalanceService> _logger;

    public FinanceOpeningBalanceService(
        ApplicationDbContext dbContext,
        FinanceSubledgerMovementService movementService,
        ILogger<FinanceOpeningBalanceService> logger)
    {
        _dbContext = dbContext;
        _movementService = movementService;
        _logger = logger;
    }

    /// <summary>
    /// Mengambil seluruh rekaman saldo awal cutover aktif per kelompok saldo (FIN-API-1.5 F.1).
    /// </summary>
    public async Task<List<OpeningBalanceResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.FinOpeningBalances
            .AsNoTracking()
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.BalanceGroup)
            .ToListAsync(cancellationToken);

        var names = await GetUserNamesAsync(items.Select(x => x.ApprovedBy), cancellationToken);

        return items.Select(x => MapToResponse(x, names)).ToList();
    }

    /// <summary>
    /// Mengambil rincian satu baris saldo awal cutover berdasarkan ID.
    /// </summary>
    public async Task<OpeningBalanceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.FinOpeningBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException("Saldo awal cutover tidak ditemukan.");
        }

        return await MapToResponseAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Mencatat baris saldo awal cutover baru berstatus DRAFT (FIN-VAL-180..182, 185).
    /// </summary>
    public async Task<OpeningBalanceResponse> CreateAsync(
        CreateOpeningBalanceRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var balanceGroup = request.BalanceGroup?.Trim().ToUpperInvariant() ?? string.Empty;

        // FIN-VAL-172: Kelompok saldo wajib dikenal
        if (!FinSubledgerBalanceGroups.All.Contains(balanceGroup))
        {
            throw new FinanceSubledgerBadRequestException("Kelompok saldo tidak dikenal.");
        }

        var reason = request.Reason?.Trim() ?? string.Empty;
        var accountingRefDoc = request.AccountingReferenceDocument?.Trim() ?? string.Empty;

        // FIN-VAL-182: Alasan dan rujukan dokumen Accounting wajib diisi
        if (string.IsNullOrWhiteSpace(reason) || string.IsNullOrWhiteSpace(accountingRefDoc))
        {
            throw new FinanceSubledgerValidationException("Alasan dan rujukan dokumen saldo awal wajib diisi.");
        }

        // FIN-VAL-181: Kelompok PIUTANG, UTANG-SUPPLIER, dan UTANG-JASA-MEDIS wajib bernilai 0.00
        if (balanceGroup is FinSubledgerBalanceGroups.Piutang
            or FinSubledgerBalanceGroups.UtangSupplier
            or FinSubledgerBalanceGroups.UtangJasaMedis)
        {
            if (request.Amount != 0m)
            {
                throw new FinanceSubledgerValidationException(
                    "Saldo awal kelompok ini harus nol karena rinciannya datang dari migrasi tagihan lama.");
            }
        }

        // FIN-VAL-185: Saldo awal kas tidak boleh negatif
        if (balanceGroup is FinSubledgerBalanceGroups.KasKasir or FinSubledgerBalanceGroups.KasKecil)
        {
            if (request.Amount < 0m)
            {
                throw new FinanceSubledgerValidationException("Saldo awal kas tidak boleh negatif.");
            }
        }

        // FIN-VAL-180: Kelompok sudah punya baris aktif
        var exists = await _dbContext.FinOpeningBalances
            .AnyAsync(x => x.BalanceGroup == balanceGroup && !x.IsDelete, cancellationToken);

        if (exists)
        {
            throw new FinanceSubledgerConflictException("Saldo awal untuk kelompok ini sudah pernah dicatat.");
        }

        var entity = new FinOpeningBalance
        {
            Id = Guid.NewGuid(),
            BalanceGroup = balanceGroup,
            Amount = request.Amount,
            CutoverDate = request.CutoverDate,
            Status = FinOpeningBalanceStatuses.Draft,
            Reason = reason,
            AccountingReferenceDocument = accountingRefDoc,
            RowVersion = Guid.NewGuid(),
            CreateBy = userId,
            CreateDateTime = DateTime.UtcNow,
            IsCancel = false,
            IsDelete = false
        };

        _dbContext.FinOpeningBalances.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Saldo awal cutover DRAFT berhasil dicatat. Id: {Id}, Group: {Group}, Amount: {Amount}, CutoverDate: {CutoverDate}",
            entity.Id, entity.BalanceGroup, entity.Amount, entity.CutoverDate);

        return await MapToResponseAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Mengoreksi saldo awal cutover yang masih berstatus DRAFT (FIN-VAL-181..183, 185).
    /// </summary>
    public async Task<OpeningBalanceResponse> UpdateAsync(
        Guid id,
        UpdateOpeningBalanceRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.FinOpeningBalances
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException("Saldo awal cutover tidak ditemukan.");
        }

        // FIN-VAL-183: Saldo awal yang sudah disetujui tidak dapat diubah
        if (entity.Status is FinOpeningBalanceStatuses.Approved or FinOpeningBalanceStatuses.Locked)
        {
            throw new FinanceSubledgerConflictException("Saldo awal yang sudah disetujui tidak dapat diubah.");
        }

        // Concurrency check
        if (entity.RowVersion != request.RowVersion)
        {
            throw new FinanceSubledgerConflictException(
                "Data saldo awal telah diubah oleh pengguna lain. Silakan muat ulang data.");
        }

        var reason = request.Reason?.Trim() ?? string.Empty;
        var accountingRefDoc = request.AccountingReferenceDocument?.Trim() ?? string.Empty;

        // FIN-VAL-182: Alasan dan rujukan dokumen Accounting wajib diisi
        if (string.IsNullOrWhiteSpace(reason) || string.IsNullOrWhiteSpace(accountingRefDoc))
        {
            throw new FinanceSubledgerValidationException("Alasan dan rujukan dokumen saldo awal wajib diisi.");
        }

        // FIN-VAL-181: Kelompok item migrasi wajib bernilai 0.00
        if (entity.BalanceGroup is FinSubledgerBalanceGroups.Piutang
            or FinSubledgerBalanceGroups.UtangSupplier
            or FinSubledgerBalanceGroups.UtangJasaMedis)
        {
            if (request.Amount != 0m)
            {
                throw new FinanceSubledgerValidationException(
                    "Saldo awal kelompok ini harus nol karena rinciannya datang dari migrasi tagihan lama.");
            }
        }

        // FIN-VAL-185: Saldo awal kas tidak boleh negatif
        if (entity.BalanceGroup is FinSubledgerBalanceGroups.KasKasir or FinSubledgerBalanceGroups.KasKecil)
        {
            if (request.Amount < 0m)
            {
                throw new FinanceSubledgerValidationException("Saldo awal kas tidak boleh negatif.");
            }
        }

        entity.Amount = request.Amount;
        entity.CutoverDate = request.CutoverDate;
        entity.Reason = reason;
        entity.AccountingReferenceDocument = accountingRefDoc;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateBy = userId;
        entity.UpdateDateTime = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Saldo awal cutover berhasil diperbarui. Id: {Id}, Group: {Group}, Amount: {Amount}",
            entity.Id, entity.BalanceGroup, entity.Amount);

        return await MapToResponseAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Menyetujui saldo awal cutover berpindah dari DRAFT ke APPROVED (FIN-STATE-1.6 F.1).
    /// </summary>
    public async Task<OpeningBalanceResponse> ApproveAsync(
        Guid id,
        ApproveOpeningBalanceRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.FinOpeningBalances
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException("Saldo awal cutover tidak ditemukan.");
        }

        if (entity.Status == FinOpeningBalanceStatuses.Locked)
        {
            throw new FinanceSubledgerConflictException("Saldo awal sudah dikunci dan tidak dapat diubah.");
        }

        // Idempoten bila sudah APPROVED
        if (entity.Status == FinOpeningBalanceStatuses.Approved)
        {
            return await MapToResponseAsync(entity, cancellationToken);
        }

        if (entity.Status != FinOpeningBalanceStatuses.Draft)
        {
            throw new FinanceSubledgerValidationException("Hanya saldo awal berstatus DRAFT yang dapat disetujui.");
        }

        // Concurrency check
        if (entity.RowVersion != request.RowVersion)
        {
            throw new FinanceSubledgerConflictException(
                "Data saldo awal telah diubah oleh pengguna lain. Silakan muat ulang data.");
        }

        if (string.IsNullOrWhiteSpace(entity.Reason) || string.IsNullOrWhiteSpace(entity.AccountingReferenceDocument))
        {
            throw new FinanceSubledgerValidationException("Seluruh ruas wajib terisi sebelum disetujui.");
        }

        entity.Status = FinOpeningBalanceStatuses.Approved;
        entity.ApprovedBy = userId;
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateBy = userId;
        entity.UpdateDateTime = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Saldo awal cutover APPROVED. Id: {Id}, Group: {Group}, Amount: {Amount}, ApprovedBy: {ApprovedBy}",
            entity.Id, entity.BalanceGroup, entity.Amount, entity.ApprovedBy);

        return await MapToResponseAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Mengunci saldo awal cutover permanen (APPROVED -> LOCKED), dan menerbitkan satu mutasi kas SALDO-AWAL bila KAS-KASIR (FIN-DES-088, FIN-VAL-184).
    /// </summary>
    public async Task<OpeningBalanceResponse> LockAsync(
        Guid id,
        LockOpeningBalanceRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var entity = await _dbContext.FinOpeningBalances
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException("Saldo awal cutover tidak ditemukan.");
        }

        // Idempoten bila sudah LOCKED
        if (entity.Status == FinOpeningBalanceStatuses.Locked)
        {
            return await MapToResponseAsync(entity, cancellationToken);
        }

        if (entity.Status == FinOpeningBalanceStatuses.Draft)
        {
            throw new FinanceSubledgerValidationException("Saldo awal harus disetujui terlebih dahulu sebelum dikunci.");
        }

        // Concurrency check
        if (entity.RowVersion != request.RowVersion)
        {
            throw new FinanceSubledgerConflictException(
                "Data saldo awal telah diubah oleh pengguna lain. Silakan muat ulang data.");
        }

        // FIN-VAL-184: CutoverDate tidak boleh melewati hari ini dalam kalender bisnis WIB
        var todayWib = FinanceBusinessDate.Today();
        if (entity.CutoverDate > todayWib)
        {
            throw new FinanceSubledgerValidationException("Tanggal cutover tidak boleh melewati hari ini.");
        }

        entity.Status = FinOpeningBalanceStatuses.Locked;
        entity.LockedAt = DateTimeOffset.UtcNow;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateBy = userId;
        entity.UpdateDateTime = DateTime.UtcNow;

        // Akibat penguncian: kelompok KAS-KASIR menerbitkan tepat satu mutasi kas SALDO-AWAL bertanggal CutoverDate (FIN-DES-088),
        // termasuk bernilai nol (pengecualian FIN-VAL-168 khusus SALDO-AWAL)
        if (entity.BalanceGroup == FinSubledgerBalanceGroups.KasKasir)
        {
            await _movementService.RecordCashMovementAsync(
                movementType: FinCashMovementTypes.SaldoAwal,
                direction: FinCashMovementDirections.In,
                amount: entity.Amount,
                businessDate: entity.CutoverDate,
                occurredAt: DateTimeOffset.UtcNow,
                sourceReferenceType: FinCashMovementSourceReferenceTypes.OpeningBalance,
                sourceReferenceId: entity.Id.ToString(),
                actorUserId: userId,
                correlationId: Guid.NewGuid(),
                causationId: entity.Id,
                notes: $"Saldo awal cutover Kas Kasir per {entity.CutoverDate:yyyy-MM-dd}. Dokumen: {entity.AccountingReferenceDocument}",
                ignoreDuplicate: true,
                cancellationToken: cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Saldo awal cutover LOCKED permanen. Id: {Id}, Group: {Group}, Amount: {Amount}, CutoverDate: {CutoverDate}",
            entity.Id, entity.BalanceGroup, entity.Amount, entity.CutoverDate);

        return await MapToResponseAsync(entity, cancellationToken);
    }

    private async Task<OpeningBalanceResponse> MapToResponseAsync(
        FinOpeningBalance entity,
        CancellationToken cancellationToken)
    {
        var names = await GetUserNamesAsync(new[] { entity.ApprovedBy }, cancellationToken);
        return MapToResponse(entity, names);
    }

    /// <summary>
    /// Mengambil nama tampilan pengguna untuk sekumpulan ID; ID yang tidak ditemukan tidak ikut dikembalikan.
    /// </summary>
    private async Task<Dictionary<Guid, string?>> GetUserNamesAsync(
        IEnumerable<Guid?> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string?>();
        }

        return await _dbContext.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    private static OpeningBalanceResponse MapToResponse(
        FinOpeningBalance entity,
        IReadOnlyDictionary<Guid, string?> names) => new()
    {
        Id = entity.Id,
        BalanceGroup = entity.BalanceGroup,
        Amount = entity.Amount,
        CutoverDate = entity.CutoverDate,
        Status = entity.Status,
        Reason = entity.Reason,
        AccountingReferenceDocument = entity.AccountingReferenceDocument,
        ApprovedBy = entity.ApprovedBy,
        ApprovedByName = entity.ApprovedBy.HasValue && names.TryGetValue(entity.ApprovedBy.Value, out var approverName)
            ? approverName
            : null,
        ApprovedAt = entity.ApprovedAt,
        LockedAt = entity.LockedAt,
        RowVersion = entity.RowVersion,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };
}
