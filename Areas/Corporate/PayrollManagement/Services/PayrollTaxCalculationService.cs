using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollTaxCalculationService : IPayrollTaxCalculationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollTaxCalculationService> _logger;

        public PayrollTaxCalculationService(
            ApplicationDbContext context,
            ILogger<PayrollTaxCalculationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<CalculatePph21ResultDto>> CalculatePph21TerAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai kalkulasi pemotongan PPh 21 TER PMK 168/2023 untuk batch penggajian ID: {RunId}. Actor: {ActorId}",
                runId, actorUserId);

            var run = await _context.TrxPayrollRuns
                .Include(r => r.PayrollPeriod)
                .Include(r => r.Employees.Where(e => e.IsActive))
                    .ThenInclude(e => e.Components.Where(c => c.IsActive))
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<CalculatePph21ResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            if (run.IsLocked || run.RunStatus is "Approved" or "PaymentProcessing" or "Paid" or "Posted" or "Closed")
            {
                return PayrollServiceResult<CalculatePph21ResultDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian {run.RunNumber} telah berstatus '{run.RunStatus}' (Locked={run.IsLocked}) dan tidak dapat dimodifikasi.");
            }

            var employees = run.Employees.ToList();
            var profileIds = employees.Select(e => e.WorkforceProfileId).Distinct().ToList();

            // Ambil profil pajak master WfpTax untuk mengisi snapshot yang belum ada
            var wfpTaxes = await _context.WfpTaxes
                .Where(t => profileIds.Contains(t.WorkforceProfileId) && t.IsActive)
                .ToDictionaryAsync(t => t.WorkforceProfileId, cancellationToken);

            var isDecemberAdjustment = (run.PayrollPeriod?.PeriodNumber == 12) ||
                                       (run.PeriodEndDateSnapshot.Month == 12);

            var details = new List<EmployeePph21CalculationDto>();

            foreach (var emp in employees)
            {
                wfpTaxes.TryGetValue(emp.WorkforceProfileId, out var taxProfile);
                var dto = ProcessEmployeePph21(emp, taxProfile, isDecemberAdjustment, actorUserId);
                details.Add(dto);
            }

            // Agregasi Ringkasan Run
            run.TotalTax = employees.Sum(e => e.TotalTax);
            run.TotalDeduction = employees.Sum(e => e.TotalDeduction);
            run.TotalNetPay = employees.Sum(e => e.NetPay);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Kalkulasi PPh 21 TER PMK 168/2023 selesai untuk {Count} pegawai pada batch {RunNumber}. Total Pajak Terpotong: Rp {TotalTax:N2}, Total Gaji Bersih: Rp {TotalNet:N2}",
                employees.Count, run.RunNumber, run.TotalTax, run.TotalNetPay);

            return PayrollServiceResult<CalculatePph21ResultDto>.Ok(
                new CalculatePph21ResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    TotalEmployeesProcessed = employees.Count,
                    TotalGrossPay = run.TotalGrossPay,
                    TotalPph21Amount = run.TotalTax,
                    TotalNetPay = run.TotalNetPay,
                    Message = $"Kalkulasi pemotongan PPh 21 TER PMK 168/2023 untuk {employees.Count} pegawai berhasil diproses.",
                    Details = details
                },
                "Kalkulasi PPh 21 TER berhasil.");
        }

        public async Task<PayrollServiceResult<EmployeePph21CalculationDto>> CalculateEmployeePph21TerAsync(
            Guid runId,
            Guid employeeRunId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var emp = await _context.TrxPayrollRunEmployees
                .Include(e => e.Components.Where(c => c.IsActive))
                .Include(e => e.PayrollRun)
                    .ThenInclude(r => r!.PayrollPeriod)
                .FirstOrDefaultAsync(e => e.Id == employeeRunId && e.PayrollRunId == runId && e.IsActive, cancellationToken);

            if (emp == null || emp.PayrollRun == null)
            {
                return PayrollServiceResult<EmployeePph21CalculationDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Data pegawai penggajian ID {employeeRunId} pada batch {runId} tidak ditemukan.");
            }

            if (emp.PayrollRun.IsLocked || emp.PayrollRun.RunStatus is "Approved" or "Paid" or "Closed")
            {
                return PayrollServiceResult<EmployeePph21CalculationDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian telah berstatus final dan tidak dapat diperbarui.");
            }

            var taxProfile = await _context.WfpTaxes
                .FirstOrDefaultAsync(t => t.WorkforceProfileId == emp.WorkforceProfileId && t.IsActive, cancellationToken);

            var isDecemberAdjustment = (emp.PayrollRun.PayrollPeriod?.PeriodNumber == 12) ||
                                       (emp.PayrollRun.PeriodEndDateSnapshot.Month == 12);

            var result = ProcessEmployeePph21(emp, taxProfile, isDecemberAdjustment, actorUserId);

            // Perbarui agregasi batch penggajian
            var run = emp.PayrollRun;
            var allEmps = await _context.TrxPayrollRunEmployees
                .Where(e => e.PayrollRunId == runId && e.IsActive)
                .ToListAsync(cancellationToken);

            run.TotalTax = allEmps.Sum(e => e.TotalTax);
            run.TotalDeduction = allEmps.Sum(e => e.TotalDeduction);
            run.TotalNetPay = allEmps.Sum(e => e.NetPay);
            run.UpdateDateTime = DateTime.UtcNow;
            run.UpdateBy = actorUserId;

            await _context.SaveChangesAsync(cancellationToken);

            return PayrollServiceResult<EmployeePph21CalculationDto>.Ok(
                result,
                $"Kalkulasi pemotongan PPh 21 untuk {emp.EmployeeNameSnapshot} berhasil.");
        }

        private EmployeePph21CalculationDto ProcessEmployeePph21(
            TrxPayrollRunEmployee emp,
            WfpTax? taxProfile,
            bool isDecemberAdjustment,
            Guid actorUserId)
        {
            // 1. Sinkronisasi data PTKP dan NPWP
            if (string.IsNullOrWhiteSpace(emp.TaxStatusSnapshot))
            {
                emp.TaxStatusSnapshot = taxProfile?.TaxStatus ?? "TK/0";
            }
            if (string.IsNullOrWhiteSpace(emp.NpwpNumberSnapshot))
            {
                emp.NpwpNumberSnapshot = taxProfile?.NpwpNumber;
            }

            var ptkpStatus = (emp.TaxStatusSnapshot ?? "TK/0").Trim().ToUpperInvariant();
            var terCategory = DetermineTerCategory(ptkpStatus);

            // 2. Bersihkan komponen pajak PPh 21 sebelumnya
            var existingTaxComponents = emp.Components
                .Where(c => c.SourceType == "Tax" || c.ComponentCodeSnapshot == "DED_PPH21")
                .ToList();

            foreach (var comp in existingTaxComponents)
            {
                _context.TrxPayrollEmployeeComponents.Remove(comp);
            }

            // 3. Tentukan Persentase Tarif Pajak TER Bulanan / Akhir Tahun
            decimal terPercentage;
            decimal pph21Amount;

            if (isDecemberAdjustment)
            {
                // Penyesuaian Masa Pajak Desember: Tarif Progresif Pasal 17 UU PPh
                (terPercentage, pph21Amount) = CalculateDecemberPph21Adjustment(emp.GrossPay, ptkpStatus);
            }
            else
            {
                // Masa Pajak Bulanan Reguler (Januari - November): TER PMK 168/2023
                terPercentage = LookupTerPercentage(terCategory, emp.GrossPay);
                pph21Amount = Math.Round(emp.GrossPay * (terPercentage / 100m), MidpointRounding.AwayFromZero);
            }

            // 4. Buat komponen rincian pemotongan PPh 21
            var taxComp = new TrxPayrollEmployeeComponent
            {
                Id = Guid.NewGuid(),
                PayrollRunEmployeeId = emp.Id,
                SourceType = "Tax",
                ComponentCodeSnapshot = "DED_PPH21",
                ComponentNameSnapshot = isDecemberAdjustment
                    ? "Pemotongan PPh 21 Penyesuaian Akhir Tahun (Pasal 17)"
                    : $"Pemotongan PPh 21 TER Kategori {terCategory} ({terPercentage}%)",
                ComponentTypeSnapshot = "Deduction",
                CalculationMethodSnapshot = "Percentage",
                Percentage = terPercentage,
                BaseAmount = emp.GrossPay,
                Amount = pph21Amount,
                TaxableAmount = emp.GrossPay,
                CurrencyCode = "IDR",
                IsTaxable = false,
                IsDisplayedOnPayslip = true,
                SortOrder = 30,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxPayrollEmployeeComponents.Add(taxComp);

            // 5. Update Akumulasi Pajak, Potongan & Gaji Bersih Pegawai
            emp.TotalTax = pph21Amount;

            var otherDeductions = emp.Components
                .Where(c => c.IsActive && c.ComponentTypeSnapshot == "Deduction" && c.ComponentCodeSnapshot != "DED_PPH21")
                .Sum(c => c.Amount);

            emp.TotalDeduction = otherDeductions + pph21Amount;
            emp.NetPay = emp.GrossPay - emp.TotalDeduction;
            emp.PaymentAmount = emp.NetPay;
            emp.UpdateDateTime = DateTime.UtcNow;
            emp.UpdateBy = actorUserId;

            return new EmployeePph21CalculationDto
            {
                PayrollRunEmployeeId = emp.Id,
                WorkforceProfileId = emp.WorkforceProfileId,
                EmployeeName = emp.EmployeeNameSnapshot,
                PtkpStatus = ptkpStatus,
                TerCategory = terCategory,
                GrossPay = emp.GrossPay,
                TerPercentage = terPercentage,
                Pph21Amount = pph21Amount,
                TotalDeductions = emp.TotalDeduction,
                NetPay = emp.NetPay
            };
        }

        /// <summary>
        /// Menentukan Kategori TER (A, B, C) PMK 168/2023 berdasarkan status PTKP pegawai.
        /// </summary>
        private static string DetermineTerCategory(string ptkpStatus)
        {
            return ptkpStatus switch
            {
                "TK/0" or "TK0" or "TK/1" or "TK1" or "K/0" or "K0" => "A",
                "TK/2" or "TK2" or "TK/3" or "TK3" or "K/1" or "K1" or "K/2" or "K2" => "B",
                "K/3" or "K3" => "C",
                _ => "A" // Standar default TER A
            };
        }

        /// <summary>
        /// Tabel Lookup Tarif Efektif Rata-Rata (TER) Bulanan sesuai Lampiran PMK No. 168 Tahun 2023.
        /// </summary>
        private static decimal LookupTerPercentage(string category, decimal monthlyGross)
        {
            if (monthlyGross <= 0) return 0m;

            return category switch
            {
                "A" => LookupTerCategoryA(monthlyGross),
                "B" => LookupTerCategoryB(monthlyGross),
                "C" => LookupTerCategoryC(monthlyGross),
                _ => LookupTerCategoryA(monthlyGross)
            };
        }

        private static decimal LookupTerCategoryA(decimal gross)
        {
            if (gross <= 5400000m) return 0m;
            if (gross <= 5650000m) return 0.25m;
            if (gross <= 5950000m) return 0.50m;
            if (gross <= 6300000m) return 0.75m;
            if (gross <= 6750000m) return 1.00m;
            if (gross <= 7500000m) return 1.25m;
            if (gross <= 8550000m) return 1.50m;
            if (gross <= 9650000m) return 1.75m;
            if (gross <= 10050000m) return 2.00m;
            if (gross <= 10350000m) return 2.25m;
            if (gross <= 10700000m) return 2.50m;
            if (gross <= 11050000m) return 3.00m;
            if (gross <= 11600000m) return 3.50m;
            if (gross <= 12500000m) return 4.00m;
            if (gross <= 13750000m) return 5.00m;
            if (gross <= 15100000m) return 6.00m;
            if (gross <= 16950000m) return 7.00m;
            if (gross <= 19750000m) return 8.00m;
            if (gross <= 24100000m) return 9.00m;
            if (gross <= 26450000m) return 10.00m;
            if (gross <= 28000000m) return 11.00m;
            if (gross <= 30050000m) return 12.00m;
            if (gross <= 32400000m) return 13.00m;
            if (gross <= 35400000m) return 14.00m;
            if (gross <= 39100000m) return 15.00m;
            if (gross <= 43850000m) return 16.00m;
            if (gross <= 47800000m) return 17.00m;
            if (gross <= 51400000m) return 18.00m;
            if (gross <= 56300000m) return 19.00m;
            if (gross <= 62200000m) return 20.00m;
            if (gross <= 68600000m) return 21.00m;
            if (gross <= 77500000m) return 22.00m;
            if (gross <= 89000000m) return 23.00m;
            if (gross <= 101900000m) return 24.00m;
            if (gross <= 114000000m) return 25.00m;
            if (gross <= 126000000m) return 26.00m;
            if (gross <= 144000000m) return 27.00m;
            if (gross <= 178000000m) return 28.00m;
            if (gross <= 231000000m) return 29.00m;
            if (gross <= 314000000m) return 30.00m;
            if (gross <= 419000000m) return 31.00m;
            if (gross <= 558000000m) return 32.00m;
            if (gross <= 774000000m) return 33.00m;
            return 34.00m;
        }

        private static decimal LookupTerCategoryB(decimal gross)
        {
            if (gross <= 6200000m) return 0m;
            if (gross <= 6500000m) return 0.25m;
            if (gross <= 6850000m) return 0.50m;
            if (gross <= 7300000m) return 0.75m;
            if (gross <= 7800000m) return 1.00m;
            if (gross <= 8850000m) return 1.50m;
            if (gross <= 9800000m) return 2.00m;
            if (gross <= 10950000m) return 3.00m;
            if (gross <= 11200000m) return 4.00m;
            if (gross <= 12050000m) return 5.00m;
            if (gross <= 12950000m) return 6.00m;
            if (gross <= 14150000m) return 7.00m;
            if (gross <= 15550000m) return 8.00m;
            if (gross <= 17050000m) return 9.00m;
            if (gross <= 19500000m) return 10.00m;
            if (gross <= 22700000m) return 11.00m;
            if (gross <= 24700000m) return 12.00m;
            if (gross <= 26350000m) return 13.00m;
            if (gross <= 28000000m) return 14.00m;
            if (gross <= 30050000m) return 15.00m;
            if (gross <= 32400000m) return 16.00m;
            if (gross <= 35400000m) return 17.00m;
            if (gross <= 39100000m) return 18.00m;
            if (gross <= 43850000m) return 19.00m;
            if (gross <= 47800000m) return 20.00m;
            if (gross <= 51400000m) return 21.00m;
            if (gross <= 56300000m) return 22.00m;
            if (gross <= 62200000m) return 23.00m;
            if (gross <= 68600000m) return 24.00m;
            if (gross <= 77500000m) return 25.00m;
            if (gross <= 89000000m) return 26.00m;
            if (gross <= 101900000m) return 27.00m;
            if (gross <= 114000000m) return 28.00m;
            if (gross <= 126000000m) return 29.00m;
            if (gross <= 144000000m) return 30.00m;
            if (gross <= 178000000m) return 31.00m;
            if (gross <= 231000000m) return 32.00m;
            if (gross <= 314000000m) return 33.00m;
            return 34.00m;
        }

        private static decimal LookupTerCategoryC(decimal gross)
        {
            if (gross <= 6600000m) return 0m;
            if (gross <= 6950000m) return 0.25m;
            if (gross <= 7350000m) return 0.50m;
            if (gross <= 7800000m) return 0.75m;
            if (gross <= 8300000m) return 1.00m;
            if (gross <= 9350000m) return 1.50m;
            if (gross <= 10350000m) return 2.00m;
            if (gross <= 11200000m) return 3.00m;
            if (gross <= 12050000m) return 4.00m;
            if (gross <= 12950000m) return 5.00m;
            if (gross <= 14150000m) return 6.00m;
            if (gross <= 15550000m) return 7.00m;
            if (gross <= 17050000m) return 8.00m;
            if (gross <= 19500000m) return 9.00m;
            if (gross <= 22700000m) return 10.00m;
            if (gross <= 24700000m) return 11.00m;
            if (gross <= 26350000m) return 12.00m;
            if (gross <= 28000000m) return 13.00m;
            if (gross <= 30050000m) return 14.00m;
            if (gross <= 32400000m) return 15.00m;
            if (gross <= 35400000m) return 16.00m;
            if (gross <= 39100000m) return 17.00m;
            if (gross <= 43850000m) return 18.00m;
            if (gross <= 47800000m) return 19.00m;
            if (gross <= 51400000m) return 20.00m;
            if (gross <= 56300000m) return 21.00m;
            if (gross <= 62200000m) return 22.00m;
            if (gross <= 68600000m) return 23.00m;
            if (gross <= 77500000m) return 24.00m;
            if (gross <= 89000000m) return 25.00m;
            if (gross <= 101900000m) return 26.00m;
            if (gross <= 114000000m) return 27.00m;
            if (gross <= 126000000m) return 28.00m;
            if (gross <= 144000000m) return 29.00m;
            if (gross <= 178000000m) return 30.00m;
            if (gross <= 231000000m) return 31.00m;
            if (gross <= 314000000m) return 32.00m;
            if (gross <= 419000000m) return 33.00m;
            return 34.00m;
        }

        /// <summary>
        /// Kalkulasi penyesuaian PPh 21 masa Desember memakai skema tarif progresif Pasal 17 UU PPh (UU HPP).
        /// </summary>
        private static (decimal EffectivePercentage, decimal TaxAmount) CalculateDecemberPph21Adjustment(
            decimal monthlyGross,
            string ptkpStatus)
        {
            if (monthlyGross <= 0) return (0m, 0m);

            // Pro-rata proyeksi penghasilan bruto disetahunkan (12 bulan)
            var annualizedGross = monthlyGross * 12m;

            // Biaya Jabatan 5% dari bruto tahunan, maksimal Rp 6.000.000 setahun
            var occupationExpense = Math.Min(annualizedGross * 0.05m, 6000000m);

            var annualizedNetIncome = annualizedGross - occupationExpense;

            // Nilai PTKP Tahunan
            var ptkpAnnualAmount = GetAnnualPtkpAmount(ptkpStatus);

            // Penghasilan Kena Pajak (PKP) disetahunkan (dibulatkan ke ribuan penuh ke bawah)
            var taxableIncome = Math.Max(0m, annualizedNetIncome - ptkpAnnualAmount);
            taxableIncome = Math.Floor(taxableIncome / 1000m) * 1000m;

            if (taxableIncome <= 0) return (0m, 0m);

            // Perhitungan Lapisan Tarif Progresif Pasal 17 UU HPP:
            // 0 s.d. 60 jt : 5%
            // > 60 jt s.d. 250 jt : 15%
            // > 250 jt s.d. 500 jt : 25%
            // > 500 jt s.d. 5 Miliar : 30%
            // > 5 Miliar : 35%
            decimal annualTax = 0m;
            decimal remainingPkp = taxableIncome;

            // Lapisan 1 (0 - 60 jt @ 5%)
            var bracket1 = Math.Min(remainingPkp, 60000000m);
            annualTax += bracket1 * 0.05m;
            remainingPkp -= bracket1;

            // Lapisan 2 (60 - 250 jt @ 15%)
            if (remainingPkp > 0)
            {
                var bracket2 = Math.Min(remainingPkp, 190000000m);
                annualTax += bracket2 * 0.15m;
                remainingPkp -= bracket2;
            }

            // Lapisan 3 (250 - 500 jt @ 25%)
            if (remainingPkp > 0)
            {
                var bracket3 = Math.Min(remainingPkp, 250000000m);
                annualTax += bracket3 * 0.25m;
                remainingPkp -= bracket3;
            }

            // Lapisan 4 (500 jt - 5 Miliar @ 30%)
            if (remainingPkp > 0)
            {
                var bracket4 = Math.Min(remainingPkp, 4500000000m);
                annualTax += bracket4 * 0.30m;
                remainingPkp -= bracket4;
            }

            // Lapisan 5 (> 5 Miliar @ 35%)
            if (remainingPkp > 0)
            {
                annualTax += remainingPkp * 0.35m;
            }

            // PPh 21 Masa Desember proporsional
            var monthlyTax = Math.Round(annualTax / 12m, MidpointRounding.AwayFromZero);
            var effectiveRate = monthlyGross > 0
                ? Math.Round((monthlyTax / monthlyGross) * 100m, 2, MidpointRounding.AwayFromZero)
                : 0m;

            return (effectiveRate, monthlyTax);
        }

        private static decimal GetAnnualPtkpAmount(string ptkpStatus)
        {
            return ptkpStatus switch
            {
                "TK/0" or "TK0" => 54000000m,
                "TK/1" or "TK1" => 58500000m,
                "TK/2" or "TK2" => 63000000m,
                "TK/3" or "TK3" => 67500000m,
                "K/0" or "K0" => 58500000m,
                "K/1" or "K1" => 63000000m,
                "K/2" or "K2" => 67500000m,
                "K/3" or "K3" => 72000000m,
                _ => 54000000m // Default TK/0
            };
        }
    }
}
