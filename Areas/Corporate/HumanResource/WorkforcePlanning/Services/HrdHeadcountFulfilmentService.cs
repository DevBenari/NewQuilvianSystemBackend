using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Layanan tautan pemenuhan kursi formasi dan langkah pemrosesan scheduler pemenuhan (BE-HRD-043).
    /// </summary>
    public sealed class HrdHeadcountFulfilmentService : IHrdHeadcountFulfilmentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdWorkforcePlanningSettingService _settingService;
        private readonly ILogger<HrdHeadcountFulfilmentService> _logger;

        public HrdHeadcountFulfilmentService(
            ApplicationDbContext dbContext,
            IHrdWorkforcePlanningSettingService settingService,
            ILogger<HrdHeadcountFulfilmentService> logger)
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<WorkforcePlanningResult<FulfilmentLinkResponse>> CreateFulfilmentLinkAsync(
            Guid headcountRequestId,
            Guid seatId,
            CreateFulfilmentLinkRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            var entity = await _dbContext.HrdHeadcountRequests
                .Include(r => r.Seats.Where(s => !s.IsDelete))
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == headcountRequestId, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Permintaan penambahan tenaga kerja tidak ditemukan.", 404);
            }

            var seat = entity.Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null)
            {
                return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Kursi tidak ditemukan pada permintaan ini.", 404);
            }

            if (seat.SeatStatus != HeadcountSeatStatus.Open)
            {
                return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Hanya kursi berstatus Open yang dapat ditautkan ke penempatan atau SK.", 400);
            }

            // Validasi aturan sumber kursi (VR-WFP-31)
            if (seat.FulfilmentSource == HeadcountFulfilmentSource.External)
            {
                if (!request.LinkedPositionAssignmentId.HasValue || request.LinkedPersonnelActionOrderId.HasValue)
                {
                    return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Kursi pemenuhan luar hanya boleh ditautkan ke penempatan pegawai baru (PositionAssignment).", 422);
                }

                var assignmentExists = await _dbContext.WfpPositionAssignments
                    .AnyAsync(a => !a.IsDelete && a.Id == request.LinkedPositionAssignmentId.Value, cancellationToken);

                if (!assignmentExists)
                {
                    return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Data penempatan posisi tidak ditemukan.", 404);
                }

                seat.LinkedPositionAssignmentId = request.LinkedPositionAssignmentId;
                seat.LinkedPersonnelActionOrderId = null;
            }
            else // Internal
            {
                if (!request.LinkedPersonnelActionOrderId.HasValue || request.LinkedPositionAssignmentId.HasValue)
                {
                    return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Kursi pemenuhan internal hanya boleh ditautkan ke SK mutasi/pindahan (PersonnelActionOrder).", 422);
                }

                var orderExists = await _dbContext.HrdPersonnelActionOrders
                    .AnyAsync(o => !o.IsDelete && o.Id == request.LinkedPersonnelActionOrderId.Value, cancellationToken);

                if (!orderExists)
                {
                    return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Data SK mutasi kepegawaian tidak ditemukan.", 404);
                }

                seat.LinkedPersonnelActionOrderId = request.LinkedPersonnelActionOrderId;
                seat.LinkedPositionAssignmentId = null;
            }

            var now = DateTime.UtcNow;
            seat.SeatStatus = HeadcountSeatStatus.Linked;
            seat.LinkedAt = now;
            seat.LinkedByUserId = userId;
            seat.UpdateDateTime = now;
            seat.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return WorkforcePlanningResult<FulfilmentLinkResponse>.Ok(
                new FulfilmentLinkResponse
                {
                    SeatId = seat.Id,
                    HeadcountRequestId = entity.Id,
                    SeatNumber = seat.SeatNumber,
                    SeatStatus = seat.SeatStatus.ToString(),
                    LinkedPositionAssignmentId = seat.LinkedPositionAssignmentId,
                    LinkedPersonnelActionOrderId = seat.LinkedPersonnelActionOrderId,
                    LinkedAt = seat.LinkedAt,
                    Message = "Kursi berhasil ditautkan ke dokumen pemenuhan."
                },
                "Tautan pemenuhan kursi berhasil dibuat.");
        }

        public async Task<WorkforcePlanningResult<FulfilmentLinkResponse>> RemoveFulfilmentLinkAsync(
            Guid headcountRequestId,
            Guid seatId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId(user);

            var entity = await _dbContext.HrdHeadcountRequests
                .Include(r => r.Seats.Where(s => !s.IsDelete))
                .FirstOrDefaultAsync(r => !r.IsDelete && r.Id == headcountRequestId, cancellationToken);

            if (entity == null)
            {
                return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Permintaan penambahan tenaga kerja tidak ditemukan.", 404);
            }

            var seat = entity.Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null)
            {
                return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Kursi tidak ditemukan pada permintaan ini.", 404);
            }

            // Validasi pelepas tautan (VR-WFP-32)
            if (seat.SeatStatus == HeadcountSeatStatus.Fulfilled)
            {
                return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Tautan kursi yang sudah terpenuhi (Fulfilled) tidak dapat dilepas.", 409);
            }

            if (seat.SeatStatus != HeadcountSeatStatus.Linked)
            {
                return WorkforcePlanningResult<FulfilmentLinkResponse>.Fail("Hanya kursi yang berstatus Linked yang dapat dilepas tautannya.", 400);
            }

            var now = DateTime.UtcNow;
            seat.SeatStatus = HeadcountSeatStatus.Open;
            seat.LinkedPositionAssignmentId = null;
            seat.LinkedPersonnelActionOrderId = null;
            seat.LinkedAt = null;
            seat.LinkedByUserId = null;
            seat.UpdateDateTime = now;
            seat.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return WorkforcePlanningResult<FulfilmentLinkResponse>.Ok(
                new FulfilmentLinkResponse
                {
                    SeatId = seat.Id,
                    HeadcountRequestId = entity.Id,
                    SeatNumber = seat.SeatNumber,
                    SeatStatus = seat.SeatStatus.ToString(),
                    Message = "Tautan pemenuhan kursi berhasil dilepas dan status kursi kembali Open."
                },
                "Tautan pemenuhan kursi berhasil dilepas.");
        }

        public async Task<int> StepProcessFulfilmentAndOverdueAsync(
            DateOnly operationalDate,
            SchedulerExecutionSummary summary,
            CancellationToken cancellationToken = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTime.UtcNow;
                int fulfilledSeatsCount = 0;

                // 1. Ambil seluruh kursi Linked
                var linkedSeats = await _dbContext.HrdHeadcountRequestSeats
                    .Include(s => s.LinkedPositionAssignment)
                    .Include(s => s.LinkedPersonnelActionOrder)
                    .Include(s => s.HeadcountRequest)
                    .Where(s => !s.IsDelete && s.SeatStatus == HeadcountSeatStatus.Linked)
                    .ToListAsync(cancellationToken);

                var affectedRequestIds = new HashSet<Guid>();

                foreach (var seat in linkedSeats)
                {
                    if (seat.FulfilmentSource == HeadcountFulfilmentSource.External)
                    {
                        if (seat.LinkedPositionAssignment != null &&
                            seat.LinkedPositionAssignment.EffectiveStartDate <= operationalDate.ToDateTime(TimeOnly.MinValue))
                        {
                            seat.SeatStatus = HeadcountSeatStatus.Fulfilled;
                            seat.FulfilledAt = operationalDate;
                            seat.UpdateDateTime = now;
                            fulfilledSeatsCount++;
                            affectedRequestIds.Add(seat.HeadcountRequestId);
                        }
                    }
                    else if (seat.FulfilmentSource == HeadcountFulfilmentSource.Internal)
                    {
                        if (seat.LinkedPersonnelActionOrder != null)
                        {
                            // SK Batal -> kembalikan ke Open (kriteria 6)
                            if (string.Equals(seat.LinkedPersonnelActionOrder.OrderStatus, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(seat.LinkedPersonnelActionOrder.OrderStatus, "Rejected", StringComparison.OrdinalIgnoreCase))
                            {
                                seat.SeatStatus = HeadcountSeatStatus.Open;
                                seat.LinkedPersonnelActionOrderId = null;
                                seat.LinkedAt = null;
                                seat.LinkedByUserId = null;
                                seat.UpdateDateTime = now;
                                affectedRequestIds.Add(seat.HeadcountRequestId);
                            }
                            else if ((string.Equals(seat.LinkedPersonnelActionOrder.OrderStatus, "Applied", StringComparison.OrdinalIgnoreCase) ||
                                      string.Equals(seat.LinkedPersonnelActionOrder.OrderStatus, "Scheduled", StringComparison.OrdinalIgnoreCase)) &&
                                     seat.LinkedPersonnelActionOrder.EffectiveDate <= operationalDate.ToDateTime(TimeOnly.MinValue))
                            {
                                seat.SeatStatus = HeadcountSeatStatus.Fulfilled;
                                seat.FulfilledAt = operationalDate;
                                seat.UpdateDateTime = now;
                                fulfilledSeatsCount++;
                                affectedRequestIds.Add(seat.HeadcountRequestId);
                            }
                        }
                    }
                }

                // 2. Hitung ulang status permintaan yang terpengaruh (kriteria 5)
                foreach (var reqId in affectedRequestIds)
                {
                    var req = await _dbContext.HrdHeadcountRequests
                        .Include(r => r.Seats.Where(s => !s.IsDelete))
                        .FirstOrDefaultAsync(r => r.Id == reqId, cancellationToken);

                    if (req == null) continue;

                    var activeSeats = req.Seats.Where(s => !s.IsDelete).ToList();
                    int totalSeats = activeSeats.Count;
                    int fulfilledCount = activeSeats.Count(s => s.SeatStatus == HeadcountSeatStatus.Fulfilled);
                    int cancelledCount = activeSeats.Count(s => s.SeatStatus == HeadcountSeatStatus.Cancelled);

                    if (fulfilledCount == totalSeats && totalSeats > 0)
                    {
                        req.RequestStatus = HeadcountRequestStatus.Fulfilled;
                        req.ClosedAt = now;
                    }
                    else if (fulfilledCount + cancelledCount == totalSeats && totalSeats > 0 && cancelledCount > 0)
                    {
                        req.RequestStatus = HeadcountRequestStatus.Closed;
                        req.ClosedAt = now;
                    }
                    else if (fulfilledCount > 0)
                    {
                        req.RequestStatus = HeadcountRequestStatus.PartiallyFulfilled;
                    }

                    req.UpdateDateTime = now;
                }

                // 3. Penanda IsOverdue berdasarkan pengaturan threshold (kriteria 7)
                var activeRequests = await _dbContext.HrdHeadcountRequests
                    .Where(r => !r.IsDelete &&
                                !r.IsOverdue &&
                                (r.RequestStatus == HeadcountRequestStatus.ReadyForRecruitment ||
                                 r.RequestStatus == HeadcountRequestStatus.PartiallyFulfilled) &&
                                r.ReadyForRecruitmentAt.HasValue)
                    .ToListAsync(cancellationToken);

                foreach (var req in activeRequests)
                {
                    var settingResult = await _settingService.GetByHospitalSiteIdAsync(req.HospitalSiteId, cancellationToken);
                    var settings = settingResult.Data;
                    if (settings?.OverdueRequestThresholdDays.HasValue == true && settings.OverdueRequestThresholdDays.Value > 0)
                    {
                        var daysElapsed = (operationalDate.ToDateTime(TimeOnly.MinValue) - req.ReadyForRecruitmentAt!.Value).TotalDays;
                        if (daysElapsed >= settings.OverdueRequestThresholdDays.Value)
                        {
                            req.IsOverdue = true;
                            req.UpdateDateTime = now;
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                var logMsg = $"Scheduler pemenuhan kursi: {fulfilledSeatsCount} kursi menjadi Fulfilled.";
                summary.StepLogs.Add(logMsg);
                _logger.LogInformation("{LogMsg}", logMsg);

                return fulfilledSeatsCount;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static Guid GetCurrentUserId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
