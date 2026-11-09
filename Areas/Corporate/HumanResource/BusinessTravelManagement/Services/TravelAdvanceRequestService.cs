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
    public interface ITravelAdvanceRequestService
    {
        Task<(List<TravelAdvanceResponseDto> Items, int TotalCount)> GetAdvancesAsync(string? status, int pageNumber, int pageSize);
        Task<TravelAdvanceResponseDto> CreateAdvanceRequestAsync(CreateTravelAdvanceRequestDto dto, Guid currentUserId, Guid currentWorkforceProfileId);
    }

    public class TravelAdvanceRequestService : ITravelAdvanceRequestService
    {
        private readonly ApplicationDbContext _context;

        public TravelAdvanceRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<TravelAdvanceResponseDto> Items, int TotalCount)> GetAdvancesAsync(string? status, int pageNumber, int pageSize)
        {
            var query = _context.TrxTravelAdvanceRequests
                .Include(a => a.BusinessTravelRequest)
                .Where(a => a.IsActive);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.AdvanceStatus.ToLower() == status.ToLower());
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new TravelAdvanceResponseDto
                {
                    Id = a.Id,
                    AdvanceRequestNumber = a.AdvanceRequestNumber,
                    BusinessTravelRequestId = a.BusinessTravelRequestId,
                    TravelRequestNumber = a.BusinessTravelRequest != null ? a.BusinessTravelRequest.TravelRequestNumber : string.Empty,
                    RequestedAmount = a.RequestedAmount,
                    ApprovedAmount = a.ApprovedAmount,
                    PaidAmount = a.PaidAmount,
                    CurrencyCode = a.CurrencyCode,
                    AdvanceStatus = a.AdvanceStatus,
                    RequestDate = a.RequestDate,
                    PaidAt = a.PaidAt,
                    Notes = a.Notes
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<TravelAdvanceResponseDto> CreateAdvanceRequestAsync(CreateTravelAdvanceRequestDto dto, Guid currentUserId, Guid currentWorkforceProfileId)
        {
            var travel = await _context.TrxBusinessTravelRequests.FindAsync(dto.BusinessTravelRequestId);
            if (travel == null)
            {
                throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");
            }

            if (travel.TravelStatus == "Cancelled" || travel.TravelStatus == "Rejected")
            {
                throw new InvalidOperationException("Tidak dapat mengajukan uang muka untuk perjalanan dinas yang dibatalkan atau ditolak");
            }

            // VR-TRV-02: Hard-cap maksimal 80% dari pagu anggaran yang disetujui (HRD-DEC-189)
            var budgetLimit = travel.ApprovedBudgetAmount > 0 ? travel.ApprovedBudgetAmount : travel.EstimatedTotalAmount;
            var maxAllowedAdvance = budgetLimit * 0.80m;

            if (dto.RequestedAmount > maxAllowedAdvance)
            {
                throw new InvalidOperationException(
                    $"Permohonan uang muka ({dto.RequestedAmount:N0} IDR) melebihi batas maksimal 80% dari pagu estimasi biaya disetujui ({maxAllowedAdvance:N0} IDR) sesuai aturan VR-TRV-02 & HRD-DEC-189");
            }

            var now = DateTime.UtcNow;
            var seq = await _context.TrxTravelAdvanceRequests.CountAsync() + 1;
            var advanceNumber = $"ADV/RSQ/{now:yyyy}/{now:MM}/{seq:D4}";

            var entity = new TrxTravelAdvanceRequest
            {
                Id = Guid.NewGuid(),
                BusinessTravelRequestId = dto.BusinessTravelRequestId,
                WorkforceProfileId = currentWorkforceProfileId,
                AdvanceRequestNumber = advanceNumber,
                RequestDate = DateOnly.FromDateTime(now),
                RequestedAmount = dto.RequestedAmount,
                ApprovedAmount = 0m,
                PaidAmount = 0m,
                CurrencyCode = travel.CurrencyCode ?? "IDR",
                AdvanceStatus = "Submitted",
                SubmittedAt = now,
                SubmittedByUserId = currentUserId,
                BankAccountId = dto.BankAccountId,
                Notes = dto.Notes,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = now
            };

            _context.TrxTravelAdvanceRequests.Add(entity);
            await _context.SaveChangesAsync();

            return new TravelAdvanceResponseDto
            {
                Id = entity.Id,
                AdvanceRequestNumber = entity.AdvanceRequestNumber,
                BusinessTravelRequestId = entity.BusinessTravelRequestId,
                TravelRequestNumber = travel.TravelRequestNumber,
                RequestedAmount = entity.RequestedAmount,
                ApprovedAmount = entity.ApprovedAmount,
                PaidAmount = entity.PaidAmount,
                CurrencyCode = entity.CurrencyCode,
                AdvanceStatus = entity.AdvanceStatus,
                RequestDate = entity.RequestDate,
                Notes = entity.Notes
            };
        }
    }
}
