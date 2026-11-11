using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services
{
    public interface IMissingReceiptDeclarationService
    {
        void ValidateMissingReceipts(List<MissingReceiptItemDto> missingReceipts, decimal totalActualAmount);
    }

    public class MissingReceiptDeclarationService : IMissingReceiptDeclarationService
    {
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
    }
}
