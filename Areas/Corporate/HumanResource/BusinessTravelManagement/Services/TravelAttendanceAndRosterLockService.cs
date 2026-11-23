using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface ITravelAttendanceAndRosterLockService
    {
        /// <summary>Presensi "Hadir Dinas Luar" per hari dinas dan gembok roster; <paramref name="actorUserId"/> dicatat sebagai pembuat/pengubah.</summary>
        Task SyncAttendanceAndLockRosterAsync(Guid travelRequestId, Guid actorUserId);

        /// <summary>Melepas gembok roster dan membatalkan tautan presensi Dinas Luar.</summary>
        Task ReleaseRosterLockAsync(Guid travelRequestId, Guid actorUserId);
    }

    public class TravelAttendanceAndRosterLockService : ITravelAttendanceAndRosterLockService
    {
        private readonly ApplicationDbContext _context;

        public TravelAttendanceAndRosterLockService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SyncAttendanceAndLockRosterAsync(Guid travelRequestId, Guid actorUserId)
        {
            var travel = await _context.TrxBusinessTravelRequests
                .Include(r => r.AttendanceLinks)
                .FirstOrDefaultAsync(r => r.Id == travelRequestId);

            if (travel == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            var now = DateTime.UtcNow;

            // HRD-DEC-193 & INT-HR-97: Sinkronisasi presensi "Hadir Dinas Luar" per hari penugasan dinas
            var currentDay = travel.StartDate;
            while (currentDay <= travel.EndDate)
            {
                var existingLink = travel.AttendanceLinks.FirstOrDefault(l => l.AttendanceDate == currentDay);
                if (existingLink == null)
                {
                    var link = new TrxTravelAttendanceLink
                    {
                        Id = Guid.NewGuid(),
                        BusinessTravelRequestId = travel.Id,
                        WorkforceProfileId = travel.WorkforceProfileId,
                        EmployeeId = travel.EmployeeId,
                        AttendanceDate = currentDay,
                        AttendanceLinkStatus = "Synced",
                        IsPayrollEligible = true,
                        IsAttendanceGenerated = true,
                        SyncedAt = now,
                        SyncMessage = "Otomasi sinkronisasi presensi Hadir Dinas Luar (INT-HR-97)",
                        IsActive = true,
                        CreateBy = actorUserId,
                        CreateDateTime = now
                    };
                    _context.TrxTravelAttendanceLinks.Add(link);
                }
                else
                {
                    existingLink.AttendanceLinkStatus = "Synced";
                    existingLink.SyncedAt = now;
                    existingLink.SyncMessage = "Diperbarui otomatis Hadir Dinas Luar (INT-HR-97)";
                    existingLink.UpdateBy = actorUserId;
                    existingLink.UpdateDateTime = now;
                }

                currentDay = currentDay.AddDays(1);
            }

            // VR-TRV-06 & HRD-DEC-193: Pasang gembok validasi jadwal (Roster Lock)
            // Memastikan penanda flag conflict di SPPD aktif untuk mencegah roster bangsal
            travel.HasScheduleConflict = true; // Penanda gembok aktif
            travel.UpdateBy = actorUserId;
            travel.UpdateDateTime = now;

            await _context.SaveChangesAsync();
        }

        public async Task ReleaseRosterLockAsync(Guid travelRequestId, Guid actorUserId)
        {
            var travel = await _context.TrxBusinessTravelRequests
                .Include(r => r.AttendanceLinks)
                .FirstOrDefaultAsync(r => r.Id == travelRequestId);

            if (travel == null) return;

            var now = DateTime.UtcNow;
            foreach (var link in travel.AttendanceLinks)
            {
                link.AttendanceLinkStatus = "Cancelled";
                link.UpdateBy = actorUserId;
                link.UpdateDateTime = now;
            }

            travel.HasScheduleConflict = false; // Gembok dilepas
            travel.UpdateBy = actorUserId;
            travel.UpdateDateTime = now;

            await _context.SaveChangesAsync();
        }
    }
}
