using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IMissingReceiptDeclarationService
    {
        void ValidateMissingReceipts(List<MissingReceiptItemDto> missingReceipts, decimal totalActualAmount);

        /// <summary>Menambahkan butir tanpa nota ke context tanpa menyimpan; pemanggil yang memanggil SaveChanges.</summary>
        Task StageItemsAsync(TrxBusinessTravelRequest travel, IReadOnlyCollection<MissingReceiptItemDto> items, Guid actorUserId);

        Task<List<MissingReceiptDeclarationDto>> GetDeclarationsAsync(Guid settlementId);
        Task<MissingReceiptDeclarationDto> DeclareAsync(Guid settlementId, MissingReceiptItemDto item, Guid actorUserId);
        Task<MissingReceiptDeclarationDto> ApproveAsync(Guid itemId, Guid actorUserId);
    }

    /// <summary>
    /// Deklarasi biaya darurat tanpa nota (TRV-B5, HRD-DEC-192, VR-TRV-08, VR-TRV-09, BE-HRD-333).
    /// </summary>
    public class MissingReceiptDeclarationService : IMissingReceiptDeclarationService
    {
        private readonly ApplicationDbContext _context;
        private readonly NumberSeriesAllocator _numberAllocator;

        public MissingReceiptDeclarationService(ApplicationDbContext context, NumberSeriesAllocator numberAllocator)
        {
            _context = context;
            _numberAllocator = numberAllocator;
        }

        public void ValidateMissingReceipts(List<MissingReceiptItemDto> missingReceipts, decimal totalActualAmount)
        {
            if (missingReceipts == null || !missingReceipts.Any()) return;

            decimal totalMissingAmount = 0m;

            foreach (var item in missingReceipts)
            {
                // VR-TRV-08: Maksimal Rp 150.000 per transaksi
                if (item.ClaimedAmount > 150000m)
                {
                    throw new InvalidOperationException(
                        $"Item '{item.ExpenseDescription}' ({item.ClaimedAmount:N0} IDR) melebihi batas maksimal deklarasi tanpa kuitansi Rp 150.000 per transaksi (VR-TRV-08, HRD-DEC-192)");
                }

                // VR-TRV-09: Alasan darurat wajib diisi minimal 15 karakter
                if (string.IsNullOrWhiteSpace(item.EmergencyReason) || item.EmergencyReason.Trim().Length < 15)
                {
                    throw new InvalidOperationException(
                        $"Alasan darurat ketiadaan kuitansi untuk item '{item.ExpenseDescription}' wajib dijelaskan minimal 15 karakter (VR-TRV-09)");
                }

                totalMissingAmount += item.ClaimedAmount;
            }

            // VR-TRV-08: Akumulasi total deklarasi tanpa kuitansi dilarang melebihi 10% dari total klaim
            if (totalActualAmount > 0)
            {
                var maxAllowedMissing = totalActualAmount * 0.10m;
                if (totalMissingAmount > maxAllowedMissing)
                {
                    throw new InvalidOperationException(
                        $"Total deklarasi tanpa kuitansi ({totalMissingAmount:N0} IDR) melebihi ambang batas 10% ({maxAllowedMissing:N0} IDR) dari total pengeluaran dinas disetujui ({totalActualAmount:N0} IDR) sesuai aturan VR-TRV-08 & HRD-DEC-192");
                }
            }
        }

        public async Task StageItemsAsync(TrxBusinessTravelRequest travel, IReadOnlyCollection<MissingReceiptItemDto> items, Guid actorUserId)
        {
            if (items == null || items.Count == 0) return;

            var claim = await GetOrCreateClaimAsync(travel, actorUserId);
            foreach (var item in items)
            {
                var entity = await BuildItemAsync(claim.Id, item, actorUserId);
                _context.TrxTravelExpenseItems.Add(entity);
                claim.ClaimedAmount += item.ClaimedAmount;
            }
        }

        public async Task<List<MissingReceiptDeclarationDto>> GetDeclarationsAsync(Guid settlementId)
        {
            var settlement = await _context.TrxTravelSettlements.AsNoTracking().FirstOrDefaultAsync(s => s.Id == settlementId)
                ?? throw new KeyNotFoundException("Berkas settlement tidak ditemukan");

            var items = await _context.TrxTravelExpenseItems.AsNoTracking()
                .Where(i => !i.IsDelete && i.IsMissingReceiptDeclared && i.TravelExpenseClaim != null
                            && i.TravelExpenseClaim.BusinessTravelRequestId == settlement.BusinessTravelRequestId)
                .OrderBy(i => i.ExpenseDate)
                .ToListAsync();

            return items.Select(Map).ToList();
        }

        public async Task<MissingReceiptDeclarationDto> DeclareAsync(Guid settlementId, MissingReceiptItemDto item, Guid actorUserId)
        {
            var settlement = await _context.TrxTravelSettlements.Include(s => s.BusinessTravelRequest).FirstOrDefaultAsync(s => s.Id == settlementId)
                ?? throw new KeyNotFoundException("Berkas settlement tidak ditemukan");
            var travel = settlement.BusinessTravelRequest ?? throw new KeyNotFoundException("Permohonan perjalanan dinas tidak ditemukan");

            if (settlement.SettlementStatus != "Submitted")
            {
                throw new InvalidOperationException($"Deklarasi tanpa nota hanya dapat ditambahkan sebelum settlement diverifikasi. Status saat ini: {settlement.SettlementStatus}.");
            }

            var existing = await GetDeclarationsAsync(settlementId);
            var all = existing.Select(e => new MissingReceiptItemDto { ExpenseDate = e.ExpenseDate, ExpenseDescription = e.ExpenseDescription, ClaimedAmount = e.ClaimedAmount, EmergencyReason = e.EmergencyReason }).ToList();
            all.Add(item);
            var newTotal = settlement.ApprovedExpenseAmount + item.ClaimedAmount;
            ValidateMissingReceipts(all, newTotal);

            var claim = await GetOrCreateClaimAsync(travel, actorUserId);
            var entity = await BuildItemAsync(claim.Id, item, actorUserId);
            _context.TrxTravelExpenseItems.Add(entity);
            claim.ClaimedAmount += item.ClaimedAmount;

            var now = DateTime.UtcNow;
            settlement.ApprovedExpenseAmount = newTotal;
            settlement.SettledAmount = newTotal;
            settlement.SettlementDifferenceAmount = Math.Abs(newTotal - settlement.AdvancePaidAmount);
            settlement.EmployeeRefundAmount = newTotal < settlement.AdvancePaidAmount ? settlement.AdvancePaidAmount - newTotal : 0m;
            settlement.CompanyPayableAmount = newTotal > settlement.AdvancePaidAmount ? newTotal - settlement.AdvancePaidAmount : 0m;
            settlement.UpdateBy = actorUserId;
            settlement.UpdateDateTime = now;

            await _context.SaveChangesAsync();
            return Map(entity);
        }

        public async Task<MissingReceiptDeclarationDto> ApproveAsync(Guid itemId, Guid actorUserId)
        {
            var item = await _context.TrxTravelExpenseItems.FirstOrDefaultAsync(i => i.Id == itemId && !i.IsDelete && i.IsMissingReceiptDeclared)
                ?? throw new KeyNotFoundException("Deklarasi tanpa nota tidak ditemukan");

            if (item.MissingReceiptApprovedBy != null)
            {
                throw new InvalidOperationException("Deklarasi tanpa nota ini sudah disetujui.");
            }

            if (item.CreateBy == actorUserId)
            {
                throw new InvalidOperationException("Pengaju deklarasi tidak boleh menyetujui deklarasinya sendiri.");
            }

            item.MissingReceiptApprovedBy = actorUserId;
            item.ApprovedAmount = item.ClaimedAmount;
            item.ItemStatus = "Approved";
            item.UpdateBy = actorUserId;
            item.UpdateDateTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Map(item);
        }

        private async Task<TrxTravelExpenseClaim> GetOrCreateClaimAsync(TrxBusinessTravelRequest travel, Guid actorUserId)
        {
            var claim = _context.TrxTravelExpenseClaims.Local.FirstOrDefault(c => c.BusinessTravelRequestId == travel.Id && !c.IsDelete)
                ?? await _context.TrxTravelExpenseClaims.FirstOrDefaultAsync(c => c.BusinessTravelRequestId == travel.Id && !c.IsDelete);
            if (claim != null) return claim;

            var now = DateTime.UtcNow;
            var number = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: BusinessTravelNumberSeries.ExpenseClaimKey,
                Prefix: BusinessTravelNumberSeries.ExpenseClaimPrefix,
                ResetPolicy: BusinessTravelNumberSeries.ExpenseClaimResetPolicy,
                SequenceDigits: BusinessTravelNumberSeries.SequenceDigits,
                ActorUserId: actorUserId == Guid.Empty ? BusinessTravelNumberSeries.SystemFallbackUserId : actorUserId,
                Instant: DateTimeOffset.UtcNow));

            claim = new TrxTravelExpenseClaim
            {
                Id = Guid.NewGuid(),
                BusinessTravelRequestId = travel.Id,
                WorkforceProfileId = travel.WorkforceProfileId,
                EmployeeId = travel.EmployeeId,
                ExpenseClaimNumber = number,
                ClaimDate = DateOnly.FromDateTime(now),
                CurrencyCode = travel.CurrencyCode ?? "IDR",
                ClaimStatus = "Submitted",
                SubmittedAt = now,
                SubmittedByUserId = actorUserId,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = now
            };
            _context.TrxTravelExpenseClaims.Add(claim);
            return claim;
        }

        private async Task<TrxTravelExpenseItem> BuildItemAsync(Guid claimId, MissingReceiptItemDto item, Guid actorUserId)
        {
            var categoryId = item.TravelExpenseCategoryId
                ?? await _context.MstTravelExpenseCategories.AsNoTracking()
                    .Where(c => !c.IsDelete && c.AllowWithoutReceipt)
                    .OrderBy(c => c.TravelExpenseCategoryCode)
                    .Select(c => (Guid?)c.Id)
                    .FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("Pilih kategori biaya perjalanan. Belum ada kategori yang mengizinkan biaya tanpa nota pada master data.");

            return new TrxTravelExpenseItem
            {
                Id = Guid.NewGuid(),
                TravelExpenseClaimId = claimId,
                TravelExpenseCategoryId = categoryId,
                ExpenseDate = item.ExpenseDate,
                ExpenseDescription = item.ExpenseDescription,
                ClaimedAmount = item.ClaimedAmount,
                UnitAmount = item.ClaimedAmount,
                BaseCurrencyAmount = item.ClaimedAmount,
                HasReceipt = false,
                IsReceiptRequired = false,
                IsMissingReceiptDeclared = true,
                MissingReceiptReason = item.EmergencyReason?.Trim(),
                ItemStatus = "Submitted",
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };
        }

        private static MissingReceiptDeclarationDto Map(TrxTravelExpenseItem i) => new()
        {
            Id = i.Id,
            TravelExpenseCategoryId = i.TravelExpenseCategoryId,
            ExpenseDate = i.ExpenseDate,
            ExpenseDescription = i.ExpenseDescription,
            ClaimedAmount = i.ClaimedAmount,
            EmergencyReason = i.MissingReceiptReason ?? string.Empty,
            ItemStatus = i.ItemStatus,
            ApprovedByUserId = i.MissingReceiptApprovedBy,
            DeclaredByUserId = i.CreateBy
        };
    }
}
