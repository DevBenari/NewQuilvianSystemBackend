using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;

/// <summary>
/// Layanan master peralihan penanganan potongan cicilan ke mesin payroll (BE-FIN-115, FIN-DEC-222, FR-FIN-258, FIN-DES-108).
/// Mengikuti pola MstDirectPaymentThreshold / DirectPaymentThresholdService (satu nilai aktif berversi dengan riwayat).
/// </summary>
public sealed class PayrollDeductionCutoverService
{
    private const string LogCategory = "Corporate.FinanceManagement.MasterData";
    private static readonly Regex PeriodRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PayrollDeductionCutoverService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Mengambil konfigurasi periode peralihan yang sedang aktif.
    /// Memulangkan null bila belum pernah ditetapkan (fail-open ke jembatan manual, AC 1).
    /// </summary>
    public async Task<PayrollDeductionCutoverResponse?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var entity = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return await MapAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Mengambil seluruh riwayat konfigurasi periode peralihan yang pernah dicatat (urut terbaru lebih dulu).
    /// </summary>
    public async Task<List<PayrollDeductionCutoverResponse>> GetHistoryAsync(CancellationToken cancellationToken)
    {
        var entities = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .Where(x => !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return [];
        }

        var userIds = entities.Select(x => x.CreateBy).Distinct().Where(id => id != Guid.Empty).ToList();
        var userNames = await _dbContext.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode, cancellationToken);

