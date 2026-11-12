using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Service realisasi klaim lembur darurat cito pasca-dinas H+1 dan klaim terencana ber-SPKL (BE-HRD-135, VR-OT-02, VR-OT-06, HRD-DEC-116, HRD-ASM-24).
    /// </summary>
    public class OvertimeRealizationService : IOvertimeRealizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOvertimeQuotaEnforcementGuard _quotaGuard;
        private readonly ILogger<OvertimeRealizationService> _logger;

        public OvertimeRealizationService(
            ApplicationDbContext context,
            IOvertimeQuotaEnforcementGuard quotaGuard,
            ILogger<OvertimeRealizationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _quotaGuard = quotaGuard ?? throw new ArgumentNullException(nameof(quotaGuard));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OvertimeRealizationServiceResult<OvertimeRealizationResultDto>> SubmitRealizationAsync(
            SubmitOvertimeRealizationCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Fail(
                    StatusCodes.Status400BadRequest, "Data klaim realisasi lembur tidak boleh kosong.");
            }

            // 1. Validasi Command
            var validator = new SubmitOvertimeRealizationCommandValidator();
            var validationResult = validator.Validate(command);
            if (!validationResult.IsValid)
            {
                var combinedErrors = string.Join("; ", validationResult.Errors);
                return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Fail(
                    StatusCodes.Status400BadRequest, combinedErrors);
            }

            // 2. Verifikasi Profil Pegawai
            var profile = await _context.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(x => x.Id == command.WorkforceProfileId && !x.IsDelete, cancellationToken);

            if (profile == null)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Fail(
                    StatusCodes.Status404NotFound, "Profil pegawai rumah sakit tidak ditemukan.");
            }

            // 3. Validasi Kategori Lembur
            bool isMedicalDirectorEscalation = false;
            TrxOvertimePlan? plan = null;

            if (command.OvertimeCategory == "Planned")
            {
                if (!command.OvertimePlanId.HasValue || command.OvertimePlanId.Value == Guid.Empty)
                {
                    return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Fail(
                        StatusCodes.Status400BadRequest, "Lembur terencana wajib menyertakan ID rencana SPKL yang telah disetujui.");
                }

                plan = await _context.Set<TrxOvertimePlan>()
                    .Include(x => x.Details)
                    .FirstOrDefaultAsync(x => x.Id == command.OvertimePlanId.Value && !x.IsDelete, cancellationToken);

                if (plan == null)
                {
                    return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Fail(
                        StatusCodes.Status404NotFound, "Dokumen SPKL rencana lembur tidak ditemukan.");
                }

                if (plan.PlanStatus != "ApprovedForWork" && plan.PlanStatus != "Approved")
                {
                    return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Fail(
                        StatusCodes.Status400BadRequest,
                        $"Realisasi lembur hanya dapat diajukan untuk SPKL berstatus ApprovedForWork. Status saat ini: {plan.PlanStatus}.");
                }

                var staffInPlan = plan.Details.FirstOrDefault(d => d.WorkforceProfileId == command.WorkforceProfileId && !d.IsDelete);
                if (staffInPlan == null)
                {
                    return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Fail(
                        StatusCodes.Status400BadRequest, "Pegawai tidak terdaftar dalam daftar pelaksana SPKL terkait.");
                }
            }
            else if (command.OvertimeCategory == "EmergencyCito")
            {
                // Evaluasi Kuota PP 35/2021 untuk penentuan eskalasi ke Direktur Pelayanan Medik (VR-OT-12, HRD-DEC-117)
                var requestedHours = Math.Round((decimal)command.ActualMinutes / 60m, 2);
                var quotaEvaluation = await _quotaGuard.EvaluateQuotaAsync(
                    command.WorkforceProfileId,
                    command.WorkDate,
                    requestedHours,
                    isEmergencyCito: true,
                    cancellationToken);

                if (!quotaEvaluation.IsAllowed || quotaEvaluation.IsMedicalDirectorEscalation)
                {
                    isMedicalDirectorEscalation = true;
                    _logger.LogWarning(
                        "Klaim lembur darurat cito pegawai {ProfileId} melampaui kuota PP 35/2021. Dieskalasikan ke Direktur Pelayanan Medik.",
                        command.WorkforceProfileId);
                }
            }

            // 4. Hitung Durasi Aktual
            int actualMinutes = command.ActualMinutes;
            if (actualMinutes <= 0)
            {
                actualMinutes = (int)(command.ActualEndAt - command.ActualStartAt).TotalMinutes;
            }

            if (actualMinutes <= 0)
            {
                return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Fail(
                    StatusCodes.Status400BadRequest, "Durasi aktual kerja lembur harus lebih dari 0 menit.");
            }

            // 5. Generate Nomor Realisasi OTR/YYYY/MM/XXXX
            var now = DateTime.UtcNow;
            var year = now.Year;
            var month = now.Month;
            var prefix = $"OTR/{year:D4}/{month:D2}/";

            var lastNumberCount = await _context.Set<TrxOvertimeRealization>()
                .Where(x => x.RealizationNumber.StartsWith(prefix))
                .CountAsync(cancellationToken);

            var nextSequence = lastNumberCount + 1;
            var realizationNumber = $"{prefix}{nextSequence:D4}";

            // 6. Buat Entitas TrxOvertimeRealization
            var initialStatus = isMedicalDirectorEscalation ? "WaitingApproval" : "WaitingVerification";

            var realization = new TrxOvertimeRealization
            {
                Id = Guid.NewGuid(),
                RealizationNumber = realizationNumber,
                OvertimeRequestId = command.OvertimePlanId ?? Guid.Empty,
                WorkforceProfileId = command.WorkforceProfileId,
                OrganizationUnitId = plan?.OrganizationUnitId,
                DepartmentId = profile.PrimaryDepartmentId,
                PositionId = profile.PrimaryPositionId,
                ActualStartDate = command.WorkDate,
                ActualEndDate = command.WorkDate,
                ActualStartAt = command.ActualStartAt,
                ActualEndAt = command.ActualEndAt,
                ActualMinutes = actualMinutes,
                RequestedMinutesSnapshot = plan != null ? plan.Details.Where(d => d.WorkforceProfileId == command.WorkforceProfileId).Sum(d => d.PlannedMinutes) : actualMinutes,
                ApprovedMinutesSnapshot = actualMinutes,
                OvertimeCategory = command.OvertimeCategory,
                CompensationType = command.CompensationType,
                EmergencyClinicalReferenceNo = command.EmergencyClinicalReferenceNo,
                ClinicalActionSummary = command.ClinicalActionSummary,
                RealizationNotes = command.RealizationNotes,
                IsMedicalDirectorEscalation = isMedicalDirectorEscalation,
                RealizationStatus = initialStatus,
                SubmittedAt = now,
                SubmittedByUserId = actorUserId,
                IsActive = true,
                CreateBy = actorUserId,
                CreateDateTime = now,
                IsDelete = false
            };

            await _context.Set<TrxOvertimeRealization>().AddAsync(realization, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Klaim lembur {RealizationNumber} ({Category}) berhasil disimpan untuk pegawai {ProfileId}. Status: {Status}",
                realization.RealizationNumber, realization.OvertimeCategory, realization.WorkforceProfileId, realization.RealizationStatus);

            var resultDto = new OvertimeRealizationResultDto
            {
                Id = realization.Id,
                RealizationNumber = realization.RealizationNumber,
                RealizationStatus = realization.RealizationStatus,
                OvertimeCategory = realization.OvertimeCategory,
                ActualMinutes = realization.ActualMinutes,
                ActualHours = Math.Round((decimal)realization.ActualMinutes / 60m, 2),
                IsMedicalDirectorEscalation = realization.IsMedicalDirectorEscalation,
                CompensationType = realization.CompensationType,
                Message = isMedicalDirectorEscalation
                    ? $"Klaim lembur darurat {realization.RealizationNumber} melampaui kuota PP 35/2021 dan berhasil dieskalasikan ke Direktur Pelayanan Medik."
                    : $"Klaim lembur {realization.RealizationNumber} berhasil diajukan dan menunggu verifikasi presensi biometrik HR."
            };

            return OvertimeRealizationServiceResult<OvertimeRealizationResultDto>.Ok(
                resultDto,
                "Klaim realisasi lembur berhasil disimpan.",
                StatusCodes.Status201Created);
        }

        public async Task<OvertimeRealizationDetailDto?> GetRealizationByIdAsync(
            Guid realizationId,
            CancellationToken cancellationToken = default)
        {
            var realization = await _context.Set<TrxOvertimeRealization>()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.MedicalDirectorApprovedBy)
                .FirstOrDefaultAsync(x => x.Id == realizationId && !x.IsDelete, cancellationToken);

            if (realization == null) return null;

            return new OvertimeRealizationDetailDto
            {
                Id = realization.Id,
                RealizationNumber = realization.RealizationNumber,
                WorkforceProfileId = realization.WorkforceProfileId,
                EmployeeName = realization.WorkforceProfile != null ? realization.WorkforceProfile.DisplayName : string.Empty,
                UnitName = realization.OrganizationUnit != null ? realization.OrganizationUnit.UnitName : string.Empty,
                OvertimeCategory = realization.OvertimeCategory,
                WorkDate = realization.ActualStartDate,
                ActualStartAt = realization.ActualStartAt,
                ActualEndAt = realization.ActualEndAt,
                ActualMinutes = realization.ActualMinutes,
                ActualHours = Math.Round((decimal)realization.ActualMinutes / 60m, 2),
                VerifiedMinutes = realization.VerifiedMinutes,
                CompensationType = realization.CompensationType,
                EmergencyClinicalReferenceNo = realization.EmergencyClinicalReferenceNo,
                ClinicalActionSummary = realization.ClinicalActionSummary,
                RealizationNotes = realization.RealizationNotes,
                RealizationStatus = realization.RealizationStatus,
                IsMedicalDirectorEscalation = realization.IsMedicalDirectorEscalation,
                MedicalDirectorApprovedById = realization.MedicalDirectorApprovedById,
                MedicalDirectorApproverName = realization.MedicalDirectorApprovedBy != null ? realization.MedicalDirectorApprovedBy.DisplayName : null,
                SubmittedAt = realization.SubmittedAt ?? realization.CreateDateTime,
                VerifiedAt = realization.VerifiedAt
            };
        }

        public async Task<PagedResult<OvertimeRealizationSummaryDto>> GetMyRealizationsAsync(
            Guid workforceProfileId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TrxOvertimeRealization>()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.ActualStartDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OvertimeRealizationSummaryDto
                {
                    Id = x.Id,
                    RealizationNumber = x.RealizationNumber,
                    OvertimeCategory = x.OvertimeCategory,
                    WorkDate = x.ActualStartDate,
                    ActualMinutes = x.ActualMinutes,
                    ActualHours = Math.Round((decimal)x.ActualMinutes / 60m, 2),
                    VerifiedMinutes = x.VerifiedMinutes,
                    CompensationType = x.CompensationType,
                    RealizationStatus = x.RealizationStatus,
                    IsMedicalDirectorEscalation = x.IsMedicalDirectorEscalation,
                    SubmittedAt = x.SubmittedAt ?? x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<OvertimeRealizationSummaryDto>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize),
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<OvertimeRealizationServiceResult<bool>> CancelRealizationAsync(
            Guid realizationId,
            CancelOvertimeRealizationCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var realization = await _context.Set<TrxOvertimeRealization>()
                .FirstOrDefaultAsync(x => x.Id == realizationId && !x.IsDelete, cancellationToken);

            if (realization == null)
            {
                return OvertimeRealizationServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound, "Klaim realisasi lembur tidak ditemukan.");
            }

            if (realization.RealizationStatus != "Draft" && realization.RealizationStatus != "WaitingVerification" && realization.RealizationStatus != "WaitingApproval")
            {
                return OvertimeRealizationServiceResult<bool>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Klaim lembur tidak dapat dibatalkan pada status saat ini ({realization.RealizationStatus}).");
            }

            realization.RealizationStatus = "Cancelled";
            realization.CancelledAt = DateTime.UtcNow;
            realization.CancelledByUserId = actorUserId;
            realization.RealizationNotes = $"{realization.RealizationNotes} | Dibatalkan: {command.Reason}";
            realization.UpdateBy = actorUserId;
            realization.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Klaim realisasi lembur {RealizationNumber} berhasil dibatalkan oleh {UserId}.",
                realization.RealizationNumber, actorUserId);

            return OvertimeRealizationServiceResult<bool>.Ok(
                true,
                $"Klaim lembur {realization.RealizationNumber} berhasil dibatalkan.",
                StatusCodes.Status200OK);
        }
    }
}
