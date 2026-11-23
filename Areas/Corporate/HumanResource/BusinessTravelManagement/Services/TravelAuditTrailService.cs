using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
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
    /// Log audit transaksi perjalanan dinas (<c>Hrd.BusinessTravel.Audit.View</c>, BE-HRD-333; 12 event matriks BE-HRD-379).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modul ini tidak punya tabel audit terpisah, jadi jejak disusun dari cap waktu dan pelaku yang tersimpan pada
    /// SPPD, uang muka, settlement, butir biaya, tautan presensi, dan jadwal potongan payroll. Yang tampil hanya
    /// peristiwa yang benar-benar tercatat.
    /// </para>
    /// <para>
    /// Nama event mengikuti permission-audit-matrix bagian 32.2 kontrak <c>v28</c> (<c>HRD-DEC-228</c>): 12 event
    /// matriks ditambah <c>HRD_TRV_REQUEST_CREATED</c>, <c>HRD_TRV_REQUEST_REJECTED</c>, dan tiga event uang muka yang
    /// sudah ada. Pesan tidak memuat nomor rekening, gaji, atau alasan medis; alasan tanpa nota dipotong 200 karakter.
    /// </para>
    /// </remarks>
    public class TravelAuditTrailService : ITravelAuditTrailService
    {
        public const string RequestCreated = "HRD_TRV_REQUEST_CREATED";
        public const string RequestSubmitted = "HRD_TRV_REQUEST_SUBMITTED";
        public const string RequestApproved = "HRD_TRV_REQUEST_APPROVED";
        public const string RequestRejected = "HRD_TRV_REQUEST_REJECTED";
        public const string AdvanceRequested = "HRD_TRV_ADVANCE_REQUESTED";
        public const string AdvanceApproved = "HRD_TRV_ADVANCE_APPROVED";
        public const string AdvanceRejected = "HRD_TRV_ADVANCE_REJECTED";
        public const string AdvancePaid = "HRD_TRV_ADVANCE_PAID";
        public const string AdvanceSettled = "HRD_TRV_ADVANCE_SETTLED";
        public const string SettlementSubmitted = "HRD_TRV_SETTLEMENT_SUBMITTED";
        public const string MissingReceiptDeclared = "HRD_TRV_MISSING_RECEIPT_DECLARED";
        public const string SettlementVerified = "HRD_TRV_SETTLEMENT_VERIFIED";
        public const string SettlementClosed = "HRD_TRV_SETTLEMENT_CLOSED";
        public const string SppdFrozenOverdue = "HRD_TRV_SPPD_FROZEN_OVERDUE";
        public const string PayrollDeductionTriggered = "HRD_TRV_PAYROLL_DEDUCTION_TRIGGERED";
        public const string AttendanceSynced = "HRD_TRV_ATTENDANCE_SYNCED";
        public const string RosterLockApplied = "HRD_TRV_ROSTER_LOCK_APPLIED";

        private const int MaxReasonLength = 200;

        private static readonly string[] ApprovedOrLater =
        {
            BusinessTravelRequestStatuses.Approved, "AdvancePaid", "InTravel", "WaitingSettlement",
            "SettlementSubmitted", "SettlementVerified", BusinessTravelRequestStatuses.Completed
        };

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
                .Select(r => new
                {
                    r.Id, r.TravelRequestNumber, r.CreateDateTime, r.CreateBy, r.TravelStatus, r.Destination, r.StartDate, r.EndDate,
                    r.SubmittedAt, r.SubmittedByUserId, r.ApprovedAt, r.ApprovedByUserId, r.ApprovedBudgetAmount,
                    r.RejectedAt, r.RejectedByUserId, r.HasScheduleConflict
                })
                .ToListAsync(cancellationToken);
            var numbers = requests.ToDictionary(r => r.Id, r => r.TravelRequestNumber);
            var ids = numbers.Keys.ToList();

            var events = new List<TravelAuditEventDto>();
            void Add(Guid travelId, string type, DateTime? at, Guid? actor, string detail)
            {
                if (at == null) return;
                events.Add(new TravelAuditEventDto { BusinessTravelRequestId = travelId, TravelRequestNumber = numbers.GetValueOrDefault(travelId, string.Empty), EventType = type, OccurredAt = at.Value, ActorUserId = actor, Detail = detail });
            }

            foreach (var r in requests)
            {
                Add(r.Id, RequestCreated, r.CreateDateTime, r.CreateBy, $"Permohonan dibuat. Status saat ini: {r.TravelStatus}.");
                Add(r.Id, RequestSubmitted, r.SubmittedAt, r.SubmittedByUserId, $"SPPD diajukan ke {r.Destination}, berangkat {r.StartDate:yyyy-MM-dd}.");
                Add(r.Id, RequestApproved, r.ApprovedAt, r.ApprovedByUserId, $"SPPD disetujui. Anggaran disetujui {r.ApprovedBudgetAmount:N0}.");
                Add(r.Id, RequestRejected, r.RejectedAt, r.RejectedByUserId, "SPPD ditolak.");
            }

            var advances = await _context.TrxTravelAdvanceRequests.AsNoTracking()
                .Where(a => !a.IsDelete && ids.Contains(a.BusinessTravelRequestId))
                .ToListAsync(cancellationToken);
            foreach (var a in advances)
            {
                Add(a.BusinessTravelRequestId, AdvanceRequested, a.SubmittedAt, a.SubmittedByUserId, $"Uang muka {a.AdvanceRequestNumber} diajukan sebesar {a.RequestedAmount:N0}.");
                Add(a.BusinessTravelRequestId, AdvanceApproved, a.ApprovedAt, a.ApprovedByUserId, $"Uang muka {a.AdvanceRequestNumber} disetujui sebesar {a.ApprovedAmount:N0}.");
                Add(a.BusinessTravelRequestId, AdvanceRejected, a.RejectedAt, a.RejectedByUserId, $"Uang muka {a.AdvanceRequestNumber} ditolak.");
                Add(a.BusinessTravelRequestId, AdvancePaid, a.PaidAt, null, $"Uang muka {a.AdvanceRequestNumber} dicairkan sebesar {a.PaidAmount:N0}.");
                Add(a.BusinessTravelRequestId, AdvanceSettled, a.SettledAt, null, $"Uang muka {a.AdvanceRequestNumber} diselesaikan.");
            }

            var settlements = await _context.TrxTravelSettlements.AsNoTracking()
                .Where(s => !s.IsDelete && ids.Contains(s.BusinessTravelRequestId))
                .ToListAsync(cancellationToken);
            foreach (var s in settlements)
            {
                Add(s.BusinessTravelRequestId, SettlementSubmitted, s.SubmittedAt, s.SubmittedByUserId, $"Settlement {s.SettlementNumber} diajukan.");
                Add(s.BusinessTravelRequestId, SettlementVerified, s.VerifiedAt, s.VerifiedByUserId, $"Settlement {s.SettlementNumber} diverifikasi sebesar {s.ApprovedExpenseAmount:N0}.");
                // Diisi worker tunggakan TRV-W2 saat settlement menunggak lebih dari 14 hari (HRD-DEC-191).
                Add(s.BusinessTravelRequestId, SppdFrozenOverdue, s.DefaultedAt, null, "Hak SPPD dibekukan: settlement menunggak lebih dari 14 hari.");
                Add(s.BusinessTravelRequestId, SettlementClosed, s.SettledAt, null, $"Settlement {s.SettlementNumber} ditutup.");
            }

            var missingReceipts = await _context.TrxTravelExpenseItems.AsNoTracking()
                .Where(i => !i.IsDelete && i.IsMissingReceiptDeclared && i.TravelExpenseClaim != null && ids.Contains(i.TravelExpenseClaim.BusinessTravelRequestId))
                .Select(i => new { i.TravelExpenseClaim!.BusinessTravelRequestId, i.ClaimedAmount, i.MissingReceiptReason, i.MissingReceiptApprovedBy, i.CreateDateTime, i.UpdateDateTime })
                .ToListAsync(cancellationToken);
            foreach (var m in missingReceipts)
            {
                var reason = (m.MissingReceiptReason ?? string.Empty).Trim();
                if (reason.Length > MaxReasonLength) reason = reason[..MaxReasonLength];
                Add(m.BusinessTravelRequestId, MissingReceiptDeclared, m.UpdateDateTime ?? m.CreateDateTime, m.MissingReceiptApprovedBy, $"Deklarasi tanpa nota {m.ClaimedAmount:N0}: {reason}");
            }

            var advanceByReference = advances.ToDictionary(a => $"TRV-ADV-{a.Id:N}", a => a.BusinessTravelRequestId);
            if (advanceByReference.Count > 0)
            {
                var references = advanceByReference.Keys.ToList();
                var deductions = await _context.TrxPayrollDeductionSchedules.AsNoTracking()
                    .Where(d => !d.IsDelete && d.ExternalReferenceId != null && references.Contains(d.ExternalReferenceId))
                    .Select(d => new { d.ExternalReferenceId, d.ScheduleNumber, d.MonthlyInstallmentAmount, d.CreateDateTime })
                    .ToListAsync(cancellationToken);
                foreach (var d in deductions)
                {
                    Add(advanceByReference[d.ExternalReferenceId!], PayrollDeductionTriggered, d.CreateDateTime, null, $"Potongan payroll {d.ScheduleNumber} dijadwalkan sebesar {d.MonthlyInstallmentAmount:N0}.");
                }
            }

            var links = await _context.TrxTravelAttendanceLinks.AsNoTracking()
                .Where(l => !l.IsDelete && ids.Contains(l.BusinessTravelRequestId) && l.SyncedAt != null)
                .Select(l => new { l.BusinessTravelRequestId, l.AttendanceDate, l.SyncedAt, l.CreateBy })
                .ToListAsync(cancellationToken);
            foreach (var l in links)
            {
                Add(l.BusinessTravelRequestId, AttendanceSynced, l.SyncedAt, l.CreateBy, $"Presensi Hadir Dinas Luar tanggal {l.AttendanceDate:yyyy-MM-dd}.");
            }

            var firstSync = links.GroupBy(l => l.BusinessTravelRequestId).ToDictionary(g => g.Key, g => g.Min(x => x.SyncedAt));
            foreach (var r in requests.Where(r => r.HasScheduleConflict && ApprovedOrLater.Contains(r.TravelStatus)))
            {
                Add(r.Id, RosterLockApplied, firstSync.GetValueOrDefault(r.Id), r.ApprovedByUserId, $"Gembok roster dipasang {r.StartDate:yyyy-MM-dd} s.d. {r.EndDate:yyyy-MM-dd}.");
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
