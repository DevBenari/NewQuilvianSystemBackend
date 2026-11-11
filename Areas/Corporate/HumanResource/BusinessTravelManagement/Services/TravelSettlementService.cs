using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface ITravelSettlementService
    {
        Task<(List<TravelSettlementResponseDto> Items, int TotalCount)> GetSettlementsAsync(string? status, bool? isOverdue, int pageNumber, int pageSize);
        Task<TravelSettlementDetailResponseDto> GetSettlementByIdAsync(Guid id);
        Task<TravelSettlementResponseDto> SubmitSettlementAsync(SubmitTravelSettlementDto dto, Guid currentUserId);
        Task<TravelSettlementResponseDto> VerifySettlementAsync(Guid settlementId, VerifySettlementDto dto, Guid currentUserId);
        Task<TravelSettlementResponseDto> CloseSettlementAsync(Guid settlementId, CloseSettlementDto dto, Guid currentUserId);
        Task<List<TravelOverdueReportDto>> GetOverdueReportAsync(Guid? departmentId, DateOnly? cutoffDate);
    }

    public class TravelSettlementService : ITravelSettlementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMissingReceiptDeclarationService _missingReceiptService;

        public TravelSettlementService(ApplicationDbContext context, IMissingReceiptDeclarationService missingReceiptService)
        {
            _context = context;
            _missingReceiptService = missingReceiptService;
        }

        public async Task<(List<TravelSettlementResponseDto> Items, int TotalCount)> GetSettlementsAsync(string? status, bool? isOverdue, int pageNumber, int pageSize)
        {
            var query = _context.TrxTravelSettlements
                .Include(s => s.BusinessTravelRequest)
                .Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s => s.SettlementStatus.ToLower() == status.ToLower());
            }

            if (isOverdue.HasValue)
            {
                query = query.Where(s => s.IsOverdue == isOverdue.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new TravelSettlementResponseDto
                {
                    Id = s.Id,
                    SettlementNumber = s.SettlementNumber,
                    BusinessTravelRequestId = s.BusinessTravelRequestId,
                    TravelRequestNumber = s.BusinessTravelRequest != null ? s.BusinessTravelRequest.TravelRequestNumber : string.Empty,
                    AdvancePaidAmount = s.AdvancePaidAmount,
                    ApprovedExpenseAmount = s.ApprovedExpenseAmount,
                    SettlementDifferenceAmount = s.SettlementDifferenceAmount,
                    SettlementType = s.EmployeeRefundAmount > 0 ? "Overpaid" : (s.CompanyPayableAmount > 0 ? "Underpaid" : "Balanced"),
                    SettlementStatus = s.SettlementStatus,
                    IsOverdue = s.IsOverdue,
                    SettlementDate = s.SettlementDate,
                    SettledAt = s.SettledAt,
                    SettlementNotes = s.SettlementNotes
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<TravelSettlementDetailResponseDto> GetSettlementByIdAsync(Guid id)
        {
            var s = await _context.TrxTravelSettlements
                .Include(x => x.BusinessTravelRequest)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null) throw new KeyNotFoundException("Berkas settlement pertanggungjawaban dinas tidak ditemukan");

            return new TravelSettlementDetailResponseDto
            {
                Id = s.Id,
                SettlementNumber = s.SettlementNumber,
                BusinessTravelRequestId = s.BusinessTravelRequestId,
                TravelRequestNumber = s.BusinessTravelRequest?.TravelRequestNumber ?? string.Empty,
                AdvancePaidAmount = s.AdvancePaidAmount,
                ApprovedExpenseAmount = s.ApprovedExpenseAmount,
                SettlementDifferenceAmount = s.SettlementDifferenceAmount,
                SettlementType = s.EmployeeRefundAmount > 0 ? "Overpaid" : (s.CompanyPayableAmount > 0 ? "Underpaid" : "Balanced"),
                SettlementStatus = s.SettlementStatus,
                IsOverdue = s.IsOverdue,
                SettlementDate = s.SettlementDate,
                SettledAt = s.SettledAt,
                SettlementNotes = s.SettlementNotes,
                EmployeeRefundAmount = s.EmployeeRefundAmount,
                CompanyPayableAmount = s.CompanyPayableAmount,
                PaymentOrRefundRef = s.PaymentOrRefundRef,
                ReceiptDocUrl = s.ReceiptDocUrl
            };
        }

        public async Task<TravelSettlementResponseDto> SubmitSettlementAsync(SubmitTravelSettlementDto dto, Guid currentUserId)
        {
            var travel = await _context.TrxBusinessTravelRequests.FindAsync(dto.BusinessTravelRequestId);
            if (travel == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            // Ambil data uang muka yang telah dicairkan
            var advance = await _context.TrxTravelAdvanceRequests
                .Where(a => a.BusinessTravelRequestId == travel.Id && a.AdvanceStatus == "Paid")
                .OrderByDescending(a => a.PaidAt)
                .FirstOrDefaultAsync();

            var advancePaid = advance?.PaidAmount ?? 0m;

            var actualTotal = (dto.ActualExpenses?.Sum(e => e.ClaimedAmount) ?? 0m) +
                              (dto.MissingReceipts?.Sum(m => m.ClaimedAmount) ?? 0m);

            // Validasi deklarasi tanpa kuitansi darurat (VR-TRV-08, 09, HRD-DEC-192)
            if (dto.MissingReceipts != null && dto.MissingReceipts.Any())
            {
                _missingReceiptService.ValidateMissingReceipts(dto.MissingReceipts, actualTotal);
            }

            var diff = Math.Abs(actualTotal - advancePaid);
            decimal refund = 0m;
            decimal payable = 0m;

            if (actualTotal < advancePaid)
            {
                // Overpaid: Pegawai menerima lebih banyak uang muka, wajib setor balik ke kasir (HRD-DEC-191)
                refund = advancePaid - actualTotal;
            }
            else if (actualTotal > advancePaid)
            {
                // Underpaid: Pengeluaran riil lebih besar dari uang muka, RS wajib membayar ke pegawai (HRD-DEC-191)
                payable = actualTotal - advancePaid;
            }

            var now = DateTime.UtcNow;
            var seq = await _context.TrxTravelSettlements.CountAsync() + 1;
            var settlementNumber = $"STL/RSQ/{now:yyyy}/{now:MM}/{seq:D4}";

            var settlement = new TrxTravelSettlement
            {
                Id = Guid.NewGuid(),
                BusinessTravelRequestId = travel.Id,
                TravelAdvanceRequestId = advance?.Id,
                SettlementNumber = settlementNumber,
                SettlementDate = DateOnly.FromDateTime(now),
                AdvancePaidAmount = advancePaid,
                ApprovedExpenseAmount = actualTotal,
                SettlementDifferenceAmount = diff,
                EmployeeRefundAmount = refund,
                CompanyPayableAmount = payable,
                SettledAmount = actualTotal,
                CurrencyCode = travel.CurrencyCode ?? "IDR",
                SettlementStatus = "Submitted",
                SubmittedAt = now,
                SubmittedByUserId = currentUserId,
                SettlementNotes = dto.SettlementNotes,
                IsOverdue = false,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = now
            };

            _context.TrxTravelSettlements.Add(settlement);

            // Butir tanpa nota disimpan supaya bisa disetujui atasan (Hrd.BusinessTravel.MissingReceipt.Approve).
            await _missingReceiptService.StageItemsAsync(travel, dto.MissingReceipts ?? new List<MissingReceiptItemDto>(), currentUserId);

            // Update status SPPD
            travel.TravelStatus = "SettlementSubmitted";
            travel.UpdateBy = currentUserId;
            travel.UpdateDateTime = now;

            await _context.SaveChangesAsync();

            return new TravelSettlementResponseDto
            {
                Id = settlement.Id,
                SettlementNumber = settlement.SettlementNumber,
                BusinessTravelRequestId = settlement.BusinessTravelRequestId,
                TravelRequestNumber = travel.TravelRequestNumber,
                AdvancePaidAmount = settlement.AdvancePaidAmount,
                ApprovedExpenseAmount = settlement.ApprovedExpenseAmount,
                SettlementDifferenceAmount = settlement.SettlementDifferenceAmount,
                SettlementType = refund > 0 ? "Overpaid" : (payable > 0 ? "Underpaid" : "Balanced"),
                SettlementStatus = settlement.SettlementStatus,
                IsOverdue = settlement.IsOverdue,
                SettlementDate = settlement.SettlementDate,
                SettlementNotes = settlement.SettlementNotes
            };
        }

        public async Task<TravelSettlementResponseDto> VerifySettlementAsync(Guid settlementId, VerifySettlementDto dto, Guid currentUserId)
        {
            var settlement = await _context.TrxTravelSettlements
                .Include(s => s.BusinessTravelRequest)
                .FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null) throw new KeyNotFoundException("Berkas settlement tidak ditemukan");

            var now = DateTime.UtcNow;
            settlement.ApprovedExpenseAmount = dto.ApprovedActualAmount;
            var advancePaid = settlement.AdvancePaidAmount;
            var actualTotal = dto.ApprovedActualAmount;

            var diff = Math.Abs(actualTotal - advancePaid);
            if (actualTotal < advancePaid)
            {
                settlement.EmployeeRefundAmount = advancePaid - actualTotal;
                settlement.CompanyPayableAmount = 0m;
            }
            else if (actualTotal > advancePaid)
            {
                settlement.CompanyPayableAmount = actualTotal - advancePaid;
                settlement.EmployeeRefundAmount = 0m;
            }
            else
            {
                settlement.EmployeeRefundAmount = 0m;
                settlement.CompanyPayableAmount = 0m;
            }

            settlement.SettlementDifferenceAmount = diff;
            settlement.SettlementStatus = "Verified";
            settlement.VerifiedAt = now;
            settlement.VerifiedByUserId = currentUserId;
            settlement.SettlementNotes = string.IsNullOrWhiteSpace(dto.VerificationNotes) ? settlement.SettlementNotes : $"{settlement.SettlementNotes} [Verifikasi: {dto.VerificationNotes}]";
            settlement.UpdateBy = currentUserId;
            settlement.UpdateDateTime = now;

            if (settlement.BusinessTravelRequest != null)
            {
                settlement.BusinessTravelRequest.TravelStatus = "SettlementVerified";
                settlement.BusinessTravelRequest.UpdateBy = currentUserId;
                settlement.BusinessTravelRequest.UpdateDateTime = now;
            }

            await _context.SaveChangesAsync();

            return new TravelSettlementResponseDto
            {
                Id = settlement.Id,
                SettlementNumber = settlement.SettlementNumber,
                BusinessTravelRequestId = settlement.BusinessTravelRequestId,
                TravelRequestNumber = settlement.BusinessTravelRequest?.TravelRequestNumber ?? string.Empty,
                AdvancePaidAmount = settlement.AdvancePaidAmount,
                ApprovedExpenseAmount = settlement.ApprovedExpenseAmount,
                SettlementDifferenceAmount = settlement.SettlementDifferenceAmount,
                SettlementType = settlement.EmployeeRefundAmount > 0 ? "Overpaid" : (settlement.CompanyPayableAmount > 0 ? "Underpaid" : "Balanced"),
                SettlementStatus = settlement.SettlementStatus,
                IsOverdue = settlement.IsOverdue,
                SettlementDate = settlement.SettlementDate,
                SettlementNotes = settlement.SettlementNotes
            };
        }

        public async Task<TravelSettlementResponseDto> CloseSettlementAsync(Guid settlementId, CloseSettlementDto dto, Guid currentUserId)
        {
            var settlement = await _context.TrxTravelSettlements
                .Include(s => s.BusinessTravelRequest)
                .FirstOrDefaultAsync(s => s.Id == settlementId);

            if (settlement == null) throw new KeyNotFoundException("Berkas settlement tidak ditemukan");

            var now = DateTime.UtcNow;
            settlement.SettlementStatus = "Completed";
            settlement.SettledAt = now;
            settlement.IsOverdue = false;
            settlement.PaymentOrRefundRef = dto.PaymentOrRefundRef;
            settlement.ReceiptDocUrl = dto.ReceiptDocUrl;
            settlement.SettlementNotes = string.IsNullOrWhiteSpace(dto.Notes) ? settlement.SettlementNotes : $"{settlement.SettlementNotes} [Penutupan: {dto.Notes}]";
            settlement.UpdateBy = currentUserId;
            settlement.UpdateDateTime = now;

            // Jika ada uang muka, update menjadi Settled
            if (settlement.TravelAdvanceRequestId.HasValue)
            {
                var adv = await _context.TrxTravelAdvanceRequests.FindAsync(settlement.TravelAdvanceRequestId.Value);
                if (adv != null)
                {
                    adv.AdvanceStatus = "Settled";
                    adv.SettledAt = now;
                    adv.UpdateBy = currentUserId;
                    adv.UpdateDateTime = now;
                }
            }

            // Update status SPPD menjadi Completed
            if (settlement.BusinessTravelRequest != null)
            {
                settlement.BusinessTravelRequest.TravelStatus = "Completed";
                settlement.BusinessTravelRequest.CompletedAt = now;
                settlement.BusinessTravelRequest.UpdateBy = currentUserId;
                settlement.BusinessTravelRequest.UpdateDateTime = now;
            }

            await _context.SaveChangesAsync();

            return new TravelSettlementResponseDto
            {
                Id = settlement.Id,
                SettlementNumber = settlement.SettlementNumber,
                BusinessTravelRequestId = settlement.BusinessTravelRequestId,
                TravelRequestNumber = settlement.BusinessTravelRequest?.TravelRequestNumber ?? string.Empty,
                AdvancePaidAmount = settlement.AdvancePaidAmount,
                ApprovedExpenseAmount = settlement.ApprovedExpenseAmount,
                SettlementDifferenceAmount = settlement.SettlementDifferenceAmount,
                SettlementType = settlement.EmployeeRefundAmount > 0 ? "Overpaid" : (settlement.CompanyPayableAmount > 0 ? "Underpaid" : "Balanced"),
                SettlementStatus = settlement.SettlementStatus,
                IsOverdue = false,
                SettlementDate = settlement.SettlementDate,
                SettledAt = settlement.SettledAt,
                SettlementNotes = settlement.SettlementNotes
            };
        }

        public async Task<List<TravelOverdueReportDto>> GetOverdueReportAsync(Guid? departmentId, DateOnly? cutoffDate)
        {
            var targetCutoff = cutoffDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

            var query = _context.TrxBusinessTravelRequests
                .Include(r => r.WorkforceProfile)
                .Include(r => r.Department)
                .Include(r => r.AdvanceRequests)
                .Include(r => r.Settlements)
                .Where(r => r.IsActive && r.TravelStatus != "Completed" && r.TravelStatus != "Cancelled" && r.TravelStatus != "Rejected")
                .Where(r => r.EndDate < targetCutoff);

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.DepartmentId == departmentId.Value);
            }

            var travels = await query.ToListAsync();
            var result = new List<TravelOverdueReportDto>();

            foreach (var t in travels)
            {
                var days = targetCutoff.DayNumber - t.EndDate.DayNumber;
                if (days > 7)
                {
                    var adv = t.AdvanceRequests.FirstOrDefault(a => a.AdvanceStatus == "Paid");
                    var stl = t.Settlements.OrderByDescending(s => s.CreateDateTime).FirstOrDefault();

                    result.Add(new TravelOverdueReportDto
                    {
                        TravelRequestId = t.Id,
                        TravelRequestNumber = t.TravelRequestNumber,
                        EmployeeName = t.WorkforceProfile?.DisplayName ?? "Pegawai RS",
                        DepartmentName = t.Department?.DepartmentName ?? "Departemen",
                        EndDate = t.EndDate,
                        OverdueDays = days,
                        AdvanceAmount = adv?.PaidAmount ?? 0m,
                        SettlementStatus = stl?.SettlementStatus ?? t.TravelStatus,
                        IsBlockedFromNewTravel = days > 14,
                        PayrollDeductionScheduled = days > 30
                    });
                }
            }

            return result.OrderByDescending(x => x.OverdueDays).ToList();
        }
    }
}
