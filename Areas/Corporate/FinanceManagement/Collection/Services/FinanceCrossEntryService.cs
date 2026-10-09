using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;

/// <summary>
/// Layanan domain Ayat Silang (Cross-Entry / Unidentified Payer Receipt).
/// Menangani pencatatan penerimaan uang yang belum ber-invoice tujuan,
/// ringkasan agregat, penatausahaan dokumen bukti, serta audit mutasi append-only.
/// </summary>
public sealed class FinanceCrossEntryService
{
    private const string LogCategory = "Corporate.FinanceManagement.Collection.CrossEntry";
    private const string DefaultUploadRootPath = "uploads";
    private const string RelativeRootFolder = "finance/cross-entry-documents";

    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".com", ".bat", ".cmd", ".ps1", ".sh", ".js", ".mjs",
        ".html", ".htm", ".php", ".asp", ".aspx", ".jsp", ".jar", ".msi", ".scr"
    };

    private static readonly IReadOnlyDictionary<string, string> CanonicalMediaTypeByExtension =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png"
        };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly LoggerService _loggerService;

    public FinanceCrossEntryService(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _environment = environment;
        _configuration = configuration;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------
    // Daftar Ayat Silang — GET /
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<CrossEntryListResponse>> GetPagedAsync(
        CrossEntryQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinCrossEntries.AsNoTracking().Where(x => !x.IsDelete);
        query = ApplyFilter(query, request);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "crossentrynumber" => descending ? query.OrderByDescending(x => x.CrossEntryNumber) : query.OrderBy(x => x.CrossEntryNumber),
            "referencenumber" => descending ? query.OrderByDescending(x => x.ReferenceNumber) : query.OrderBy(x => x.ReferenceNumber),
            "originalamount" => descending ? query.OrderByDescending(x => x.OriginalAmount) : query.OrderBy(x => x.OriginalAmount),
            "transactiondate" => descending ? query.OrderByDescending(x => x.TransactionDate) : query.OrderBy(x => x.TransactionDate),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var pagedRows = await query
            .Include(x => x.InsuranceProvider)
            .Include(x => x.BankAccount)
                .ThenInclude(b => b.Bank)
            .Include(x => x.Receipt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = pagedRows.Select(x => x.CreateBy).Distinct().ToList();
        var userMap = userIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode, cancellationToken)
            : new Dictionary<Guid, string?>();

        var items = pagedRows.Select(x =>
        {
            var unallocated = x.Receipt?.UnallocatedAmount ?? x.OriginalAmount;
            var allocated = x.Receipt?.AllocatedAmount ?? 0m;
            string? createdByName = null;
            if (userMap.TryGetValue(x.CreateBy, out var name)) createdByName = name;

            return new CrossEntryListResponse
            {
                Id = x.Id,
                CrossEntryNumber = x.CrossEntryNumber,
                ReferenceNumber = x.ReferenceNumber,
                TransactionDate = x.TransactionDate,
                InsuranceProviderId = x.InsuranceProviderId,
                InsuranceProviderName = x.InsuranceProvider?.InsuranceProviderName ?? "-",
                BankAccountId = x.BankAccountId,
                BankName = x.BankAccount?.Bank?.BankName ?? x.BankAccount?.AccountName ?? "-",
                BankAccountName = x.BankAccount?.AccountName ?? "-",
                OriginalAmount = x.OriginalAmount,
                AllocatedAmount = allocated,
                AvailableBalance = unallocated,
                Status = x.Status,
                CreatedByName = createdByName,
                Description = x.Description,
                RowVersion = x.RowVersion
            };
        }).ToList();

        return new PagedResult<CrossEntryListResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    // ------------------------------------------------------------------------------------
    // Ringkasan Agregat — GET /summary
    // ------------------------------------------------------------------------------------

    public async Task<CrossEntrySummaryResponse> GetSummaryAsync(
        CrossEntryQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinCrossEntries.AsNoTracking().Where(x => !x.IsDelete);
        query = ApplyFilter(query, request);

        var aggregate = await query
            .Include(x => x.Receipt)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalCredit = g.Sum(x => x.Receipt != null ? x.Receipt.Amount : x.OriginalAmount),
                TotalDebit = g.Sum(x => x.Receipt != null ? x.Receipt.AllocatedAmount : 0m),
                AvailableBalance = g.Sum(x => x.Receipt != null ? x.Receipt.UnallocatedAmount : x.OriginalAmount),
                TransactionCount = g.Count()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new CrossEntrySummaryResponse
        {
            TotalCredit = aggregate?.TotalCredit ?? 0m,
            TotalDebit = aggregate?.TotalDebit ?? 0m,
            AvailableBalance = aggregate?.AvailableBalance ?? 0m,
            TransactionCount = aggregate?.TransactionCount ?? 0
        };
    }

    // ------------------------------------------------------------------------------------
    // Rincian Ayat Silang — GET /{id}
    // ------------------------------------------------------------------------------------

    public async Task<CrossEntryDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entry = await _dbContext.FinCrossEntries
            .Include(x => x.InsuranceProvider)
            .Include(x => x.BankAccount)
                .ThenInclude(b => b.Bank)
            .Include(x => x.Receipt)
            .Include(x => x.Transactions)
            .Include(x => x.Documents)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Dokumen Ayat Silang tidak ditemukan.");

        // Sinkronisasi status jika diperlukan
        if (entry.Receipt != null && entry.Status != FinCrossEntryStatuses.Cancelled)
        {
            var correctStatus = entry.Receipt.UnallocatedAmount == 0m
                ? FinCrossEntryStatuses.FullyUsed
                : entry.Receipt.UnallocatedAmount < entry.Receipt.Amount
                    ? FinCrossEntryStatuses.PartiallyUsed
                    : FinCrossEntryStatuses.Open;

            if (entry.Status != correctStatus)
            {
                entry.Status = correctStatus;
                entry.UpdateDateTime = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var creator = await _dbContext.Users.AsNoTracking()
            .Where(u => u.Id == entry.CreateBy)
            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
            .FirstOrDefaultAsync(cancellationToken);

        var txUserIds = entry.Transactions.Select(t => t.CreateBy).Distinct().ToList();
        var txUserMap = txUserIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => txUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode, cancellationToken)
            : new Dictionary<Guid, string?>();

        var docUserIds = entry.Documents.Select(d => d.CreateBy).Distinct().ToList();
        var docUserMap = docUserIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => docUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode, cancellationToken)
            : new Dictionary<Guid, string?>();

        var transactions = entry.Transactions
            .Where(t => !t.IsDelete)
            .OrderBy(t => t.OccurredAt)
            .ThenBy(t => t.CreateDateTime)
            .Select(t =>
            {
                string? txCreatedByName = null;
                if (txUserMap.TryGetValue(t.CreateBy, out var name)) txCreatedByName = name;

                var isCredit = string.Equals(t.Direction, FinCrossEntryDirections.Credit, StringComparison.OrdinalIgnoreCase);

                return new CrossEntryTransactionResponse
                {
                    Id = t.Id,
                    CrossEntryId = t.CrossEntryId,
                    TransactionType = t.TransactionType,
                    Direction = t.Direction,
                    Amount = t.Amount,
                    BalanceAfterTransaction = t.BalanceAfterTransaction,
                    OccurredAt = t.OccurredAt,
                    ReceiptAllocationId = t.ReceiptAllocationId,
                    ReversalOfTransactionId = t.ReversalOfTransactionId,
                    Description = t.Description,
                    TransactionDateIn = isCredit ? t.OccurredAt : null,
                    CreditAmount = isCredit ? t.Amount : 0m,
                    TransactionDateOut = isCredit ? null : t.OccurredAt,
                    DebitAmount = isCredit ? 0m : t.Amount,
                    CreatedByName = txCreatedByName
                };
            })
            .ToList();

        var documents = entry.Documents
            .Where(d => !d.IsDelete)
            .OrderByDescending(d => d.CreateDateTime)
            .Select(d =>
            {
                string? docUploadedByName = null;
                if (docUserMap.TryGetValue(d.CreateBy, out var name)) docUploadedByName = name;

                return new CrossEntryDocumentResponse
                {
                    Id = d.Id,
                    CrossEntryId = d.CrossEntryId,
                    DocumentName = d.DocumentName,
                    OriginalFileName = d.OriginalFileName,
                    StoredFileName = d.StoredFileName,
                    RelativePath = d.RelativePath,
                    ContentType = d.ContentType,
                    FileSize = d.FileSize,
                    Description = d.Description,
                    UploadedAt = d.CreateDateTime,
                    UploadedByName = docUploadedByName
                };
            })
            .ToList();

        var unallocated = entry.Receipt?.UnallocatedAmount ?? entry.OriginalAmount;
        var allocated = entry.Receipt?.AllocatedAmount ?? 0m;

        return new CrossEntryDetailResponse
        {
            Id = entry.Id,
            CrossEntryNumber = entry.CrossEntryNumber,
            ReferenceNumber = entry.ReferenceNumber,
            TransactionDate = entry.TransactionDate,
            InsuranceProviderId = entry.InsuranceProviderId,
            InsuranceProviderName = entry.InsuranceProvider?.InsuranceProviderName ?? "-",
            BankAccountId = entry.BankAccountId,
            BankName = entry.BankAccount?.Bank?.BankName ?? entry.BankAccount?.AccountName ?? "-",
            BankAccountName = entry.BankAccount?.AccountName ?? "-",
            BankAccountNumber = entry.BankAccount?.AccountNumber,
            OriginalAmount = entry.OriginalAmount,
            AllocatedAmount = allocated,
            AvailableBalance = unallocated,
            Status = entry.Status,
            Description = entry.Description,
            CreatedByName = creator,
            CreateDateTime = entry.CreateDateTime,
            RowVersion = entry.RowVersion,
            ReceiptId = entry.ReceiptId,
            ReceiptNumber = entry.Receipt?.ReceiptNumber ?? entry.CrossEntryNumber,
            Transactions = transactions,
            Documents = documents
        };
    }

    // ------------------------------------------------------------------------------------
    // Pembuatan Dokumen Ayat Silang Baru (Atomic dengan FinReceipt & Initial Transaction)
    // ------------------------------------------------------------------------------------

    public async Task<CrossEntryDetailResponse> CreateAsync(
        CreateCrossEntryRequest request,
        string? idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var cleanNumber = request.CrossEntryNumber.Trim().ToUpperInvariant();
        var cleanRef = request.ReferenceNumber.Trim();

        // 1. Validasi Keunikan Nomor Ayat Silang
        var exists = await _dbContext.FinCrossEntries.AsNoTracking()
            .AnyAsync(x => !x.IsDelete && x.CrossEntryNumber == cleanNumber, cancellationToken);
        if (exists)
        {
            throw new FinanceCrossEntryConflictException($"No. Ayat Silang '{cleanNumber}' sudah terdaftar.");
        }

        // 2. Validasi Master Data Asuransi
        if (request.InsuranceProviderId == Guid.Empty)
        {
            throw new FinanceCrossEntryBadRequestException("Perusahaan asuransi/penjamin tidak valid.");
        }

        var provider = await _dbContext.MstInsuranceProviders.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == request.InsuranceProviderId && !p.IsDelete, cancellationToken)
            ?? throw new FinanceCrossEntryBadRequestException("Perusahaan asuransi/penjamin tidak valid.");

        if (!provider.IsActive)
        {
            throw new FinanceCrossEntryBadRequestException("Perusahaan asuransi/penjamin sudah tidak aktif.");
        }

        // 3. Validasi Master Data Rekening Bank
        var bank = await _dbContext.MstBankAccounts.AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == request.BankAccountId && !b.IsDelete, cancellationToken)
            ?? throw new FinanceCrossEntryBadRequestException("Rekening bank rumah sakit tidak ditemukan.");

        if (!bank.IsActive)
        {
            throw new FinanceCrossEntryBadRequestException("Rekening bank rumah sakit tidak aktif.");
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_CROSS_ENTRY_{cleanNumber}", cancellationToken);

            var now = DateTimeOffset.UtcNow;
            var transactionDateTime = new DateTimeOffset(
                request.TransactionDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

            // A. Buat FinReceipt (Single Source of Truth Kas/Bank)
            var receipt = new FinReceipt
            {
                Id = Guid.NewGuid(),
                ReceiptNumber = cleanNumber,
                SourceType = FinReceiptSourceTypes.ArCollection,
                BankAccountId = request.BankAccountId,
                Amount = request.Amount,
                AllocatedAmount = 0m,
                UnallocatedAmount = request.Amount,
                ProviderReference = cleanRef,
                ProviderEventId = idempotencyKey,
                OccurredAt = transactionDateTime,
                Status = FinReceiptStatuses.Received,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinReceipts.Add(receipt);

            // B. Buat FinCrossEntry
            var crossEntry = new FinCrossEntry
            {
                Id = Guid.NewGuid(),
                CrossEntryNumber = cleanNumber,
                ReferenceNumber = cleanRef,
                InsuranceProviderId = request.InsuranceProviderId,
                BankAccountId = request.BankAccountId,
                ReceiptId = receipt.Id,
                OriginalAmount = request.Amount,
                TransactionDate = request.TransactionDate,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Status = FinCrossEntryStatuses.Open,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            receipt.CorrelationId = crossEntry.Id;
            receipt.CausationId = crossEntry.Id;
            _dbContext.FinCrossEntries.Add(crossEntry);

            // C. Buat Mutasi Awal (Initial Receipt Transaction)
            var initialTransaction = new FinCrossEntryTransaction
            {
                Id = Guid.NewGuid(),
                CrossEntryId = crossEntry.Id,
                TransactionType = FinCrossEntryTransactionTypes.InitialReceipt,
                Direction = FinCrossEntryDirections.Credit,
                Amount = request.Amount,
                BalanceAfterTransaction = request.Amount,
                OccurredAt = transactionDateTime,
                Description = "Saldo awal penerimaan Ayat Silang",
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinCrossEntryTransactions.Add(initialTransaction);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "FinanceCrossEntry.Create",
                $"Dokumen Ayat Silang berhasil dibuat. CrossEntryId={crossEntry.Id}, No={cleanNumber}, Amount={request.Amount:N0}",
                new
                {
                    CrossEntryId = crossEntry.Id,
                    cleanNumber,
                    request.InsuranceProviderId,
                    request.BankAccountId,
                    request.Amount,
                    ActorUserId = actorUserId
                });

            return await GetByIdAsync(crossEntry.Id, cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            await RollbackAsync(transaction);
            throw new FinanceCrossEntryConflictException($"No. Ayat Silang '{cleanNumber}' sudah terdaftar oleh pengguna lain.");
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
    // Opsi Ayat Silang untuk Pelunasan Invoice AR — GET /options
    // ------------------------------------------------------------------------------------

    public async Task<List<CrossEntryOptionResponse>> GetOptionsAsync(
        CrossEntryOptionQuery query, CancellationToken cancellationToken)
    {
        var q = _dbContext.FinCrossEntries.AsNoTracking()
            .Include(x => x.BankAccount)
            .Include(x => x.Receipt)
            .Where(x => !x.IsDelete
                && x.InsuranceProviderId == query.InsuranceProviderId
                && x.Status != FinCrossEntryStatuses.Cancelled);

        if (query.OnlyAvailable)
        {
            q = q.Where(x => x.Receipt != null && x.Receipt.UnallocatedAmount > 0
                && x.Status != FinCrossEntryStatuses.FullyUsed);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            q = q.Where(x =>
                EF.Functions.ILike(x.CrossEntryNumber, pattern)
                || EF.Functions.ILike(x.ReferenceNumber, pattern));
        }

        return await q
            .OrderByDescending(x => x.TransactionDate)
            .Select(x => new CrossEntryOptionResponse
            {
                Id = x.Id,
                ReceiptId = x.ReceiptId,
                CrossEntryNumber = x.CrossEntryNumber,
                ReferenceNumber = x.ReferenceNumber,
                BankName = x.BankAccount != null && x.BankAccount.Bank != null ? x.BankAccount.Bank.BankName : (x.BankAccount != null ? x.BankAccount.AccountName : "-"),
                BankAccountName = x.BankAccount != null ? x.BankAccount.AccountName : "-",
                TransactionDate = x.TransactionDate,
                OriginalAmount = x.OriginalAmount,
                AvailableBalance = x.Receipt != null ? x.Receipt.UnallocatedAmount : x.OriginalAmount
            })
            .ToListAsync(cancellationToken);
    }

    // ------------------------------------------------------------------------------------
    // Manajemen Dokumen Lampiran — Upload, List, Download
    // ------------------------------------------------------------------------------------

    public async Task<CrossEntryDocumentResponse> UploadDocumentAsync(
        Guid crossEntryId,
        string documentName,
        string? description,
        IFormFile? file,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entry = await _dbContext.FinCrossEntries
            .SingleOrDefaultAsync(x => x.Id == crossEntryId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Dokumen Ayat Silang tidak ditemukan.");

        if (file == null || file.Length <= 0)
        {
            throw new FinanceCrossEntryValidationException("Berkas dokumen wajib dilampirkan dan tidak boleh kosong.");
        }

        if (string.IsNullOrWhiteSpace(documentName))
        {
            throw new FinanceCrossEntryValidationException("Nama dokumen wajib diisi.");
        }

        var originalFileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalFileName) || originalFileName.Length > 255)
        {
            throw new FinanceCrossEntryValidationException("Nama berkas terlalu panjang. Maksimal 255 karakter.");
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || BlockedExtensions.Contains(extension))
        {
            throw new FinanceCrossEntryValidationException("Jenis berkas ini tidak dapat diunggah demi alasan keamanan.");
        }

        if (!CanonicalMediaTypeByExtension.TryGetValue(extension, out var expectedMediaType))
        {
            throw new FinanceCrossEntryValidationException("Dokumen lampiran hanya dapat berupa PDF, JPG, JPEG, atau PNG.");
        }

        var contentType = file.ContentType?.Trim() ?? string.Empty;
        if (!string.Equals(contentType, expectedMediaType, StringComparison.OrdinalIgnoreCase))
        {
            throw new FinanceCrossEntryValidationException("Isi berkas tidak sesuai dengan jenis ekstensinya.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new FinanceCrossEntryTooLargeException("Ukuran berkas melebihi batas maksimal 10 MB.");
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var now = DateTimeOffset.UtcNow;
        var relativeDirectory = $"{RelativeRootFolder}/{now.Year:0000}/{now.Month:00}";
        var relativePath = $"{relativeDirectory}/{storedFileName}";

        string physicalPath;
        try
        {
            physicalPath = ResolvePhysicalPath(relativePath);
        }
        catch (InvalidOperationException)
        {
            throw new FinanceCrossEntryValidationException("Jalur berkas tidak valid.");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        await using (var outputStream = new FileStream(
            physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await using var inputStream = file.OpenReadStream();
            await inputStream.CopyToAsync(outputStream, cancellationToken);
        }

        var document = new FinCrossEntryDocument
        {
            Id = Guid.NewGuid(),
            CrossEntryId = entry.Id,
            DocumentName = documentName.Trim(),
            OriginalFileName = originalFileName,
            StoredFileName = storedFileName,
            RelativePath = relativePath,
            ContentType = expectedMediaType,
            FileSize = file.Length,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        try
        {
            _dbContext.FinCrossEntryDocuments.Add(document);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            DeleteIfExists(physicalPath);
            throw;
        }

        var uploader = await _dbContext.Users.AsNoTracking()
            .Where(u => u.Id == actorUserId)
            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
            .FirstOrDefaultAsync(cancellationToken);

        return new CrossEntryDocumentResponse
        {
            Id = document.Id,
            CrossEntryId = document.CrossEntryId,
            DocumentName = document.DocumentName,
            OriginalFileName = document.OriginalFileName,
            StoredFileName = document.StoredFileName,
            RelativePath = document.RelativePath,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            Description = document.Description,
            UploadedAt = document.CreateDateTime,
            UploadedByName = uploader
        };
    }

    public async Task<List<CrossEntryDocumentResponse>> GetDocumentsAsync(
        Guid crossEntryId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.FinCrossEntries.AsNoTracking()
            .AnyAsync(x => x.Id == crossEntryId && !x.IsDelete, cancellationToken);
        if (!exists) throw new KeyNotFoundException("Dokumen Ayat Silang tidak ditemukan.");

        var docs = await _dbContext.FinCrossEntryDocuments.AsNoTracking()
            .Where(x => x.CrossEntryId == crossEntryId && !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        var userIds = docs.Select(d => d.CreateBy).Distinct().ToList();
        var userMap = userIds.Count > 0
            ? await _dbContext.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode, cancellationToken)
            : new Dictionary<Guid, string?>();

        return docs.Select(d =>
        {
            string? uploader = null;
            if (userMap.TryGetValue(d.CreateBy, out var name)) uploader = name;

            return new CrossEntryDocumentResponse
            {
                Id = d.Id,
                CrossEntryId = d.CrossEntryId,
                DocumentName = d.DocumentName,
                OriginalFileName = d.OriginalFileName,
                StoredFileName = d.StoredFileName,
                RelativePath = d.RelativePath,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                Description = d.Description,
                UploadedAt = d.CreateDateTime,
                UploadedByName = uploader
            };
        }).ToList();
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadDocumentAsync(
        Guid crossEntryId, Guid documentId, CancellationToken cancellationToken)
    {
        var doc = await _dbContext.FinCrossEntryDocuments.AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == documentId && d.CrossEntryId == crossEntryId && !d.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Dokumen lampiran tidak ditemukan.");

        var physicalPath = ResolvePhysicalPath(doc.RelativePath);
        if (!File.Exists(physicalPath))
        {
            throw new KeyNotFoundException("Berkas fisik dokumen lampiran tidak ditemukan di penyimpanan server.");
        }

        var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);
        return (stream, doc.ContentType, doc.OriginalFileName);
    }

    // ------------------------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------------------------

    private string ResolvePhysicalPath(string relativePath)
    {
        var root = _configuration["FileStorage:UploadRootPath"]?.Trim();
        var uploadRoot = string.IsNullOrWhiteSpace(root)
            ? Path.Combine(_environment.ContentRootPath, DefaultUploadRootPath)
            : Path.IsPathRooted(root) ? root : Path.Combine(_environment.ContentRootPath, root);

        var normalizedRoot = Path.GetFullPath(uploadRoot);
        var combined = Path.GetFullPath(Path.Combine(normalizedRoot, relativePath));
        if (!combined.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Path traversal attempt detected.");
        }

        return combined;
    }

    private static void DeleteIfExists(string physicalPath)
    {
        try { if (File.Exists(physicalPath)) File.Delete(physicalPath); }
        catch { /* ignored */ }
    }

    private static IQueryable<FinCrossEntry> ApplyFilter(
        IQueryable<FinCrossEntry> query, CrossEntryQuery request)
    {
        if (request.StartDate.HasValue)
            query = query.Where(x => x.TransactionDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(x => x.TransactionDate <= request.EndDate.Value);

        if (request.InsuranceProviderId.HasValue)
            query = query.Where(x => x.InsuranceProviderId == request.InsuranceProviderId.Value);

        if (request.BankAccountId.HasValue)
            query = query.Where(x => x.BankAccountId == request.BankAccountId.Value);

        if (!string.IsNullOrWhiteSpace(request.ReferenceNumber))
            query = query.Where(x => EF.Functions.ILike(x.ReferenceNumber, $"%{request.ReferenceNumber.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(request.CrossEntryNumber))
            query = query.Where(x => EF.Functions.ILike(x.CrossEntryNumber, $"%{request.CrossEntryNumber.Trim()}%"));

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.CrossEntryNumber, pattern)
                || EF.Functions.ILike(x.ReferenceNumber, pattern)
                || (x.Description != null && EF.Functions.ILike(x.Description, pattern))
                || (x.InsuranceProvider != null && EF.Functions.ILike(x.InsuranceProvider.InsuranceProviderName, pattern))
                || (x.BankAccount != null && (
                    (x.BankAccount.Bank != null && EF.Functions.ILike(x.BankAccount.Bank.BankName, pattern))
                    || EF.Functions.ILike(x.BankAccount.AccountName, pattern)
                    || EF.Functions.ILike(x.BankAccount.AccountNumber, pattern)
                )));
        }

        return query;
    }

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
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

public sealed class FinanceCrossEntryBadRequestException(string message) : Exception(message);
public sealed class FinanceCrossEntryConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
public sealed class FinanceCrossEntryValidationException(string message) : Exception(message);
public sealed class FinanceCrossEntryTooLargeException(string message) : Exception(message);
