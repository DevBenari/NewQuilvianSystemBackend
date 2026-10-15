using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingDepositOrderService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing.DepositOrders";
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingNumberSeriesService _numberSeries;
    private readonly BillingDepositService _depositService;
    private readonly CashierShiftService _cashierShiftService;
    private readonly LoggerService _loggerService;

    public BillingDepositOrderService(
        ApplicationDbContext dbContext,
        BillingNumberSeriesService numberSeries,
        BillingDepositService depositService,
        CashierShiftService cashierShiftService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _numberSeries = numberSeries;
        _depositService = depositService;
        _cashierShiftService = cashierShiftService;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Membuat permintaan pembayaran deposit baru (mis. Initial Deposit dari Admisi Rawat Inap).
    /// Bukan transaksi kas riil; uang baru diakui saat kasir memproses pembayaran.
    /// </summary>
    public async Task<DepositOrderDetailDto> CreateOrderAsync(
        CreateDepositOrderRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new BillingDepositValidationException("Data permintaan order deposit wajib diisi.");

        if (request.EpisodeId == Guid.Empty)
            throw new BillingDepositValidationException("EpisodeId wajib diisi.");

        if (request.EncounterId == Guid.Empty)
            throw new BillingDepositValidationException("EncounterId wajib diisi.");

        if (request.RequestedAmount <= 0)
            throw new BillingDepositValidationException("Nominal deposit yang diminta harus lebih besar dari 0.");

        // Validasi Episode & Encounter
        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.EpisodeId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Episode rawat inap {request.EpisodeId} tidak ditemukan.");

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.EncounterId && !x.IsDelete && !x.IsCancel, cancellationToken)
            ?? throw new KeyNotFoundException($"Encounter rawat inap {request.EncounterId} tidak ditemukan.");

        if (episode.EncounterId != encounter.Id)
            throw new BillingDepositValidationException("EncounterId tidak cocok dengan Episode rawat inap.");

        var patientId = episode.PatientId;

        // Idempotency check: jika idempotency key sudah ada, kembalikan hasil sebelumnya (replay)
        var existingByIdempotency = await _dbContext.BilDepositPaymentOrders
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey && !x.IsDelete, cancellationToken);

        if (existingByIdempotency != null)
        {
            return await GetOrderDetailAsync(existingByIdempotency.Id, cancellationToken);
        }

        // Jika INITIAL_DEPOSIT, satu episode hanya boleh punya satu order aktif yang sah
        if (string.Equals(request.OrderType, DepositOrderTypes.InitialDeposit, StringComparison.OrdinalIgnoreCase))
        {
            var existingInitial = await _dbContext.BilDepositPaymentOrders
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x =>
                    x.EpisodeId == request.EpisodeId &&
                    x.OrderType == DepositOrderTypes.InitialDeposit &&
                    x.Status != DepositOrderStatuses.Cancelled &&
                    !x.IsDelete, cancellationToken);

            if (existingInitial != null)
            {
                // Kembalikan order aktif yang sudah ada
                return await GetOrderDetailAsync(existingInitial.Id, cancellationToken);
            }
        }

        var now = DateTimeOffset.UtcNow;
        var orderNumber = await _numberSeries.AllocateDepositOrderNumberAsync(
            actorUserId, now, cancellationToken);

        var order = new BilDepositPaymentOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            EpisodeId = request.EpisodeId,
            EncounterId = request.EncounterId,
            PatientId = patientId,
            OrderType = string.IsNullOrWhiteSpace(request.OrderType) ? DepositOrderTypes.InitialDeposit : request.OrderType.Trim(),
            SourceType = string.IsNullOrWhiteSpace(request.SourceType) ? DepositOrderSourceTypes.InpatientAdmission : request.SourceType.Trim(),
            SourceReferenceId = request.SourceReferenceId ?? request.EpisodeId,
            RequestedAmount = request.RequestedAmount,
            PaidAmount = 0m,
            OutstandingAmount = request.RequestedAmount,
            Status = DepositOrderStatuses.Unpaid,
            DueDate = request.DueDate,
            PolicyId = request.PolicyId,
            Remarks = request.Reason,
            IdempotencyKey = request.IdempotencyKey,
            CorrelationId = request.EpisodeId,
            CausationId = request.EncounterId,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.BilDepositPaymentOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(
            LogCategory,
            "CreateDepositPaymentOrder",
            $"Order deposit {order.OrderNumber} sebesar Rp {order.RequestedAmount:N0} berhasil dibuat untuk pasien {patientId}.",
            new
            {
                order.Id,
                order.OrderNumber,
                order.RequestedAmount,
                PatientId = patientId,
                ActorUserId = actorUserId
            });

        return await GetOrderDetailAsync(order.Id, cancellationToken);
    }

    /// <summary>
    /// Mencari order deposit berdasarkan kata kunci No. RM, Nama Pasien, NIK, atau No. Order.
    /// Dilengkapi filter status, jenis order, dan pagination server-side.
    /// </summary>
    public async Task<PagedResult<DepositOrderSummaryDto>> SearchOrdersAsync(
        DepositOrderQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

        var baseQuery = _dbContext.BilDepositPaymentOrders.AsNoTracking()
            .Where(x => !x.IsDelete);

        // Filter status
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            baseQuery = baseQuery.Where(x => x.Status == query.Status.Trim());
        }

        // Filter order type
        if (!string.IsNullOrWhiteSpace(query.OrderType))
        {
            baseQuery = baseQuery.Where(x => x.OrderType == query.OrderType.Trim());
        }

        // Filter episode / encounter / patient
        if (query.EpisodeId.HasValue && query.EpisodeId.Value != Guid.Empty)
        {
            baseQuery = baseQuery.Where(x => x.EpisodeId == query.EpisodeId.Value);
        }

        if (query.EncounterId.HasValue && query.EncounterId.Value != Guid.Empty)
        {
            baseQuery = baseQuery.Where(x => x.EncounterId == query.EncounterId.Value);
        }

        if (query.PatientId.HasValue && query.PatientId.Value != Guid.Empty)
        {
            baseQuery = baseQuery.Where(x => x.PatientId == query.PatientId.Value);
        }

        // Filter tanggal
        if (query.DateFrom.HasValue)
        {
            var fromUtc = query.DateFrom.Value.UtcDateTime;
            baseQuery = baseQuery.Where(x => x.CreateDateTime >= fromUtc);
        }

        if (query.DateTo.HasValue)
        {
            var toUtc = query.DateTo.Value.UtcDateTime;
            baseQuery = baseQuery.Where(x => x.CreateDateTime <= toUtc);
        }

        // Join dengan Patient, Encounter, dan Episode untuk pencarian komprehensif
        var joinedQuery = from order in baseQuery
                          join patient in _dbContext.Set<MstPatient>().AsNoTracking() on order.PatientId equals patient.Id into pj
                          from patient in pj.DefaultIfEmpty()
                          join encounter in _dbContext.RegPatientEncounters.AsNoTracking() on order.EncounterId equals encounter.Id into ej
                          from encounter in ej.DefaultIfEmpty()
                          join episode in _dbContext.Set<InpEpisode>().AsNoTracking() on order.EpisodeId equals episode.Id into epj
                          from episode in epj.DefaultIfEmpty()
                          select new
                          {
                              Order = order,
                              Patient = patient,
                              Encounter = encounter,
                              Episode = episode
                          };

        // Pencarian: No RM, Nama Pasien, NIK, No Order
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            joinedQuery = joinedQuery.Where(x =>
                x.Order.OrderNumber.ToLower().Contains(term) ||
                (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(term)) ||
                (x.Patient != null && x.Patient.FullName.ToLower().Contains(term)) ||
                (x.Patient != null && x.Patient.IdentityNumber != null && x.Patient.IdentityNumber.ToLower().Contains(term)) ||
                (x.Episode != null && x.Episode.EpisodeNumber.ToLower().Contains(term)) ||
                (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(term)));
        }

        var totalCount = await joinedQuery.CountAsync(cancellationToken);

        // Sorting
        var orderedQuery = joinedQuery.OrderByDescending(x => x.Order.CreateDateTime);

        var rawItems = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = rawItems.Select(x => x.Order.CreateBy).Distinct().ToList();
        var users = await _dbContext.Set<ApplicationUser>().AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : (u.UserName ?? string.Empty), cancellationToken);

        var items = rawItems.Select(x =>
        {
            var ord = x.Order;
            var pat = x.Patient;
            var enc = x.Encounter;
            var ep = x.Episode;

            var createdByName = users.TryGetValue(ord.CreateBy, out var uName) ? uName : "-";

            return new DepositOrderSummaryDto
            {
                Id = ord.Id,
                OrderNumber = ord.OrderNumber,
                EpisodeId = ord.EpisodeId,
                EpisodeNumber = ep?.EpisodeNumber ?? string.Empty,
                EncounterId = ord.EncounterId,
                EncounterNumber = enc?.EncounterNumber ?? string.Empty,
                PatientId = ord.PatientId,
                MedicalRecordNumber = pat?.MedicalRecordNumber ?? string.Empty,
                PatientName = pat?.FullName ?? string.Empty,
                IdentityNumberMasked = MaskIdentityNumber(pat?.IdentityNumber),
                OrderType = ord.OrderType,
                OrderTypeName = FormatOrderTypeName(ord.OrderType),
                SourceType = ord.SourceType,
                RequestedAmount = ord.RequestedAmount,
                PaidAmount = ord.PaidAmount,
                OutstandingAmount = ord.OutstandingAmount,
                Status = ord.Status,
                StatusName = FormatStatusName(ord.Status),
                DueDate = ord.DueDate,
                CreatedAt = new DateTimeOffset(ord.CreateDateTime, TimeSpan.FromHours(7)),
                CreatedByName = createdByName
            };
        }).ToList();

        return new PagedResult<DepositOrderSummaryDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalCount,
            TotalPage = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <summary>
    /// Mengambil detail order deposit lengkap beserta riwayat mutasi pembayaran dan ringkasan akun deposit.
    /// </summary>
    public async Task<DepositOrderDetailDto> GetOrderDetailAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
            throw new BillingDepositValidationException("OrderId wajib diisi.");

        var order = await _dbContext.BilDepositPaymentOrders.AsNoTracking()
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Order deposit {orderId} tidak ditemukan.");

        var patient = await _dbContext.Set<MstPatient>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == order.PatientId, cancellationToken);

        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == order.EpisodeId, cancellationToken);

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == order.EncounterId, cancellationToken);

        // Ambil nama pembuat order
        var creator = await _dbContext.Set<ApplicationUser>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == order.CreateBy, cancellationToken);
        var createdByName = !string.IsNullOrWhiteSpace(creator?.DisplayName) ? creator.DisplayName : (creator?.UserName ?? "-");

        // Riwayat pembayaran
        var paymentHistory = new List<DepositOrderPaymentItemDto>();
        if (order.Payments != null && order.Payments.Count > 0)
        {
            var movementIds = order.Payments.Select(p => p.DepositMovementId).ToList();
            var movements = await _dbContext.BilDepositMovements.AsNoTracking()
                .Where(m => movementIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

            var methodIds = movements.Where(m => m.PaymentMethodId.HasValue).Select(m => m.PaymentMethodId!.Value).Distinct().ToList();
            var methods = await _dbContext.MstPaymentMethods.AsNoTracking()
                .Where(m => methodIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, cancellationToken);

            var shiftIds = movements.Where(m => m.CashierShiftId.HasValue).Select(m => m.CashierShiftId!.Value).Distinct().ToList();
            var shifts = await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(s => shiftIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, cancellationToken);

            var userIds = movements.Select(m => m.CreateBy).Distinct().ToList();
            var paymentUsers = await _dbContext.Set<ApplicationUser>().AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => !string.IsNullOrWhiteSpace(u.DisplayName) ? u.DisplayName : (u.UserName ?? string.Empty), cancellationToken);

            foreach (var p in order.Payments.OrderByDescending(x => x.CreateDateTime))
            {
                var mov = movements.FirstOrDefault(m => m.Id == p.DepositMovementId);
                var method = mov?.PaymentMethodId.HasValue == true && methods.TryGetValue(mov.PaymentMethodId.Value, out var mth) ? mth : null;
                var shift = mov?.CashierShiftId.HasValue == true && shifts.TryGetValue(mov.CashierShiftId.Value, out var sft) ? sft : null;
                var cashierName = mov != null && paymentUsers.TryGetValue(mov.CreateBy, out var cName) ? cName : "-";

                paymentHistory.Add(new DepositOrderPaymentItemDto
                {
                    PaymentId = p.Id,
                    DepositMovementId = p.DepositMovementId,
                    SettlementId = p.SettlementId,
                    ReceiptNumber = mov?.ReferenceNumber ?? $"RCP-{p.Id.ToString()[..8].ToUpperInvariant()}",
                    PaymentMethodName = method?.PaymentMethodName ?? "Kasir",
                    IsCash = method?.IsCash ?? true,
                    Amount = p.Amount,
                    ReferenceNumber = mov?.ReferenceNumber,
                    Status = p.Status,
                    PaidAt = mov != null ? mov.OccurredAt : new DateTimeOffset(p.CreateDateTime, TimeSpan.FromHours(7)),
                    CashierName = cashierName,
                    CashierShiftNumber = shift?.ShiftNumber
                });
            }
        }

        // Ringkasan Akun Deposit saat ini
        DepositAccountBriefDto? depositAccountSummary = null;
        var depositAccount = await _dbContext.BilDepositAccounts.AsNoTracking()
            .Include(x => x.Movements)
            .FirstOrDefaultAsync(x => x.EncounterId == order.EncounterId && !x.IsDelete, cancellationToken);

        if (depositAccount != null)
        {
            var activeMovs = depositAccount.Movements.Where(x => !x.IsDelete).ToList();
            var totalRec = activeMovs
                .Where(x => x.MovementType == BillingDepositMovementTypes.TopUp)
                .Sum(x => x.Amount) -
                activeMovs.Where(x => x.MovementType == BillingDepositMovementTypes.Reversal).Sum(x => x.Amount);

            var totalAlloc = activeMovs
                .Where(x => x.MovementType == BillingDepositMovementTypes.Allocation)
                .Sum(x => x.Amount);

            depositAccountSummary = new DepositAccountBriefDto
            {
                AccountId = depositAccount.Id,
                AccountNumber = depositAccount.AccountNumber,
                AvailableBalance = depositAccount.AvailableBalance,
                TotalReceived = Math.Max(0m, totalRec),
                TotalAllocated = totalAlloc
            };
        }

        return new DepositOrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            EpisodeId = order.EpisodeId,
            EpisodeNumber = episode?.EpisodeNumber ?? string.Empty,
            EncounterId = order.EncounterId,
            EncounterNumber = encounter?.EncounterNumber ?? string.Empty,
            PatientId = order.PatientId,
            MedicalRecordNumber = patient?.MedicalRecordNumber ?? string.Empty,
            PatientName = patient?.FullName ?? string.Empty,
            IdentityNumberMasked = MaskIdentityNumber(patient?.IdentityNumber),
            OrderType = order.OrderType,
            OrderTypeName = FormatOrderTypeName(order.OrderType),
            SourceType = order.SourceType,
            RequestedAmount = order.RequestedAmount,
            PaidAmount = order.PaidAmount,
            OutstandingAmount = order.OutstandingAmount,
            Status = order.Status,
            StatusName = FormatStatusName(order.Status),
            DueDate = order.DueDate,
            CreatedAt = new DateTimeOffset(order.CreateDateTime, TimeSpan.FromHours(7)),
            CreatedByName = createdByName,
            PolicyId = order.PolicyId,
            Remarks = order.Remarks,
            RowVersion = order.RowVersion,
            CancelledAt = order.CancelledAt,
            CancellationReason = order.CancellationReason,
            PaymentHistory = paymentHistory,
            DepositAccountSummary = depositAccountSummary
        };
    }

    /// <summary>
    /// Memproses penerimaan pembayaran deposit kasir untuk order yang bersangkutan.
    /// Menghubungkan mutasi TOP_UP secara atomik ke order dan memperbarui status outstanding.
    /// Menghasilkan kwitansi/receipt resmi pembayaran deposit.
    /// </summary>
    public async Task<PayDepositOrderResponse> PayOrderAsync(
        Guid orderId,
        PayDepositOrderRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
            throw new BillingDepositValidationException("OrderId wajib diisi.");

        if (request == null)
            throw new BillingDepositValidationException("Data pembayaran order wajib diisi.");

        if (request.Amount <= 0)
            throw new BillingDepositValidationException("Nominal pembayaran harus lebih besar dari Rp 0.");

        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                if (_dbContext.Database.CurrentTransaction is null)
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(
                        IsolationLevel.Serializable, cancellationToken);
                }
                await AcquireLockAsync($"BIL_DEP_ORDER_{orderId:N}", cancellationToken);
            }

            var order = await _dbContext.BilDepositPaymentOrders
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException($"Order deposit {orderId} tidak ditemukan.");

            // Idempotency check untuk pembayaran order
            var priorPayment = await _dbContext.BilDepositOrderPayments
                .FirstOrDefaultAsync(x => x.DepositOrderId == order.Id && x.DepositMovement != null && x.DepositMovement.IdempotencyKey == request.IdempotencyKey, cancellationToken);

            if (priorPayment != null)
            {
                var priorReceipt = await BuildReceiptDtoAsync(order, priorPayment, cancellationToken);
                if (transaction != null) await transaction.CommitAsync(cancellationToken);

                return new PayDepositOrderResponse
                {
                    DepositOrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderStatus = order.Status,
                    RequestedAmount = order.RequestedAmount,
                    TotalPaidAmount = order.PaidAmount,
                    OutstandingAmount = order.OutstandingAmount,
                    PaymentAmount = priorPayment.Amount,
                    IsReplay = true,
                    Receipt = priorReceipt
                };
            }

            if (order.Status == DepositOrderStatuses.Cancelled)
                throw new BillingDepositValidationException("Order deposit ini telah dibatalkan dan tidak dapat menerima pembayaran.");

            if (order.Status == DepositOrderStatuses.Paid || order.OutstandingAmount <= 0)
                throw new BillingDepositValidationException("Order deposit ini sudah lunas (Outstanding: Rp 0).");

            if (request.Amount > order.OutstandingAmount)
                throw new BillingDepositValidationException(
                    $"Nominal pembayaran (Rp {request.Amount:N0}) melebihi sisa tagihan order (Rp {order.OutstandingAmount:N0}). Overpayment tidak diizinkan.");

            // Concurrency check
            if (request.ExpectedRowVersion.HasValue && request.ExpectedRowVersion.Value != order.RowVersion)
                throw new BillingDepositConflictException("Data order telah berubah. Muat ulang sebelum melanjutkan.");

            // Validasi metode pembayaran & shift kasir
            var paymentMethod = await _dbContext.MstPaymentMethods.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.PaymentMethodId && !x.IsDelete && !x.IsCancel, cancellationToken)
                ?? throw new KeyNotFoundException("Metode pembayaran tidak ditemukan.");

            if (!paymentMethod.IsActive || !paymentMethod.IsAvailableForBilling)
                throw new BillingDepositValidationException("Metode pembayaran tidak aktif atau tidak tersedia untuk billing.");

            if (paymentMethod.IsInsurance || paymentMethod.IsCompanyGuarantor)
                throw new BillingDepositValidationException("Pembayaran deposit tidak dapat menggunakan metode asuransi atau penjamin.");

            if (paymentMethod.IsNeedReferenceNumber && string.IsNullOrWhiteSpace(request.ReferenceNumber))
                throw new BillingDepositValidationException($"Metode {paymentMethod.PaymentMethodName} mewajibkan nomor referensi / nomor transaksi.");

            // Eksekusi penerimaan dana aktual melalui BillingDepositService (rekonsiliasi saldo & cashier shift)
            var topUpRequest = new DepositTopUpRequest
            {
                Amount = request.Amount,
                PaymentMethodId = request.PaymentMethodId,
                PaymentMethodAccountId = request.PaymentMethodAccountId,
                ReferenceNumber = request.ReferenceNumber,
                Reason = $"Pembayaran order {order.OrderNumber}: {request.Reason}",
                CorrelationId = request.CorrelationId,
                CausationId = order.Id,
                ExpectedRowVersion = null
            };

            var settlement = await _depositService.TopUpAsync(
                order.EncounterId,
                topUpRequest,
                request.IdempotencyKey,
                actorUserId,
                cancellationToken,
                deferAudit: true);

            var movementId = settlement.DepositMovementId
                ?? throw new BillingDepositValidationException("Penerimaan deposit gagal menghasilkan ID mutasi buku besar.");

            // Catat relasi pembayaran ke order
            var orderPayment = new BilDepositOrderPayment
            {
                Id = Guid.NewGuid(),
                DepositOrderId = order.Id,
                DepositMovementId = movementId,
                SettlementId = settlement.Id,
                Amount = request.Amount,
                Status = DepositOrderPaymentStatuses.Success,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.BilDepositOrderPayments.Add(orderPayment);
            order.Payments.Add(orderPayment);

            // Perbarui saldo order
            order.PaidAmount = checked(order.PaidAmount + request.Amount);
            order.OutstandingAmount = checked(order.RequestedAmount - order.PaidAmount);

            if (order.OutstandingAmount <= 0)
            {
                order.Status = DepositOrderStatuses.Paid;
                order.OutstandingAmount = 0m;
            }
            else
            {
                order.Status = DepositOrderStatuses.PartiallyPaid;
            }

            order.RowVersion = Guid.NewGuid();
            order.UpdateDateTime = DateTime.UtcNow;
            order.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            var receipt = await BuildReceiptDtoAsync(order, orderPayment, cancellationToken);

            await _loggerService.AuditAsync(
                LogCategory,
                "PayDepositPaymentOrder",
                $"Pembayaran order deposit {order.OrderNumber} sebesar Rp {request.Amount:N0} berhasil dicatat (Sisa: Rp {order.OutstandingAmount:N0}, Status: {order.Status}).",
                new
                {
                    order.Id,
                    order.OrderNumber,
                    PaymentAmount = request.Amount,
                    order.OutstandingAmount,
                    order.Status,
                    ActorUserId = actorUserId
                });

            return new PayDepositOrderResponse
            {
                DepositOrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderStatus = order.Status,
                RequestedAmount = order.RequestedAmount,
                TotalPaidAmount = order.PaidAmount,
                OutstandingAmount = order.OutstandingAmount,
                PaymentAmount = request.Amount,
                Settlement = settlement,
                Receipt = receipt,
                IsReplay = false
            };
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// Membatalkan order deposit yang belum memiliki pembayaran.
    /// Order yang sudah dibayar (penuh atau sebagian) dilarang dibatalkan langsung.
    /// </summary>
    public async Task<DepositOrderDetailDto> CancelOrderAsync(
        Guid orderId,
        CancelDepositOrderRequest request,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (orderId == Guid.Empty)
            throw new BillingDepositValidationException("OrderId wajib diisi.");

        if (string.IsNullOrWhiteSpace(request?.Reason))
            throw new BillingDepositValidationException("Alasan pembatalan wajib diisi.");

        var order = await _dbContext.BilDepositPaymentOrders
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == orderId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Order deposit {orderId} tidak ditemukan.");

        if (order.Status == DepositOrderStatuses.Cancelled)
            return await GetOrderDetailAsync(order.Id, cancellationToken);

        if (order.PaidAmount > 0 || order.Payments.Any(p => p.Status == DepositOrderPaymentStatuses.Success))
        {
            throw new BillingDepositValidationException(
                "Order deposit yang sudah memiliki pembayaran tidak dapat dibatalkan langsung. Lakukan reversal atau refund pembayaran terlebih dahulu.");
        }

        order.Status = DepositOrderStatuses.Cancelled;
        order.CancelledAt = DateTimeOffset.UtcNow;
        order.CancelledBy = actorUserId;
        order.CancellationReason = request.Reason.Trim();
        order.RowVersion = Guid.NewGuid();
        order.UpdateDateTime = DateTime.UtcNow;
        order.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(
            LogCategory,
            "CancelDepositPaymentOrder",
            $"Order deposit {order.OrderNumber} dibatalkan dengan alasan: {order.CancellationReason}.",
            new
            {
                order.Id,
                order.OrderNumber,
                order.CancellationReason,
                ActorUserId = actorUserId
            });

        return await GetOrderDetailAsync(order.Id, cancellationToken);
    }

    /// <summary>
    /// Menghasilkan format Kwitansi / Bukti Penerimaan Deposit resmi.
    /// </summary>
    public async Task<DepositReceiptDto> GetReceiptByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await _dbContext.BilDepositOrderPayments
            .Include(x => x.DepositOrder)
            .FirstOrDefaultAsync(x => x.Id == paymentId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException($"Bukti pembayaran deposit {paymentId} tidak ditemukan.");

        return await BuildReceiptDtoAsync(payment.DepositOrder, payment, cancellationToken);
    }

    private async Task<DepositReceiptDto> BuildReceiptDtoAsync(
        BilDepositPaymentOrder order,
        BilDepositOrderPayment payment,
        CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Set<MstPatient>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == order.PatientId, cancellationToken);

        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == order.EpisodeId, cancellationToken);

        var encounter = await _dbContext.RegPatientEncounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == order.EncounterId, cancellationToken);

        var movement = await _dbContext.BilDepositMovements.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == payment.DepositMovementId, cancellationToken);

        MstPaymentMethod? method = null;
        if (movement?.PaymentMethodId.HasValue == true)
        {
            method = await _dbContext.MstPaymentMethods.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == movement.PaymentMethodId.Value, cancellationToken);
        }

        BilCashierShift? shift = null;
        if (movement?.CashierShiftId.HasValue == true)
        {
            shift = await _dbContext.BilCashierShifts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == movement.CashierShiftId.Value, cancellationToken);
        }

        var cashier = movement != null
            ? await _dbContext.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == movement.CreateBy, cancellationToken)
            : null;
        var cashierName = !string.IsNullOrWhiteSpace(cashier?.DisplayName) ? cashier.DisplayName : (cashier?.UserName ?? "Petugas Kasir");

        var receiptNumber = movement?.ReferenceNumber ?? $"RCP-DEP-{payment.Id.ToString()[..8].ToUpperInvariant()}";

        return new DepositReceiptDto
        {
            ReceiptNumber = receiptNumber,
            OrderNumber = order.OrderNumber,
            MedicalRecordNumber = patient?.MedicalRecordNumber ?? "-",
            PatientName = patient?.FullName ?? "-",
            EpisodeNumber = episode?.EpisodeNumber ?? "-",
            EncounterNumber = encounter?.EncounterNumber ?? "-",
            Amount = payment.Amount,
            AmountInWords = NumberToWordsIndonesian((long)payment.Amount) + " Rupiah",
            PaymentMethodName = method?.PaymentMethodName ?? "Tunai (Cash)",
            IsCash = method?.IsCash ?? true,
            ReferenceNumber = movement?.ReferenceNumber,
            PaymentDateTime = movement != null ? movement.OccurredAt : new DateTimeOffset(payment.CreateDateTime, TimeSpan.FromHours(7)),
            CashierName = cashierName,
            CashierShiftNumber = shift?.ShiftNumber,
            Notes = $"Pembayaran uang muka rawat inap untuk order {order.OrderNumber} ({FormatStatusName(order.Status)})."
        };
    }

    private static string MaskIdentityNumber(string? identityNumber)
    {
        if (string.IsNullOrWhiteSpace(identityNumber)) return "-";
        var str = identityNumber.Trim();
        if (str.Length <= 6) return new string('*', str.Length);
        return str[..4] + new string('*', str.Length - 6) + str[^2..];
    }

    private static string FormatOrderTypeName(string? orderType) =>
        orderType switch
        {
            DepositOrderTypes.InitialDeposit => "Uang Muka Admisi",
            DepositOrderTypes.AdditionalTopUp => "Top-Up Tambahan",
            _ => orderType ?? "-"
        };

    private static string FormatStatusName(string? status) =>
        status switch
        {
            DepositOrderStatuses.Unpaid => "Belum Dibayar",
            DepositOrderStatuses.PartiallyPaid => "Terbayar Sebagian",
            DepositOrderStatuses.Paid => "Lunas",
            DepositOrderStatuses.Cancelled => "Dibatalkan",
            DepositOrderStatuses.Expired => "Kedaluwarsa",
            _ => status ?? "-"
        };

    private static string NumberToWordsIndonesian(long number)
    {
        if (number == 0) return "Nol";
        if (number < 0) return "Minus " + NumberToWordsIndonesian(-number);

        var words = "";

        if (number / 1000000000000 > 0)
        {
            words += NumberToWordsIndonesian(number / 1000000000000) + " Triliun ";
            number %= 1000000000000;
        }

        if (number / 1000000000 > 0)
        {
            words += NumberToWordsIndonesian(number / 1000000000) + " Miliar ";
            number %= 1000000000;
        }

        if (number / 1000000 > 0)
        {
            words += NumberToWordsIndonesian(number / 1000000) + " Juta ";
            number %= 1000000;
        }

        if (number / 1000 > 0)
        {
            if (number / 1000 == 1)
                words += "Seribu ";
            else
                words += NumberToWordsIndonesian(number / 1000) + " Ribu ";
            number %= 1000;
        }

        if (number / 100 > 0)
        {
            if (number / 100 == 1)
                words += "Seratus ";
            else
                words += NumberToWordsIndonesian(number / 100) + " Ratus ";
            number %= 100;
        }

        if (number > 0)
        {
            var units = new[] { "Nol", "Satu", "Dua", "Tiga", "Empat", "Lima", "Enam", "Tujuh", "Delapan", "Sembilan", "Sepuluh", "Sebelas" };

            if (number < 12)
                words += units[number] + " ";
            else if (number < 20)
                words += NumberToWordsIndonesian(number - 10) + " Belas ";
            else
            {
                words += units[number / 10] + " Puluh ";
                if (number % 10 > 0)
                    words += units[number % 10] + " ";
            }
        }

        return words.Trim();
    }

    private async Task AcquireLockAsync(string lockKey, CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return;
        var lockId = (long)(uint)lockKey.GetHashCode();
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({lockId})",
            cancellationToken);
    }
}