        return entities.Select(x => new PayrollDeductionCutoverResponse
        {
            Id = x.Id,
            AutomaticFromPeriod = x.AutomaticFromPeriod,
            ChangeReason = x.ChangeReason,
            IsActive = x.IsActive,
            CreatedBy = x.CreateBy,
            CreatedByName = userNames.TryGetValue(x.CreateBy, out var name) ? name : null,
            CreatedAt = x.CreateDateTime,
            RowVersion = x.RowVersion
        }).ToList();
    }

    /// <summary>
    /// Menetapkan periode pertama yang ditangani mesin payroll otomatis (AC 2, AC 3, AC 4).
    /// Baris lama dinonaktifkan (IsActive = false), baris baru disisipkan sebagai aktif (IsActive = true).
    /// </summary>
    public async Task<PayrollDeductionCutoverResponse> SetCutoverAsync(
        SetPayrollDeductionCutoverRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        // AC 4 / FIN-VAL-286: Alasan perubahan wajib diisi
        if (string.IsNullOrWhiteSpace(request.ChangeReason))
        {
            throw new PayrollDeductionCutoverValidationException("Alasan perubahan periode peralihan wajib diisi.");
        }

        // Validasi format periode YYYY-MM (contoh: 2027-01)
        var period = request.AutomaticFromPeriod?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(period) || !PeriodRegex.IsMatch(period))
        {
            throw new PayrollDeductionCutoverBadRequestException(
                "Format periode harus YYYY-MM dengan bulan 01 sampai 12 (contoh: 2027-01).");
        }

        var activeEntity = await _dbContext.MstPayrollDeductionCutovers
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);

        if (activeEntity is not null)
        {
            // Validasi penanda versi konkurensi (FIN-API-1.11 §S.2)
            if (request.ExpectedRowVersion.HasValue
                && request.ExpectedRowVersion.Value != Guid.Empty
                && request.ExpectedRowVersion.Value != activeEntity.RowVersion)
            {
                throw new PayrollDeductionCutoverConflictException(
                    "Periode peralihan sudah diubah oleh orang lain. Muat ulang sebelum melanjutkan.");
            }

            // AC 3 / FIN-VAL-286: Mundur ke periode yang sudah punya batch manual DISAHKAN ditolak 422
            await ValidateNotMovingBackToApprovedManualBatchPeriodAsync(period, cancellationToken);

            // AC 2: Nonaktifkan versi lama
            activeEntity.IsActive = false;
            activeEntity.UpdateDateTime = DateTime.UtcNow;
            activeEntity.UpdateBy = actorUserId;
        }

        // AC 2: Sisipkan baris baru sebagai aktif
        var newEntity = new MstPayrollDeductionCutover
        {
            Id = Guid.NewGuid(),
            AutomaticFromPeriod = period,
            ChangeReason = request.ChangeReason.Trim(),
            IsActive = true,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.MstPayrollDeductionCutovers.Add(newEntity);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new PayrollDeductionCutoverConflictException(
                "Periode peralihan sudah diubah oleh orang lain. Muat ulang sebelum melanjutkan.");
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new PayrollDeductionCutoverConflictException(
                "Periode peralihan sudah ditetapkan oleh orang lain. Muat ulang sebelum melanjutkan.");
        }

        await _loggerService.AuditAsync(
            LogCategory,
            activeEntity is null ? "PayrollDeductionCutover.Create" : "PayrollDeductionCutover.Update",
            "Penetapan periode peralihan pemotongan cicilan ke mesin payroll.",
            new
            {
                CutoverId = newEntity.Id,
                newEntity.AutomaticFromPeriod,
                newEntity.ChangeReason,
                ActorUserId = actorUserId
            });

        return await MapAsync(newEntity, cancellationToken);
    }

    /// <summary>
    /// AC 3 / FIN-VAL-286: Mundur ke periode yang sudah memiliki batch manual DISAHKAN ditolak 422.
    /// Diaktifkan / dievaluasi terhadap tabel FinPayrollDeductionBatch jika tabel tersebut sudah ada di database (BE-FIN-117).
    /// </summary>
    private async Task ValidateNotMovingBackToApprovedManualBatchPeriodAsync(string newPeriod, CancellationToken cancellationToken)
    {
        try
        {
            var connection = _dbContext.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT EXISTS (
                    SELECT 1 FROM information_schema.tables 
                    WHERE table_schema = 'public' AND table_name = 'FinPayrollDeductionBatch'
                );";
            var exists = (bool)(await cmd.ExecuteScalarAsync(cancellationToken) ?? false);
            if (exists)
            {
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = @"
                    SELECT ""Period"" FROM ""public"".""FinPayrollDeductionBatch""
                    WHERE ""Period"" >= @p0 AND ""Status"" = 'DISAHKAN' AND ""IsDelete"" = false
                    ORDER BY ""Period"" DESC LIMIT 1;";
                var param = checkCmd.CreateParameter();
                param.ParameterName = "@p0";
                param.Value = newPeriod;
                checkCmd.Parameters.Add(param);

                var conflictPeriod = (string?)await checkCmd.ExecuteScalarAsync(cancellationToken);
                if (!string.IsNullOrEmpty(conflictPeriod))
                {
                    throw new PayrollDeductionCutoverValidationException(
                        $"Periode peralihan tidak boleh mundur ke {newPeriod} karena periode {conflictPeriod} sudah memiliki batch manual yang DISAHKAN.");
                }
            }
        }
        catch (PayrollDeductionCutoverValidationException)
        {
            throw;
        }
        catch
        {
            // Bila koneksi database tidak dapat dibuka atau tabel belum dibuat, jangan blokir
        }
    }

    private async Task<PayrollDeductionCutoverResponse> MapAsync(
        MstPayrollDeductionCutover entity, CancellationToken cancellationToken)
    {
        return new PayrollDeductionCutoverResponse
        {
            Id = entity.Id,
            AutomaticFromPeriod = entity.AutomaticFromPeriod,
            ChangeReason = entity.ChangeReason,
            IsActive = entity.IsActive,
            CreatedBy = entity.CreateBy,
            CreatedByName = await GetUserNameAsync(entity.CreateBy, cancellationToken),
            CreatedAt = entity.CreateDateTime,
            RowVersion = entity.RowVersion
        };
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
        => (exception.InnerException as PostgresException)?.SqlState == PostgresErrorCodes.UniqueViolation;

    private async Task<string?> GetUserNameAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

