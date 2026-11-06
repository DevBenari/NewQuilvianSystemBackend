using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Service buku besar mutasi saldo cuti pegawai rumah sakit secara atomik dan audit-tracked (BE-HRD-118, HRD-DEC-110).
    /// </summary>
    public class LeaveBalanceLedgerService
    {
        private static readonly string[] ActiveReservationStatuses =
        {
            "Draft",
            "Submitted",
            "WaitingSupervisorApproval",
            "WaitingManagerApproval",
            "WaitingHrVerification",
            "WaitingApproval"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<LeaveBalanceLedgerService> _logger;

        public LeaveBalanceLedgerService(
            ApplicationDbContext dbContext,
            ILogger<LeaveBalanceLedgerService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<LeaveBalanceLedgerSummaryDto?> GetSummaryAsync(
            Guid workforceProfileId,
            Guid? leaveTypeId = null,
            int? year = null,
            CancellationToken cancellationToken = default)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;

            // Jika leaveTypeId tidak diisi, ambil default Cuti Tahunan (Annual Leave)
            var typeId = leaveTypeId;
            if (!typeId.HasValue || typeId.Value == Guid.Empty)
            {
                var annualType = await _dbContext.Set<MstLeaveType>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => !t.IsDelete && (t.LeaveTypeCode == "ANNUAL" || t.LeaveTypeName.Contains("Tahunan")), cancellationToken);
                if (annualType != null)
                {
                    typeId = annualType.Id;
                }
            }

            if (!typeId.HasValue)
            {
                return null;
            }

            var workforce = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workforceProfileId && !w.IsDelete, cancellationToken);
            var leaveType = await _dbContext.Set<MstLeaveType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == typeId.Value && !t.IsDelete, cancellationToken);

            if (workforce == null || leaveType == null)
            {
                return null;
            }

            // Query seluruh mutasi ledger tahun target
            var ledgers = await _dbContext.Set<TrxLeaveBalanceLedger>()
                .AsNoTracking()
                .Where(l => l.WorkforceProfileId == workforceProfileId &&
                            l.LeaveTypeId == typeId.Value &&
                            l.PeriodYear == targetYear &&
                            !l.IsDelete)
                .ToListAsync(cancellationToken);

            var totalEntitlement = ledgers.Where(l => l.TransactionType == "Entitlement").Sum(l => l.AmountDays);
            var totalTaken = Math.Abs(ledgers.Where(l => l.TransactionType == "Taken").Sum(l => l.AmountDays));
            var totalRestored = ledgers.Where(l => l.TransactionType == "CancellationRestore" || l.TransactionType == "RecallRestore").Sum(l => l.AmountDays);
            var totalForfeited = Math.Abs(ledgers.Where(l => l.TransactionType == "Forfeiture").Sum(l => l.AmountDays));
            var totalAdjusted = ledgers.Where(l => l.TransactionType == "Adjustment").Sum(l => l.AmountDays);

            // Carry-forward aktif: transaksi CarryForward yang belum forfeited dan belum melewati ExpiryDate
            var now = DateTime.UtcNow;
            var carryForwardTransactions = ledgers.Where(l => l.TransactionType == "CarryForward").ToList();
            var activeCarryForward = carryForwardTransactions
                .Where(l => !l.IsForfeited && (!l.ExpiryDate.HasValue || l.ExpiryDate.Value >= now))
                .Sum(l => l.AmountDays);

            DateTime? cfExpiryDate = carryForwardTransactions
                .Where(l => !l.IsForfeited && l.ExpiryDate.HasValue)
                .Select(l => l.ExpiryDate)
                .FirstOrDefault();

            var currentBalance = totalEntitlement + activeCarryForward - totalTaken + totalRestored - totalForfeited + totalAdjusted;
            currentBalance = Math.Max(0, currentBalance);

            // Hitung reserved days dari permohonan cuti pending approval yang sedang aktif
            var reservedDays = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .Where(r => r.WorkforceProfileId == workforceProfileId &&
                            r.LeaveTypeId == typeId.Value &&
                            !r.IsDelete &&
                            r.StartDate.Year == targetYear &&
                            ActiveReservationStatuses.Contains(r.LeaveRequestStatus))
                .SumAsync(r => r.CalculatedWorkingDays > 0 ? r.CalculatedWorkingDays : r.RequestedDays, cancellationToken);

            var availableBalance = Math.Max(0, currentBalance - reservedDays);

            // Peringatan kedaluwarsa sisa carry-forward jika mendekati 30 Juni (misal dalam 45 hari sebelum 30 Juni)
            var isExpiringSoon = false;
            string? expiryWarning = null;
            if (activeCarryForward > 0 && cfExpiryDate.HasValue)
            {
                var daysUntilExpiry = (cfExpiryDate.Value - now).TotalDays;
                if (daysUntilExpiry is >= 0 and <= 45)
                {
                    isExpiringSoon = true;
                    expiryWarning = $"Perhatian: Anda memiliki sisa kuota cuti bawaan tahun lalu sebanyak {activeCarryForward:N1} hari yang akan kedaluwarsa pada {cfExpiryDate.Value:dd MMMM yyyy} (HRD-DEC-110). Manfaatkan sebelum tanggal tersebut.";
                }
            }

            return new LeaveBalanceLedgerSummaryDto
            {
                WorkforceProfileId = workforceProfileId,
                EmployeeCode = workforce.ProfileCode,
                EmployeeName = workforce.DisplayName,
                LeaveTypeId = typeId.Value,
                LeaveTypeCode = leaveType.LeaveTypeCode,
                LeaveTypeName = leaveType.LeaveTypeName,
                PeriodYear = targetYear,
                TotalEntitlementDays = totalEntitlement,
                ActiveCarryForwardDays = activeCarryForward,
                CarryForwardExpiryDate = cfExpiryDate,
                TotalTakenDays = totalTaken,
                TotalRestoredDays = totalRestored,
                TotalForfeitedDays = totalForfeited,
                TotalAdjustedDays = totalAdjusted,
                CurrentBalanceDays = currentBalance,
                ReservedDays = reservedDays,
                AvailableBalanceDays = availableBalance,
                IsCarryForwardExpiringSoon = isExpiringSoon,
                ExpiryWarningMessage = expiryWarning
            };
        }

        public async Task<LeaveBalanceLedgerPagedResponse> GetPagedLedgerAsync(
            TrxLeaveBalanceLedgerQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var page = Math.Max(1, request.PageNumber);
            var size = Math.Clamp(request.PageSize, 1, 100);

            var query = _dbContext.Set<TrxLeaveBalanceLedger>()
                .AsNoTracking()
                .Include(l => l.WorkforceProfile)
                .Include(l => l.LeaveType)
                .Include(l => l.ReferenceRequest)
                .Include(l => l.ReferenceRecall)
                .Include(l => l.ReferenceAdjustment)
                .Where(l => !l.IsDelete);

            if (request.WorkforceProfileId.HasValue && request.WorkforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(l => l.WorkforceProfileId == request.WorkforceProfileId.Value);
            }
            if (request.LeaveTypeId.HasValue && request.LeaveTypeId.Value != Guid.Empty)
            {
                query = query.Where(l => l.LeaveTypeId == request.LeaveTypeId.Value);
            }
            if (request.PeriodYear.HasValue)
            {
                query = query.Where(l => l.PeriodYear == request.PeriodYear.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.TransactionType))
            {
                var trimmedType = request.TransactionType.Trim();
                query = query.Where(l => l.TransactionType == trimmedType);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(l => l.TransactionDate)
                .ThenByDescending(l => l.CreateDateTime)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(l => new LeaveBalanceLedgerItemDto
                {
                    Id = l.Id,
                    WorkforceProfileId = l.WorkforceProfileId,
                    EmployeeName = l.WorkforceProfile != null ? l.WorkforceProfile.DisplayName : null,
                    LeaveTypeId = l.LeaveTypeId,
                    LeaveTypeName = l.LeaveType != null ? l.LeaveType.LeaveTypeName : string.Empty,
                    PeriodYear = l.PeriodYear,
                    TransactionType = l.TransactionType,
                    AmountDays = l.AmountDays,
                    BalanceBefore = l.BalanceBefore,
                    BalanceAfter = l.BalanceAfter,
                    TransactionDate = l.TransactionDate,
                    ReferenceRequestId = l.ReferenceRequestId,
                    ReferenceRequestNumber = l.ReferenceRequest != null ? l.ReferenceRequest.RequestNumber : null,
                    ReferenceRecallId = l.ReferenceRecallId,
                    ReferenceRecallNumber = l.ReferenceRecall != null ? l.ReferenceRecall.RecallNumber : null,
                    ReferenceAdjustmentId = l.ReferenceAdjustmentId,
                    AuditDocumentNumber = l.ReferenceAdjustment != null ? l.ReferenceAdjustment.AuditDocumentNumber : null,
                    ExpiryDate = l.ExpiryDate,
                    IsForfeited = l.IsForfeited,
                    Notes = l.Notes
                })
                .ToListAsync(cancellationToken);

            return new LeaveBalanceLedgerPagedResponse
            {
                PageNumber = page,
                PageSize = size,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)size),
                Items = items
            };
        }

        public async Task<LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>> PostYearlyEntitlementAsync(
            PostLeaveEntitlementRequest request,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            var workforce = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == request.WorkforceProfileId && !w.IsDelete, cancellationToken);
            if (workforce == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(StatusCodes.Status404NotFound, "Pegawai tidak ditemukan.");
            }

            // Cegah duplikasi transaksi Entitlement untuk tahun yang sama
            var existingEntitlement = await _dbContext.Set<TrxLeaveBalanceLedger>()
                .AsNoTracking()
                .AnyAsync(l => l.WorkforceProfileId == request.WorkforceProfileId &&
                               l.LeaveTypeId == request.LeaveTypeId &&
                               l.PeriodYear == request.PeriodYear &&
                               l.TransactionType == "Entitlement" &&
                               !l.IsDelete, cancellationToken);
            if (existingEntitlement)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Hak cuti tahunan (Entitlement) untuk tahun {request.PeriodYear} sudah pernah dialokasikan kepada pegawai ini.");
            }

            var summary = await GetSummaryAsync(request.WorkforceProfileId, request.LeaveTypeId, request.PeriodYear, cancellationToken);
            var balanceBefore = summary?.CurrentBalanceDays ?? 0;
            var quota = request.QuotaDays > 0 ? request.QuotaDays : 12.0m;
            var balanceAfter = balanceBefore + quota;

            var ledger = new TrxLeaveBalanceLedger
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                LeaveTypeId = request.LeaveTypeId,
                PeriodYear = request.PeriodYear,
                TransactionType = "Entitlement",
                AmountDays = quota,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                TransactionDate = DateTime.UtcNow,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? $"Alokasi hak cuti tahunan {request.PeriodYear} (+{quota:N1} hari)" : request.Notes.Trim(),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId ?? Guid.Empty,
                UpdateDateTime = DateTime.UtcNow,
                UpdateBy = actorUserId ?? Guid.Empty
            };

            _dbContext.Set<TrxLeaveBalanceLedger>().Add(ledger);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Entitlement cuti {Quota} hari untuk pegawai {WorkforceId} tahun {Year} berhasil dibukukan.", quota, request.WorkforceProfileId, request.PeriodYear);

            return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Ok(
                new LeaveBalanceLedgerItemDto
                {
                    Id = ledger.Id,
                    WorkforceProfileId = ledger.WorkforceProfileId,
                    EmployeeName = workforce.DisplayName,
                    LeaveTypeId = ledger.LeaveTypeId,
                    PeriodYear = ledger.PeriodYear,
                    TransactionType = ledger.TransactionType,
                    AmountDays = ledger.AmountDays,
                    BalanceBefore = ledger.BalanceBefore,
                    BalanceAfter = ledger.BalanceAfter,
                    TransactionDate = ledger.TransactionDate,
                    Notes = ledger.Notes
                },
                "Hak cuti tahunan berhasil dialokasikan.");
        }

        public async Task<LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>> PostDebitTakenAsync(
            Guid workforceProfileId,
            Guid leaveTypeId,
            decimal days,
            Guid leaveRequestId,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            if (days <= 0)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(StatusCodes.Status400BadRequest, "Besaran hari cuti harus lebih besar dari 0.");
            }

            var leaveRequest = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == leaveRequestId && !r.IsDelete, cancellationToken);
            if (leaveRequest == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(StatusCodes.Status404NotFound, "Permohonan cuti tidak ditemukan.");
            }

            var year = leaveRequest.StartDate.Year;
            var summary = await GetSummaryAsync(workforceProfileId, leaveTypeId, year, cancellationToken);
            var balanceBefore = summary?.CurrentBalanceDays ?? 0;

            // VR-LEV-01: Validasi saldo mencukupi
            if (balanceBefore < days)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Saldo cuti tidak mencukupi (Tersedia: {balanceBefore:N1} hari, Diminta: {days:N1} hari). Transaksi ditolak (VR-LEV-01).");
            }

            var balanceAfter = balanceBefore - days;
            var ledger = new TrxLeaveBalanceLedger
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                LeaveTypeId = leaveTypeId,
                PeriodYear = year,
                TransactionType = "Taken",
                AmountDays = -days,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                TransactionDate = DateTime.UtcNow,
                ReferenceRequestId = leaveRequestId,
                Notes = $"Pemotongan cuti disetujui ({leaveRequest.RequestNumber}) sebesar {days:N1} hari kerja.",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId ?? Guid.Empty,
                UpdateDateTime = DateTime.UtcNow,
                UpdateBy = actorUserId ?? Guid.Empty
            };

            _dbContext.Set<TrxLeaveBalanceLedger>().Add(ledger);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Debit cuti {Days} hari untuk permohonan {RequestNumber} berhasil dibukukan.", days, leaveRequest.RequestNumber);

            return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Ok(
                new LeaveBalanceLedgerItemDto
                {
                    Id = ledger.Id,
                    WorkforceProfileId = ledger.WorkforceProfileId,
                    LeaveTypeId = ledger.LeaveTypeId,
                    PeriodYear = ledger.PeriodYear,
                    TransactionType = ledger.TransactionType,
                    AmountDays = ledger.AmountDays,
                    BalanceBefore = ledger.BalanceBefore,
                    BalanceAfter = ledger.BalanceAfter,
                    TransactionDate = ledger.TransactionDate,
                    ReferenceRequestId = ledger.ReferenceRequestId,
                    ReferenceRequestNumber = leaveRequest.RequestNumber,
                    Notes = ledger.Notes
                },
                "Pemotongan saldo cuti berhasil dicatat di buku besar.");
        }

        public async Task<LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>> PostCancellationRestoreAsync(
            Guid leaveRequestId,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            var leaveRequest = await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == leaveRequestId && !r.IsDelete, cancellationToken);
            if (leaveRequest == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(StatusCodes.Status404NotFound, "Permohonan cuti tidak ditemukan.");
            }

            var year = leaveRequest.StartDate.Year;
            var days = leaveRequest.CalculatedWorkingDays > 0 ? leaveRequest.CalculatedWorkingDays : leaveRequest.RequestedDays;
            var summary = await GetSummaryAsync(leaveRequest.WorkforceProfileId, leaveRequest.LeaveTypeId, year, cancellationToken);
            var balanceBefore = summary?.CurrentBalanceDays ?? 0;
            var balanceAfter = balanceBefore + days;

            var ledger = new TrxLeaveBalanceLedger
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = leaveRequest.WorkforceProfileId,
                LeaveTypeId = leaveRequest.LeaveTypeId,
                PeriodYear = year,
                TransactionType = "CancellationRestore",
                AmountDays = days,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                TransactionDate = DateTime.UtcNow,
                ReferenceRequestId = leaveRequestId,
                Notes = $"Pemulihan saldo akibat pembatalan cuti ({leaveRequest.RequestNumber}) sebesar +{days:N1} hari.",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId ?? Guid.Empty,
                UpdateDateTime = DateTime.UtcNow,
                UpdateBy = actorUserId ?? Guid.Empty
            };

            _dbContext.Set<TrxLeaveBalanceLedger>().Add(ledger);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Ok(
                new LeaveBalanceLedgerItemDto
                {
                    Id = ledger.Id,
                    WorkforceProfileId = ledger.WorkforceProfileId,
                    LeaveTypeId = ledger.LeaveTypeId,
                    PeriodYear = ledger.PeriodYear,
                    TransactionType = ledger.TransactionType,
                    AmountDays = ledger.AmountDays,
                    BalanceBefore = ledger.BalanceBefore,
                    BalanceAfter = ledger.BalanceAfter,
                    TransactionDate = ledger.TransactionDate,
                    ReferenceRequestId = ledger.ReferenceRequestId,
                    ReferenceRequestNumber = leaveRequest.RequestNumber,
                    Notes = ledger.Notes
                },
                "Pemulihan saldo cuti pembatalan berhasil dibukukan.");
        }

        public async Task<LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>> PostRecallRestoreAsync(
            Guid recallRequestId,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            var recall = await _dbContext.Set<TrxLeaveRecallRequest>()
                .Include(r => r.LeaveRequest)
                .FirstOrDefaultAsync(r => r.Id == recallRequestId && !r.IsDelete, cancellationToken);
            if (recall == null || recall.LeaveRequest == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(StatusCodes.Status404NotFound, "Permohonan dinas darurat recall tidak ditemukan.");
            }

            var year = recall.EffectiveRecallDate.Year;
            var summary = await GetSummaryAsync(recall.WorkforceProfileId, recall.LeaveRequest.LeaveTypeId, year, cancellationToken);
            var balanceBefore = summary?.CurrentBalanceDays ?? 0;
            var balanceAfter = balanceBefore + recall.RestoredDays;

            var ledger = new TrxLeaveBalanceLedger
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = recall.WorkforceProfileId,
                LeaveTypeId = recall.LeaveRequest.LeaveTypeId,
                PeriodYear = year,
                TransactionType = "RecallRestore",
                AmountDays = recall.RestoredDays,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                TransactionDate = DateTime.UtcNow,
                ReferenceRequestId = recall.LeaveRequestId,
                ReferenceRecallId = recall.Id,
                Notes = $"Pemulihan saldo akibat dinas darurat recall ({recall.RecallNumber}) sebesar +{recall.RestoredDays:N1} hari (HRD-DEC-113).",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId ?? Guid.Empty,
                UpdateDateTime = DateTime.UtcNow,
                UpdateBy = actorUserId ?? Guid.Empty
            };

            _dbContext.Set<TrxLeaveBalanceLedger>().Add(ledger);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Ok(
                new LeaveBalanceLedgerItemDto
                {
                    Id = ledger.Id,
                    WorkforceProfileId = ledger.WorkforceProfileId,
                    LeaveTypeId = ledger.LeaveTypeId,
                    PeriodYear = ledger.PeriodYear,
                    TransactionType = ledger.TransactionType,
                    AmountDays = ledger.AmountDays,
                    BalanceBefore = ledger.BalanceBefore,
                    BalanceAfter = ledger.BalanceAfter,
                    TransactionDate = ledger.TransactionDate,
                    ReferenceRecallId = recall.Id,
                    ReferenceRecallNumber = recall.RecallNumber,
                    Notes = ledger.Notes
                },
                "Pemulihan saldo cuti akibat recall dinas darurat berhasil dibukukan.");
        }

        public async Task<LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>> PostManualAdjustmentAsync(
            ManualLeaveAdjustmentRequest request,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.AuditDocumentNumber))
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nomor dokumen audit / SK Direksi wajib disertakan untuk penyesuaian saldo cuti resmi (HRD-DEC-115).");
            }
            if (request.AdjustmentDays <= 0)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Besaran hari penyesuaian saldo harus lebih besar dari 0.");
            }

            var isIncrease = string.Equals(request.Direction, "Increase", StringComparison.OrdinalIgnoreCase);
            var signedDays = isIncrease ? request.AdjustmentDays : -request.AdjustmentDays;

            var summary = await GetSummaryAsync(request.WorkforceProfileId, request.LeaveTypeId, request.PeriodYear, cancellationToken);
            var balanceBefore = summary?.CurrentBalanceDays ?? 0;
            var balanceAfter = balanceBefore + signedDays;

            if (balanceAfter < 0)
            {
                return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Penyesuaian pengurangan sebesar {request.AdjustmentDays:N1} hari menyebabkan saldo menjadi negatif. Transaksi ditolak.");
            }

            // Buat entitas TrxLeaveAdjustment resmi
            var adjustment = new TrxLeaveAdjustment
            {
                Id = Guid.NewGuid(),
                AdjustmentNumber = $"ADJ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
                WorkforceProfileId = request.WorkforceProfileId,
                LeaveBalanceId = Guid.Empty, // decoupled into ledger
                LeaveTypeId = request.LeaveTypeId,
                LeaveEntitlementPeriodId = Guid.Empty,
                LeaveAdjustmentReasonId = Guid.Empty,
                Direction = isIncrease ? "Credit" : "Debit",
                RequestedDays = request.AdjustmentDays,
                ApprovedDays = request.AdjustmentDays,
                PostedDays = request.AdjustmentDays,
                EffectiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
                AdjustmentStatus = "Posted",
                AuditDocumentNumber = request.AuditDocumentNumber.Trim(),
                Reason = request.Reason.Trim(),
                RequestedAt = DateTime.UtcNow,
                RequestedByUserId = actorUserId ?? Guid.Empty,
                ApprovedAt = DateTime.UtcNow,
                ApprovedByUserId = actorUserId ?? Guid.Empty,
                PostedAt = DateTime.UtcNow,
                PostedByUserId = actorUserId ?? Guid.Empty,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId ?? Guid.Empty,
                UpdateDateTime = DateTime.UtcNow,
                UpdateBy = actorUserId ?? Guid.Empty
            };
            _dbContext.Set<TrxLeaveAdjustment>().Add(adjustment);

            var ledger = new TrxLeaveBalanceLedger
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = request.WorkforceProfileId,
                LeaveTypeId = request.LeaveTypeId,
                PeriodYear = request.PeriodYear,
                TransactionType = "Adjustment",
                AmountDays = signedDays,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                TransactionDate = DateTime.UtcNow,
                ReferenceAdjustmentId = adjustment.Id,
                Notes = $"Penyesuaian saldo resmi HR ({request.AuditDocumentNumber}): {request.Reason}",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId ?? Guid.Empty,
                UpdateDateTime = DateTime.UtcNow,
                UpdateBy = actorUserId ?? Guid.Empty
            };
            _dbContext.Set<TrxLeaveBalanceLedger>().Add(ledger);

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Penyesuaian saldo cuti {SignedDays} hari untuk pegawai {WorkforceId} berhasil dibukukan dengan dokumen {AuditDoc}.",
                signedDays, request.WorkforceProfileId, request.AuditDocumentNumber);

            return LeaveBalanceLedgerServiceResult<LeaveBalanceLedgerItemDto>.Ok(
                new LeaveBalanceLedgerItemDto
                {
                    Id = ledger.Id,
                    WorkforceProfileId = ledger.WorkforceProfileId,
                    LeaveTypeId = ledger.LeaveTypeId,
                    PeriodYear = ledger.PeriodYear,
                    TransactionType = ledger.TransactionType,
                    AmountDays = ledger.AmountDays,
                    BalanceBefore = ledger.BalanceBefore,
                    BalanceAfter = ledger.BalanceAfter,
                    TransactionDate = ledger.TransactionDate,
                    ReferenceAdjustmentId = adjustment.Id,
                    AuditDocumentNumber = adjustment.AuditDocumentNumber,
                    Notes = ledger.Notes
                },
                "Penyesuaian saldo cuti resmi berhasil dibukukan.");
        }
    }
}
