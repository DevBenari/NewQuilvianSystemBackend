using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IDirectExpenseClaimService
    {
        Task<(List<ExpenseClaimResponseDto> Items, int TotalCount)> GetExpenseClaimsAsync(
            Guid? workforceProfileId, string? status, Guid? categoryId, DateOnly? startDate, DateOnly? endDate, int pageNumber, int pageSize);
        Task<ExpenseClaimDetailResponseDto> GetExpenseClaimByIdAsync(Guid id);
        Task<ExpenseClaimResponseDto> CreateExpenseClaimAsync(CreateExpenseClaimDto dto, Guid currentUserId, Guid currentWorkforceProfileId);
    }

    public class DirectExpenseClaimService : IDirectExpenseClaimService
    {
        private readonly ApplicationDbContext _context;

        public DirectExpenseClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<ExpenseClaimResponseDto> Items, int TotalCount)> GetExpenseClaimsAsync(
            Guid? workforceProfileId, string? status, Guid? categoryId, DateOnly? startDate, DateOnly? endDate, int pageNumber, int pageSize)
        {
            var query = _context.TrxExpenseClaims
                .Include(c => c.WorkforceProfile)
                .Where(c => c.IsActive);

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                query = query.Where(c => c.WorkforceProfileId == workforceProfileId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.ClaimStatus.ToLower() == status.ToLower());
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.ClaimDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.ClaimDate <= endDate.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ExpenseClaimResponseDto
                {
                    Id = c.Id,
                    ClaimNumber = c.ClaimNumber,
                    WorkforceProfileId = c.WorkforceProfileId,
                    EmployeeName = c.WorkforceProfile != null ? c.WorkforceProfile.DisplayName : "Pegawai Rumah Sakit",
                    ClaimTitle = c.ClaimTitle,
                    ClaimType = c.ClaimType,
                    ClaimDate = c.ClaimDate,
                    TotalClaimedAmount = c.TotalClaimedAmount,
                    TotalApprovedAmount = c.TotalApprovedAmount,
                    TotalPaidAmount = c.TotalPaidAmount,
                    CurrencyCode = c.CurrencyCode,
                    ClaimStatus = c.ClaimStatus,
                    CreatedAt = c.CreateDateTime
                })
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<ExpenseClaimDetailResponseDto> GetExpenseClaimByIdAsync(Guid id)
        {
            var c = await _context.TrxExpenseClaims
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Department)
                .Include(x => x.CostCenter)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) throw new KeyNotFoundException("Klaim pengeluaran operasional non-trip tidak ditemukan");

            return new ExpenseClaimDetailResponseDto
            {
                Id = c.Id,
                ClaimNumber = c.ClaimNumber,
                WorkforceProfileId = c.WorkforceProfileId,
                EmployeeName = c.WorkforceProfile?.DisplayName ?? "Pegawai Rumah Sakit",
                ClaimTitle = c.ClaimTitle,
                ClaimType = c.ClaimType,
                ClaimDate = c.ClaimDate,
                TotalClaimedAmount = c.TotalClaimedAmount,
                TotalApprovedAmount = c.TotalApprovedAmount,
                TotalPaidAmount = c.TotalPaidAmount,
                CurrencyCode = c.CurrencyCode,
                ClaimStatus = c.ClaimStatus,
                ClaimDescription = c.ClaimDescription,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.Department?.DepartmentName,
                CostCenterId = c.CostCenterId,
                CostCenterName = c.CostCenter?.CostCenterName,
                CreatedAt = c.CreateDateTime,
                Items = c.Items.Select(i => new CreateExpenseClaimItemDto
                {
                    ExpenseCategoryId = i.ExpenseCategoryId,
                    ExpenseDate = i.TransactionDate,
                    ExpenseDescription = i.Description,
                    MerchantName = i.MerchantName,
                    ReceiptNumber = i.MerchantTaxNumber,
                    ClaimedAmount = i.ClaimedAmount,
                    ReceiptFilePath = null
                }).ToList()
            };
        }

        public async Task<ExpenseClaimResponseDto> CreateExpenseClaimAsync(CreateExpenseClaimDto dto, Guid currentUserId, Guid currentWorkforceProfileId)
        {
            if (dto.Items == null || !dto.Items.Any())
            {
                throw new InvalidOperationException("Pengajuan klaim non-trip wajib memiliki minimal 1 rincian item pengeluaran");
            }

            var now = DateTime.UtcNow;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // VR-TRV-16: Kuitansi pembelian non-trip wajib memiliki tanggal dalam bulan berjalan
            foreach (var item in dto.Items)
            {
                if (item.ExpenseDate.Year != currentYear || item.ExpenseDate.Month != currentMonth)
                {
                    // Toleransi bulan lalu hanya jika <= H+3 hari awal bulan
                    var isEarlyMonthGrace = now.Day <= 3 &&
                        ((item.ExpenseDate.Month == currentMonth - 1 && item.ExpenseDate.Year == currentYear) ||
                         (currentMonth == 1 && item.ExpenseDate.Month == 12 && item.ExpenseDate.Year == currentYear - 1));

                    if (!isEarlyMonthGrace)
                    {
                        throw new InvalidOperationException(
                            $"Item pengeluaran '{item.ExpenseDescription}' ({item.ExpenseDate:dd/MM/yyyy}) di luar batas bulan berjalan (VR-TRV-16, HRD-DEC-188)");
                    }
                }
            }

            var totalClaim = dto.Items.Sum(i => i.ClaimedAmount);
            var seq = await _context.TrxExpenseClaims.CountAsync() + 1;
            var claimNumber = $"EXP/RSQ/{now:yyyy}/{now:MM}/{seq:D4}";

            var defaultExpenseCategory = await _context.MstExpenseCategories.FirstOrDefaultAsync();

            var claim = new TrxExpenseClaim
            {
                Id = Guid.NewGuid(),
                ClaimNumber = claimNumber,
                WorkforceProfileId = currentWorkforceProfileId,
                DepartmentId = dto.DepartmentId,
                CostCenterId = dto.CostCenterId,
                ClaimType = dto.ClaimType,
                ClaimTitle = dto.ClaimTitle,
                ClaimDescription = dto.ClaimDescription,
                ClaimDate = DateOnly.FromDateTime(now),
                CurrencyCode = "IDR",
                TotalClaimedAmount = totalClaim,
                TotalApprovedAmount = totalClaim, // Standar awal mengikuti pengajuan
                TotalPaidAmount = 0m,
                ClaimStatus = "Submitted",
                SubmittedAt = now,
                SubmittedByUserId = currentUserId,
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = now
            };

            foreach (var itemDto in dto.Items)
            {
                var itemEntity = new TrxExpenseClaimItem
                {
                    Id = Guid.NewGuid(),
                    ExpenseClaimId = claim.Id,
                    ExpenseCategoryId = itemDto.ExpenseCategoryId ?? defaultExpenseCategory?.Id ?? Guid.NewGuid(),
                    CostCenterId = dto.CostCenterId,
                    TransactionDate = itemDto.ExpenseDate,
                    Description = itemDto.ExpenseDescription,
                    MerchantName = itemDto.MerchantName,
                    MerchantTaxNumber = itemDto.ReceiptNumber,
                    ClaimedAmount = itemDto.ClaimedAmount,
                    ApprovedAmount = itemDto.ClaimedAmount,
                    CurrencyCode = "IDR",
                    RequiresReceipt = true,
                    HasReceipt = !string.IsNullOrWhiteSpace(itemDto.ReceiptFilePath),
                    ItemStatus = "Submitted",
                    IsActive = true,
                    CreateBy = currentUserId,
                    CreateDateTime = now
                };
                claim.Items.Add(itemEntity);
            }

            _context.TrxExpenseClaims.Add(claim);
            await _context.SaveChangesAsync();

            return new ExpenseClaimResponseDto
            {
                Id = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                WorkforceProfileId = claim.WorkforceProfileId,
                EmployeeName = "Pegawai Pemohon",
                ClaimTitle = claim.ClaimTitle,
                ClaimType = claim.ClaimType,
                ClaimDate = claim.ClaimDate,
                TotalClaimedAmount = claim.TotalClaimedAmount,
                TotalApprovedAmount = claim.TotalApprovedAmount,
                TotalPaidAmount = claim.TotalPaidAmount,
                CurrencyCode = claim.CurrencyCode,
                ClaimStatus = claim.ClaimStatus,
                CreatedAt = claim.CreateDateTime
            };
        }
    }
}
