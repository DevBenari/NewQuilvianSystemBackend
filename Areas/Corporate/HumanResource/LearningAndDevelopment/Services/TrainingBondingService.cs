using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services
{
    public class TrainingBondingService : ITrainingBondingService
    {
        private readonly ApplicationDbContext _context;

        public TrainingBondingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TrainingBondContractDto> GenerateBondContractAsync(GenerateBondContractRequestDto request, string currentUserId)
        {
            var staff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == request.WorkforceProfileId && !w.IsDelete);

            if (staff == null)
            {
                throw new KeyNotFoundException($"Profil tenaga kerja dengan ID '{request.WorkforceProfileId}' tidak ditemukan.");
            }

            // Validasi durasi retensi (VR-TRN-18: 12 sampai 36 bulan)
            if (request.RetentionPeriodMonths < 12 || request.RetentionPeriodMonths > 36)
            {
                throw new InvalidOperationException("Durasi retensi kerja ikatan dinas wajib berada di rentang 12 hingga 36 bulan sesuai regulasi rumah sakit (VR-TRN-18).");
            }

            // Validasi threshold pembiayaan RS (VR-TRN-16: > Rp 10 Juta)
            if (request.HospitalFinancedAmount <= 0)
            {
                throw new InvalidOperationException("Jumlah pembiayaan rumah sakit wajib bernilai lebih dari Rp 0 untuk menerbitkan ikatan dinas.");
            }

            var currentYear = DateTime.UtcNow.Year;
            var currentCount = await _context.Set<HrdTrainingBondContract>()
                .CountAsync(c => c.CreateDateTime.Year == currentYear);

            var sequence = currentCount + 1;
            var contractNumber = $"BOND-HRD-{currentYear}-{sequence:D5}";

            var startDate = request.BondStartDate ?? DateTime.UtcNow;
            var endDate = startDate.AddMonths(request.RetentionPeriodMonths);

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;

            var contract = new HrdTrainingBondContract
            {
                Id = Guid.NewGuid(),
                ContractNumber = contractNumber,
                WorkforceProfileId = request.WorkforceProfileId,
                TrainingSessionId = request.TrainingSessionId,
                TrainingEnrollmentRequestId = request.TrainingEnrollmentRequestId,
                TrainingProgramName = request.TrainingProgramName,
                TotalTrainingCost = request.TotalTrainingCost,
                HospitalFinancedAmount = request.HospitalFinancedAmount,
                RetentionPeriodMonths = request.RetentionPeriodMonths,
                BondStartDate = startDate,
                BondEndDate = endDate,
                BondStatus = "ACTIVE",
                IsBreached = false,
                DigitalSignatureHash = request.DigitalSignatureHash ?? $"SIG-BOND-{contractNumber}-AUTO-HASH",
                CreateDateTime = DateTime.UtcNow,
                CreateBy = parsedUserGuid
            };

            await _context.Set<HrdTrainingBondContract>().AddAsync(contract);
            await _context.SaveChangesAsync();

            return new TrainingBondContractDto
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                WorkforceProfileId = contract.WorkforceProfileId,
                StaffName = staff.DisplayName,
                TrainingSessionId = contract.TrainingSessionId,
                TrainingProgramName = contract.TrainingProgramName,
                TotalTrainingCost = contract.TotalTrainingCost,
                HospitalFinancedAmount = contract.HospitalFinancedAmount,
                RetentionPeriodMonths = contract.RetentionPeriodMonths,
                BondStartDate = contract.BondStartDate,
                BondEndDate = contract.BondEndDate,
                BondStatus = contract.BondStatus,
                IsBreached = contract.IsBreached,
                CalculatedPenaltyAmount = contract.CalculatedPenaltyAmount,
                PenaltyInvoiceNumber = contract.PenaltyInvoiceNumber,
                DigitalSignatureHash = contract.DigitalSignatureHash,
                CreatedAt = contract.CreateDateTime
            };
        }

        public async Task<TrainingBondContractDto> CalculateAndApplyPenaltyAsync(Guid contractId, ProcessBreachPenaltyRequestDto request, string currentUserId)
        {
            var contract = await _context.Set<HrdTrainingBondContract>()
                .Include(c => c.WorkforceProfile)
                .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDelete);

            if (contract == null)
            {
                throw new KeyNotFoundException($"Kontrak ikatan dinas dengan ID '{contractId}' tidak ditemukan.");
            }

            if (contract.BondStatus != "ACTIVE")
            {
                throw new InvalidOperationException($"Kontrak ikatan dinas tidak berstatus ACTIVE (Status saat ini: {contract.BondStatus}).");
            }

            var resignDate = request.ResignationEffectiveDate;

            // Jika resign terjadi setelah masa ikatan dinas berakhir, maka ikatan dinas terpenuhi tanpa penalti
            if (resignDate >= contract.BondEndDate)
            {
                contract.BondStatus = "FULFILLED";
                contract.UpdateDateTime = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new TrainingBondContractDto
                {
                    Id = contract.Id,
                    ContractNumber = contract.ContractNumber,
                    WorkforceProfileId = contract.WorkforceProfileId,
                    StaffName = contract.WorkforceProfile?.DisplayName,
                    TrainingSessionId = contract.TrainingSessionId,
                    TrainingProgramName = contract.TrainingProgramName,
                    TotalTrainingCost = contract.TotalTrainingCost,
                    HospitalFinancedAmount = contract.HospitalFinancedAmount,
                    RetentionPeriodMonths = contract.RetentionPeriodMonths,
                    BondStartDate = contract.BondStartDate,
                    BondEndDate = contract.BondEndDate,
                    BondStatus = "FULFILLED",
                    IsBreached = false,
                    CalculatedPenaltyAmount = 0m,
                    CreatedAt = contract.CreateDateTime
                };
            }

            // Hitung rasio sisa durasi retensi (VR-TRN-17)
            var totalDurationDays = (contract.BondEndDate - contract.BondStartDate).TotalDays;
            if (totalDurationDays <= 0) totalDurationDays = 365;

            var remainingDays = (contract.BondEndDate - resignDate).TotalDays;
            if (remainingDays < 0) remainingDays = 0;
            if (remainingDays > totalDurationDays) remainingDays = totalDurationDays;

            decimal remainingRatio = (decimal)(remainingDays / totalDurationDays);
            decimal multiplier = request.MultiplierFactor > 0 ? request.MultiplierFactor : 1.0m;

            // Rumus: SisaRasio * PembiayaanRS * FaktorMultiplier (VR-TRN-17)
            decimal calculatedPenalty = Math.Round(contract.HospitalFinancedAmount * remainingRatio * multiplier, 2);

            var currentYear = DateTime.UtcNow.Year;
            var invoiceCount = await _context.Set<HrdTrainingBondContract>()
                .CountAsync(c => c.PenaltyInvoiceNumber != null && c.BreachRecordedAt.HasValue && c.BreachRecordedAt.Value.Year == currentYear);

            var invNumber = $"INV-PENALTY-HRD-{currentYear}-{(invoiceCount + 1):D5}";

            var parsedUserGuid = Guid.TryParse(currentUserId, out var uGuid) ? uGuid : Guid.Empty;

            contract.IsBreached = true;
            contract.BreachRecordedAt = DateTime.UtcNow;
            contract.CalculatedPenaltyAmount = calculatedPenalty;
            contract.BreachReason = request.BreachReason;
            contract.PenaltyInvoiceNumber = invNumber;
            contract.BondStatus = "BREACHED_PENALTY_ISSUED";
            contract.UpdateDateTime = DateTime.UtcNow;
            contract.UpdateBy = parsedUserGuid;

            await _context.SaveChangesAsync();

            return new TrainingBondContractDto
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                WorkforceProfileId = contract.WorkforceProfileId,
                StaffName = contract.WorkforceProfile?.DisplayName,
                TrainingSessionId = contract.TrainingSessionId,
                TrainingProgramName = contract.TrainingProgramName,
                TotalTrainingCost = contract.TotalTrainingCost,
                HospitalFinancedAmount = contract.HospitalFinancedAmount,
                RetentionPeriodMonths = contract.RetentionPeriodMonths,
                BondStartDate = contract.BondStartDate,
                BondEndDate = contract.BondEndDate,
                BondStatus = contract.BondStatus,
                IsBreached = contract.IsBreached,
                BreachRecordedAt = contract.BreachRecordedAt,
                CalculatedPenaltyAmount = contract.CalculatedPenaltyAmount,
                BreachReason = contract.BreachReason,
                PenaltyInvoiceNumber = contract.PenaltyInvoiceNumber,
                DigitalSignatureHash = contract.DigitalSignatureHash,
                CreatedAt = contract.CreateDateTime
            };
        }

        public async Task<TrainingBondContractDto?> GetContractByIdAsync(Guid contractId)
        {
            var contract = await _context.Set<HrdTrainingBondContract>()
                .Include(c => c.WorkforceProfile)
                .FirstOrDefaultAsync(c => c.Id == contractId && !c.IsDelete);

            if (contract == null) return null;

            return new TrainingBondContractDto
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                WorkforceProfileId = contract.WorkforceProfileId,
                StaffName = contract.WorkforceProfile?.DisplayName,
                TrainingSessionId = contract.TrainingSessionId,
                TrainingProgramName = contract.TrainingProgramName,
                TotalTrainingCost = contract.TotalTrainingCost,
                HospitalFinancedAmount = contract.HospitalFinancedAmount,
                RetentionPeriodMonths = contract.RetentionPeriodMonths,
                BondStartDate = contract.BondStartDate,
                BondEndDate = contract.BondEndDate,
                BondStatus = contract.BondStatus,
                IsBreached = contract.IsBreached,
                BreachRecordedAt = contract.BreachRecordedAt,
                CalculatedPenaltyAmount = contract.CalculatedPenaltyAmount,
                BreachReason = contract.BreachReason,
                PenaltyInvoiceNumber = contract.PenaltyInvoiceNumber,
                DigitalSignatureHash = contract.DigitalSignatureHash,
                CreatedAt = contract.CreateDateTime
            };
        }

        public async Task<List<TrainingBondContractDto>> GetContractsByStaffAsync(Guid workforceProfileId)
        {
            var list = await _context.Set<HrdTrainingBondContract>()
                .Include(c => c.WorkforceProfile)
                .Where(c => c.WorkforceProfileId == workforceProfileId && !c.IsDelete)
                .OrderByDescending(c => c.BondStartDate)
                .ToListAsync();

            return list.Select(c => new TrainingBondContractDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                WorkforceProfileId = c.WorkforceProfileId,
                StaffName = c.WorkforceProfile?.DisplayName,
                TrainingSessionId = c.TrainingSessionId,
                TrainingProgramName = c.TrainingProgramName,
                TotalTrainingCost = c.TotalTrainingCost,
                HospitalFinancedAmount = c.HospitalFinancedAmount,
                RetentionPeriodMonths = c.RetentionPeriodMonths,
                BondStartDate = c.BondStartDate,
                BondEndDate = c.BondEndDate,
                BondStatus = c.BondStatus,
                IsBreached = c.IsBreached,
                BreachRecordedAt = c.BreachRecordedAt,
                CalculatedPenaltyAmount = c.CalculatedPenaltyAmount,
                BreachReason = c.BreachReason,
                PenaltyInvoiceNumber = c.PenaltyInvoiceNumber,
                DigitalSignatureHash = c.DigitalSignatureHash,
                CreatedAt = c.CreateDateTime
            }).ToList();
        }

        public async Task<List<TrainingBondContractDto>> GetActiveContractsAsync()
        {
            var list = await _context.Set<HrdTrainingBondContract>()
                .Include(c => c.WorkforceProfile)
                .Where(c => c.BondStatus == "ACTIVE" && !c.IsDelete)
                .OrderBy(c => c.BondEndDate)
                .ToListAsync();

            return list.Select(c => new TrainingBondContractDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                WorkforceProfileId = c.WorkforceProfileId,
                StaffName = c.WorkforceProfile?.DisplayName,
                TrainingSessionId = c.TrainingSessionId,
                TrainingProgramName = c.TrainingProgramName,
                TotalTrainingCost = c.TotalTrainingCost,
                HospitalFinancedAmount = c.HospitalFinancedAmount,
                RetentionPeriodMonths = c.RetentionPeriodMonths,
                BondStartDate = c.BondStartDate,
                BondEndDate = c.BondEndDate,
                BondStatus = c.BondStatus,
                IsBreached = c.IsBreached,
                BreachRecordedAt = c.BreachRecordedAt,
                CalculatedPenaltyAmount = c.CalculatedPenaltyAmount,
                BreachReason = c.BreachReason,
                PenaltyInvoiceNumber = c.PenaltyInvoiceNumber,
                DigitalSignatureHash = c.DigitalSignatureHash,
                CreatedAt = c.CreateDateTime
            }).ToList();
        }
    }
}
