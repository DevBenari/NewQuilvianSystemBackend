using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingDiscountService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingCalculationService _calculationService;
    private readonly LoggerService _loggerService;

    public BillingDiscountService(
        ApplicationDbContext dbContext, BillingCalculationService calculationService, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _calculationService = calculationService;
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

            var isVoucherSearch = !string.IsNullOrWhiteSpace(request.VoucherCode);
            MstDiscountPolicy? policy;
            if (isVoucherSearch)
            {
                var normalizedCode = request.VoucherCode!.Trim().ToUpperInvariant();
                policy = await _dbContext.MstDiscountPolicies.FirstOrDefaultAsync(
                    x => x.Code.ToUpper() == normalizedCode && !x.IsDelete,
                    cancellationToken);
                if (policy is null)
                    throw new KeyNotFoundException("Voucher tidak ditemukan.");
            }
            else
            {
                policy = await _dbContext.MstDiscountPolicies.FirstOrDefaultAsync(
                    x => x.Id == request.DiscountPolicyId && !x.IsDelete,
                    cancellationToken);
                if (policy is null)
                    throw new KeyNotFoundException("Policy diskon tidak ditemukan.");
            }

            var now = DateTimeOffset.UtcNow;
            if (!policy.IsActive)
                throw new BillingDiscountValidationException(isVoucherSearch
                    ? "Voucher tidak aktif."
                    : "Policy diskon tidak aktif.");

            if (policy.EffectiveFrom > now)
                throw new BillingDiscountValidationException(isVoucherSearch
                    ? "Voucher belum dapat digunakan (belum berlaku)."
                    : "Policy diskon belum efektif.");

            if (policy.EffectiveTo.HasValue && now >= policy.EffectiveTo.Value)
                throw new BillingDiscountValidationException(isVoucherSearch
                    ? "Voucher sudah kedaluwarsa."
                    : "Policy diskon sudah kedaluwarsa.");

            var item = ResolveTargetItem(invoice, policy, request.InvoiceItemId);
            if (item?.Category.IsAdministrationFee == true)
                throw new BillingDiscountValidationException("Biaya administrasi tidak dapat didiskon.");

            var duplicate = await _dbContext.BilDiscountApplications.AnyAsync(
                x => x.InvoiceId == invoice.Id && x.DiscountPolicyId == policy.Id
                    && x.InvoiceItemId == request.InvoiceItemId && !x.IsDelete,
                cancellationToken);
            if (duplicate)
                throw new BillingDiscountConflictException(isVoucherSearch
                    ? "Voucher sudah pernah diterapkan pada invoice ini."
                    : "Policy diskon sudah diterapkan pada target invoice yang sama.");

            if (policy.DiscountType == DiscountPolicyValues.PromoTotal || policy.DiscountType == DiscountPolicyValues.PromoItem)
            {
                var currentCalculation = invoice.CurrentCalculationVersion > 0
                    ? await _dbContext.BilCalculationVersions.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.InvoiceId == invoice.Id
                            && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete, cancellationToken)
                    : null;

                if (currentCalculation is null || IsCalculationStale(currentCalculation, invoice))
                {
                    await EnsureCalculationExistsAsync(invoice, actorUserId, cancellationToken);
                }
            }

            var (requestedAmount, amount, status) = await ResolveApplicationAsync(
                invoice, item, policy, request.RequestedAmount, actorUserId, cancellationToken);

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

            // Diskon yang langsung efektif (PromoTotal/PromoItem) MUST mengubah total invoice
            // seketika - tanpa ini, invoice.CurrentCalculationVersion tetap menunjuk versi lama
            // yang belum memasukkan diskon yang baru saja tersimpan, dan sisa tagihan yang dibaca
            // Menu Pembayaran/popup Bayar tetap salah sampai ada peristiwa lain yang memicu
            // recalculate (mis. submit pembayaran). Diskon jasa dokter (PendingDoctor/PendingFinance)
            // sengaja TIDAK memicu ini - baru berlaku setelah ApproveDoctorAsync.
            CalculationResponse? calculation = null;
            if (status == BillingDiscountApprovalStatuses.Approved)
                calculation = await RecalculateAfterDiscountChangeAsync(
                    invoice, $"Kalkulasi ulang setelah penerapan diskon {policy.Code}.", actorUserId, cancellationToken);

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await AuditAsync("BillingDiscount.Apply", entity, actorUserId, "NONE", status, 0, EffectiveAmount(entity), request.Reason);
            return Map(entity, invoice.RowVersion, calculation);
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

            var encounterDoctorId = await _dbContext.RegPatientEncounters.AsNoTracking()
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

            // Diskon dokter baru mulai mengurangi total invoice persis di titik ini (Approved) -
            // sama seperti ApplyAsync, invoice.CurrentCalculationVersion MUST diperbarui seketika,
            // bukan menunggu peristiwa lain.
            var calculation = await RecalculateAfterDiscountChangeAsync(
                application.Invoice, $"Kalkulasi ulang setelah approval diskon jasa dokter {application.DiscountPolicy?.Code}.",
                actorUserId, cancellationToken);

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await AuditAsync("BillingDoctorDiscount.Approve", application, actorUserId, beforeStatus,
                application.ApprovalStatus, 0, application.Amount, request.Reason);
            return Map(application, application.Invoice.RowVersion, calculation);
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

    public async Task<DiscountResponse> CancelAsync(
        Guid invoiceId,
        Guid discountId,
        CancelDiscountRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (actorUserId == Guid.Empty)
            throw new BillingDiscountForbiddenException("Identitas pengguna tidak valid.");

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
                .Include(x => x.DiscountApplications).ThenInclude(x => x.DiscountPolicy)
                .FirstOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice Billing tidak ditemukan.");

            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingDiscountValidationException("Invoice final tidak dapat menerima perubahan diskon.");

            if (request.ExpectedRowVersion != Guid.Empty && invoice.RowVersion != request.ExpectedRowVersion)
                throw new BillingDiscountConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");

            var application = invoice.DiscountApplications
                .FirstOrDefault(x => x.Id == discountId && !x.IsDelete)
                ?? throw new KeyNotFoundException("Penerapan diskon tidak ditemukan.");

            var beforeStatus = application.ApprovalStatus;
            var beforeAmount = application.Amount;
            var policyCode = application.DiscountPolicy?.Code ?? "PROMO";

            application.IsDelete = true;
            application.DeleteDateTime = DateTime.UtcNow;
            application.DeleteBy = actorUserId;

            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var reason = string.IsNullOrWhiteSpace(request.Reason)
                ? $"Pembatalan promo/voucher {policyCode}."
                : request.Reason.Trim();

            CalculationResponse? calculation = null;
            if (beforeStatus == BillingDiscountApprovalStatuses.Approved)
            {
                calculation = await RecalculateAfterDiscountChangeAsync(
                    invoice, reason, actorUserId, cancellationToken);
            }

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);

            await AuditAsync("BillingDiscount.Cancel", application, actorUserId, beforeStatus, "CANCELLED", beforeAmount, 0, reason);

            return Map(application, invoice.RowVersion, calculation);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDiscountConflictException("Pembatalan diskon tidak dapat disimpan karena invoice telah berubah.", exception);
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

    // Antrean approval milik dokter yang sedang login. Kepemilikan ditentukan dari DPJP encounter
    // (User.DoctorId == RegPatientEncounter.DoctorId) - aturan kepemilikan yang sama persis dipakai
    // ApproveDoctorAsync, sehingga daftar ini tidak pernah menampilkan pengajuan yang pada akhirnya
    // akan ditolak backend saat disetujui. Akun non-dokter mendapat daftar kosong, bukan error.
    public async Task<PagedResult<DoctorDiscountApprovalResponse>> GetPendingDoctorApprovalsAsync(
        DoctorDiscountApprovalQuery request,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var empty = new PagedResult<DoctorDiscountApprovalResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = 0,
            TotalPage = 0
        };

        if (actorUserId == Guid.Empty) return empty;

        var actorDoctorId = await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == actorUserId && x.IsActive)
            .Select(x => x.DoctorId)
            .FirstOrDefaultAsync(cancellationToken);
        if (!actorDoctorId.HasValue || actorDoctorId.Value == Guid.Empty) return empty;

        var query =
            from application in _dbContext.BilDiscountApplications.AsNoTracking()
            join invoice in _dbContext.BilInvoices.AsNoTracking()
                on application.InvoiceId equals invoice.Id
            join encounter in _dbContext.RegPatientEncounters.AsNoTracking()
                on invoice.EncounterId equals encounter.Id
            join patient in _dbContext.MstPatients.AsNoTracking()
                on encounter.PatientId equals patient.Id
            join policy in _dbContext.MstDiscountPolicies.AsNoTracking()
                on application.DiscountPolicyId equals policy.Id
            where !application.IsDelete
                && application.DiscountType == DiscountPolicyValues.Doctor
                && application.ApprovalStatus == BillingDiscountApprovalStatuses.PendingDoctor
                && !invoice.IsDelete
                && invoice.Status == BillingInvoiceStatuses.Open
                && !encounter.IsDelete
                && !encounter.IsCancel
                && encounter.DoctorId == actorDoctorId
            select new { application, invoice, encounter, patient, policy };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                x.invoice.InvoiceNumber.ToUpper().Contains(search) ||
                x.patient.FullName.ToUpper().Contains(search) ||
                x.patient.MedicalRecordNumber.ToUpper().Contains(search));
        }

        var totalData = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(x => x.application.CreateDateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.application.Id,
                x.application.InvoiceId,
                x.invoice.InvoiceNumber,
                x.invoice.ServiceType,
                x.invoice.RowVersion,
                x.invoice.EncounterId,
                x.encounter.EncounterNumber,
                PatientName = x.patient.FullName,
                x.patient.MedicalRecordNumber,
                PolicyCode = x.policy.Code,
                PolicyName = x.policy.Name,
                x.application.InvoiceItemId,
                x.application.RequestedAmount,
                x.application.Amount,
                x.application.Reason,
                x.application.RequestedBy,
                x.application.CreateDateTime
            })
            .ToListAsync(cancellationToken);

        var itemIds = rows.Where(x => x.InvoiceItemId.HasValue)
            .Select(x => x.InvoiceItemId!.Value).Distinct().ToList();
        var itemDescriptions = itemIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.BilInvoiceItems.AsNoTracking()
                .Where(x => itemIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DescriptionSnapshot, cancellationToken);

        var requesterIds = rows.Select(x => x.RequestedBy).Distinct().ToList();
        var requesterNames = requesterIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await _dbContext.Users.AsNoTracking()
                .Where(x => requesterIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        return new PagedResult<DoctorDiscountApprovalResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
            Items = rows.Select(x =>
            {
                var isSelfRequested = x.RequestedBy == actorUserId;
                return new DoctorDiscountApprovalResponse
                {
                    Id = x.Id,
                    InvoiceId = x.InvoiceId,
                    InvoiceNumber = x.InvoiceNumber,
                    ServiceType = x.ServiceType,
                    InvoiceRowVersion = x.RowVersion,
                    EncounterId = x.EncounterId,
                    EncounterNumber = x.EncounterNumber,
                    PatientName = x.PatientName,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    PolicyCode = x.PolicyCode,
                    PolicyName = x.PolicyName,
                    ItemDescription = x.InvoiceItemId.HasValue
                        && itemDescriptions.TryGetValue(x.InvoiceItemId.Value, out var description)
                            ? description
                            : null,
                    RequestedAmount = x.RequestedAmount,
                    Amount = x.Amount,
                    Reason = x.Reason,
                    RequestedBy = x.RequestedBy,
                    RequestedByName = requesterNames.TryGetValue(x.RequestedBy, out var name) ? name : null,
                    CreateDateTime = x.CreateDateTime,
                    CanApprove = !isSelfRequested,
                    BlockedReason = isSelfRequested
                        ? "Pengaju tidak dapat menyetujui pengajuannya sendiri."
                        : null
                };
            }).ToList()
        };
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

    internal static DiscountResponse Map(
        BilDiscountApplication entity, Guid invoiceRowVersion, CalculationResponse? calculation = null) => new()
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
        ApprovalStatus = entity.IsDelete ? "CANCELLED" : entity.ApprovalStatus,
        RequestedBy = entity.RequestedBy,
        ApprovedBy = entity.ApprovedBy,
        Reason = entity.Reason,
        IsEffective = !entity.IsDelete && entity.ApprovalStatus == BillingDiscountApprovalStatuses.Approved,
        RequiresFinanceApproval = !entity.IsDelete && entity.ApprovalStatus == BillingDiscountApprovalStatuses.PendingFinance,
        InvoiceRowVersion = invoiceRowVersion,
        CreateDateTime = entity.CreateDateTime,
        UpdateDateTime = entity.UpdateDateTime,
        // Baru: satu-satunya sumber "authoritative totals" setelah promo/diskon berhasil - FE
        // TIDAK perlu request GET terpisah untuk membaca PatientAmount/TotalDiscount terbaru.
        // Null pada diskon jasa dokter yang masih PendingDoctor/PendingFinance (belum efektif,
        // belum ada kalkulasi baru) dan pada listing riwayat diskon lama (lihat BillingInvoiceService).
        Calculation = calculation
    };

    private async Task<(decimal RequestedAmount, decimal Amount, string Status)> ResolveApplicationAsync(
        BilInvoice invoice,
        BilInvoiceItem? item,
        MstDiscountPolicy policy,
        decimal? requestedAmount,
        Guid actorUserId,
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
            // BE fix (permintaan pengguna, orchestration Apply Promo): sebelum revisi ini, promo
            // total ditolak mentah-mentah kalau invoice belum pernah dihitung
            // (invoice.CurrentCalculationVersion <= 0) - memaksa kasir tahu dan menjalankan langkah
            // teknis "Hitung Invoice" terpisah lebih dulu. Menu Pembayaran hanya memanggil
            // PreviewCalculationAsync saat halaman dibuka (BillingCalculationService.cs,
            // sengaja tidak persist - membuka halaman bukan peristiwa bisnis), jadi
            // CurrentCalculationVersion tetap 0 sampai ada peristiwa yang benar-benar mempersist
            // versi kalkulasi (sebelumnya cuma submit pembayaran). Di sinilah kalkulasi pertama
            // dipersist otomatis begitu ada peristiwa bisnis nyata yang membutuhkannya - bukan
            // menghapus penjaganya, hanya memindahkan siapa yang memicunya dari kasir ke sistem.
            if (invoice.CurrentCalculationVersion <= 0)
                await EnsureCalculationExistsAsync(invoice, actorUserId, cancellationToken);
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
            if (requestedItemId.HasValue && requestedItemId.Value != Guid.Empty)
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
        if (request.DiscountPolicyId == Guid.Empty && string.IsNullOrWhiteSpace(request.VoucherCode))
            throw new BillingDiscountValidationException("Pilih promo atau masukkan kode voucher.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingDiscountValidationException("ExpectedRowVersion wajib diisi.");
        if (actorUserId == Guid.Empty)
            throw new BillingDiscountForbiddenException("Identitas pengguna tidak valid.");
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            request.Reason = !string.IsNullOrWhiteSpace(request.VoucherCode)
                ? $"Penerapan voucher {request.VoucherCode.Trim()}."
                : "Penerapan promo/diskon pada invoice.";
        }
    }

    private static bool IsCalculationStale(BilCalculationVersion calculation, BilInvoice invoice)
    {
        var activeItems = invoice.Items
            .Where(x => !x.IsDelete && x.Status != BillingInvoiceItemStatuses.Voided)
            .ToList();

        var currentActiveGross = activeItems.Sum(x => Money(x.Quantity * x.UnitPrice));
        if (currentActiveGross != calculation.GrossAmount)
            return true;

        if (activeItems.Any(x => x.CreateDateTime > calculation.CalculatedAt
            || (x.UpdateDateTime.HasValue && x.UpdateDateTime.Value > calculation.CalculatedAt)))
            return true;

        var breakdown = BillingCalculationService.DeserializeBreakdown(calculation.BreakdownSnapshot);
        if (breakdown.Items.Count != activeItems.Count)
            return true;

        var activeItemIds = activeItems.Select(x => x.Id).ToHashSet();
        if (breakdown.Items.Any(x => !activeItemIds.Contains(x.InvoiceItemId)))
            return true;

        return false;
    }

    // BE fix (orchestration Apply Promo): dipanggil hanya ketika invoice.CurrentCalculationVersion
    // masih 0 - memastikan ada MINIMAL satu BilCalculationVersion persisted sebelum promo total
    // menghitung basisnya, tanpa memaksa kasir menjalankan langkah "Hitung Invoice" terpisah.
    // Reuse penuh BillingCalculationService.RecalculateAsync (bukan formula baru) - method itu
    // sengaja membuka transaction sendiri HANYA bila belum ada transaction berjalan pada
    // ApplicationDbContext yang sama, jadi pemanggilan dari sini ikut transaction Serializable
    // milik ApplyAsync (BeginTransactionAsync/CommitAsync/RollbackAsync tetap wewenang pemanggil
    // terluar). invoice adalah instance yang sama yang sedang dilacak _dbContext, sehingga
    // RowVersion dan CurrentCalculationVersion yang ditulis RecalculateAsync otomatis terlihat
    // pada variabel invoice milik pemanggil tanpa perlu re-fetch.
    private async Task EnsureCalculationExistsAsync(BilInvoice invoice, Guid actorUserId, CancellationToken cancellationToken)
    {
        try
        {
            await _calculationService.RecalculateAsync(
                invoice.Id,
                new RecalculateInvoiceRequest
                {
                    ExpectedRowVersion = invoice.RowVersion,
                    Reason = "Kalkulasi otomatis sebelum menerapkan promo/voucher."
                },
                actorUserId,
                cancellationToken);
        }
        catch (BillingCalculationValidationException exception)
        {
            // Diterjemahkan ke kosakata exception BillingDiscount supaya
            // BillingInvoicesController.ExecuteDiscountCommandAsync (yang hanya mengenal
            // BillingDiscount*Exception) tetap memetakannya ke 422 - bukan lolos tak tertangani
            // ke GlobalExceptionMiddleware yang hanya mengembalikan 500 generik.
            throw new BillingDiscountValidationException(exception.Message);
        }
        catch (BillingCalculationConflictException exception)
        {
            throw new BillingDiscountConflictException(exception.Message);
        }
    }

    // BE fix (orchestration Apply Promo/Approve Diskon Dokter): dipanggil SETELAH sebuah diskon
    // baru saja menjadi efektif (Approved), supaya BilCalculationVersion aktif langsung
    // mencerminkan diskon itu - BillingCalculationService.CalculateAsync meng-query ulang
    // BilDiscountApplications berstatus Approved dari database setiap kali dipanggil, jadi
    // baris yang baru saja di-SaveChangesAsync otomatis ikut terhitung tanpa logic tambahan apa
    // pun di sini. Dipanggil di dalam transaction yang sama dengan penyimpanan diskon (atomicity
    // - diskon dan kalkulasi barunya sama-sama commit atau sama-sama rollback).
    private async Task<CalculationResponse> RecalculateAfterDiscountChangeAsync(
        BilInvoice invoice, string reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        try
        {
            return await _calculationService.RecalculateAsync(
                invoice.Id,
                new RecalculateInvoiceRequest { ExpectedRowVersion = invoice.RowVersion, Reason = reason },
                actorUserId,
                cancellationToken);
        }
        catch (BillingCalculationValidationException exception)
        {
            throw new BillingDiscountValidationException(exception.Message);
        }
        catch (BillingCalculationConflictException exception)
        {
            throw new BillingDiscountConflictException(exception.Message);
        }
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
