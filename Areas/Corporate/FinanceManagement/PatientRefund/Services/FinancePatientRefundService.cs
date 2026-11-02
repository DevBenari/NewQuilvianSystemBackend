using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Services;

public sealed class FinancePatientRefundService
{
    private const string LogCategory = "Corporate.FinanceManagement.PatientRefund";
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceAccountingOutboxService _outboxService;
    private readonly LoggerService _loggerService;

    public FinancePatientRefundService(
        ApplicationDbContext dbContext,
        FinanceAccountingOutboxService outboxService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _outboxService = outboxService;
        _loggerService = loggerService;
    }

    public async Task<(IReadOnlyList<PatientRefundItemDto> Items, int TotalCount)> GetPagedListAsync(
        PatientRefundFilterDto filter,
        CancellationToken cancellationToken)
    {
        // Query basis: BilRefundableCredit yang aktif
        var baseQuery = from credit in _dbContext.BilRefundableCredits.AsNoTracking()
                        where !credit.IsDelete
                        join invoice in _dbContext.BilInvoices.AsNoTracking() on credit.InvoiceId equals invoice.Id
                        where !invoice.IsDelete
                        join encounter in _dbContext.RegPatientEncounters.AsNoTracking() on invoice.EncounterId equals encounter.Id
                        where !encounter.IsDelete
                        join patient in _dbContext.MstPatients.AsNoTracking() on encounter.PatientId equals patient.Id
                        where !patient.IsDelete
                        select new
                        {
                            Credit = credit,
                            Invoice = invoice,
                            Encounter = encounter,
                            Patient = patient
                        };

        if (filter.StartDate.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Credit.RecognizedAt >= filter.StartDate.Value);
        }
        if (filter.EndDate.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Credit.RecognizedAt <= filter.EndDate.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.SourceType))
        {
            var st = filter.SourceType.Trim().ToUpperInvariant();
            baseQuery = baseQuery.Where(x => x.Credit.SourceType == st);
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            baseQuery = baseQuery.Where(x =>
                x.Patient.MedicalRecordNumber.ToLower().Contains(search) ||
                x.Patient.FullName.ToLower().Contains(search) ||
                x.Invoice.InvoiceNumber.ToLower().Contains(search));
        }

        var creditRows = await baseQuery.ToListAsync(cancellationToken);
        var creditIds = creditRows.Select(x => x.Credit.Id).Distinct().ToList();

        var requests = await _dbContext.Set<FinPatientRefundRequest>().AsNoTracking()
            .Where(x => creditIds.Contains(x.RefundableCreditId) && !x.IsDelete)
            .OrderByDescending(x => x.SubmittedAt)
            .ToListAsync(cancellationToken);

        var requestLookup = requests.ToLookup(x => x.RefundableCreditId);
        var senderAccounts = await LoadSenderAccountsAsync(
            requests.Where(x => x.BankAccountId.HasValue).Select(x => x.BankAccountId!.Value).Distinct().ToList(),
            cancellationToken);
        var items = new List<PatientRefundItemDto>();

        foreach (var row in creditRows)
        {
            var creditRequests = requestLookup[row.Credit.Id].ToList();
            if (creditRequests.Count == 0)
            {
                // Baris credit murni belum pernah diajukan
                if (row.Credit.AvailableAmount > 0)
                {
                    items.Add(new PatientRefundItemDto
                    {
                        Id = row.Credit.Id,
                        RefundRequestId = null,
                        RefundableCreditId = row.Credit.Id,
                        PatientId = row.Patient.Id,
                        MedicalRecordNumber = row.Patient.MedicalRecordNumber,
                        PatientName = row.Patient.FullName,
                        InvoiceId = row.Invoice.Id,
                        InvoiceNumber = row.Invoice.InvoiceNumber,
                        EncounterId = row.Encounter.Id,
                        EncounterNumber = row.Encounter.EncounterNumber,
                        SourceType = row.Credit.SourceType,
                        SourceId = row.Credit.SourceId,
                        RecognizedAt = row.Credit.RecognizedAt,
                        OriginalAmount = row.Credit.OriginalAmount,
                        AvailableAmount = row.Credit.AvailableAmount,
                        RequestedAmount = 0m,
                        ApprovedAmount = null,
                        PaidAmount = 0m,
                        Status = FinPatientRefundStatuses.Available,
                        Reason = null,
                        PaymentMethod = null,
                        ReferenceNumber = null,
                        RequestedBy = null,
                        ApprovedBy = null,
                        SubmittedAt = null,
                        ApprovedAt = null,
                        CompletedAt = null,
                        RowVersion = Guid.Empty
                    });
                }
            }
            else
            {
                // Setiap request membentuk baris tersendiri
                decimal totalReservedOrPaidOnCredit = 0m;
                foreach (var req in creditRequests)
                {
                    if (req.Status == FinPatientRefundStatuses.Pending || req.Status == FinPatientRefundStatuses.Approved)
                    {
                        totalReservedOrPaidOnCredit += (req.ApprovedAmount ?? req.RequestedAmount) - req.PaidAmount;
                    }
                    items.Add(new PatientRefundItemDto
                    {
                        Id = req.Id,
                        RefundRequestId = req.Id,
                        RefundableCreditId = row.Credit.Id,
                        PatientId = row.Patient.Id,
                        MedicalRecordNumber = row.Patient.MedicalRecordNumber,
                        PatientName = row.Patient.FullName,
                        InvoiceId = row.Invoice.Id,
                        InvoiceNumber = row.Invoice.InvoiceNumber,
                        EncounterId = row.Encounter.Id,
                        EncounterNumber = row.Encounter.EncounterNumber,
                        SourceType = row.Credit.SourceType,
                        SourceId = row.Credit.SourceId,
                        RecognizedAt = row.Credit.RecognizedAt,
                        OriginalAmount = row.Credit.OriginalAmount,
                        AvailableAmount = row.Credit.AvailableAmount,
                        RequestedAmount = req.RequestedAmount,
                        ApprovedAmount = req.ApprovedAmount,
                        PaidAmount = req.PaidAmount,
                        Status = req.Status,
                        Reason = req.Reason,
                        PaymentMethod = req.PaymentMethod,
                        BankAccountId = req.BankAccountId,
                        SenderBankId = SenderAccountOf(senderAccounts, req.BankAccountId)?.BankId,
                        SenderBankName = SenderAccountOf(senderAccounts, req.BankAccountId)?.Bank?.BankName,
                        SenderAccountNumber = SenderAccountOf(senderAccounts, req.BankAccountId)?.AccountNumber,
                        SenderAccountName = SenderAccountOf(senderAccounts, req.BankAccountId)?.AccountName,
                        PayeeBankName = req.PayeeBankName,
                        PayeeAccountNumber = req.PayeeAccountNumber,
                        PayeeAccountName = req.PayeeAccountName,
                        ReferenceNumber = req.ReferenceNumber,
                        RequestedBy = req.RequestedBy,
                        ApprovedBy = req.ApprovedBy,
                        SubmittedAt = req.SubmittedAt,
                        ApprovedAt = req.ApprovedAt,
                        CompletedAt = req.CompletedAt,
                        RowVersion = req.RowVersion
                    });
                }

                // Jika masih ada sisa dana yang belum di-reserve pada credit ini, tampilkan juga opsi AVAILABLE
                var remainingFree = row.Credit.AvailableAmount - totalReservedOrPaidOnCredit;
                if (remainingFree > 0)
                {
                    items.Add(new PatientRefundItemDto
                    {
                        Id = row.Credit.Id,
                        RefundRequestId = null,
                        RefundableCreditId = row.Credit.Id,
                        PatientId = row.Patient.Id,
                        MedicalRecordNumber = row.Patient.MedicalRecordNumber,
                        PatientName = row.Patient.FullName,
                        InvoiceId = row.Invoice.Id,
                        InvoiceNumber = row.Invoice.InvoiceNumber,
                        EncounterId = row.Encounter.Id,
                        EncounterNumber = row.Encounter.EncounterNumber,
                        SourceType = row.Credit.SourceType,
                        SourceId = row.Credit.SourceId,
                        RecognizedAt = row.Credit.RecognizedAt,
                        OriginalAmount = row.Credit.OriginalAmount,
                        AvailableAmount = remainingFree,
                        RequestedAmount = 0m,
                        ApprovedAmount = null,
                        PaidAmount = 0m,
                        Status = FinPatientRefundStatuses.Available,
                        Reason = null,
                        PaymentMethod = null,
                        ReferenceNumber = null,
                        RequestedBy = null,
                        ApprovedBy = null,
                        SubmittedAt = null,
                        ApprovedAt = null,
                        CompletedAt = null,
                        RowVersion = Guid.Empty
                    });
                }
            }
        }

        // Filter status
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var st = filter.Status.Trim().ToUpperInvariant();
            items = items.Where(x => string.Equals(x.Status, st, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Filter nominal
        if (filter.MinAmount.HasValue)
        {
            items = items.Where(x => (x.RequestedAmount > 0 ? x.RequestedAmount : x.AvailableAmount) >= filter.MinAmount.Value).ToList();
        }
        if (filter.MaxAmount.HasValue)
        {
            items = items.Where(x => (x.RequestedAmount > 0 ? x.RequestedAmount : x.AvailableAmount) <= filter.MaxAmount.Value).ToList();
        }

        // Sorting
        var sortDirection = string.Equals(filter.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        items = (filter.SortField?.ToLowerInvariant()) switch
        {
            "patientname" => sortDirection ? items.OrderBy(x => x.PatientName).ToList() : items.OrderByDescending(x => x.PatientName).ToList(),
            "medicalrecordnumber" => sortDirection ? items.OrderBy(x => x.MedicalRecordNumber).ToList() : items.OrderByDescending(x => x.MedicalRecordNumber).ToList(),
            "invoicenumber" => sortDirection ? items.OrderBy(x => x.InvoiceNumber).ToList() : items.OrderByDescending(x => x.InvoiceNumber).ToList(),
            "requestedamount" => sortDirection ? items.OrderBy(x => x.RequestedAmount).ToList() : items.OrderByDescending(x => x.RequestedAmount).ToList(),
            "status" => sortDirection ? items.OrderBy(x => x.Status).ToList() : items.OrderByDescending(x => x.Status).ToList(),
            _ => sortDirection ? items.OrderBy(x => x.SubmittedAt ?? x.RecognizedAt).ToList() : items.OrderByDescending(x => x.SubmittedAt ?? x.RecognizedAt).ToList(),
        };

        var totalCount = items.Count;
        var pagedItems = items
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return (pagedItems, totalCount);
    }

    private async Task<Dictionary<Guid, MstBankAccount>> LoadSenderAccountsAsync(
        IReadOnlyCollection<Guid> bankAccountIds,
        CancellationToken cancellationToken)
    {
        if (bankAccountIds.Count == 0)
            return new Dictionary<Guid, MstBankAccount>();

        var accounts = await _dbContext.MstBankAccounts.AsNoTracking()
            .Include(x => x.Bank)
            .Where(x => bankAccountIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        return accounts.ToDictionary(x => x.Id);
    }

    private static MstBankAccount? SenderAccountOf(
        IReadOnlyDictionary<Guid, MstBankAccount> accounts,
        Guid? bankAccountId)
        => bankAccountId.HasValue && accounts.TryGetValue(bankAccountId.Value, out var account) ? account : null;

    public async Task<PatientRefundSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var credits = await _dbContext.BilRefundableCredits.AsNoTracking()
            .Where(x => !x.IsDelete)
            .ToListAsync(cancellationToken);

        var requests = await _dbContext.Set<FinPatientRefundRequest>().AsNoTracking()
            .Where(x => !x.IsDelete)
            .ToListAsync(cancellationToken);

        var totalAvailable = credits.Sum(x => x.AvailableAmount);
        var pendingRequests = requests.Where(x => x.Status == FinPatientRefundStatuses.Pending).ToList();
        var approvedRequests = requests.Where(x => x.Status == FinPatientRefundStatuses.Approved).ToList();
        var partiallyPaidRequests = requests.Where(x => x.Status == FinPatientRefundStatuses.PartiallyPaid).ToList();
        var paidRequests = requests.Where(x => x.Status == FinPatientRefundStatuses.Paid).ToList();

        var totalPending = pendingRequests.Sum(x => x.RequestedAmount);
        var totalApproved = approvedRequests.Sum(x => (x.ApprovedAmount ?? x.RequestedAmount) - x.PaidAmount);
        var totalPaid = requests.Sum(x => x.PaidAmount);

        // Effective available refund adalah total AvailableAmount dikurangi pengajuan yang sedang diproses (pending & approved)
        var reserved = totalPending + totalApproved;
        var effectiveAvailable = Math.Max(0m, totalAvailable - reserved);

        return new PatientRefundSummaryDto
        {
            TotalAvailableRefundAmount = effectiveAvailable,
            TotalRequestedRefundAmount = requests.Sum(x => x.RequestedAmount),
            TotalPendingApprovalAmount = totalPending,
            TotalApprovedWaitingPaymentAmount = totalApproved,
            TotalPaidAmount = totalPaid,
            PendingCount = pendingRequests.Count,
            ApprovedCount = approvedRequests.Count + partiallyPaidRequests.Count,
            PaidCount = paidRequests.Count,
            AvailableCreditCount = credits.Count(x => x.AvailableAmount > 0)
        };
    }

    public async Task<PatientRefundDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await _dbContext.Set<FinPatientRefundRequest>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        Guid creditId = request?.RefundableCreditId ?? id;

        var credit = await _dbContext.BilRefundableCredits.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == creditId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Data saldo refund atau pengajuan tidak ditemukan.");

        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == credit.InvoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice terkait tidak ditemukan.");

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoice.EncounterId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kunjungan pasien tidak ditemukan.");

        var patient = await _dbContext.MstPatients.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == encounter.PatientId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Data pasien tidak ditemukan.");

        var depositAccount = await _dbContext.BilDepositAccounts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.EncounterId == invoice.EncounterId && !x.IsDelete, cancellationToken);

        // Rekening pengirim diturunkan dari master rekening bank + master bank, bukan dari teks yang
        // pernah dikirim browser. Rekening tujuan pasien (Payee*) tetap data terpisah di bawah.
        MstBankAccount? senderAccount = null;
        if (request?.BankAccountId.HasValue == true)
        {
            var senderAccounts = await LoadSenderAccountsAsync([request.BankAccountId.Value], cancellationToken);
            senderAccount = SenderAccountOf(senderAccounts, request.BankAccountId);
        }

        return new PatientRefundDetailDto
        {
            Id = request?.Id ?? credit.Id,
            RefundRequestId = request?.Id,
            RefundableCreditId = credit.Id,
            PatientId = patient.Id,
            MedicalRecordNumber = patient.MedicalRecordNumber,
            PatientName = patient.FullName,
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            EncounterId = encounter.Id,
            EncounterNumber = encounter.EncounterNumber,
            ServiceType = invoice.ServiceType,
            DepositAccountNumber = depositAccount?.AccountNumber,
            SourceType = credit.SourceType,
            SourceId = credit.SourceId,
            RecognizedAt = credit.RecognizedAt,
            OriginalAmount = credit.OriginalAmount,
            AvailableAmount = credit.AvailableAmount,
            RequestedAmount = request?.RequestedAmount ?? 0m,
            ApprovedAmount = request?.ApprovedAmount,
            PaidAmount = request?.PaidAmount ?? 0m,
            Status = request?.Status ?? FinPatientRefundStatuses.Available,
            Reason = request?.Reason,
            PaymentMethod = request?.PaymentMethod,
            BankAccountId = request?.BankAccountId,
            BankAccountName = senderAccount?.AccountName,
            SenderBankId = senderAccount?.BankId,
            SenderBankName = senderAccount?.Bank?.BankName,
            SenderAccountNumber = senderAccount?.AccountNumber,
            SenderAccountName = senderAccount?.AccountName,
            PayeeBankName = request?.PayeeBankName,
            PayeeAccountNumber = request?.PayeeAccountNumber,
            PayeeAccountName = request?.PayeeAccountName,
            ReferenceNumber = request?.ReferenceNumber,
            ExecutionNotes = request?.ExecutionNotes,
            RejectionReason = request?.RejectionReason,
            FailedReason = request?.FailedReason,
            RequestedBy = request?.RequestedBy,
            RequestedByName = null,
            ApprovedBy = request?.ApprovedBy,
            ApprovedByName = null,
            SubmittedAt = request?.SubmittedAt,
            ApprovedAt = request?.ApprovedAt,
            CompletedAt = request?.CompletedAt,
            RowVersion = request?.RowVersion ?? Guid.Empty
        };
    }

    public async Task<PatientRefundDetailDto> CreateRequestAsync(
        CreatePatientRefundRequestDto dto,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (dto.RequestedAmount <= 0)
            throw new FinancePatientRefundValidationException("Nominal pengajuan harus lebih besar dari nol.");
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new FinancePatientRefundValidationException("Alasan pengajuan refund wajib diisi.");
        if (!PatientRefundTransferPolicy.TryNormalizeMethod(dto.PaymentMethod, out var paymentMethod))
            throw new FinancePatientRefundValidationException(PatientRefundTransferPolicy.TransferOnlyMessage);
        if (dto.RefundableCreditId == Guid.Empty)
            throw new FinancePatientRefundValidationException("Sumber saldo refund wajib dipilih dari daftar saldo refund.");

        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                // Satu saldo satu antrean: dua pengajuan atas saldo yang sama berjalan bergantian,
                // sehingga pemeriksaan cadangan di bawah selalu melihat pengajuan yang sudah tersimpan.
                await AcquireLockAsync($"FIN_REFUNDABLE_CREDIT_{dto.RefundableCreditId:N}", cancellationToken);
            }

            // Cek idempotency. Kiriman ulang dikembalikan apa adanya, SEBELUM rekening diperiksa ulang,
            // supaya pengajuan yang sudah sah tidak gagal hanya karena rekening kemudian dinonaktifkan.
            var existing = await _dbContext.Set<FinPatientRefundRequest>()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (existing is not null)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return await GetByIdAsync(existing.Id, cancellationToken);
            }

            // Rekening pengirim diperiksa ulang terhadap master (tidak mempercayai label/nomor browser),
            // dan rekening tujuan pasien dijaga tetap terpisah dari rekening pengirim.
            var source = await PatientRefundTransferPolicy.ResolveSourceAccountAsync(
                _dbContext, dto.BankAccountId, dto.SenderBankId, cancellationToken);
            if (!source.IsValid)
                throw new FinancePatientRefundValidationException(source.Error!);

            var payeeError = PatientRefundTransferPolicy.ValidatePayee(
                source.Account!, dto.PayeeBankName, dto.PayeeAccountNumber, dto.PayeeAccountName);
            if (payeeError is not null)
                throw new FinancePatientRefundValidationException(payeeError);

            var credit = await _dbContext.BilRefundableCredits
                .SingleOrDefaultAsync(x => x.Id == dto.RefundableCreditId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Saldo refundable credit tidak ditemukan.");

            if (credit.Status != BillingRefundableCreditStatuses.Available || credit.AvailableAmount <= 0)
                throw new FinancePatientRefundValidationException("Saldo refundable credit sudah habis.");

            // Cek reservation
            var reserved = await _dbContext.Set<FinPatientRefundRequest>()
                .Where(x => x.RefundableCreditId == credit.Id
                    && !x.IsDelete
                    && (x.Status == FinPatientRefundStatuses.Pending || x.Status == FinPatientRefundStatuses.Approved))
                .SumAsync(x => (x.ApprovedAmount ?? x.RequestedAmount) - x.PaidAmount, cancellationToken);

            var effectiveAvailable = credit.AvailableAmount - reserved;
            if (dto.RequestedAmount > effectiveAvailable)
            {
                throw new FinancePatientRefundValidationException(
                    $"Nominal pengajuan (Rp {dto.RequestedAmount:N2}) melebihi sisa saldo refund yang dapat diajukan (Rp {effectiveAvailable:N2}).");
            }

            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == credit.InvoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice terkait tidak ditemukan.");

            var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == invoice.EncounterId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Encounter terkait tidak ditemukan.");

            var entity = new FinPatientRefundRequest
            {
                PatientId = encounter.PatientId,
                InvoiceId = credit.InvoiceId,
                RefundableCreditId = credit.Id,
                RequestedAmount = dto.RequestedAmount,
                ApprovedAmount = null,
                PaidAmount = 0m,
                Status = FinPatientRefundStatuses.Pending,
                Reason = dto.Reason.Trim(),
                RequestedBy = actorUserId,
                SubmittedAt = DateTimeOffset.UtcNow,
                PaymentMethod = paymentMethod,
                BankAccountId = source.Account!.BankAccountId,
                PayeeBankName = dto.PayeeBankName?.Trim(),
                PayeeAccountNumber = dto.PayeeAccountNumber?.Trim(),
                PayeeAccountName = dto.PayeeAccountName?.Trim(),
                IdempotencyKey = idempotencyKey,
                CorrelationId = dto.CorrelationId != Guid.Empty ? dto.CorrelationId : Guid.NewGuid(),
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.Set<FinPatientRefundRequest>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "FinancePatientRefund.Submitted",
                "Pengajuan pengembalian saldo refund pasien berhasil diajukan ke antrean persetujuan Finance.",
                new
                {
                    entity.Id,
                    entity.RefundableCreditId,
                    entity.PatientId,
                    entity.InvoiceId,
                    entity.RequestedAmount,
                    entity.PaymentMethod,
                    entity.BankAccountId,
                    ActorUserId = actorUserId
                });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }
        catch (Exception exception) when (IsConcurrencyFailure(exception))
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new FinancePatientRefundConflictException(
                "Pengajuan bersamaan atas saldo yang sama sedang diproses. Muat ulang daftar lalu coba lagi.", exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<PatientRefundDetailDto> ApproveAsync(
        Guid id,
        ApprovePatientRefundRequestDto dto,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<FinPatientRefundRequest>()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan refund tidak ditemukan.");

        if (entity.RowVersion != dto.ExpectedRowVersion)
            throw new FinancePatientRefundConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");

        if (entity.Status != FinPatientRefundStatuses.Pending)
            throw new FinancePatientRefundValidationException($"Pengajuan tidak dalam status PENDING (status saat ini: {entity.Status}).");

        // Maker-checker invariant
        if (entity.RequestedBy == actorUserId)
            throw new FinancePatientRefundValidationException("Pengaju (maker) tidak boleh menyetujui pengajuannya sendiri (checker).");

        var approvedAmount = dto.ApprovedAmount ?? entity.RequestedAmount;
        if (approvedAmount <= 0 || approvedAmount > entity.RequestedAmount)
            throw new FinancePatientRefundValidationException("Nominal persetujuan harus positif dan tidak melebihi nominal pengajuan.");

        // Pengajuan lama bermetode tunai tidak lagi dapat disetujui: pengembalian dana pasien hanya
        // melalui Transfer Bank.
        if (!PatientRefundTransferPolicy.TryNormalizeMethod(entity.PaymentMethod, out _))
            throw new FinancePatientRefundValidationException(
                $"{PatientRefundTransferPolicy.TransferOnlyMessage} Pengajuan ini bermetode {entity.PaymentMethod}; tolak lalu minta kasir mengajukan ulang sebagai Transfer Bank.");

        // Rekening pengirim diperiksa ulang pada saat persetujuan (bisa saja dinonaktifkan sejak
        // diajukan). Penyetuju boleh merevisinya di sini; revisi dicatat pada audit, dan setelah
        // persetujuan rekening terkunci kecuali diubah saat pencairan dengan alasan tertulis.
        var source = await PatientRefundTransferPolicy.ResolveSourceAccountAsync(
            _dbContext, dto.BankAccountId ?? entity.BankAccountId, dto.SenderBankId, cancellationToken);
        if (!source.IsValid)
            throw new FinancePatientRefundValidationException(source.Error!);

        var payeeError = PatientRefundTransferPolicy.ValidatePayee(
            source.Account!, entity.PayeeBankName, entity.PayeeAccountNumber, entity.PayeeAccountName);
        if (payeeError is not null)
            throw new FinancePatientRefundValidationException(
                $"{payeeError} Tolak pengajuan ini bila data tujuan tidak dapat dilengkapi.");

        var previousBankAccountId = entity.BankAccountId;
        entity.BankAccountId = source.Account!.BankAccountId;

        entity.ApprovedAmount = approvedAmount;
        entity.ApprovedBy = actorUserId;
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.Status = FinPatientRefundStatuses.Approved;
        if (!string.IsNullOrWhiteSpace(dto.Note)) entity.ExecutionNotes = dto.Note.Trim();
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new FinancePatientRefundConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }

        await _loggerService.AuditAsync(
            LogCategory,
            "FinancePatientRefund.Approved",
            "Pengajuan refund pasien telah disetujui untuk diproses pembayaran.",
            new
            {
                entity.Id,
                entity.ApprovedAmount,
                PreviousBankAccountId = previousBankAccountId,
                entity.BankAccountId,
                SourceAccountRevised = previousBankAccountId.HasValue && previousBankAccountId != entity.BankAccountId,
                ApprovedBy = actorUserId
            });

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<PatientRefundDetailDto> RejectAsync(
        Guid id,
        RejectPatientRefundRequestDto dto,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            throw new FinancePatientRefundValidationException("Alasan penolakan pengajuan wajib diisi.");

        var entity = await _dbContext.Set<FinPatientRefundRequest>()
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Pengajuan refund tidak ditemukan.");

        if (entity.RowVersion != dto.ExpectedRowVersion)
            throw new FinancePatientRefundConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");

        if (entity.Status != FinPatientRefundStatuses.Pending)
            throw new FinancePatientRefundValidationException($"Pengajuan tidak dalam status PENDING (status saat ini: {entity.Status}).");

        if (entity.RequestedBy == actorUserId)
            throw new FinancePatientRefundValidationException("Pengaju (maker) tidak boleh menolak/menyetujui pengajuannya sendiri (checker).");

        entity.RejectionReason = dto.RejectionReason.Trim();
        entity.ApprovedBy = actorUserId;
        entity.ApprovedAt = DateTimeOffset.UtcNow;
        entity.Status = FinPatientRefundStatuses.Rejected;
        entity.RowVersion = Guid.NewGuid();
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.UpdateBy = actorUserId;

        // Pemulihan saldo deposit jika pengajuan berasal dari sisa deposit ranap (release reservasi)
        var credit = await _dbContext.BilRefundableCredits
            .SingleOrDefaultAsync(x => x.Id == entity.RefundableCreditId && !x.IsDelete, cancellationToken);
        if (credit != null && credit.SourceType == BillingRefundableCreditSourceTypes.Settlement)
        {
            var depositMovement = await _dbContext.BilDepositMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == credit.SourceId && !x.IsDelete, cancellationToken);
            if (depositMovement != null)
            {
                var depositAccount = await _dbContext.BilDepositAccounts
                    .SingleOrDefaultAsync(x => x.Id == depositMovement.DepositAccountId && !x.IsDelete, cancellationToken);
                if (depositAccount != null)
                {
                    depositAccount.AvailableBalance += entity.RequestedAmount;
                    depositAccount.RowVersion = Guid.NewGuid();
                    depositAccount.UpdateDateTime = DateTime.UtcNow;
                    depositAccount.UpdateBy = actorUserId;

                    var reverseMovement = new BilDepositMovement
                    {
                        DepositAccountId = depositAccount.Id,
                        MovementType = BillingDepositMovementTypes.Release,
                        Amount = entity.RequestedAmount,
                        ReversesMovementId = depositMovement.Id,
                        Reason = $"Pemulihan saldo deposit karena penolakan refund Finance: {dto.RejectionReason.Trim()}",
                        OccurredAt = DateTimeOffset.UtcNow,
                        IdempotencyKey = Guid.NewGuid(),
                        CorrelationId = entity.CorrelationId,
                        CausationId = entity.Id,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorUserId
                    };
                    _dbContext.BilDepositMovements.Add(reverseMovement);
                }
            }
            credit.AvailableAmount = 0m;
            credit.Status = BillingRefundableCreditStatuses.Exhausted;
            credit.UpdateDateTime = DateTime.UtcNow;
            credit.UpdateBy = actorUserId;
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new FinancePatientRefundConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }

        await _loggerService.AuditAsync(
            LogCategory,
            "FinancePatientRefund.Rejected",
            "Pengajuan refund pasien telah ditolak.",
            new
            {
                entity.Id,
                Reason = entity.RejectionReason,
                RejectedBy = actorUserId
            });

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    /// <summary>
    /// Kegagalan serialisasi transaksi (SQLSTATE 40001) atau tabrakan penyimpanan akibat dua permintaan
    /// bersamaan. Keduanya berarti pihak lain lebih dulu; pemakai diminta memuat ulang, bukan menerima 500.
    /// </summary>
    private static bool IsConcurrencyFailure(Exception exception)
    {
        if (exception is DbUpdateException)
            return true;

        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (string.Equals(current.GetType().Name, "PostgresException", StringComparison.OrdinalIgnoreCase))
            {
                var sqlState = current.GetType().GetProperty("SqlState")?.GetValue(current) as string;
                if (sqlState == "40001") return true;
            }
        }

        return false;
    }

    public async Task<PatientRefundDetailDto> ExecutePaymentAsync(
        Guid id,
        ExecutePatientRefundRequestDto dto,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
            }

            var entity = await _dbContext.Set<FinPatientRefundRequest>()
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengajuan refund tidak ditemukan.");

            if (entity.RowVersion != dto.ExpectedRowVersion)
                throw new FinancePatientRefundConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");

            if (entity.Status != FinPatientRefundStatuses.Approved && entity.Status != FinPatientRefundStatuses.PartiallyPaid)
                throw new FinancePatientRefundValidationException($"Pengajuan belum disetujui untuk pembayaran (status: {entity.Status}).");

            var totalPayable = entity.ApprovedAmount ?? entity.RequestedAmount;
            var remainingToPay = totalPayable - entity.PaidAmount;

            if (dto.PaidAmount <= 0 || dto.PaidAmount > remainingToPay)
                throw new FinancePatientRefundValidationException($"Nominal pembayaran tidak valid. Sisa yang harus dibayar: Rp {remainingToPay:N2}.");

            // Pencairan hanya melalui Transfer Bank. Diperiksa di server — bukan hanya di formulir —
            // dan berlaku juga bagi pengajuan lama bermetode tunai.
            if (!PatientRefundTransferPolicy.TryNormalizeMethod(dto.PaymentMethod, out _)
                || !PatientRefundTransferPolicy.TryNormalizeMethod(entity.PaymentMethod, out _))
                throw new FinancePatientRefundValidationException(PatientRefundTransferPolicy.TransferOnlyMessage);

            if (string.IsNullOrWhiteSpace(dto.ReferenceNumber))
                throw new FinancePatientRefundValidationException("Nomor referensi bukti transfer bank wajib diisi.");

            // Rekening pengirim: memakai rekening pada pengajuan yang disetujui dan diperiksa ulang
            // (bisa dinonaktifkan sejak disetujui). Mengganti rekening saat pencairan diperbolehkan —
            // agar pencairan tidak buntu — tetapi harus lolos pemeriksaan yang sama, disertai alasan
            // tertulis pada catatan, dan dicatat pada audit.
            var storedBankAccountId = entity.BankAccountId;
            var requestedBankAccountId = dto.BankAccountId ?? storedBankAccountId;
            var sourceChanged = storedBankAccountId.HasValue && requestedBankAccountId != storedBankAccountId;

            if (sourceChanged && string.IsNullOrWhiteSpace(dto.Notes))
                throw new FinancePatientRefundValidationException(
                    "Rekening pengirim berbeda dari rekening yang disetujui. Tuliskan alasan penggantian pada catatan pembayaran.");

            var source = await PatientRefundTransferPolicy.ResolveSourceAccountAsync(
                _dbContext, requestedBankAccountId, null, cancellationToken);
            if (!source.IsValid)
                throw new FinancePatientRefundValidationException(source.Error!);

            var payeeError = PatientRefundTransferPolicy.ValidatePayee(
                source.Account!, entity.PayeeBankName, entity.PayeeAccountNumber, entity.PayeeAccountName);
            if (payeeError is not null)
                throw new FinancePatientRefundValidationException(payeeError);

            var credit = await _dbContext.BilRefundableCredits
                .SingleOrDefaultAsync(x => x.Id == entity.RefundableCreditId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Saldo refundable credit tidak ditemukan.");

            if (dto.PaidAmount > credit.AvailableAmount)
                throw new FinancePatientRefundValidationException($"Saldo refundable credit tidak mencukupi (tersedia: Rp {credit.AvailableAmount:N2}).");

            // 1. Kurangi saldo refundable liability
            credit.AvailableAmount -= dto.PaidAmount;
            if (credit.AvailableAmount == 0)
            {
                credit.Status = BillingRefundableCreditStatuses.Exhausted;
            }
            credit.UpdateDateTime = DateTime.UtcNow;
            credit.UpdateBy = actorUserId;

            // 2. Perbarui baris request pengajuan
            entity.PaidAmount += dto.PaidAmount;
            if (entity.PaidAmount >= totalPayable)
            {
                entity.Status = FinPatientRefundStatuses.Paid;
                entity.CompletedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                entity.Status = FinPatientRefundStatuses.PartiallyPaid;
            }

            entity.PaymentMethod = FinPatientRefundPaymentMethods.Transfer;
            entity.BankAccountId = source.Account!.BankAccountId;
            entity.ReferenceNumber = dto.ReferenceNumber.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Notes)) entity.ExecutionNotes = dto.Notes.Trim();
            entity.ExecutedBy = actorUserId;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            // 3. Pencairan tunai tidak lagi didukung (metode selalu Transfer Bank), sehingga tidak ada
            //    mutasi kas yang dicatat di sini.

            // 4. Terbitkan kejadian Finance -> Accounting Outbox (satu kali)
            await _outboxService.StageEventAsync(new AccountingOutboxEventRequest
            {
                EventTypeCode = FinAccountingEventTypeCodes.PengembalianUangMuka,
                SourceTransactionId = entity.Id.ToString(),
                EventOccurredAt = DateTimeOffset.UtcNow,
                AccountingDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Amount = dto.PaidAmount,
                CorrelationId = entity.CorrelationId,
                CausationId = entity.CorrelationId,
                ActorUserId = actorUserId
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "FinancePatientRefund.Executed",
                "Pembayaran pengembalian saldo refund pasien berhasil dicairkan.",
                new
                {
                    entity.Id,
                    entity.PaidAmount,
                    entity.PaymentMethod,
                    entity.ReferenceNumber,
                    entity.BankAccountId,
                    PreviousBankAccountId = storedBankAccountId,
                    SourceAccountChanged = sourceChanged,
                    CreditAvailableAfter = credit.AvailableAmount,
                    ExecutedBy = actorUserId
                });

            return await GetByIdAsync(entity.Id, cancellationToken);
        }
        catch (Exception exception) when (IsConcurrencyFailure(exception))
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new FinancePatientRefundConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }
}

public abstract class FinancePatientRefundException : Exception
{
    protected FinancePatientRefundException(string message) : base(message) { }
    protected FinancePatientRefundException(string message, Exception inner) : base(message, inner) { }
}

public sealed class FinancePatientRefundValidationException(string message)
    : FinancePatientRefundException(message);

public sealed class FinancePatientRefundConflictException : FinancePatientRefundException
{
    public FinancePatientRefundConflictException(string message) : base(message) { }
    public FinancePatientRefundConflictException(string message, Exception inner) : base(message, inner) { }
}

