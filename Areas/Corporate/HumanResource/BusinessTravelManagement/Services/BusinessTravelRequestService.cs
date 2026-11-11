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
    public interface IBusinessTravelRequestService
    {
        Task<(List<BusinessTravelResponseDto> Items, int TotalCount)> GetRequestsAsync(
            Guid? workforceProfileId, string? status, DateOnly? startDate, DateOnly? endDate, Guid? departmentId, int pageNumber, int pageSize);
        Task<BusinessTravelDetailResponseDto> GetRequestByIdAsync(Guid id);
        Task<BusinessTravelResponseDto> CreateRequestAsync(CreateBusinessTravelRequestDto dto, Guid currentUserId, Guid currentWorkforceProfileId);
        Task<BusinessTravelResponseDto> UpdateRequestAsync(Guid id, UpdateBusinessTravelRequestDto dto, Guid currentUserId);
        Task<BusinessTravelResponseDto> CancelRequestAsync(Guid id, CancelTravelRequestDto dto, Guid currentUserId);
    }

    public class BusinessTravelRequestService : IBusinessTravelRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITravelBudgetAndConflictValidationService _conflictValidationService;
        private readonly IMstTravelAllowanceRateService _rateService;

        public BusinessTravelRequestService(
            ApplicationDbContext context,
            ITravelBudgetAndConflictValidationService conflictValidationService,
            IMstTravelAllowanceRateService rateService)
        {
            _context = context;
            _conflictValidationService = conflictValidationService;
            _rateService = rateService;
        }

        private static int CalculateInclusiveDays(DateOnly start, DateOnly end)
        {
            return end.DayNumber - start.DayNumber + 1;
        }

        public async Task<(List<BusinessTravelResponseDto> Items, int TotalCount)> GetRequestsAsync(
            Guid? workforceProfileId, string? status, DateOnly? startDate, DateOnly? endDate, Guid? departmentId, int pageNumber, int pageSize)
        {
            var query = _context.TrxBusinessTravelRequests
                .Include(r => r.WorkforceProfile)
                .Where(r => r.IsActive);

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(r => r.WorkforceProfileId == workforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.TravelStatus.ToLower() == status.ToLower());
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.EndDate <= endDate.Value);
            }

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.DepartmentId == departmentId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new BusinessTravelResponseDto
                {
                    Id = r.Id,
                    TravelRequestNumber = r.TravelRequestNumber,
                    WorkforceProfileId = r.WorkforceProfileId,
                    EmployeeName = r.WorkforceProfile != null ? r.WorkforceProfile.DisplayName : "Pegawai Rumah Sakit",
                    TravelTitle = r.TravelTitle,
                    Origin = r.Origin,
                    Destination = r.Destination,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TravelDayCount = r.TravelDayCount,
                    EstimatedTotalAmount = r.EstimatedTotalAmount,
                    ApprovedBudgetAmount = r.ApprovedBudgetAmount,
                    TravelStatus = r.TravelStatus,
                    CurrencyCode = r.CurrencyCode,
                    CreatedAt = r.CreateDateTime
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<BusinessTravelDetailResponseDto> GetRequestByIdAsync(Guid id)
        {
            var r = await _context.TrxBusinessTravelRequests
                .Include(x => x.WorkforceProfile)
                .Include(x => x.CostCenter)
                .Include(x => x.DestinationZone)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            return new BusinessTravelDetailResponseDto
            {
                Id = r.Id,
                TravelRequestNumber = r.TravelRequestNumber,
                WorkforceProfileId = r.WorkforceProfileId,
                EmployeeName = r.WorkforceProfile?.DisplayName ?? "Pegawai Rumah Sakit",
                TravelTitle = r.TravelTitle,
                TravelPurpose = r.TravelPurpose,
                ActivityDescription = r.ActivityDescription,
                Origin = r.Origin,
                Destination = r.Destination,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                TravelDayCount = r.TravelDayCount,
                EstimatedTransportationAmount = r.EstimatedTransportationAmount,
                EstimatedAccommodationAmount = r.EstimatedAccommodationAmount,
                EstimatedAllowanceAmount = r.EstimatedAllowanceAmount,
                EstimatedOtherAmount = r.EstimatedOtherAmount,
                EstimatedTotalAmount = r.EstimatedTotalAmount,
                ApprovedBudgetAmount = r.ApprovedBudgetAmount,
                TravelStatus = r.TravelStatus,
                CurrencyCode = r.CurrencyCode,
                IsDomestic = r.IsDomestic,
                IsInternational = r.IsInternational,
                IsUrgent = r.IsUrgent,
                CostCenterId = r.CostCenterId,
                CostCenterName = r.CostCenter?.CostCenterName,
                DestinationZoneId = r.DestinationZoneId,
                DestinationZoneName = r.DestinationZone?.DestinationZoneName,
                DocumentPdfUrl = $"/api/v1/hrd/travel/requests/{r.Id}/document-pdf",
                ApprovalNotes = r.ApprovalNotes,
                CancellationReason = r.CancellationReason,
                CreatedAt = r.CreateDateTime
            };
        }

        public async Task<BusinessTravelResponseDto> CreateRequestAsync(CreateBusinessTravelRequestDto dto, Guid currentUserId, Guid currentWorkforceProfileId)
        {
            var inclusiveDays = CalculateInclusiveDays(dto.StartDate, dto.EndDate);
            if (inclusiveDays <= 0)
            {
                throw new InvalidOperationException("Durasi hari perjalanan dinas minimal 1 hari (VR-TRV-19)");
            }

            // Hitung uang harian otomatis bila belum diisi (TRV-B3, VR-TRV-04)
            var allowanceAmount = dto.EstimatedAllowanceAmount;
            if (allowanceAmount <= 0)
            {
                allowanceAmount = await _rateService.CalculateTotalAllowanceAsync(dto.DestinationZoneId, null, inclusiveDays);
            }

            var totalEstimated = dto.EstimatedTransportationAmount + dto.EstimatedAccommodationAmount + allowanceAmount + dto.EstimatedOtherAmount;

            // Validasi konflik cuti, jadwal dinas lain & pagu anggaran (VR-TRV-10, 11, 12, 14)
            await _conflictValidationService.ValidateTravelRequestAsync(
                currentWorkforceProfileId, dto.StartDate, dto.EndDate, dto.CostCenterId, totalEstimated);

            var now = DateTime.UtcNow;
            var seq = await _context.TrxBusinessTravelRequests.CountAsync() + 1;
            var regNumber = $"SPPD/RSQ/{now:yyyy}/{now:MM}/{seq:D4}";

            var defaultTravelType = await _context.MstTravelTypes.FirstOrDefaultAsync();

            var entity = new TrxBusinessTravelRequest
            {
                Id = Guid.NewGuid(),
                TravelRequestNumber = regNumber,
                WorkforceProfileId = currentWorkforceProfileId,
                TravelTypeId = dto.TravelTypeId ?? defaultTravelType?.Id ?? Guid.NewGuid(),
                CostCenterId = dto.CostCenterId,
                DestinationZoneId = dto.DestinationZoneId,
                TravelTitle = dto.TravelTitle,
                TravelPurpose = dto.TravelPurpose,
                ActivityDescription = dto.ActivityDescription,
                Origin = dto.Origin,
                Destination = dto.Destination,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TravelDayCount = inclusiveDays,
                EstimatedTransportationAmount = dto.EstimatedTransportationAmount,
                EstimatedAccommodationAmount = dto.EstimatedAccommodationAmount,
                EstimatedAllowanceAmount = allowanceAmount,
                EstimatedOtherAmount = dto.EstimatedOtherAmount,
                EstimatedTotalAmount = totalEstimated,
                ApprovedBudgetAmount = totalEstimated, // Standar awal mengikuti estimasi
                CurrencyCode = string.IsNullOrWhiteSpace(dto.CurrencyCode) ? "IDR" : dto.CurrencyCode,
                IsDomestic = dto.IsDomestic,
                IsInternational = dto.IsInternational,
                IsUrgent = dto.IsUrgent,
                TravelStatus = "Submitted",
                SubmittedAt = now,
                SubmittedByUserId = currentUserId,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = now
            };

            _context.TrxBusinessTravelRequests.Add(entity);
            await _context.SaveChangesAsync();

            return new BusinessTravelResponseDto
            {
                Id = entity.Id,
                TravelRequestNumber = entity.TravelRequestNumber,
                WorkforceProfileId = entity.WorkforceProfileId,
                EmployeeName = "Pegawai Pemohon",
                TravelTitle = entity.TravelTitle,
                Origin = entity.Origin,
                Destination = entity.Destination,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                TravelDayCount = entity.TravelDayCount,
                EstimatedTotalAmount = entity.EstimatedTotalAmount,
                ApprovedBudgetAmount = entity.ApprovedBudgetAmount,
                TravelStatus = entity.TravelStatus,
                CurrencyCode = entity.CurrencyCode,
                CreatedAt = entity.CreateDateTime
            };
        }

        public async Task<BusinessTravelResponseDto> UpdateRequestAsync(Guid id, UpdateBusinessTravelRequestDto dto, Guid currentUserId)
        {
            var entity = await _context.TrxBusinessTravelRequests.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            if (entity.TravelStatus != "Draft" && entity.TravelStatus != "Submitted")
            {
                throw new InvalidOperationException("Hanya SPPD berstatus Draft atau Submitted yang dapat diperbarui");
            }

            var inclusiveDays = CalculateInclusiveDays(dto.StartDate, dto.EndDate);
            if (inclusiveDays <= 0)
            {
                throw new InvalidOperationException("Durasi hari perjalanan dinas minimal 1 hari (VR-TRV-19)");
            }

            var totalEstimated = dto.EstimatedTransportationAmount + dto.EstimatedAccommodationAmount + dto.EstimatedAllowanceAmount + dto.EstimatedOtherAmount;

            await _conflictValidationService.ValidateTravelRequestAsync(
                entity.WorkforceProfileId, dto.StartDate, dto.EndDate, entity.CostCenterId, totalEstimated, entity.Id);

            entity.TravelTitle = dto.TravelTitle;
            entity.TravelPurpose = dto.TravelPurpose;
            entity.ActivityDescription = dto.ActivityDescription;
            entity.Origin = dto.Origin;
            entity.Destination = dto.Destination;
            entity.StartDate = dto.StartDate;
            entity.EndDate = dto.EndDate;
            entity.TravelDayCount = inclusiveDays;
            entity.EstimatedTransportationAmount = dto.EstimatedTransportationAmount;
            entity.EstimatedAccommodationAmount = dto.EstimatedAccommodationAmount;
            entity.EstimatedAllowanceAmount = dto.EstimatedAllowanceAmount;
            entity.EstimatedOtherAmount = dto.EstimatedOtherAmount;
            entity.EstimatedTotalAmount = totalEstimated;
            entity.ApprovedBudgetAmount = totalEstimated;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new BusinessTravelResponseDto
            {
                Id = entity.Id,
                TravelRequestNumber = entity.TravelRequestNumber,
                WorkforceProfileId = entity.WorkforceProfileId,
                EmployeeName = "Pegawai Pemohon",
                TravelTitle = entity.TravelTitle,
                Origin = entity.Origin,
                Destination = entity.Destination,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                TravelDayCount = entity.TravelDayCount,
                EstimatedTotalAmount = entity.EstimatedTotalAmount,
                ApprovedBudgetAmount = entity.ApprovedBudgetAmount,
                TravelStatus = entity.TravelStatus,
                CurrencyCode = entity.CurrencyCode,
                CreatedAt = entity.CreateDateTime
            };
        }

        public async Task<BusinessTravelResponseDto> CancelRequestAsync(Guid id, CancelTravelRequestDto dto, Guid currentUserId)
        {
            var entity = await _context.TrxBusinessTravelRequests.FindAsync(id);
            if (entity == null) throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            if (entity.TravelStatus == "Completed" || entity.TravelStatus == "Cancelled")
            {
                throw new InvalidOperationException("Perjalanan dinas yang sudah selesai atau sudah dibatalkan tidak dapat diubah");
            }

            if (string.IsNullOrWhiteSpace(dto.CancellationReason) || dto.CancellationReason.Trim().Length < 15)
            {
                throw new InvalidOperationException("Alasan pembatalan dinas wajib diisi tertulis minimal 15 karakter (VR-TRV-20)");
            }

            var now = DateTime.UtcNow;
            entity.TravelStatus = "Cancelled";
            entity.CancellationReason = dto.CancellationReason;
            entity.CancelledAt = now;
            entity.CancelledByUserId = currentUserId;
            entity.UpdateBy = currentUserId;
            entity.UpdateDateTime = now;

            await _context.SaveChangesAsync();

            return new BusinessTravelResponseDto
            {
                Id = entity.Id,
                TravelRequestNumber = entity.TravelRequestNumber,
                WorkforceProfileId = entity.WorkforceProfileId,
                EmployeeName = "Pegawai Pemohon",
                TravelTitle = entity.TravelTitle,
                Origin = entity.Origin,
                Destination = entity.Destination,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                TravelDayCount = entity.TravelDayCount,
                EstimatedTotalAmount = entity.EstimatedTotalAmount,
                ApprovedBudgetAmount = entity.ApprovedBudgetAmount,
                TravelStatus = entity.TravelStatus,
                CurrencyCode = entity.CurrencyCode,
                CreatedAt = entity.CreateDateTime
            };
        }
    }
}
