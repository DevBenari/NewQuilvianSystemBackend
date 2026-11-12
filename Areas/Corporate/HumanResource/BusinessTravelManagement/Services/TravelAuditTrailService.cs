using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface ITravelAuditTrailService
    {
        Task<PagedResult<TravelAuditEventDto>> GetAuditTrailAsync(Guid? travelRequestId, DateTime? fromUtc, DateTime? toUtc, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Log audit transaksi perjalanan dinas (<c>Hrd.BusinessTravel.Audit.View</c>, BE-HRD-333).
    /// </summary>
    /// <remarks>
    /// Modul ini tidak punya tabel audit terpisah, jadi jejak disusun dari cap waktu dan pelaku yang tersimpan pada
    /// permohonan, persetujuan, uang muka, dan settlement. Yang tampil hanya peristiwa yang benar-benar tercatat.
    /// </remarks>
    public class TravelAuditTrailService : ITravelAuditTrailService
    {
        private readonly ApplicationDbContext _context;

        public TravelAuditTrailService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<TravelAuditEventDto>> GetAuditTrailAsync(Guid? travelRequestId, DateTime? fromUtc, DateTime? toUtc, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var requests = await _context.TrxBusinessTravelRequests.AsNoTracking()
                .Where(r => !r.IsDelete && (travelRequestId == null || r.Id == travelRequestId))
                .Select(r => new { r.Id, r.TravelRequestNumber, r.CreateDateTime, r.CreateBy, r.TravelStatus })
                .ToListAsync(cancellationToken);
            var numbers = requests.ToDictionary(r => r.Id, r => r.TravelRequestNumber);
            var ids = numbers.Keys.ToList();

            var events = new List<TravelAuditEventDto>();
            void Add(Guid travelId, string type, DateTime? at, Guid? actor, string detail)
            {
                if (at == null) return;
                events.Add(new TravelAuditEventDto { BusinessTravelRequestId = travelId, TravelRequestNumber = numbers.GetValueOrDefault(travelId, string.Empty), EventType = type, OccurredAt = at.Value, ActorUserId = actor, Detail = detail });
            }

            foreach (var r in requests) Add(r.Id, "HRD_TRV_REQUEST_CREATED", r.CreateDateTime, r.CreateBy, $"Permohonan dibuat. Status saat ini: {r.TravelStatus}.");

            var approvals = await _context.TrxBusinessTravelApprovals.AsNoTracking()
                .Where(a => !a.IsDelete && ids.Contains(a.BusinessTravelRequestId) && a.ActionAt != null)
                .ToListAsync(cancellationToken);
            foreach (var a in approvals) Add(a.BusinessTravelRequestId, "HRD_TRV_REQUEST_APPROVAL", a.ActionAt, a.ActualActionByUserId, $"{a.ApprovalRole}: {a.ApprovalStatus}. {a.Comments}".Trim());

            var advances = await _context.TrxTravelAdvanceRequests.AsNoTracking()
                .Where(a => !a.IsDelete && ids.Contains(a.BusinessTravelRequestId))
                .ToListAsync(cancellationToken);
            foreach (var a in advances)
            {
                Add(a.BusinessTravelRequestId, "HRD_TRV_ADVANCE_SUBMITTED", a.SubmittedAt, a.SubmittedByUserId, $"Uang muka {a.AdvanceRequestNumber} diajukan sebesar {a.RequestedAmount:N0}.");
                Add(a.BusinessTravelRequestId, "HRD_TRV_ADVANCE_APPROVED", a.ApprovedAt, a.ApprovedByUserId, $"Uang muka {a.AdvanceRequestNumber} disetujui sebesar {a.ApprovedAmount:N0}.");
                Add(a.BusinessTravelRequestId, "HRD_TRV_ADVANCE_REJECTED", a.RejectedAt, a.RejectedByUserId, $"Uang muka {a.AdvanceRequestNumber} ditolak.");
                Add(a.BusinessTravelRequestId, "HRD_TRV_ADVANCE_PAID", a.PaidAt, null, $"Uang muka {a.AdvanceRequestNumber} dicairkan sebesar {a.PaidAmount:N0}.");
                Add(a.BusinessTravelRequestId, "HRD_TRV_ADVANCE_SETTLED", a.SettledAt, null, $"Uang muka {a.AdvanceRequestNumber} diselesaikan.");
            }

            var settlements = await _context.TrxTravelSettlements.AsNoTracking()
                .Where(s => !s.IsDelete && ids.Contains(s.BusinessTravelRequestId))
                .ToListAsync(cancellationToken);
            foreach (var s in settlements)
            {
                Add(s.BusinessTravelRequestId, "HRD_TRV_SETTLEMENT_SUBMITTED", s.SubmittedAt, s.SubmittedByUserId, $"Settlement {s.SettlementNumber} diajukan.");
                Add(s.BusinessTravelRequestId, "HRD_TRV_SETTLEMENT_VERIFIED", s.VerifiedAt, s.VerifiedByUserId, $"Settlement {s.SettlementNumber} diverifikasi sebesar {s.ApprovedExpenseAmount:N0}.");
                Add(s.BusinessTravelRequestId, "HRD_TRV_SETTLEMENT_DEFAULTED", s.DefaultedAt, null, $"Settlement {s.SettlementNumber} menunggak lebih dari 14 hari.");
                Add(s.BusinessTravelRequestId, "HRD_TRV_SETTLEMENT_CLOSED", s.SettledAt, null, $"Settlement {s.SettlementNumber} ditutup.");
            }

            var filtered = events
                .Where(e => (fromUtc == null || e.OccurredAt >= fromUtc) && (toUtc == null || e.OccurredAt <= toUtc))
                .OrderByDescending(e => e.OccurredAt)
                .ToList();

            return new PagedResult<TravelAuditEventDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = filtered.Count,
                TotalPage = (int)Math.Ceiling(filtered.Count / (double)pageSize),
                Items = filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }
    }
}
