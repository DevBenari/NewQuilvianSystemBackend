using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IMstTravelAllowanceRateService
    {
        Task<decimal> CalculatePerDiemRateAsync(Guid? destinationZoneId, Guid? employeeGradeId);
        Task<decimal> CalculateTotalAllowanceAsync(Guid? destinationZoneId, Guid? employeeGradeId, int travelDayCount);
        Task<List<TravelAllowanceRateResponseDto>> GetRatesAsync(Guid? zoneId, Guid? gradeId);
        Task<TravelAllowanceRateResponseDto> ConfigureRateAsync(ConfigureTravelRateDto dto, Guid currentUserId);
        Task<List<TravelDestinationZoneResponseDto>> GetZonesAsync();
    }

    public class MstTravelAllowanceRateService : IMstTravelAllowanceRateService
    {
        private readonly ApplicationDbContext _context;

        public MstTravelAllowanceRateService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculatePerDiemRateAsync(Guid? destinationZoneId, Guid? employeeGradeId)
        {
            if (!destinationZoneId.HasValue) return 250000m; // Default baseline rate IDR

            var rate = await _context.MstTravelAllowanceRates
                .Where(r => r.IsActive && r.DestinationZoneId == destinationZoneId.Value)
                .Where(r => !employeeGradeId.HasValue || r.EmployeeGradeId == employeeGradeId.Value || r.EmployeeGradeId == null)
                .OrderByDescending(r => r.Priority)
                .FirstOrDefaultAsync();

            if (rate != null && rate.RateAmount > 0)
            {
                return rate.RateAmount;
            }

            // Fallback default zonasi bila master belum di-seed
            var zone = await _context.MstTravelDestinationZones.FindAsync(destinationZoneId.Value);
            var zoneCode = zone?.DestinationZoneCode ?? "ZONA1";

            return zoneCode.ToUpper() switch
            {
                "ZONA3" => 1500000m, // Internasional
                "ZONA2" => 500000m,  // Luar Provinsi / Nasional
                _ => 250000m        // Lokal
            };
        }

        public async Task<decimal> CalculateTotalAllowanceAsync(Guid? destinationZoneId, Guid? employeeGradeId, int travelDayCount)
        {
            if (travelDayCount <= 0) travelDayCount = 1;
            var perDiemRate = await CalculatePerDiemRateAsync(destinationZoneId, employeeGradeId);
            return perDiemRate * travelDayCount;
        }

        public async Task<List<TravelAllowanceRateResponseDto>> GetRatesAsync(Guid? zoneId, Guid? gradeId)
        {
            var query = _context.MstTravelAllowanceRates
                .Include(r => r.DestinationZone)
                .Include(r => r.EmployeeGrade)
                .Where(r => r.IsActive);

            if (zoneId.HasValue) query = query.Where(r => r.DestinationZoneId == zoneId.Value);
            if (gradeId.HasValue) query = query.Where(r => r.EmployeeGradeId == gradeId.Value);

            var rates = await query.ToListAsync();

            if (!rates.Any())
            {
                // Fallback virtual rates jika belum di-seed di database
                return new List<TravelAllowanceRateResponseDto>
                {
                    new TravelAllowanceRateResponseDto
                    {
                        Id = Guid.NewGuid(),
                        DestinationZoneId = Guid.Empty,
                        ZoneName = "Zona 1 - Lokal",
                        AllowanceRateCode = "RATE-LOKAL-G1",
                        AllowanceRateName = "Uang Harian Dinas Lokal",
                        RateAmount = 250000m,
                        CurrencyCode = "IDR",
                        IsActive = true
                    },
                    new TravelAllowanceRateResponseDto
                    {
                        Id = Guid.NewGuid(),
                        DestinationZoneId = Guid.Empty,
                        ZoneName = "Zona 2 - Nasional",
                        AllowanceRateCode = "RATE-NAS-G1",
                        AllowanceRateName = "Uang Harian Dinas Nasional",
                        RateAmount = 500000m,
                        CurrencyCode = "IDR",
                        IsActive = true
                    },
                    new TravelAllowanceRateResponseDto
                    {
                        Id = Guid.NewGuid(),
                        DestinationZoneId = Guid.Empty,
                        ZoneName = "Zona 3 - Internasional",
                        AllowanceRateCode = "RATE-INTL-G1",
                        AllowanceRateName = "Uang Harian Dinas Internasional",
                        RateAmount = 1500000m,
                        CurrencyCode = "IDR",
                        IsActive = true
                    }
                };
            }

            return rates.Select(r => new TravelAllowanceRateResponseDto
            {
                Id = r.Id,
                DestinationZoneId = r.DestinationZoneId ?? Guid.Empty,
                ZoneName = r.DestinationZone?.DestinationZoneName ?? "Umum",
                EmployeeGradeId = r.EmployeeGradeId,
                GradeName = r.EmployeeGrade?.GradeName,
                AllowanceRateCode = r.AllowanceRateCode,
                AllowanceRateName = r.AllowanceRateName,
                RateAmount = r.RateAmount,
                CurrencyCode = r.CurrencyCode,
                IsActive = r.IsActive
            }).ToList();
        }

        public async Task<TravelAllowanceRateResponseDto> ConfigureRateAsync(ConfigureTravelRateDto dto, Guid currentUserId)
        {
            var zone = await _context.MstTravelDestinationZones.FindAsync(dto.DestinationZoneId);
            if (zone == null) throw new KeyNotFoundException("Zonasi tujuan tidak ditemukan");

            var defaultPolicy = await _context.MstTravelPolicies.FirstOrDefaultAsync();
            var defaultExpenseCategory = await _context.MstTravelExpenseCategories.FirstOrDefaultAsync();

            var rate = new MstTravelAllowanceRate
            {
                Id = Guid.NewGuid(),
                TravelPolicyId = defaultPolicy?.Id ?? Guid.NewGuid(),
                TravelExpenseCategoryId = defaultExpenseCategory?.Id ?? Guid.NewGuid(),
                DestinationZoneId = dto.DestinationZoneId,
                EmployeeGradeId = dto.EmployeeGradeId,
                AllowanceRateCode = dto.AllowanceRateCode,
                AllowanceRateName = dto.AllowanceRateName,
                RateAmount = dto.RateAmount,
                CurrencyCode = dto.CurrencyCode ?? "IDR",
                Description = dto.Description,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.MstTravelAllowanceRates.Add(rate);
            await _context.SaveChangesAsync();

            return new TravelAllowanceRateResponseDto
            {
                Id = rate.Id,
                DestinationZoneId = dto.DestinationZoneId,
                ZoneName = zone.DestinationZoneName,
                EmployeeGradeId = dto.EmployeeGradeId,
                AllowanceRateCode = rate.AllowanceRateCode,
                AllowanceRateName = rate.AllowanceRateName,
                RateAmount = rate.RateAmount,
                CurrencyCode = rate.CurrencyCode,
                IsActive = rate.IsActive
            };
        }

        public async Task<List<TravelDestinationZoneResponseDto>> GetZonesAsync()
        {
            var zones = await _context.MstTravelDestinationZones
                .Where(z => z.IsActive)
                .OrderBy(z => z.DestinationZoneCode)
                .ToListAsync();

            if (!zones.Any())
            {
                return new List<TravelDestinationZoneResponseDto>
                {
                    new TravelDestinationZoneResponseDto { Id = Guid.NewGuid(), ZoneCode = "ZONA1", ZoneName = "Zona 1 - Lokal / Dalam Provinsi", Description = "Perjalanan dinas antar-kota dalam provinsi yang sama", IsActive = true },
                    new TravelDestinationZoneResponseDto { Id = Guid.NewGuid(), ZoneCode = "ZONA2", ZoneName = "Zona 2 - Luar Provinsi / Nasional", Description = "Perjalanan dinas antar-provinsi di wilayah Indonesia", IsActive = true },
                    new TravelDestinationZoneResponseDto { Id = Guid.NewGuid(), ZoneCode = "ZONA3", ZoneName = "Zona 3 - Internasional", Description = "Perjalanan dinas rujukan atau konferensi medis internasional", IsActive = true }
                };
            }

            return zones.Select(z => new TravelDestinationZoneResponseDto
            {
                Id = z.Id,
                ZoneCode = z.DestinationZoneCode,
                ZoneName = z.DestinationZoneName,
                Description = z.Description,
                IsActive = z.IsActive
            }).ToList();
        }
    }
}
