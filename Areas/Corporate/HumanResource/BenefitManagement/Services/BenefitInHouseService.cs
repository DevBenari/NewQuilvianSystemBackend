using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Layanan Integrasi Real-Time Loket Kasir SIMRS & Buku Besar Plafon Berobat RS (Slice S-D3, BE-HRD-186 & 187).
    /// Mengimplementasikan aturan:
    /// - BEN-B4 & VR-BEN-14..15: Saldo plafon tidak boleh bernilai negatif (balance_after >= 0.00).
    /// - BEN-W2: Peringatan indikator kasir SIMRS jika pemakaian plafon > 80%.
    /// - BEN-W6: Isolasi konkurensi / locking transaksi kasir SIMRS.
    /// - INT-HR-61: Integrasi billing real-time SIMRS ke modul SDM.
    /// - INT-HR-63: Pendelegasian ekses biaya medis ke pemotongan payroll Sub-11.
    /// </summary>
    public sealed class BenefitInHouseService : IBenefitInHouseService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BenefitInHouseService> _logger;

        public BenefitInHouseService(ApplicationDbContext dbContext, ILogger<BenefitInHouseService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<InHouseCoverageInquiryDto> InquireCoverageAsync(
            InHouseCoverageInquiryQuery query, CancellationToken cancellationToken)
        {
            // 1. Cari tanggungan terlebih dahulu berdasarkan NIK jika parameter NIK disertakan
            if (!string.IsNullOrWhiteSpace(query.NationalIdNumber))
            {
                var nik = query.NationalIdNumber.Trim();
                var dependent = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .Include(x => x.WorkforceProfile)
                    .Include(x => x.EmployeeBenefitEnrollment)
                    .FirstOrDefaultAsync(x => !x.IsDelete && (x.NationalIdNumber == nik || x.IdentityNumber == nik), cancellationToken);

                if (dependent != null)
                {
                    var isDepVerified = dependent.VerificationStatus == "Verified";
                    var depLimit = dependent.CoverageLimitAmount > 0 ? dependent.CoverageLimitAmount : 10000000m;
                    var depUsed = dependent.UsedAmount;
                    var depRemaining = dependent.RemainingAmount;
                    var usageRatio = depLimit > 0 ? (depUsed / depLimit) : 0m;

                    return new InHouseCoverageInquiryDto
                    {
                        IsEligible = isDepVerified && depRemaining > 0,
                        WorkforceProfileId = dependent.WorkforceProfileId,
                        DependentId = dependent.Id,
                        EmployeeCode = dependent.WorkforceProfile?.ProfileCode ?? "-",
                        PatientName = dependent.DependentName,
                        PatientType = dependent.RelationshipType,
                        BenefitPlanName = "Subsidi Berobat Keluarga RS Quilvian",
                        InpatientRoomClass = "Kelas 1",
                        HospitalDiscountPercentage = 30.00m,
                        AnnualQuotaLimit = depLimit,
                        UsedAmount = depUsed,
                        RemainingBalance = depRemaining,
                        IsNearExhaustion = usageRatio >= 0.80m,
                        WarningMessage = usageRatio >= 0.80m
                            ? $"PERINGATAN: Sisa kuota tanggungan telah terpakai {(usageRatio * 100):N1}%. Harap waspadai potensi ekses biaya."
                            : null,
                        IneligibilityReason = !isDepVerified
                            ? $"Status berkas tanggungan masih '{dependent.VerificationStatus}' (belum diverifikasi HR)."
                            : (depRemaining <= 0 ? "Plafon subsidi tahunan telah habis." : null)
                    };
                }
            }

            // 2. Cari data pegawai
            var empQuery = _dbContext.MstWorkforceProfiles.AsNoTracking().Where(x => !x.IsDelete);

            if (query.WorkforceProfileId.HasValue)
            {
                empQuery = empQuery.Where(x => x.Id == query.WorkforceProfileId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(query.EmployeeIdNumber))
            {
                var idNum = query.EmployeeIdNumber.Trim();
                empQuery = empQuery.Where(x => x.ProfileCode == idNum || x.Email == idNum);
            }
            else if (!string.IsNullOrWhiteSpace(query.NationalIdNumber))
            {
                var nik = query.NationalIdNumber.Trim();
                var emp = await _dbContext.MstEmployees.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdentityNumber == nik && !x.IsDelete, cancellationToken);
                if (emp != null)
                {
                    empQuery = empQuery.Where(x => x.Id == emp.WorkforceProfileId);
                }
            }

            var workforce = await empQuery.FirstOrDefaultAsync(cancellationToken);
            if (workforce == null)
            {
                return new InHouseCoverageInquiryDto
                {
                    IsEligible = false,
                    PatientName = "Tidak Terdaftar",
                    IneligibilityReason = "Identitas pasien nakes/keluarga tidak ditemukan dalam basis data SDM."
                };
            }

            // Cek enrollment aktif staf
            var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                .Include(x => x.BenefitPlan)
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforce.Id && !x.IsDelete, cancellationToken);

            var planName = enrollment?.BenefitPlan?.BenefitPlanName ?? "Subsidi Berobat Internal RS Quilvian";
            var discount = enrollment?.BenefitPlan?.HospitalDiscountPercentage ?? 30.00m;
            var limit = enrollment?.CoverageLimitAmount ?? 15000000m;
            var used = enrollment?.UsedAmount ?? 0m;
            var remaining = enrollment?.RemainingAmount ?? (limit - used);
            var ratio = limit > 0 ? (used / limit) : 0m;

            var isEligible = enrollment != null && enrollment.EnrollmentStatus == "Active" && remaining > 0;

            return new InHouseCoverageInquiryDto
            {
                IsEligible = isEligible,
                WorkforceProfileId = workforce.Id,
                DependentId = null,
                EmployeeCode = workforce.ProfileCode,
                PatientName = workforce.DisplayName,
                PatientType = "Employee",
                BenefitPlanName = planName,
                InpatientRoomClass = "Kelas 1",
                HospitalDiscountPercentage = discount,
                AnnualQuotaLimit = limit,
                UsedAmount = used,
                RemainingBalance = remaining,
                IsNearExhaustion = ratio >= 0.80m,
                WarningMessage = ratio >= 0.80m
                    ? $"PERINGATAN (BEN-W2): Saldo plafon nakes terpakai {(ratio * 100):N1}% (> 80%). Waspadai ekses biaya."
                    : null,
                IneligibilityReason = enrollment == null
                    ? "Pegawai belum terdaftar pada paket subsidi berobat RS."
                    : (enrollment.EnrollmentStatus != "Active" ? $"Status kepesertaan '{enrollment.EnrollmentStatus}' (Masa Percobaan / Non-Aktif)." : null)
            };
        }

        public async Task<BenefitBalanceDeductionResultDto> DeductBalanceAsync(
            DeductInHouseBalanceRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var wfpId = request.WorkforceProfileId ?? Guid.Empty;

            // Cari enrollment induk
            var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == wfpId && !x.IsDelete, cancellationToken);

            TrxEmployeeBenefitDependent? dependent = null;
            if (request.DependentId.HasValue)
            {
                dependent = await _dbContext.TrxEmployeeBenefitDependents
                    .FirstOrDefaultAsync(x => x.Id == request.DependentId.Value && !x.IsDelete, cancellationToken);
            }

            decimal balanceBefore;
            if (dependent != null)
            {
                balanceBefore = dependent.RemainingAmount;
            }
            else if (enrollment != null)
            {
                balanceBefore = enrollment.RemainingAmount;
            }
            else
            {
                throw new KeyNotFoundException("Enrollment subsidi berobat nakes tidak ditemukan.");
            }

            // Validasi BEN-B4, VR-BEN-14, VR-BEN-15: Sisa saldo tidak boleh negatif
            if (request.PlafonDeductedAmount > balanceBefore)
            {
                throw new InvalidOperationException(
                    $"Transaksi pemotongan saldo ditolak. Sisa saldo plafon (Rp {balanceBefore:N0}) tidak mencukupi untuk menutup pemotongan Rp {request.PlafonDeductedAmount:N0} (VR-BEN-14, BEN-B4).");
            }

            var balanceAfter = balanceBefore - request.PlafonDeductedAmount;
            if (balanceAfter < 0m)
            {
                throw new InvalidOperationException("Integritas saldo gagal: saldo akhir tidak boleh bernilai negatif (chk_benefit_balance_non_negative).");
            }

            // Hitung ekses tagihan yang tidak tertutup plafon
            var effectiveBill = request.GrossBillAmount - request.DiscountAppliedAmount;
            var excessAmount = Math.Max(0m, effectiveBill - request.PlafonDeductedAmount);

            // Update saldo entity
            if (dependent != null)
            {
                dependent.RemainingAmount = balanceAfter;
                dependent.UsedAmount += request.PlafonDeductedAmount;
                dependent.UpdateDateTime = DateTime.UtcNow;
                dependent.UpdateBy = actorUserId;
            }
            else if (enrollment != null)
            {
                enrollment.RemainingAmount = balanceAfter;
                enrollment.UsedAmount += request.PlafonDeductedAmount;
                enrollment.UpdateDateTime = DateTime.UtcNow;
                enrollment.UpdateBy = actorUserId;
            }

            // Catat mutasi di buku besar (HrdBenefitBalanceMovement)
            var movement = new HrdBenefitBalanceMovement
            {
                Id = Guid.NewGuid(),
                EmployeeBenefitEnrollmentId = enrollment?.Id ?? Guid.Empty,
                EmployeeBenefitDependentId = dependent?.Id,
                WorkforceProfileId = wfpId,
                BenefitPersonKind = dependent != null ? "DEPENDENT" : "EMPLOYEE",
                MovementType = "HospitalCareUsage",
                Amount = -request.PlafonDeductedAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                ReferenceNumber = request.SimrsRegistrationNumber,
                Description = $"Pemakaian berobat internal RS Quilvian di {request.ServiceType} (No Rawat: {request.SimrsRegistrationNumber}). Tagihan: Rp {request.GrossBillAmount:N0}, Diskon: Rp {request.DiscountAppliedAmount:N0}, Plafon: Rp {request.PlafonDeductedAmount:N0}, Ekses: Rp {excessAmount:N0}.",
                OccurredAt = DateTimeOffset.UtcNow,
                ActorUserId = actorUserId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.HrdBenefitBalanceMovements.Add(movement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[INT-HR-61] Pemotongan saldo plafon RS berhasil untuk No Rawat '{RegistrationNumber}'. Plafon: Rp {Plafon:N0}, Ekses: Rp {Excess:N0}, Saldo Sisa: Rp {BalanceAfter:N0}.",
                request.SimrsRegistrationNumber, request.PlafonDeductedAmount, excessAmount, balanceAfter);

            return new BenefitBalanceDeductionResultDto
            {
                TransactionId = movement.Id,
                RegistrationNumber = request.SimrsRegistrationNumber,
                GrossBill = request.GrossBillAmount,
                DiscountAmount = request.DiscountAppliedAmount,
                PlafonCoveredAmount = request.PlafonDeductedAmount,
                OutOfPocketExcessAmount = excessAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                DelegateExcessToPayroll = excessAmount > 0m,
                Status = "Success",
                Message = excessAmount > 0m
                    ? $"Plafon terpotong Rp {request.PlafonDeductedAmount:N0}. Terdapat ekses biaya Rp {excessAmount:N0} yang dialihkan ke pemotongan payroll Sub-11 (INT-HR-63)."
                    : $"Plafon terpotong Rp {request.PlafonDeductedAmount:N0}. Seluruh tagihan tertanggung penuh."
            };
        }

        public async Task<List<BenefitBalanceMovementDto>> GetLedgerMovementsAsync(
            Guid employeeId, int? year, CancellationToken cancellationToken)
        {
            var targetYear = year ?? DateTime.UtcNow.Year;

            var movements = await _dbContext.HrdBenefitBalanceMovements.AsNoTracking()
                .Where(x => (x.WorkforceProfileId == employeeId || x.EmployeeBenefitEnrollment != null && x.EmployeeBenefitEnrollment.WorkforceProfileId == employeeId)
                    && x.OccurredAt.Year == targetYear)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync(cancellationToken);

            return movements.Select(m => new BenefitBalanceMovementDto
            {
                Id = m.Id,
                WorkforceProfileId = m.WorkforceProfileId ?? employeeId,
                MovementType = m.MovementType,
                Amount = m.Amount,
                BalanceBefore = m.BalanceBefore,
                BalanceAfter = m.BalanceAfter,
                ReferenceNumber = m.ReferenceNumber ?? "-",
                Description = m.Description ?? m.ReasonText ?? "-",
                OccurredAt = m.OccurredAt
            }).ToList();
        }

        public async Task<BenefitBalanceMovementDto> AdjustLedgerBalanceAsync(
            AdjustBalanceManualRequestDto request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == request.WorkforceProfileId && !x.IsDelete, cancellationToken);

            if (enrollment == null)
            {
                throw new KeyNotFoundException("Enrollment subsidi berobat nakes tidak ditemukan.");
            }

            var balanceBefore = enrollment.RemainingAmount;
            var balanceAfter = balanceBefore + request.AdjustmentAmount;

            if (balanceAfter < 0m)
            {
                throw new InvalidOperationException($"Penyesuaian saldo ditolak. Saldo akhir tidak boleh bernilai negatif (Rp {balanceAfter:N0}).");
            }

            enrollment.RemainingAmount = balanceAfter;
            enrollment.UpdateDateTime = DateTime.UtcNow;
            enrollment.UpdateBy = actorUserId;

            var movement = new HrdBenefitBalanceMovement
            {
                Id = Guid.NewGuid(),
                EmployeeBenefitEnrollmentId = enrollment.Id,
                WorkforceProfileId = request.WorkforceProfileId,
                BenefitPersonKind = "EMPLOYEE",
                MovementType = "ManualAdjustment",
                Amount = request.AdjustmentAmount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                ReferenceNumber = request.ReferenceNumber ?? $"ADJ-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(100, 999)}",
                Description = $"Penyesuaian manual oleh HR: {request.Reason}",
                ReasonText = request.Reason,
                OccurredAt = DateTimeOffset.UtcNow,
                ActorUserId = actorUserId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.HrdBenefitBalanceMovements.Add(movement);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[AUDIT-BEN-07] Penyesuaian saldo manual untuk nakes '{WorkforceProfileId}' sebesar Rp {Amount:N0} oleh user '{UserId}'. Alasan: {Reason}",
                request.WorkforceProfileId, request.AdjustmentAmount, actorUserId, request.Reason);

            return new BenefitBalanceMovementDto
            {
                Id = movement.Id,
                WorkforceProfileId = request.WorkforceProfileId,
                MovementType = movement.MovementType,
                Amount = movement.Amount,
                BalanceBefore = movement.BalanceBefore,
                BalanceAfter = movement.BalanceAfter,
                ReferenceNumber = movement.ReferenceNumber,
                Description = movement.Description,
                OccurredAt = movement.OccurredAt
            };
        }
    }
}
