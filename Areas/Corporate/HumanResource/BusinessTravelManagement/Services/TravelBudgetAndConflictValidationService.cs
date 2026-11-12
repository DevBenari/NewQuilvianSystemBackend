using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface ITravelBudgetAndConflictValidationService
    {
        Task ValidateTravelRequestAsync(Guid workforceProfileId, DateOnly startDate, DateOnly endDate, Guid? costCenterId, decimal estimatedTotalAmount, Guid? ignoreTravelId = null);
        Task<bool> IsEmployeeBlockedFromTravelAsync(Guid workforceProfileId);
    }

    public class TravelBudgetAndConflictValidationService : ITravelBudgetAndConflictValidationService
    {
        private readonly ApplicationDbContext _context;

        public TravelBudgetAndConflictValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsEmployeeBlockedFromTravelAsync(Guid workforceProfileId)
        {
            // VR-TRV-14: Settlement menunggak > 14 hari membekukan pembuatan SPPD baru pegawai bersangkutan (HRD-DEC-191)
            var hasOverdueSettlement = await _context.TrxTravelSettlements
                .Include(s => s.BusinessTravelRequest)
                .Where(s => s.BusinessTravelRequest != null && s.BusinessTravelRequest.WorkforceProfileId == workforceProfileId)
                .Where(s => s.IsOverdue && s.SettledAt == null && s.SettlementStatus != "Completed" && s.SettlementStatus != "Cancelled")
                .AnyAsync();

            return hasOverdueSettlement;
        }

        public async Task ValidateTravelRequestAsync(Guid workforceProfileId, DateOnly startDate, DateOnly endDate, Guid? costCenterId, decimal estimatedTotalAmount, Guid? ignoreTravelId = null)
        {
            // 1. Validasi urutan tanggal & durasi inklusif (VR-TRV-19)
            if (endDate < startDate)
            {
                throw new InvalidOperationException("Tanggal kepulangan tidak boleh mendahului tanggal keberangkatan");
            }

            // 2. Validasi sanksi pembekuan SPPD akibat tunggakan settlement > 14 hari (VR-TRV-14, HRD-DEC-191)
            if (await IsEmployeeBlockedFromTravelAsync(workforceProfileId))
            {
                throw new InvalidOperationException("Hak pengajuan SPPD dibekukan karena Anda masih memiliki tunggakan settlement perjalanan dinas sebelumnya yang melewati batas waktu");
            }

            // 3. Validasi bentrok jadwal dengan SPPD lain milik pegawai (VR-TRV-10)
            var queryOtherTravel = _context.TrxBusinessTravelRequests
                .Where(r => r.WorkforceProfileId == workforceProfileId && r.IsActive)
                .Where(r => r.TravelStatus != "Cancelled" && r.TravelStatus != "Rejected");

            if (ignoreTravelId.HasValue)
            {
                queryOtherTravel = queryOtherTravel.Where(r => r.Id != ignoreTravelId.Value);
            }

            var hasTravelConflict = await queryOtherTravel
                .Where(r => (startDate <= r.EndDate && endDate >= r.StartDate))
                .AnyAsync();

            if (hasTravelConflict)
            {
                throw new InvalidOperationException("Terdapat penugasan perjalanan dinas lain yang tumpang tindih pada rentang tanggal tersebut (VR-TRV-10)");
            }

            // 4. Validasi bentrok dengan cuti yang disetujui di Sub-08 (VR-TRV-11)
            var hasLeaveConflict = await _context.WfpLeaveRequests
                .Where(l => l.WorkforceProfileId == workforceProfileId && l.IsActive)
                .Where(l => l.LeaveRequestStatus == "Approved" || l.LeaveRequestStatus == "Submitted")
                .Where(l => (startDate <= l.EndDate && endDate >= l.StartDate))
                .AnyAsync();

            if (hasLeaveConflict)
            {
                throw new InvalidOperationException("Pegawai tercatat memiliki jadwal cuti yang disetujui pada rentang tanggal yang sama (VR-TRV-11)");
            }

            // 5. Validasi ketersediaan pagu anggaran unit cost center (VR-TRV-12)
            if (costCenterId.HasValue && costCenterId.Value != Guid.Empty)
            {
                var costCenter = await _context.MstCostCenters.FindAsync(costCenterId.Value);
                if (costCenter != null && !costCenter.IsActive)
                {
                    throw new InvalidOperationException("Pusat biaya (cost center) unit kerja tidak aktif");
                }
            }
        }
    }
}
