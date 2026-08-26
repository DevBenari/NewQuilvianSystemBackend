using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingDiscountService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public BillingDiscountService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<DiscountResponse> ApplyAsync(
        Guid invoiceId,
        ApplyDiscountRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateApplyRequest(request, actorUserId);

        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_DISCOUNT_{invoiceId:N}", cancellationToken);
            }

            var invoice = await _dbContext.BilInvoices
                .Include(x => x.Items).ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");
            EnsureMutableInvoice(invoice, request.ExpectedRowVersion);

            var now = DateTimeOffset.UtcNow;
            var policy = await _dbContext.MstDiscountPolicies.FirstOrDefaultAsync(
                x => x.Id == request.DiscountPolicyId && !x.IsDelete && x.IsActive
                    && x.EffectiveFrom <= now && (x.EffectiveTo == null || now < x.EffectiveTo),
                cancellationToken)
                ?? throw new KeyNotFoundException("Policy diskon tidak ditemukan, tidak aktif, atau belum efektif.");

            var item = ResolveTargetItem(invoice, policy, request.InvoiceItemId);
            if (item?.Category.IsAdministrationFee == true)
                throw new BillingDiscountValidationException("Biaya administrasi tidak dapat didiskon.");

            var duplicate = await _dbContext.BilDiscountApplications.AnyAsync(
                x => x.InvoiceId == invoice.Id && x.DiscountPolicyId == policy.Id
                    && x.InvoiceItemId == request.InvoiceItemId && !x.IsDelete,
                cancellationToken);
            if (duplicate)
                throw new BillingDiscountConflictException("Policy diskon sudah diterapkan pada target invoice yang sama.");

            var (requestedAmount, amount, status) = await ResolveApplicationAsync(
                invoice, item, policy, request.RequestedAmount, cancellationToken);

            if (item is not null)
            {
                var reservedAmount = await _dbContext.BilDiscountApplications.AsNoTracking()
                    .Where(x => x.InvoiceId == invoice.Id && x.InvoiceItemId == item.Id && !x.IsDelete)
                    .SumAsync(x => x.Amount, cancellationToken);
                var grossAmount = Money(item.Quantity * item.UnitPrice);
                if (reservedAmount + amount > grossAmount)
                    throw new BillingDiscountValidationException("Total diskon item melebihi nilai bruto item.");
            }

            var entity = new BilDiscountApplication
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                InvoiceItemId = item?.Id,
                InvoiceItem = item,
                DiscountPolicyId = policy.Id,
                DiscountPolicy = policy,
                DiscountType = policy.DiscountType,
                RequestedAmount = requestedAmount,
                Amount = amount,
                ApprovalStatus = status,
                RequestedBy = actorUserId,
                Reason = request.Reason.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.BilDiscountApplications.Add(entity);
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await AuditAsync("BillingDiscount.Apply", entity, actorUserId, "NONE", status, 0, EffectiveAmount(entity), request.Reason);
            return Map(entity, invoice.RowVersion);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDiscountConflictException("Diskon tidak dapat disimpan karena invoice telah berubah.", exception);
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

    public async Task<DiscountResponse> ApproveDoctorAsync(
        Guid invoiceId,
        Guid discountId,
        ApproveDiscountRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingDiscountValidationException("ExpectedRowVersion wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new BillingDiscountForbiddenException("Identitas pengguna tidak valid.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingDiscountValidationException("Alasan approval wajib diisi.");

        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_DISCOUNT_{invoiceId:N}", cancellationToken);
            }

            var application = await _dbContext.BilDiscountApplications
                .Include(x => x.Invoice)
                .Include(x => x.DiscountPolicy)
                .FirstOrDefaultAsync(x => x.Id == discountId && x.InvoiceId == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Pengajuan diskon tidak ditemukan.");
            EnsureMutableInvoice(application.Invoice, request.ExpectedRowVersion);

            if (application.DiscountType != DiscountPolicyValues.Doctor)
                throw new BillingDiscountValidationException("Hanya diskon jasa dokter yang memakai approval dokter.");
            if (application.ApprovalStatus == BillingDiscountApprovalStatuses.PendingFinance)
                throw new BillingDiscountValidationException("Diskon melewati limit policy dan memerlukan approval Finance melalui alur exception.");
            if (application.ApprovalStatus != BillingDiscountApprovalStatuses.PendingDoctor)
                throw new BillingDiscountConflictException("Pengajuan diskon tidak lagi menunggu approval dokter.");
            if (application.RequestedBy == actorUserId)
                throw new BillingDiscountForbiddenException("Pembuat pengajuan tidak boleh menyetujui pengajuannya sendiri.");

            var encounterDoctorId = await _dbContext.TrxPatientEncounters.AsNoTracking()
                .Where(x => x.Id == application.Invoice.EncounterId && !x.IsDelete && !x.IsCancel)
                .Select(x => x.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);
            if (!encounterDoctorId.HasValue || encounterDoctorId == Guid.Empty)
                throw new BillingDiscountValidationException("Dokter penanggung jawab encounter belum terpetakan.");

            var actorDoctorId = await _dbContext.Users.AsNoTracking()
                .Where(x => x.Id == actorUserId && x.IsActive)
                .Select(x => x.DoctorId)
                .FirstOrDefaultAsync(cancellationToken);
            if (!actorDoctorId.HasValue || actorDoctorId.Value != encounterDoctorId.Value)
                throw new BillingDiscountForbiddenException("Diskon jasa dokter hanya dapat disetujui oleh dokter pemilik share.");

            var beforeStatus = application.ApprovalStatus;
            application.ApprovalStatus = BillingDiscountApprovalStatuses.Approved;
            application.ApprovedBy = actorUserId;
            application.UpdateDateTime = DateTime.UtcNow;
            application.UpdateBy = actorUserId;
            application.Invoice.RowVersion = Guid.NewGuid();
            application.Invoice.UpdateDateTime = DateTime.UtcNow;
            application.Invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await AuditAsync("BillingDoctorDiscount.Approve", application, actorUserId, beforeStatus,
                application.ApprovalStatus, 0, application.Amount, request.Reason);
            return Map(application, application.Invoice.RowVersion);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDiscountConflictException("Approval diskon tidak dapat disimpan karena invoice telah berubah.", exception);
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

    internal static decimal CalculatePolicyAmount(MstDiscountPolicy policy, decimal basisAmount)
    {
        var normalizedBasis = Money(Math.Max(0, basisAmount));
        var rawAmount = policy.ValueType == DiscountPolicyValues.Percentage
            ? normalizedBasis * policy.Value / 100m
            : policy.Value;
        var limited = policy.Limit.HasValue ? Math.Min(rawAmount, policy.Limit.Value) : rawAmount;
        return Money(Math.Min(normalizedBasis, Math.Max(0, limited)));
    }

    internal static DiscountResponse Map(BilDiscountApplication entity, Guid invoiceRowVersion) => new()
    {
        Id = entity.Id,
        InvoiceId = entity.InvoiceId,
        InvoiceItemId = entity.InvoiceItemId,
        DiscountPolicyId = entity.DiscountPolicyId,
        PolicyCode = entity.DiscountPolicy?.Code ?? string.Empty,
        DiscountType = entity.DiscountType,
        TargetComponent = entity.DiscountPolicy?.TargetComponent ?? string.Empty,
        RequestedAmount = entity.RequestedAmount,
        Amount = entity.Amount,
        ApprovalStatus = entity.ApprovalStatus,
        RequestedBy = entity.RequestedBy,
        ApprovedBy = entity.ApprovedBy,
        Reason = entity.Reason,
        IsEffective = entity.ApprovalStatus == BillingDiscountApprovalStatuses.Approved,
        RequiresFinanceApproval = entity.ApprovalStatus == BillingDiscountApprovalStatuses.PendingFinance,
        InvoiceRowVersion = invoiceRowVersion,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime
    };

    private async Task<(decimal RequestedAmount, decimal Amount, string Status)> ResolveApplicationAsync(
        BilInvoice invoice,
        BilInvoiceItem? item,
        MstDiscountPolicy policy,
        decimal? requestedAmount,
        CancellationToken cancellationToken)
    {
        if (policy.DiscountType == DiscountPolicyValues.Doctor)
        {
            if (!requestedAmount.HasValue || requestedAmount <= 0)
                throw new BillingDiscountValidationException("Nominal diskon jasa dokter wajib diisi.");
            if (item is null || item.DoctorShare <= 0)
                throw new BillingDiscountValidationException("Item tidak memiliki komponen jasa dokter yang dapat didiskon.");

            var reservedDoctorAmount = await _dbContext.BilDiscountApplications.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.InvoiceItemId == item.Id
                    && x.DiscountType == DiscountPolicyValues.Doctor && !x.IsDelete)
                .SumAsync(x => x.Amount, cancellationToken);
            var amount = Money(requestedAmount.Value);
            if (reservedDoctorAmount + amount > item.DoctorShare)
                throw new BillingDiscountValidationException(
                    "Diskon dokter melebihi komponen jasa dokter; perubahan bagian rumah sakit memerlukan alur exception Finance.");

            var requiresFinance = policy.Limit.HasValue && amount > policy.Limit.Value;
            return (amount, amount, requiresFinance
                ? BillingDiscountApprovalStatuses.PendingFinance
                : BillingDiscountApprovalStatuses.PendingDoctor);
        }

        if (requestedAmount.HasValue)
            throw new BillingDiscountValidationException("Nominal promo ditentukan oleh master policy dan tidak boleh diubah per transaksi.");

        decimal basisAmount;
        if (policy.DiscountType == DiscountPolicyValues.PromoItem)
        {
            basisAmount = Money((item?.Quantity ?? 0) * (item?.UnitPrice ?? 0));
        }
        else
        {
            if (invoice.CurrentCalculationVersion <= 0)
                throw new BillingDiscountValidationException("Hitung invoice terlebih dahulu sebelum menerapkan promo total.");
            var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                .FirstOrDefaultAsync(x => x.InvoiceId == invoice.Id
                    && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete, cancellationToken)
                ?? throw new BillingDiscountConflictException("Versi kalkulasi invoice saat ini tidak ditemukan.");
            var breakdown = BillingCalculationService.DeserializeBreakdown(calculation.BreakdownSnapshot);
            var discountableItemIds = invoice.Items.Where(x => !x.Category.IsAdministrationFee)
                .Select(x => x.Id).ToHashSet();
            var discountableItems = breakdown.Items
                .Where(x => discountableItemIds.Contains(x.InvoiceItemId))
                .Sum(x => x.NetAmount);
            var discountablePatientAmount = Math.Max(
                0, calculation.PatientAmount - breakdown.AdministrationFee.AppliedAmount);
            basisAmount = Math.Min(discountablePatientAmount, discountableItems);
        }

        var promoAmount = CalculatePolicyAmount(policy, basisAmount);
        if (promoAmount <= 0)
            throw new BillingDiscountValidationException("Policy promo tidak menghasilkan nominal diskon pada target ini.");
        return (promoAmount, promoAmount, BillingDiscountApprovalStatuses.Approved);
    }

    private static BilInvoiceItem? ResolveTargetItem(
        BilInvoice invoice,
        MstDiscountPolicy policy,
        Guid? requestedItemId)
    {
        if (policy.DiscountType == DiscountPolicyValues.PromoTotal)
        {
            if (requestedItemId.HasValue)
                throw new BillingDiscountValidationException("Promo total tidak boleh menargetkan item invoice tertentu.");
            return null;
        }

        if (!requestedItemId.HasValue || requestedItemId == Guid.Empty)
            throw new BillingDiscountValidationException("InvoiceItemId wajib diisi untuk diskon item atau jasa dokter.");
        return invoice.Items.FirstOrDefault(x => x.Id == requestedItemId.Value
            && !x.IsDelete && x.Status != BillingInvoiceItemStatuses.Voided)
            ?? throw new KeyNotFoundException("Item invoice aktif tidak ditemukan.");
    }

    private static void ValidateApplyRequest(ApplyDiscountRequest request, Guid actorUserId)
    {
        if (request.DiscountPolicyId == Guid.Empty)
            throw new BillingDiscountValidationException("DiscountPolicyId wajib diisi.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingDiscountValidationException("ExpectedRowVersion wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new BillingDiscountForbiddenException("Identitas pengguna tidak valid.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingDiscountValidationException("Alasan penerapan diskon wajib diisi.");
    }

    private static void EnsureMutableInvoice(BilInvoice invoice, Guid expectedRowVersion)
    {
        if (invoice.Status != BillingInvoiceStatuses.Open)
            throw new BillingDiscountValidationException("Invoice final tidak dapat menerima perubahan diskon.");
        if (invoice.RowVersion != expectedRowVersion)
            throw new BillingDiscountConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");
    }

    private Task AuditAsync(
        string action,
        BilDiscountApplication entity,
        Guid actorUserId,
        string beforeStatus,
        string afterStatus,
        decimal beforeAmount,
        decimal afterAmount,
        string reason) =>
        _loggerService.AuditAsync(LogCategory, action,
            $"DiscountApplication={entity.Id:N}; status {beforeStatus}->{afterStatus}; nominal efektif {beforeAmount:0.00}->{afterAmount:0.00}.", new
        {
            DiscountApplicationId = entity.Id,
            InvoiceId = entity.InvoiceId,
            entity.InvoiceItemId,
            entity.DiscountPolicyId,
            entity.DiscountType,
            BeforeStatus = beforeStatus,
            AfterStatus = afterStatus,
            BeforeAmount = beforeAmount,
            AfterAmount = afterAmount,
            UserId = actorUserId,
            Reason = reason.Trim()
        });

    private static decimal EffectiveAmount(BilDiscountApplication entity) =>
        entity.ApprovalStatus == BillingDiscountApprovalStatuses.Approved ? entity.Amount : 0;

    private static decimal Money(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private async Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        await _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);
}

public sealed class BillingDiscountValidationException(string message) : Exception(message);
public sealed class BillingDiscountForbiddenException(string message) : Exception(message);

public sealed class BillingDiscountConflictException : Exception
{
    public BillingDiscountConflictException(string message) : base(message) { }
    public BillingDiscountConflictException(string message, Exception innerException) : base(message, innerException) { }
}
