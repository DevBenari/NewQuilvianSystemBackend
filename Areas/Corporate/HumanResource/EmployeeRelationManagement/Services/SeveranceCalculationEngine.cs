using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class SeveranceCalculationEngine : ISeveranceCalculationEngine
    {
        private readonly IEmployeeRelationUnitOfWork _uow;

        public SeveranceCalculationEngine(IEmployeeRelationUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<SeveranceCalculationResponseDto> CalculateSeveranceAsync(CalculateSeveranceRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var separation = await _uow.Separations.FirstOrDefaultAsync(x => x.Id == request.SeparationId, cancellationToken);
            if (separation == null)
            {
                throw new KeyNotFoundException($"Proses terminasi dengan ID {request.SeparationId} tidak ditemukan.");
            }

            var employeeId = separation.EmployeeId ?? separation.WorkforceProfileId;
            var employee = await _uow.Employees.FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            var workforceProfile = await _uow.WorkforceProfiles.FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);

            // VR-EMP-20: Masa kerja dihitung dari HireDate sah ke EffectiveDate
            var hireDate = employee?.JoinDate ?? separation.RequestDate.AddYears(-3);
            var effectiveDate = separation.EffectiveSeparationDate;

            var totalMonths = Math.Max(0, ((effectiveDate.Year - hireDate.Year) * 12) + effectiveDate.Month - hireDate.Month);
            var serviceYears = totalMonths / 12;
            var serviceMonths = totalMonths % 12;

            decimal upAmount = 0m;
            decimal upmkAmount = 0m;
            decimal uphAmount = 0m;
            decimal uangPisahAmount = 0m;

            var wage = request.BaseMonthlyWage;
            var reason = separation.SeparationType;

            // Hitung UP Dasar (Pasal 40 ayat 2 PP 35/2021)
            decimal baseUpFactor = serviceYears switch
            {
                < 1 => 1m,
                1 => 2m,
                2 => 3m,
                3 => 4m,
                4 => 5m,
                5 => 6m,
                6 => 7m,
                7 => 8m,
                _ => 9m // Maksimal 9 bulan upah
            };

            // Hitung UPMK Dasar (Pasal 40 ayat 3 PP 35/2021)
            decimal baseUpmkFactor = serviceYears switch
            {
                < 3 => 0m,
                < 6 => 2m,
                < 9 => 3m,
                < 12 => 4m,
                < 15 => 5m,
                < 18 => 6m,
                < 21 => 7m,
                < 24 => 8m,
                _ => 10m // 24 tahun ke atas = 10 bulan upah
            };

            // Penerapan Pengali Alasan Pemutusan Hubungan Kerja (Pasal 43 s.d. 52 PP 35/2021)
            if (string.Equals(reason, "VoluntaryResign", StringComparison.OrdinalIgnoreCase))
            {
                // Resign Sukarela: UP = 0, UPMK = 0, UPH cuti + Uang Pisah
                upAmount = 0m;
                upmkAmount = 0m;
                uangPisahAmount = wage * 1m; // Sesuai PP/PKB RS MMC
            }
            else if (string.Equals(reason, "Retirement", StringComparison.OrdinalIgnoreCase))
            {
                // Pensiun Normal (Pasal 56): 1.75x UP + 1x UPMK
                upAmount = 1.75m * baseUpFactor * wage;
                upmkAmount = 1m * baseUpmkFactor * wage;
            }
            else if (string.Equals(reason, "ContractEnd", StringComparison.OrdinalIgnoreCase))
            {
                // Kompensasi PKWT (Pasal 15-17 PP 35/2021, VR-EMP-21): (serviceMonthsTotal / 12) * wage
                upAmount = 0m;
                upmkAmount = 0m;
                uangPisahAmount = Math.Round(((decimal)totalMonths / 12m) * wage, 2);
            }
            else if (string.Equals(reason, "DisciplinarySP3", StringComparison.OrdinalIgnoreCase))
            {
                // PHK Pelanggaran SP3 (Pasal 52 ayat 1): 0.5x UP + 1x UPMK
                upAmount = 0.5m * baseUpFactor * wage;
                upmkAmount = 1m * baseUpmkFactor * wage;
            }
            else
            {
                // Pelanggaran Berat / Efisiensi Default: 0.5x UP + 1x UPMK
                upAmount = 0.5m * baseUpFactor * wage;
                upmkAmount = 1m * baseUpmkFactor * wage;
            }

            // VR-EMP-22: UPH Cuti Tahunan = (UnusedLeaveDays * (BaseMonthlyWage / 21))
            if (request.UnusedLeaveDays > 0)
            {
                uphAmount = Math.Round(request.UnusedLeaveDays * (wage / 21m), 2);
            }

            var grossTotal = upAmount + upmkAmount + uphAmount + uangPisahAmount;

            // Hitung Pajak PPh 21 Final atas Pesangon (PP 68/2009)
            // 0 s.d 50 jt = 0%
            // 50 s.d 100 jt = 5%
            // 100 s.d 500 jt = 15%
            // > 500 jt = 25%
            decimal tax = 0m;
            if (grossTotal > 500_000_000m)
            {
                tax = (50_000_000m * 0.05m) + (400_000_000m * 0.15m) + ((grossTotal - 500_000_000m) * 0.25m);
            }
            else if (grossTotal > 100_000_000m)
            {
                tax = (50_000_000m * 0.05m) + ((grossTotal - 100_000_000m) * 0.15m);
            }
            else if (grossTotal > 50_000_000m)
            {
                tax = (grossTotal - 50_000_000m) * 0.05m;
            }

            var netTotal = grossTotal - tax;

            var existingCalc = await _uow.SeveranceCalculations.FirstOrDefaultAsync(x => x.SeparationId == request.SeparationId, cancellationToken);
            if (existingCalc == null)
            {
                existingCalc = new HrdSeveranceCalculation
                {
                    CalculationId = Guid.NewGuid(),
                    SeparationId = request.SeparationId,
                    CreateDateTime = DateTime.UtcNow
                };
                await _uow.AddSeveranceCalculationAsync(existingCalc, cancellationToken);
            }

            existingCalc.BaseMonthlyWage = wage;
            existingCalc.ServiceYears = serviceYears;
            existingCalc.ServiceMonths = serviceMonths;
            existingCalc.SeverancePayAmount = Math.Round(upAmount, 2);
            existingCalc.ServiceAwardAmount = Math.Round(upmkAmount, 2);
            existingCalc.RightsCompensationAmount = Math.Round(uphAmount, 2);
            existingCalc.SeparationAllowanceAmount = Math.Round(uangPisahAmount, 2);
            existingCalc.TotalSeveranceGross = Math.Round(grossTotal, 2);
            existingCalc.TaxDeduction = Math.Round(tax, 2);
            existingCalc.TotalSeveranceNet = Math.Round(netTotal, 2);
            existingCalc.Status = "Draft";
            existingCalc.IsApprovedByFinanceDirector = false;
            existingCalc.UpdateDateTime = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);

            return MapToDto(existingCalc);
        }

        public async Task<SeveranceCalculationDetailDto?> GetSeveranceBreakdownAsync(Guid separationId, CancellationToken cancellationToken = default)
        {
            var calc = await _uow.SeveranceCalculations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SeparationId == separationId, cancellationToken);

            if (calc == null) return null;

            var separation = await _uow.Separations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == separationId, cancellationToken);

            return new SeveranceCalculationDetailDto
            {
                CalculationId = calc.CalculationId,
                SeparationId = calc.SeparationId,
                BaseMonthlyWage = calc.BaseMonthlyWage,
                ServiceYears = calc.ServiceYears,
                ServiceMonths = calc.ServiceMonths,
                SeverancePayAmount = calc.SeverancePayAmount,
                ServiceAwardAmount = calc.ServiceAwardAmount,
                RightsCompensationAmount = calc.RightsCompensationAmount,
                SeparationAllowanceAmount = calc.SeparationAllowanceAmount,
                TotalSeveranceGross = calc.TotalSeveranceGross,
                TaxDeduction = calc.TaxDeduction,
                TotalSeveranceNet = calc.TotalSeveranceNet,
                Status = calc.Status,
                IsApprovedByFinanceDirector = calc.IsApprovedByFinanceDirector,
                SeparationReason = separation?.SeparationType ?? "Unknown",
                LegalReference = "PP No. 35 Tahun 2021 Pasal 40 s.d. 59 & PP No. 68 Tahun 2009 (PPh 21 Final)",
                CalculationFormulaNotes = $"Masa kerja sah {calc.ServiceYears} tahun {calc.ServiceMonths} bulan. UP: Rp {calc.SeverancePayAmount:N0}, UPMK: Rp {calc.ServiceAwardAmount:N0}, UPH: Rp {calc.RightsCompensationAmount:N0}, Uang Pisah: Rp {calc.SeparationAllowanceAmount:N0}.",
                FinanceApprovalNotes = calc.FinanceApprovalNotes,
                ApprovedAt = calc.ApprovedAt,
                CreatedAt = calc.CreateDateTime
            };
        }

        public async Task<SeveranceCalculationResponseDto> ApproveSeveranceAsync(Guid separationId, ApproveSeveranceRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var calc = await _uow.SeveranceCalculations.FirstOrDefaultAsync(x => x.SeparationId == separationId, cancellationToken);
            if (calc == null)
            {
                throw new KeyNotFoundException($"Lembar hitung pesangon untuk proses terminasi {separationId} tidak ditemukan.");
            }

            // VR-EMP-23: Otorisasi Direktur Keuangan sebelum pesangon disahkan
            if (!request.IsApproved)
            {
                calc.Status = "RejectedByFinanceDirector";
                calc.IsApprovedByFinanceDirector = false;
            }
            else
            {
                calc.Status = "ApprovedByFinanceDirector";
                calc.IsApprovedByFinanceDirector = true;
                calc.ApprovedAt = DateTime.UtcNow;
            }

            calc.FinanceApprovalNotes = request.Notes?.Trim();
            calc.UpdateDateTime = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);
            return MapToDto(calc);
        }

        private static SeveranceCalculationResponseDto MapToDto(HrdSeveranceCalculation entity)
        {
            return new SeveranceCalculationResponseDto
            {
                CalculationId = entity.CalculationId,
                SeparationId = entity.SeparationId,
                BaseMonthlyWage = entity.BaseMonthlyWage,
                ServiceYears = entity.ServiceYears,
                ServiceMonths = entity.ServiceMonths,
                SeverancePayAmount = entity.SeverancePayAmount,
                ServiceAwardAmount = entity.ServiceAwardAmount,
                RightsCompensationAmount = entity.RightsCompensationAmount,
                SeparationAllowanceAmount = entity.SeparationAllowanceAmount,
                TotalSeveranceGross = entity.TotalSeveranceGross,
                TaxDeduction = entity.TaxDeduction,
                TotalSeveranceNet = entity.TotalSeveranceNet,
                Status = entity.Status,
                IsApprovedByFinanceDirector = entity.IsApprovedByFinanceDirector,
                CreatedAt = entity.CreateDateTime
            };
        }
    }
}
