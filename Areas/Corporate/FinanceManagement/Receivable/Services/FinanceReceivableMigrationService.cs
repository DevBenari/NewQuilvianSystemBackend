using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// BE-FIN-098, FIN-DEC-196, data-dictionary.md §8.2. Impor saldo lama piutang manfaat karyawan
/// (sebelum sistem ini berjalan), satu baris per kartu piutang lama, NIP divalidasi ke
/// <c>MstEmployee</c> dan dipetakan ke <c>BenefitOwnerId</c>.
///
/// SENGAJA berdiri sendiri, BUKAN memperluas <c>FinanceOpeningItemBatchService</c> (wewenang
/// eksplisit pengguna pada sesi ini) — service itu sudah dipakai 7 task sebelumnya
/// (`BE-FIN-079/082/085/086/088/089/090`), belum pernah dikompilasi pada sesi ini, dan
/// `CK_FinOpeningItemBatch_ItemKind` DB check constraint-nya terpatok `('RECEIVABLE',
/// 'SUPPLIER_PAYABLE')` — menambah nilai baru butuh migration tersendiri (di luar wewenang task
/// ini). Karena itu batch header di sini tetap memakai `ItemKind = RECEIVABLE` (nilai yang sudah
/// sah), dan distingsi manfaat karyawan hidup sepenuhnya pada `FinReceivable.DebtorType` +
/// `BenefitOwnerId` (skema yang sudah ada sejak `BE-FIN-079`) — nol migration baru.
///
/// Alur satu-langkah (unggah = validasi = proses, BUKAN siklus DRAFT→VALIDATED→APPROVED→LOCKED
/// milik service generik) — konsisten dengan `FIN-API-1.9` §P.2 yang hanya mendefinisikan SATU
/// endpoint (`M.5.1`: `201 Created` langsung). Semua-atau-tidak-sama-sekali: satu baris gagal
/// berarti nol baris diproses (`M.5.2`, `M.5.3`).
/// </summary>
public sealed class FinanceReceivableMigrationService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.Migration";
    private static readonly string[] ValidRelationships = ["SELF", "SPOUSE", "CHILD", "PARENT", "OTHER"];

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceSubledgerMovementService _subledgerMovementService;

    public FinanceReceivableMigrationService(
        ApplicationDbContext dbContext, LoggerService loggerService, FinanceSubledgerMovementService subledgerMovementService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _subledgerMovementService = subledgerMovementService;
    }

    public async Task<EmployeeReceivableBatchResponse> ImportAsync(
        ImportEmployeeReceivableBatchRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var file = request.File;
        if (file is null || file.Length == 0)
            throw new ReceivableMigrationValidationException("Berkas impor wajib diunggah dan tidak boleh kosong.");

        var extension = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (extension is not ("csv" or "xlsx"))
            throw new ReceivableMigrationValidationException("Format berkas hanya mendukung CSV atau XLSX.");

        var rows = extension == "csv" ? await ParseCsvAsync(file, cancellationToken) : ParseXlsx(file);
        if (rows.Count == 0)
            throw new ReceivableMigrationValidationException("Berkas tidak memuat baris data.");

        var errors = new List<EmployeeReceivableRowError>();
        var seenCardNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var nips = rows.Select(r => r.GetValueOrDefault("NIP", string.Empty).Trim())
            .Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        // FIN-VAL-246: NIP MUST terdaftar dan aktif di MstEmployee — dicari sekali, bukan per baris.
        var employeesByNip = await _dbContext.MstEmployees.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive && nips.Contains(x.EmployeeNumber))
            .ToDictionaryAsync(x => x.EmployeeNumber, x => x, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var parsed = new List<(int RowNumber, string CardNumber, MstEmployee Employee, string Relationship,
            decimal OriginalAmount, decimal OutstandingAmount, DateOnly TransactionDate, string? OldInvoiceNumber,
            string? OldEncounterNumber, string? Notes)>();

        foreach (var row in rows)
        {
            var rowNumber = row.RowNumber;
            var cardNumber = row.GetValueOrDefault("NomorKartuLama", string.Empty).Trim();
            var nip = row.GetValueOrDefault("NIP", string.Empty).Trim();
            var relationship = row.GetValueOrDefault("HubunganKeluarga", string.Empty).Trim().ToUpperInvariant();

            if (cardNumber.Length == 0)
            {
                errors.Add(Error(rowNumber, cardNumber, "FORMAT", $"Baris {rowNumber}: NomorKartuLama wajib diisi."));
                continue;
            }
            if (seenCardNumbers.TryGetValue(cardNumber, out var firstRow))
            {
                errors.Add(Error(rowNumber, cardNumber, "DUPLICATE", $"Baris {rowNumber}: NomorKartuLama '{cardNumber}' kembar dengan baris {firstRow}."));
                continue;
            }
            seenCardNumbers[cardNumber] = rowNumber;

            if (nip.Length == 0 || !employeesByNip.TryGetValue(nip, out var employee))
            {
                errors.Add(Error(rowNumber, cardNumber, "FIN-VAL-246",
                    $"Baris {rowNumber}: NIP {nip} tidak terdaftar atau tidak aktif di master kepegawaian HR."));
                continue;
            }

            if (!ValidRelationships.Contains(relationship))
            {
                errors.Add(Error(rowNumber, cardNumber, "FORMAT",
                    $"Baris {rowNumber}: HubunganKeluarga harus salah satu dari SELF/SPOUSE/CHILD/PARENT/OTHER."));
                continue;
            }

            if (!decimal.TryParse(row.GetValueOrDefault("OriginalAmount", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out var originalAmount)
                || originalAmount <= 0)
            {
                errors.Add(Error(rowNumber, cardNumber, "FORMAT", $"Baris {rowNumber}: OriginalAmount wajib angka lebih besar dari nol."));
                continue;
            }

            if (!decimal.TryParse(row.GetValueOrDefault("OutstandingAmount", string.Empty), NumberStyles.Number, CultureInfo.InvariantCulture, out var outstandingAmount)
                || outstandingAmount <= 0)
            {
                errors.Add(Error(rowNumber, cardNumber, "FORMAT", $"Baris {rowNumber}: OutstandingAmount wajib angka lebih besar dari nol."));
                continue;
            }

            if (outstandingAmount > originalAmount)
            {
                errors.Add(Error(rowNumber, cardNumber, "FORMAT", $"Baris {rowNumber}: OutstandingAmount tidak boleh melebihi OriginalAmount."));
                continue;
            }

            if (!DateOnly.TryParse(row.GetValueOrDefault("TanggalTransaksi", string.Empty), CultureInfo.InvariantCulture, DateTimeStyles.None, out var transactionDate)
                || transactionDate > request.CutoverDate)
            {
                errors.Add(Error(rowNumber, cardNumber, "FORMAT", $"Baris {rowNumber}: TanggalTransaksi tidak valid atau melewati tanggal cutover."));
                continue;
            }

            parsed.Add((rowNumber, cardNumber, employee, relationship, originalAmount, outstandingAmount, transactionDate,
                NullIfEmpty(row.GetValueOrDefault("NomorInvoiceLama", string.Empty)),
                NullIfEmpty(row.GetValueOrDefault("NomorKunjungan", string.Empty)),
                NullIfEmpty(row.GetValueOrDefault("Catatan", string.Empty))));
        }

        if (errors.Count > 0)
            throw new ReceivableMigrationValidationException("Berkas impor memuat baris yang tidak valid.", errors);

        // Kontrol total MUST sama persis — mencegah baris hilang/terbaca ganda secara senyap.
        var totalOutstanding = parsed.Sum(x => x.OutstandingAmount);
        if (totalOutstanding != request.ControlTotalAmount)
            throw new ReceivableMigrationValidationException(
                $"Kontrol total {request.ControlTotalAmount:N2} tidak cocok dengan jumlah baris {totalOutstanding:N2}. Batch tidak diproses.");

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_RECEIVABLE_MIGRATION_{request.AccountingReferenceDocument}", cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var correlationId = Guid.NewGuid();

            var batch = new FinOpeningItemBatch
            {
                BatchNumber = GenerateBatchNumber(),
                ItemKind = FinOpeningItemBatchItemKinds.Receivable,
                Status = FinOpeningItemBatchStatuses.Locked,
                CutoverDate = request.CutoverDate,
                TotalItemCount = parsed.Count,
                TotalOutstandingAmount = totalOutstanding,
                DeclaredAccountingOpeningAmount = request.ControlTotalAmount,
                AccountingReferenceDocument = request.AccountingReferenceDocument,
                UploadedFileName = file.FileName.Length <= 260 ? file.FileName : file.FileName[..260],
                SourceFormat = extension.ToUpperInvariant(),
                ApprovedBy = actorUserId,
                ApprovedAt = now,
                LockedAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinOpeningItemBatches.Add(batch);

            foreach (var item in parsed)
            {
                var receivable = new FinReceivable
                {
                    ReceivableNumber = GenerateReceivableNumber(),
                    OpeningItemBatchId = batch.Id,
                    DebtorType = FinReceivableDebtorTypes.EmployeeBenefit,
                    BenefitOwnerId = item.Employee.Id,
                    BenefitRelationship = item.Relationship,
                    OriginalAmount = item.OriginalAmount,
                    OutstandingAmount = item.OutstandingAmount,
                    // CK_FinReceivable_Balance: OriginalAmount = Outstanding + Allocated + Adjusted +
                    // WrittenOff. Selisih OriginalAmount-OutstandingAmount pada baris migrasi berarti
                    // sudah terbayar sebagian SEBELUM cutover (pelajaran dari bug yang ditemukan dan
                    // diperbaiki pada BE-FIN-095) — dicatat sebagai AllocatedAmount, bukan dibiarkan 0.
                    AllocatedAmount = item.OriginalAmount - item.OutstandingAmount,
                    DueDate = request.CutoverDate,
                    Status = FinReceivableStatuses.Outstanding,
                    ClaimStatus = FinReceivableClaimStatuses.NotRequired,
                    RecognizedAt = item.TransactionDate.ToDateTime(TimeOnly.MinValue),
                    CorrelationId = correlationId,
                    CausationId = batch.Id,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                receivable.Items.Add(new FinReceivableItem
                {
                    Description = $"Migrasi kartu lama {item.CardNumber}" +
                        (item.OldInvoiceNumber is not null ? $" (invoice lama {item.OldInvoiceNumber})" : string.Empty),
                    Amount = item.OriginalAmount,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
                _dbContext.FinReceivables.Add(receivable);

                await _subledgerMovementService.RecordReceivableMovementAsync(
                    receivable: receivable,
                    movementType: FinReceivableMovementTypes.PembukaanMigrasi,
                    deltaAmount: item.OutstandingAmount,
                    balanceBefore: 0m,
                    occurredAt: now,
                    actorUserId: actorUserId,
                    correlationId: correlationId,
                    causationId: batch.Id,
                    referenceNumber: item.CardNumber,
                    openingItemBatchId: batch.Id,
                    notes: item.Notes,
                    cancellationToken: cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "Migration.EmployeeBenefit.Imported",
                $"Batch migrasi piutang karyawan diterbitkan. BatchId={batch.Id} TotalItemCount={batch.TotalItemCount} TotalOutstandingAmount={batch.TotalOutstandingAmount:N2}",
                new { BatchId = batch.Id, batch.TotalItemCount, batch.TotalOutstandingAmount, ActorUserId = actorUserId });

            return new EmployeeReceivableBatchResponse
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                TotalItemCount = batch.TotalItemCount,
                TotalOutstandingAmount = batch.TotalOutstandingAmount,
                CutoverDate = batch.CutoverDate
            };
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Parsing berkas — sengaja ditulis sendiri (bukan dipakai ulang dari
    // FinanceOpeningItemBatchService, yang menyimpan helper parsing-nya sebagai method privat
    // tidak dapat diakses lintas class), tetap pola sederhana yang konsisten: CSV dipisah koma,
    // XLSX lewat ClosedXML (satu-satunya paket pemroses spreadsheet yang terpasang).
    // ------------------------------------------------------------------------------------

    private static async Task<List<RawRow>> ParseCsvAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine)) return [];
        var headers = headerLine.Split(',').Select(h => h.Trim().Trim('"')).ToArray();

        var rows = new List<RawRow>();
        var rowNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cells = line.Split(',').Select(c => c.Trim().Trim('"')).ToArray();
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length && i < cells.Length; i++) values[headers[i]] = cells[i];
            rows.Add(new RawRow(rowNumber, values));
        }
        return rows;
    }

    private static List<RawRow> ParseXlsx(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();
        var headerRow = worksheet.Row(1);
        var lastColumn = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
        var headers = Enumerable.Range(1, lastColumn).Select(i => headerRow.Cell(i).GetString().Trim()).ToArray();

        var rows = new List<RawRow>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var r = 2; r <= lastRow; r++)
        {
            var excelRow = worksheet.Row(r);
            if (excelRow.IsEmpty()) continue;
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length; i++) values[headers[i]] = excelRow.Cell(i + 1).GetString().Trim();
            rows.Add(new RawRow(r, values));
        }
        return rows;
    }

    private sealed record RawRow(int RowNumber, Dictionary<string, string> Values)
    {
        public string GetValueOrDefault(string key, string fallback) => Values.GetValueOrDefault(key, fallback);
    }

    private static EmployeeReceivableRowError Error(int rowNumber, string cardNumber, string code, string message) => new()
    {
        RowNumber = rowNumber,
        NomorKartuLama = cardNumber.Length == 0 ? null : cardNumber,
        Code = code,
        Message = message
    };

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GenerateBatchNumber()
    {
        var candidate = $"MIG-EMB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string GenerateReceivableNumber()
    {
        var candidate = $"AR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);
}

public sealed class ReceivableMigrationValidationException : Exception
{
    public List<EmployeeReceivableRowError> RowErrors { get; }

    public ReceivableMigrationValidationException(string message, List<EmployeeReceivableRowError>? rowErrors = null) : base(message)
    {
        RowErrors = rowErrors ?? [];
    }
}
