using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    /// <summary>
    /// Service penyedia integrasi kompensasi siaga (on-call standby) dan pemanggilan cito ke modul Lembur (09) dan Payroll (10) (BE-HRD-101, INT-HR-38, HRD-DEC-103).
    /// </summary>
    public class OnCallCompensationHandoffService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OnCallCompensationHandoffService> _logger;

        public OnCallCompensationHandoffService(
            ApplicationDbContext context,
            ILogger<OnCallCompensationHandoffService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Mengambil rekapitulasi kompensasi siaga standby dan tiket call-in per pegawai untuk periode/rentang tanggal tertentu.
        /// Dikonsumsi oleh modul Payroll (10) untuk tunjangan siaga bulanan dan modul Lembur (09) untuk insentif call-out (INT-HR-38).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<OnCallCompensationSummaryResponse>> GetOnCallCompensationSummaryAsync(
            Guid? rosterPeriodId = null,
            Guid? hospitalSiteId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxOnCallAssignments
                .AsNoTracking()
                .Include(o => o.WorkforceProfile)
                .Include(o => o.OnCallType)
                .Where(o => o.IsActive)
                .AsQueryable();

            if (rosterPeriodId.HasValue && rosterPeriodId.Value != Guid.Empty)
            {
                query = query.Where(o => o.RosterPeriodId == rosterPeriodId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.EndAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.StartAt <= endDate.Value);
            }

            var assignments = await query.ToListAsync(cancellationToken);

            // Kelompokkan data per tenaga kerja / staf medis (WorkforceProfileId)
            var grouped = assignments
                .GroupBy(a => a.WorkforceProfileId)
                .ToList();

            var staffItems = new List<OnCallCompensationStaffItemDto>();
            int totalCallIn = 0;
            int totalSlaMet = 0;
            double grandStandbyHours = 0;

            foreach (var group in grouped)
            {
                var first = group.First();
                var profile = first.WorkforceProfile;

                int onCallShiftCount = group.Count();

                // Jam standby dihitung dari penugasan valid (Confirmed, Active, Completed)
                double standbyHours = group
                    .Where(a => a.AssignmentStatus == "Confirmed" || a.AssignmentStatus == "Active" || a.AssignmentStatus == "Completed")
                    .Sum(a => Math.Max(0, (a.EndAt - a.StartAt).TotalHours));

                int callInCount = group.Count(a => a.IsCalledIn);
                int slaMetCount = group.Count(a => a.IsCalledIn && a.IsSlaMet == true);
                int slaBreachedCount = group.Count(a => a.IsCalledIn && a.IsSlaMet == false);

                // Perhitungan menit cito on-call: minimum 120 menit per kejadian panggil cito sesuai standar RS
                int overtimeMinutes = group
                    .Where(a => a.IsCalledIn && a.ArrivalTime.HasValue)
                    .Sum(a =>
                    {
                        var duration = (int)(a.EndAt - a.ArrivalTime!.Value).TotalMinutes;
                        return Math.Max(120, duration > 0 ? duration : 120);
                    });

                staffItems.Add(new OnCallCompensationStaffItemDto
                {
                    WorkforceProfileId = group.Key,
                    WorkforceProfileCode = profile?.ProfileCode ?? string.Empty,
                    WorkforceDisplayName = profile?.DisplayName ?? string.Empty,
                    OnCallShiftCount = onCallShiftCount,
                    StandbyHours = Math.Round(standbyHours, 2),
                    CallInCount = callInCount,
                    SlaMetCount = slaMetCount,
                    SlaBreachedCount = slaBreachedCount,
                    OvertimeMinutes = overtimeMinutes
                });

                totalCallIn += callInCount;
                totalSlaMet += slaMetCount;
                grandStandbyHours += standbyHours;
            }

            var response = new OnCallCompensationSummaryResponse
            {
                RosterPeriodId = rosterPeriodId,
                TotalOnCallShifts = assignments.Count,
                TotalStandbyHours = Math.Round(grandStandbyHours, 2),
                TotalCallInCount = totalCallIn,
                TotalSlaMetCount = totalSlaMet,
                StaffItems = staffItems.OrderBy(s => s.WorkforceDisplayName).ToList()
            };

            _logger.LogInformation("Data handoff kompensasi siaga (INT-HR-38) berhasil diagregasikan. Periode: {PeriodId}, Total Staf: {StaffCount}, Total Jam Standby: {Hours}",
                rosterPeriodId, staffItems.Count, grandStandbyHours);

            return SchedulingRequestServiceResult<OnCallCompensationSummaryResponse>.Ok(
                response,
                "Rekapitulasi kompensasi siaga dan panggil cito berhasil dihasilkan untuk modul Payroll dan Lembur.");
        }
    }
}
