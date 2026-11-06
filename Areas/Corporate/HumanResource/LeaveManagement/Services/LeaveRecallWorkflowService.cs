using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    /// <summary>
    /// Service alur pemanggilan cuti darurat (Leave Recall), otorisasi HR Manager,
    /// dan pemulihan saldo kredit serta jadwal roster dinas aktif
    /// (BE-HRD-126, BE-HRD-127, HRD-DEC-113, LEV-B5, LEV-W4, VR-LEV-14, VR-LEV-15, VR-LEV-16, INT-HR-46).
    /// </summary>
    public class LeaveRecallWorkflowService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly LeaveBalanceLedgerService _ledgerService;
        private readonly ILogger<LeaveRecallWorkflowService> _logger;

        public LeaveRecallWorkflowService(
            ApplicationDbContext dbContext,
            LeaveBalanceLedgerService ledgerService,
            ILogger<LeaveRecallWorkflowService> logger)
        {
            _dbContext = dbContext;
            _ledgerService = ledgerService;
            _logger = logger;
        }

        /// <summary>
        /// Query daftar pemanggilan cuti darurat rumah sakit.
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<List<LeaveRecallDto>>> GetRecallsAsync(
            Guid? unitId = null,
            string? status = null,
            int? year = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<TrxLeaveRecallRequest>()
                .Include(r => r.LeaveRequest)
                    .ThenInclude(l => l!.WorkforceProfile)
                .Include(r => r.LeaveRequest)
                    .ThenInclude(l => l!.OrganizationUnit)
                .AsNoTracking()
                .Where(r => !r.IsDelete);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (year.HasValue)
            {
                query = query.Where(r => r.EffectiveRecallDate.Year == year.Value);
            }

            if (unitId.HasValue && unitId.Value != Guid.Empty)
            {
                query = query.Where(r => r.LeaveRequest != null &&
                    (r.LeaveRequest.OrganizationUnitId == unitId.Value || r.LeaveRequest.DepartmentId == unitId.Value));
            }

            var rows = await query
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

            var items = rows.Select(r => new LeaveRecallDto
            {
                Id = r.Id,
                RecallNumber = r.RecallNumber,
                LeaveRequestId = r.LeaveRequestId,
                RequestNumber = r.LeaveRequest?.RequestNumber ?? string.Empty,
                WorkforceProfileId = r.WorkforceProfileId,
                EmployeeCode = r.LeaveRequest?.WorkforceProfile?.ProfileCode ?? string.Empty,
                EmployeeName = r.LeaveRequest?.WorkforceProfile?.DisplayName ?? string.Empty,
                UnitName = r.LeaveRequest?.OrganizationUnit?.UnitName ?? string.Empty,
                EffectiveRecallDate = r.EffectiveRecallDate,
                RestoredDays = r.RestoredDays,
                EmergencyCategory = r.EmergencyCategory,
                RecallReason = r.RecallReason,
                Status = r.Status,
                RequestedByHeadUnitId = r.RequestedByHeadUnitId,
                RequestedByHeadUnitName = "Kepala Ruangan / Unit",
                RequestedAt = r.RequestedAt,
                ApprovedByHrManagerId = r.ApprovedByHrManagerId,
                ApprovedByHrManagerName = r.ApprovedByHrManagerId.HasValue ? "HR Manager" : null,
                ApprovedAt = r.ApprovedAt,
                RejectionReason = r.RejectionReason
            }).ToList();

            return LeaveBalanceLedgerServiceResult<List<LeaveRecallDto>>.Ok(
                items,
                $"Berhasil mengambil {items.Count} berkas pemanggilan dinas darurat.");
        }

        /// <summary>
        /// Inisiasi tiket pemanggilan kembali cuti aktif oleh Kepala Ruangan / Kepala Unit (BE-HRD-126).
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<LeaveRecallDto>> InitiateRecallAsync(
            InitiateLeaveRecallCommand command,
            Guid headUnitUserId,
            CancellationToken cancellationToken = default)
        {
            if (command == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Fail(StatusCodes.Status400BadRequest, "Command tidak boleh kosong.");
            }

            var leave = await _dbContext.Set<WfpLeaveRequest>()
                .Include(l => l.WorkforceProfile)
                .Include(l => l.OrganizationUnit)
                .FirstOrDefaultAsync(l => l.Id == command.LeaveRequestId && !l.IsDelete, cancellationToken);

            if (leave == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Fail(StatusCodes.Status404NotFound, "Permohonan cuti tidak ditemukan.");
            }

            // Validasi status permohonan harus sedang aktif (Approved atau Taken) (VR-LEV-14, HRD-DEC-113)
            if (leave.LeaveRequestStatus != "Approved" && leave.LeaveRequestStatus != "Taken")
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pemanggilan dinas darurat hanya dapat dilakukan pada pegawai yang sedang menjalani cuti aktif (VR-LEV-14, HRD-DEC-113).");
            }

            // Validasi tanggal efektif recall harus berada dalam rentang pelaksanaan cuti
            if (command.EffectiveRecallDate < leave.StartDate || command.EffectiveRecallDate > leave.EndDate)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal efektif recall harus berada di antara tanggal mulai dan selesai cuti.");
            }

            // Perhitungan hari cuti yang batal dinikmati dan dipulihkan (RestoredDays) (BE-HRD-127, VR-LEV-16)
            int calendarDays = leave.EndDate.DayNumber - command.EffectiveRecallDate.DayNumber + 1;
            decimal restoredDays = (decimal)calendarDays;
            decimal totalDuration = leave.CalculatedWorkingDays > 0 ? leave.CalculatedWorkingDays : leave.RequestedDays;
            if (restoredDays > totalDuration)
            {
                restoredDays = totalDuration;
            }

            if (restoredDays <= 0)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Jumlah hari pemulihan cuti tidak valid atau kurang dari sama dengan 0 (VR-LEV-16).");
            }

            // Cek apakah sudah ada tiket recall aktif untuk cuti ini
            var exists = await _dbContext.Set<TrxLeaveRecallRequest>()
                .AnyAsync(r => r.LeaveRequestId == leave.Id &&
                               r.Status != "RejectedByHrManager" &&
                               r.Status != "Cancelled" &&
                               !r.IsDelete, cancellationToken);

            if (exists)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Fail(
                    StatusCodes.Status409Conflict,
                    "Sudah ada permohonan dinas darurat aktif untuk tiket cuti ini.");
            }

            var now = DateTime.UtcNow;
            var recall = new TrxLeaveRecallRequest
            {
                Id = Guid.NewGuid(),
                RecallNumber = $"RCL-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                LeaveRequestId = leave.Id,
                WorkforceProfileId = leave.WorkforceProfileId,
                EffectiveRecallDate = command.EffectiveRecallDate,
                RestoredDays = restoredDays,
                EmergencyCategory = command.EmergencyCategory,
                RecallReason = command.RecallReason.Trim(),
                Status = "RequestedByHeadUnit",
                RequestedByHeadUnitId = headUnitUserId,
                RequestedAt = now,
                IsActive = true,
                CreateBy = headUnitUserId,
                CreateDateTime = now
            };

            await _dbContext.Set<TrxLeaveRecallRequest>().AddAsync(recall, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tiket recall dinas darurat {RecallNumber} berhasil diinisiasi oleh Kepala Unit {UserId} untuk pegawai {EmployeeId}.",
                recall.RecallNumber, headUnitUserId, leave.WorkforceProfileId);

            return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Ok(
                new LeaveRecallDto
                {
                    Id = recall.Id,
                    RecallNumber = recall.RecallNumber,
                    LeaveRequestId = leave.Id,
                    RequestNumber = leave.RequestNumber,
                    WorkforceProfileId = leave.WorkforceProfileId,
                    EmployeeCode = leave.WorkforceProfile?.ProfileCode ?? string.Empty,
                    EmployeeName = leave.WorkforceProfile?.DisplayName ?? string.Empty,
                    UnitName = leave.OrganizationUnit?.UnitName ?? string.Empty,
                    EffectiveRecallDate = recall.EffectiveRecallDate,
                    RestoredDays = recall.RestoredDays,
                    EmergencyCategory = recall.EmergencyCategory,
                    RecallReason = recall.RecallReason,
                    Status = recall.Status,
                    RequestedByHeadUnitId = headUnitUserId,
                    RequestedByHeadUnitName = "Kepala Ruangan",
                    RequestedAt = now
                },
                "Tiket pemanggilan dinas darurat berhasil diinisiasi dan menunggu persetujuan HR Manager.");
        }

        /// <summary>
        /// Persetujuan resmi recall dinas darurat oleh HR Manager, pemulihan saldo mutasi kredit,
        /// dan pemulihan jadwal shift Roster 06 (BE-HRD-126, BE-HRD-127, LEV-B5, LEV-W4, INT-HR-46).
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<LeaveRecallExecutionResultDto>> ApproveRecallAsync(
            Guid recallId,
            Guid hrManagerUserId,
            bool isHrManager,
            ApproveLeaveRecallCommand command,
            CancellationToken cancellationToken = default)
        {
            // Otorisasi ketat tunggal: hanya HR Manager yang berwenang (LEV-B5, VR-LEV-15, HRD-DEC-113)
            if (!isHrManager)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallExecutionResultDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya HR Manager yang memiliki wewenang menyetujui pemanggilan cuti darurat rumah sakit (VR-LEV-15, LEV-B5, HRD-DEC-113).");
            }

            var recall = await _dbContext.Set<TrxLeaveRecallRequest>()
                .Include(r => r.LeaveRequest)
                .FirstOrDefaultAsync(r => r.Id == recallId && !r.IsDelete, cancellationToken);

            if (recall == null || recall.LeaveRequest == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallExecutionResultDto>.Fail(StatusCodes.Status404NotFound, "Tiket recall tidak ditemukan.");
            }

            if (recall.Status != "RequestedByHeadUnit")
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallExecutionResultDto>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Tiket recall tidak dalam status dapat disetujui (Status saat ini: {recall.Status}).");
            }

            var now = DateTime.UtcNow;
            recall.Status = "ApprovedByHrManager";
            recall.ApprovedByHrManagerId = hrManagerUserId;
            recall.ApprovedAt = now;
            recall.UpdateBy = hrManagerUserId;
            recall.UpdateDateTime = now;

            // 1. Eksekusi posting pemulihan kredit saldo RecallRestore ke buku besar (BE-HRD-127, HRD-DEC-113, LEV-W4)
            var ledgerResult = await _ledgerService.PostRecallRestoreAsync(recall.Id, hrManagerUserId, cancellationToken);
            if (!ledgerResult.Success)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallExecutionResultDto>.Fail(
                    ledgerResult.StatusCode,
                    $"Gagal memulihkan saldo cuti pegawai: {ledgerResult.Message}");
            }

            // 2. Pemulihan status jadwal dinas shift di modul Scheduling 06 dari OnLeave ke Scheduled (INT-HR-46)
            var shiftsToRestore = await _dbContext.Set<TrxShiftAssignment>()
                .Where(s => !s.IsDelete && !s.IsCancel &&
                            s.WorkforceProfileId == recall.WorkforceProfileId &&
                            s.ShiftDate >= recall.EffectiveRecallDate &&
                            s.ShiftDate <= recall.LeaveRequest.EndDate)
                .ToListAsync(cancellationToken);

            foreach (var shift in shiftsToRestore)
            {
                shift.AssignmentStatus = "Scheduled";
                shift.HasLeaveConflict = false;
                shift.UpdateBy = hrManagerUserId;
                shift.UpdateDateTime = now;
            }

            // Update status permohonan cuti menjadi Recalled
            recall.LeaveRequest.LeaveRequestStatus = "Recalled";
            recall.LeaveRequest.UpdateBy = hrManagerUserId;
            recall.LeaveRequest.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Recall {RecallNumber} disetujui resmi oleh HR Manager {UserId}. Saldo {Days} hari dipulihkan dan jadwal shift diaktifkan kembali.",
                recall.RecallNumber, hrManagerUserId, recall.RestoredDays);

            var summary = await _ledgerService.GetSummaryAsync(recall.WorkforceProfileId, recall.LeaveRequest.LeaveTypeId, now.Year, cancellationToken);

            return LeaveBalanceLedgerServiceResult<LeaveRecallExecutionResultDto>.Ok(
                new LeaveRecallExecutionResultDto
                {
                    RecallId = recall.Id,
                    RecallNumber = recall.RecallNumber,
                    RestoredDays = recall.RestoredDays,
                    BalanceRemaining = summary?.AvailableBalanceDays,
                    Status = recall.Status,
                    ExecutedAt = now,
                    Message = $"Pemanggilan dinas darurat disetujui oleh HR Manager. Saldo {recall.RestoredDays} hari dipulihkan ke ledger dan shift pegawai diaktifkan kembali."
                },
                "Persetujuan pemanggilan dinas darurat berhasil dieksekusi.");
        }

        /// <summary>
        /// Penolakan tiket recall oleh HR Manager.
        /// </summary>
        public async Task<LeaveBalanceLedgerServiceResult<LeaveRecallDto>> RejectRecallAsync(
            Guid recallId,
            Guid hrManagerUserId,
            bool isHrManager,
            RejectLeaveRecallCommand command,
            CancellationToken cancellationToken = default)
        {
            if (!isHrManager)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya HR Manager yang memiliki wewenang menolak pemanggilan cuti darurat (VR-LEV-15, LEV-B5).");
            }

            var recall = await _dbContext.Set<TrxLeaveRecallRequest>()
                .Include(r => r.LeaveRequest)
                .FirstOrDefaultAsync(r => r.Id == recallId && !r.IsDelete, cancellationToken);

            if (recall == null)
            {
                return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Fail(StatusCodes.Status404NotFound, "Tiket recall tidak ditemukan.");
            }

            var now = DateTime.UtcNow;
            recall.Status = "RejectedByHrManager";
            recall.RejectionReason = command.RejectionReason.Trim();
            recall.ApprovedByHrManagerId = hrManagerUserId;
            recall.ApprovedAt = now;
            recall.UpdateBy = hrManagerUserId;
            recall.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Recall {RecallNumber} ditolak oleh HR Manager {UserId}. Alasan: {Reason}.",
                recall.RecallNumber, hrManagerUserId, command.RejectionReason);

            return LeaveBalanceLedgerServiceResult<LeaveRecallDto>.Ok(
                new LeaveRecallDto
                {
                    Id = recall.Id,
                    RecallNumber = recall.RecallNumber,
                    LeaveRequestId = recall.LeaveRequestId,
                    RequestNumber = recall.LeaveRequest?.RequestNumber ?? string.Empty,
                    WorkforceProfileId = recall.WorkforceProfileId,
                    EffectiveRecallDate = recall.EffectiveRecallDate,
                    RestoredDays = recall.RestoredDays,
                    EmergencyCategory = recall.EmergencyCategory,
                    RecallReason = recall.RecallReason,
                    Status = recall.Status,
                    RequestedByHeadUnitId = recall.RequestedByHeadUnitId,
                    ApprovedByHrManagerId = hrManagerUserId,
                    ApprovedAt = now,
                    RejectionReason = recall.RejectionReason
                },
                "Permohonan dinas darurat recall berhasil ditolak.");
        }
    }
}
