using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Layanan verifikasi silang log presensi biometrik, penegakan Min-Duration Rule, pembulatan kelipatan 30 menit ke bawah, dan deteksi anomali selisih (BE-HRD-139, INT-HR-50, VR-OT-07 s.d. VR-OT-09, HRD-DEC-119).
    /// </summary>
    public class OvertimeBiometricVerificationService : IOvertimeBiometricVerificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OvertimeBiometricVerificationService> _logger;

        public OvertimeBiometricVerificationService(
            ApplicationDbContext context,
            ILogger<OvertimeBiometricVerificationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OvertimeBiometricVerificationResultDto> VerifyBiometricAsync(
            Guid realizationId,
            Guid hrAdminUserId,
            CancellationToken cancellationToken = default)
        {
            var realization = await _context.Set<TrxOvertimeRealization>()
                .Include(x => x.WorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == realizationId && !x.IsDelete, cancellationToken);

            if (realization == null)
            {
                throw new KeyNotFoundException($"Klaim realisasi lembur dengan ID {realizationId} tidak ditemukan.");
            }

            // 1. Ambil data presensi harian dari Sub-07 (INT-HR-50)
            var attendance = await _context.Set<HrdAttendanceDaily>()
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == realization.WorkforceProfileId &&
                                          x.AttendanceDate == realization.ActualStartDate &&
                                          !x.IsDelete, cancellationToken);

            DateTime? biometricCheckIn = attendance?.FirstCheckInAt;
            DateTime? biometricCheckOut = attendance?.LastCheckOutAt;
            int biometricExcessMinutes = 0;

            if (attendance != null)
            {
                if (attendance.OvertimeMinutes > 0)
                {
                    biometricExcessMinutes = attendance.OvertimeMinutes;
                }
                else if (attendance.ScheduledCheckOutAt.HasValue && attendance.LastCheckOutAt.HasValue &&
                         attendance.LastCheckOutAt.Value > attendance.ScheduledCheckOutAt.Value)
                {
                    biometricExcessMinutes = (int)(attendance.LastCheckOutAt.Value - attendance.ScheduledCheckOutAt.Value).TotalMinutes;
                }
                else if (attendance.IsRestDay || attendance.IsHoliday)
                {
                    biometricExcessMinutes = attendance.ActualWorkMinutes;
                }

                if (attendance.HasMissingPunch || !attendance.FirstCheckInAt.HasValue || !attendance.LastCheckOutAt.HasValue)
                {
                    biometricExcessMinutes = 0;
                }
            }

            // 2. Evaluasi Discrepancy (> 30 menit atau lupa tap / tidak ada data presensi) (VR-OT-10, HRD-DEC-119)
            int claimedMinutes = realization.ActualMinutes;
            int varianceMinutes = Math.Abs(claimedMinutes - biometricExcessMinutes);
            bool isDiscrepancy = (varianceMinutes > 30) || (biometricExcessMinutes == 0);
            string? discrepancyReason = null;

            if (isDiscrepancy)
            {
                if (biometricExcessMinutes == 0)
                {
                    discrepancyReason = "Mesin biometrik tidak mencatat kehadiran lengkap (lupa tap in / tap out atau mesin offline) pada tanggal dinas.";
                }
                else
                {
                    discrepancyReason = $"Selisih jam klaim ({claimedMinutes} menit) dengan log mesin biometrik ({biometricExcessMinutes} menit) melebihi ambang batas toleransi 30 menit ({varianceMinutes} menit varians).";
                }
            }

            // 3. Evaluasi Min-Duration Rule & Pembulatan 30 Menit ke Bawah (VR-OT-07, VR-OT-08, VR-OT-09)
            int eligibleMinutes = 0;
            int verifiedPayableMinutes = 0;

            if (!isDiscrepancy)
            {
                // Min-Duration Rule: min(Claimed, Biometric) (VR-OT-07, OT-W3)
                eligibleMinutes = Math.Min(claimedMinutes, biometricExcessMinutes);

                // Pembulatan ke bawah kelipatan 30 menit (VR-OT-08, OT-B8)
                verifiedPayableMinutes = (eligibleMinutes / 30) * 30;

                // Ambang batas bawah minimum 30 menit (VR-OT-09)
                if (eligibleMinutes < 30)
                {
                    verifiedPayableMinutes = 0;
                }
            }

            // 4. Update atau Tambah TrxOvertimeVerification
            var verification = await _context.Set<TrxOvertimeVerification>()
                .FirstOrDefaultAsync(x => x.OvertimeRealizationId == realization.Id && !x.IsDelete, cancellationToken);

            var now = DateTime.UtcNow;

            if (verification == null)
            {
                verification = new TrxOvertimeVerification
                {
                    Id = Guid.NewGuid(),
                    OvertimeRealizationId = realization.Id,
                    VerificationType = "HR",
                    VerificationOrder = 1,
                    VerifierUserId = hrAdminUserId,
                    VerifiedByHrAdminId = hrAdminUserId,
                    VerificationStatus = isDiscrepancy ? "FlaggedDiscrepancy" : "AutoVerified",
                    ActionAt = now,
                    SubmittedMinutes = claimedMinutes,
                    EligibleMinutes = eligibleMinutes,
                    VerifiedMinutes = verifiedPayableMinutes,
                    VerifiedPayableMinutes = verifiedPayableMinutes,
                    BiometricCheckIn = biometricCheckIn,
                    BiometricCheckOut = biometricCheckOut,
                    BiometricExcessMinutes = biometricExcessMinutes,
                    DiscrepancyReason = discrepancyReason,
                    IsAttendanceMatched = !isDiscrepancy,
                    HasVariance = isDiscrepancy,
                    IsFinalVerification = !isDiscrepancy,
                    IsActive = true,
                    CreateBy = hrAdminUserId,
                    CreateDateTime = now,
                    IsDelete = false
                };

                await _context.Set<TrxOvertimeVerification>().AddAsync(verification, cancellationToken);
            }
            else
            {
                verification.VerificationStatus = isDiscrepancy ? "FlaggedDiscrepancy" : "AutoVerified";
                verification.ActionAt = now;
                verification.SubmittedMinutes = claimedMinutes;
                verification.EligibleMinutes = eligibleMinutes;
                verification.VerifiedMinutes = verifiedPayableMinutes;
                verification.VerifiedPayableMinutes = verifiedPayableMinutes;
                verification.BiometricCheckIn = biometricCheckIn;
                verification.BiometricCheckOut = biometricCheckOut;
                verification.BiometricExcessMinutes = biometricExcessMinutes;
                verification.DiscrepancyReason = discrepancyReason;
                verification.IsAttendanceMatched = !isDiscrepancy;
                verification.HasVariance = isDiscrepancy;
                verification.IsFinalVerification = !isDiscrepancy;
                verification.VerifiedByHrAdminId = hrAdminUserId;
                verification.UpdateBy = hrAdminUserId;
                verification.UpdateDateTime = now;
            }

            // 5. Update TrxOvertimeRealization
            if (isDiscrepancy)
            {
                // Ditahan dari payroll, menunggu penyesuaian manual bukti fisik logbook oleh HR (VR-OT-10, VR-OT-11)
                realization.RealizationStatus = "FlaggedDiscrepancy";
                realization.VerifiedMinutes = 0;
                realization.VarianceMinutes = varianceMinutes;
            }
            else
            {
                realization.RealizationStatus = "Verified";
                realization.VerifiedMinutes = verifiedPayableMinutes;
                realization.VarianceMinutes = varianceMinutes;
                realization.VerifiedAt = now;
                realization.VerifiedByUserId = hrAdminUserId;
            }

            realization.UpdateBy = hrAdminUserId;
            realization.UpdateDateTime = now;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Verifikasi biometrik klaim {RealizationNumber} tuntas. Status: {Status}, Claimed: {Claimed}m, Biometric: {Biometric}m, Payable: {Payable}m",
                realization.RealizationNumber, verification.VerificationStatus, claimedMinutes, biometricExcessMinutes, verifiedPayableMinutes);

            return new OvertimeBiometricVerificationResultDto
            {
                VerificationId = verification.Id,
                RealizationId = realization.Id,
                RealizationNumber = realization.RealizationNumber,
                VerificationStatus = verification.VerificationStatus,
                ClaimedMinutes = claimedMinutes,
                BiometricExcessMinutes = biometricExcessMinutes,
                EligibleMinutes = eligibleMinutes,
                VerifiedPayableMinutes = verifiedPayableMinutes,
                IsDiscrepancyFlagged = isDiscrepancy,
                DiscrepancyReason = discrepancyReason,
                BiometricCheckIn = biometricCheckIn,
                BiometricCheckOut = biometricCheckOut,
                Message = isDiscrepancy
                    ? $"Klaim {realization.RealizationNumber} terdeteksi memiliki ketidaksesuaian log presensi biometrik ({discrepancyReason}). Klaim ditahan dan dialihkan ke antrean investigasi logbook fisik HR."
                    : $"Verifikasi biometrik klaim {realization.RealizationNumber} berhasil disetujui otomatis: {verifiedPayableMinutes} menit ({Math.Round((decimal)verifiedPayableMinutes / 60m, 2)} jam) berbayar."
            };
        }
    }
}
