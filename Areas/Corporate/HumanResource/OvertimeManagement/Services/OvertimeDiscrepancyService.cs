using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Layanan penanganan anomali discrepancy presensi biometrik dan penyesuaian evaluasi logbook fisik ruangan oleh HR Admin (BE-HRD-140, VR-OT-10, VR-OT-11, HRD-DEC-119).
    /// </summary>
    public class OvertimeDiscrepancyService : IOvertimeDiscrepancyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OvertimeDiscrepancyService> _logger;

        public OvertimeDiscrepancyService(
            ApplicationDbContext context,
            ILogger<OvertimeDiscrepancyService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<OvertimeDiscrepancyQueueItemDto>> GetDiscrepancyQueueAsync(
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TrxOvertimeVerification>()
                .Include(x => x.OvertimeRealization)
                    .ThenInclude(r => r!.WorkforceProfile)
                .Include(x => x.OvertimeRealization)
                    .ThenInclude(r => r!.OrganizationUnit)
                .Where(x => !x.IsDelete && x.VerificationStatus == "FlaggedDiscrepancy");

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OvertimeDiscrepancyQueueItemDto
                {
                    VerificationId = x.Id,
                    RealizationId = x.OvertimeRealizationId,
                    RealizationNumber = x.OvertimeRealization != null ? x.OvertimeRealization.RealizationNumber : string.Empty,
                    WorkforceProfileId = x.OvertimeRealization != null ? x.OvertimeRealization.WorkforceProfileId : Guid.Empty,
                    EmployeeName = x.OvertimeRealization != null && x.OvertimeRealization.WorkforceProfile != null
                        ? x.OvertimeRealization.WorkforceProfile.DisplayName
                        : string.Empty,
                    UnitName = x.OvertimeRealization != null && x.OvertimeRealization.OrganizationUnit != null
                        ? x.OvertimeRealization.OrganizationUnit.UnitName
                        : string.Empty,
                    WorkDate = x.OvertimeRealization != null ? x.OvertimeRealization.ActualStartDate : DateOnly.MinValue,
                    ClaimedMinutes = x.SubmittedMinutes,
                    BiometricExcessMinutes = x.BiometricExcessMinutes,
                    VarianceMinutes = Math.Abs(x.SubmittedMinutes - x.BiometricExcessMinutes),
                    BiometricCheckIn = x.BiometricCheckIn,
                    BiometricCheckOut = x.BiometricCheckOut,
                    DiscrepancyReason = x.DiscrepancyReason,
                    VerificationStatus = x.VerificationStatus,
                    EmergencyClinicalReferenceNo = x.OvertimeRealization != null ? x.OvertimeRealization.EmergencyClinicalReferenceNo : null,
                    FlaggedAt = x.ActionAt ?? x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimeDiscrepancyQueueItemDto>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize),
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>> ResolveDiscrepancyAsync(
            Guid verificationId,
            ResolveOvertimeDiscrepancyCommand command,
            Guid hrAdminUserId,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                return OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>.Fail(
                    StatusCodes.Status400BadRequest, "Data penyelesaian anomali presensi tidak boleh kosong.");
            }

            if (string.IsNullOrWhiteSpace(command.PhysicalLogbookReferenceNo))
            {
                return OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>.Fail(
                    StatusCodes.Status400BadRequest, "Nomor referensi dokumen logbook fisik ruangan / surat pengantar wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(command.ManualAdjustmentNotes))
            {
                return OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>.Fail(
                    StatusCodes.Status400BadRequest, "Catatan justifikasi tertulis evaluasi HR wajib disertakan.");
            }

            var verification = await _context.Set<TrxOvertimeVerification>()
                .Include(x => x.OvertimeRealization)
                .FirstOrDefaultAsync(x => x.Id == verificationId && !x.IsDelete, cancellationToken);

            if (verification == null)
            {
                return OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>.Fail(
                    StatusCodes.Status404NotFound, "Data verifikasi presensi lembur tidak ditemukan.");
            }

            if (verification.VerificationStatus != "FlaggedDiscrepancy")
            {
                return OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Hanya verifikasi berstatus FlaggedDiscrepancy yang dapat diselesaikan lewat jalur evaluasi logbook fisik. Status saat ini: {verification.VerificationStatus}.");
            }

            var realization = verification.OvertimeRealization;
            if (realization == null)
            {
                return OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>.Fail(
                    StatusCodes.Status404NotFound, "Data induk realisasi lembur tidak ditemukan.");
            }

            var prevStatus = verification.VerificationStatus;
            var now = DateTime.UtcNow;
            int verifiedPayableMinutes = 0;
            string newVerificationStatus = string.Empty;
            string newRealizationStatus = string.Empty;
            string message = string.Empty;

            if (command.ResolutionAction == "ApproveAdjusted")
            {
                // Pembulatan ke bawah kelipatan 30 menit (VR-OT-08, OT-B8)
                verifiedPayableMinutes = (command.AdjustedMinutes / 30) * 30;
                if (command.AdjustedMinutes < 30)
                {
                    verifiedPayableMinutes = 0; // VR-OT-09
                }

                newVerificationStatus = "ManuallyAdjusted";
                newRealizationStatus = "Verified";

                verification.VerificationStatus = newVerificationStatus;
                verification.VerifiedMinutes = verifiedPayableMinutes;
                verification.VerifiedPayableMinutes = verifiedPayableMinutes;
                verification.ManualAdjustmentNotes = $"[Ref Logbook: {command.PhysicalLogbookReferenceNo}] {command.ManualAdjustmentNotes}";
                verification.VerifiedByHrAdminId = hrAdminUserId;
                verification.ActionAt = now;
                verification.IsFinalVerification = true;
                verification.UpdateBy = hrAdminUserId;
                verification.UpdateDateTime = now;

                realization.VerifiedMinutes = verifiedPayableMinutes;
                realization.RealizationStatus = newRealizationStatus;
                realization.RealizationNotes = $"{realization.RealizationNotes} | Penyesuaian Manual HR (Ref: {command.PhysicalLogbookReferenceNo}): {command.ManualAdjustmentNotes}";
                realization.VerifiedAt = now;
                realization.VerifiedByUserId = hrAdminUserId;
                realization.UpdateBy = hrAdminUserId;
                realization.UpdateDateTime = now;

                message = $"Anomali presensi klaim {realization.RealizationNumber} berhasil diselesaikan melalui penyesuaian manual bukti logbook fisik: {verifiedPayableMinutes} menit ({Math.Round((decimal)verifiedPayableMinutes / 60m, 2)} jam) berbayar disetujui.";
            }
            else if (command.ResolutionAction == "RejectDiscrepancy")
            {
                newVerificationStatus = "RejectedDiscrepancy";
                newRealizationStatus = "Rejected";

                verification.VerificationStatus = newVerificationStatus;
                verification.VerifiedMinutes = 0;
                verification.VerifiedPayableMinutes = 0;
                verification.ManualAdjustmentNotes = $"[DITOLAK - Ref Logbook: {command.PhysicalLogbookReferenceNo}] {command.ManualAdjustmentNotes}";
                verification.VerifiedByHrAdminId = hrAdminUserId;
                verification.ActionAt = now;
                verification.IsFinalVerification = true;
                verification.UpdateBy = hrAdminUserId;
                verification.UpdateDateTime = now;

                realization.VerifiedMinutes = 0;
                realization.RealizationStatus = newRealizationStatus;
                realization.RealizationNotes = $"{realization.RealizationNotes} | Penolakan Anomali Presensi (Ref: {command.PhysicalLogbookReferenceNo}): {command.ManualAdjustmentNotes}";
                realization.UpdateBy = hrAdminUserId;
                realization.UpdateDateTime = now;

                message = $"Anomali presensi klaim {realization.RealizationNumber} ditolak oleh HR Admin berdasarkan hasil audit fisik logbook ruangan. Klaim tidak diakui berbayar.";
            }
            else
            {
                return OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Aksi penyelesaian tidak valid: {command.ResolutionAction}. Pilihan yang valid: ApproveAdjusted atau RejectDiscrepancy.");
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Anomali discrepancy verifikasi {VerificationId} untuk klaim {RealizationNumber} diselesaikan dengan aksi {Action} oleh HR Admin {AdminId}. Menit berbayar: {Minutes}",
                verification.Id, realization.RealizationNumber, command.ResolutionAction, hrAdminUserId, verifiedPayableMinutes);

            var result = new ResolveOvertimeDiscrepancyResultDto
            {
                VerificationId = verification.Id,
                RealizationId = realization.Id,
                RealizationNumber = realization.RealizationNumber,
                PreviousVerificationStatus = prevStatus,
                NewVerificationStatus = newVerificationStatus,
                AdjustedMinutes = command.AdjustedMinutes,
                VerifiedPayableMinutes = verifiedPayableMinutes,
                RealizationStatus = newRealizationStatus,
                Message = message
            };

            return OvertimePlanningServiceResult<ResolveOvertimeDiscrepancyResultDto>.Ok(
                result,
                "Penyelesaian anomali presensi berhasil diproses.",
                StatusCodes.Status200OK);
        }
    }
}
