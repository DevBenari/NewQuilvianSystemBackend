using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Layanan otorisasi eskalasi khusus Direktur Pelayanan Medik darurat cito melampaui batas kuota PP 35/2021 (BE-HRD-138, VR-OT-12, VR-OT-24, HRD-DEC-117, OT-W1).
    /// </summary>
    public class MedicalDirectorOvertimeEscalationService : IMedicalDirectorOvertimeEscalationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MedicalDirectorOvertimeEscalationService> _logger;

        public MedicalDirectorOvertimeEscalationService(
            ApplicationDbContext context,
            ILogger<MedicalDirectorOvertimeEscalationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedResult<MedicalDirectorOvertimeEscalationDto>> GetEscalationQueueAsync(
            string? urgency = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<TrxOvertimeRealization>()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OrganizationUnit)
                .Where(x => !x.IsDelete && x.IsMedicalDirectorEscalation && x.MedicalDirectorApprovedById == null);

            var totalCount = await query.CountAsync(cancellationToken);

            var realizations = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = new List<MedicalDirectorOvertimeEscalationDto>();

            foreach (var r in realizations)
            {
                // Hitung jam kerja 7 hari terakhir (Past 7 Days Rolling Work & Overtime Hours) (VR-OT-24)
                var startDate = r.ActualStartDate.AddDays(-7);
                var endDate = r.ActualStartDate.AddDays(-1);

                var pastWorkMinutes = await _context.Set<HrdAttendanceDaily>()
                    .Where(a => a.WorkforceProfileId == r.WorkforceProfileId &&
                                !a.IsDelete &&
                                a.AttendanceDate >= startDate &&
                                a.AttendanceDate <= endDate)
                    .SumAsync(a => a.ActualWorkMinutes, cancellationToken);

                var pastOvertimeMinutes = await _context.Set<TrxOvertimeRealization>()
                    .Where(ot => ot.WorkforceProfileId == r.WorkforceProfileId &&
                                 !ot.IsDelete &&
                                 ot.Id != r.Id &&
                                 ot.ActualStartDate >= startDate &&
                                 ot.ActualStartDate <= endDate &&
                                 (ot.RealizationStatus == "Verified" || ot.RealizationStatus == "PostedToPayroll" || ot.RealizationStatus == "WaitingVerification"))
                    .SumAsync(ot => ot.ActualMinutes, cancellationToken);

                var past7DaysTotalHours = Math.Round((decimal)(pastWorkMinutes + pastOvertimeMinutes) / 60m, 1);

                items.Add(new MedicalDirectorOvertimeEscalationDto
                {
                    RealizationId = r.Id,
                    RealizationNumber = r.RealizationNumber,
                    WorkforceProfileId = r.WorkforceProfileId,
                    EmployeeName = r.WorkforceProfile != null ? r.WorkforceProfile.DisplayName : string.Empty,
                    UnitName = r.OrganizationUnit != null ? r.OrganizationUnit.UnitName : string.Empty,
                    WorkDate = r.ActualStartDate,
                    ActualMinutes = r.ActualMinutes,
                    ActualHours = Math.Round((decimal)r.ActualMinutes / 60m, 2),
                    Past7DaysTotalHours = past7DaysTotalHours,
                    EmergencyClinicalReferenceNo = r.EmergencyClinicalReferenceNo ?? string.Empty,
                    ClinicalActionSummary = r.ClinicalActionSummary ?? string.Empty,
                    UrgencyReason = past7DaysTotalHours >= 60m
                        ? "PERINGATAN RISIKO FATIGUE TINGGI: Total jam kerja 7 hari terakhir melebihi 60 jam. Otorisasi dispensasi wajib memastikan waktu pemulihan staf."
                        : "Penyelamatan Nyawa Pasien (Life-Saving Emergency Cito) melampaui batas kuota lembur PP 35/2021.",
                    SubmittedAt = r.SubmittedAt ?? r.CreateDateTime
                });
            }

            return new PagedResult<MedicalDirectorOvertimeEscalationDto>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = (int)Math.Ceiling((double)totalCount / pageSize),
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> ApproveEscalationAsync(
            Guid realizationId,
            ApproveMedicalDirectorEscalationCommand command,
            Guid directorUserId,
            CancellationToken cancellationToken = default)
        {
            var realization = await _context.Set<TrxOvertimeRealization>()
                .FirstOrDefaultAsync(x => x.Id == realizationId && !x.IsDelete, cancellationToken);

            if (realization == null)
            {
                return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "Klaim realisasi lembur darurat medis cito tidak ditemukan.");
            }

            if (!realization.IsMedicalDirectorEscalation || realization.MedicalDirectorApprovedById != null)
            {
                return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Klaim ini tidak berada dalam status eskalasi Direktur Pelayanan Medik atau sudah disetujui sebelumnya.");
            }

            var restHours = command.MandatoryRestHoursGranted > 0 ? command.MandatoryRestHoursGranted : 11;
            var prevStatus = realization.RealizationStatus;

            // Update status dan bukti otorisasi Direktur Medis (VR-OT-12, HRD-DEC-117)
            realization.MedicalDirectorApprovedById = directorUserId;
            realization.IsMedicalDirectorEscalation = false; // Dispensasi diberikan
            realization.RealizationStatus = "WaitingVerification"; // Melanjutkan ke verifikasi biometrik HR
            realization.RealizationNotes = $"{realization.RealizationNotes} | Otorisasi Direktur Medis: {command.DispensatoryReason} (Mandatory Rest: {restHours} jam)";
            realization.UpdateBy = directorUserId;
            realization.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Dispensasi darurat medis cito untuk klaim {RealizationNumber} berhasil disetujui oleh Direktur Medis {DirectorUserId} dengan istirahat wajib {RestHours} jam.",
                realization.RealizationNumber, directorUserId, restHours);

            return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Ok(
                new OvertimeApprovalResultDto
                {
                    Id = realization.Id,
                    Number = realization.RealizationNumber,
                    PreviousStatus = prevStatus,
                    NewStatus = realization.RealizationStatus,
                    Message = $"Otorisasi dispensasi darurat medis {realization.RealizationNumber} berhasil disetujui oleh Direktur Pelayanan Medik. Status beralih ke verifikasi biometrik HR."
                },
                "Dispensasi darurat cito berhasil diterbitkan.",
                StatusCodes.Status200OK);
        }

        public async Task<OvertimePlanningServiceResult<OvertimeApprovalResultDto>> RejectEscalationAsync(
            Guid realizationId,
            string rejectionReason,
            Guid directorUserId,
            CancellationToken cancellationToken = default)
        {
            var realization = await _context.Set<TrxOvertimeRealization>()
                .FirstOrDefaultAsync(x => x.Id == realizationId && !x.IsDelete, cancellationToken);

            if (realization == null)
            {
                return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    "Klaim realisasi lembur darurat medis cito tidak ditemukan.");
            }

            var prevStatus = realization.RealizationStatus;
            realization.RealizationStatus = "Rejected";
            realization.IsMedicalDirectorEscalation = false;
            realization.RealizationNotes = $"{realization.RealizationNotes} | Penolakan Direktur Medis: {rejectionReason}";
            realization.UpdateBy = directorUserId;
            realization.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Eskalasi dispensasi darurat medis cito klaim {RealizationNumber} ditolak oleh Direktur Medis {DirectorUserId}: {Reason}",
                realization.RealizationNumber, directorUserId, rejectionReason);

            return OvertimePlanningServiceResult<OvertimeApprovalResultDto>.Ok(
                new OvertimeApprovalResultDto
                {
                    Id = realization.Id,
                    Number = realization.RealizationNumber,
                    PreviousStatus = prevStatus,
                    NewStatus = "Rejected",
                    Message = $"Eskalasi darurat medis {realization.RealizationNumber} ditolak oleh Direktur Pelayanan Medik."
                },
                "Eskalasi lembur darurat cito ditolak.",
                StatusCodes.Status200OK);
        }
    }
}
